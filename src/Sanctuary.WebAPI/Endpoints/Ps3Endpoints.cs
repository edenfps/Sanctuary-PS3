using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Sanctuary.Database;
using Sanctuary.Database.Entities;
using Sanctuary.Core.Helpers;
using Sanctuary.Game;

namespace Sanctuary.WebAPI.Endpoints;

// The PS3 client uses XML-over-HTTP before it connects to the normal UDP gateway.
// These routes are deliberately confined to this separate PS3 server build.
public static class Ps3Endpoints
{
    private const string LocalPs3Account = "ps3-local";
    public static void MapPs3Endpoints(this WebApplication app)
    {
        app.MapGet("/app/appconfig.php", AppConfig);
        app.MapMethods("/ps3/ps3ws/account/info.php", ["GET", "POST"], AccountInfoAsync);
        app.MapMethods("/ps3/ps3ws/account/info.action", ["GET", "POST"], AccountInfoAsync);
        app.MapMethods("/ps3/ps3ws/game/serverList.php", ["GET", "POST"], ServerList);
        app.MapMethods("/ps3/ps3ws/game/serverList.action", ["GET", "POST"], ServerList);
        app.MapMethods("/ps3/ps3ws/character/login.php", ["GET", "POST"], CharacterLoginAsync);
        app.MapMethods("/ps3/ps3ws/character/login.action", ["GET", "POST"], CharacterLoginAsync);
        app.MapMethods("/ps3/ps3ws/character/checkName.php", ["GET", "POST"], CheckName);
        app.MapMethods("/ps3/ps3ws/character/checkName.action", ["GET", "POST"], CheckName);
        app.MapMethods("/ps3/ps3ws/character/create.php", ["GET", "POST"], CharacterCreate);
        app.MapMethods("/ps3/ps3ws/character/create.action", ["GET", "POST"], CharacterCreate);
    }

    private static IResult Xml(XElement value) => Results.Text(value.ToString(SaveOptions.DisableFormatting), "text/xml");

    private static IResult AppConfig() => Xml(new XElement("AppConfig",
        new XElement("Login"),
        new XElement("GameList",
            new XElement("Game", new XAttribute("env", "live"),
                new XElement("Labels", new XElement("Label", new XAttribute("id", 2))),
                new XElement("Versions",
                    new XElement("Version", new XAttribute("id", "scee-live-heads"),
                        new XElement("NPCommunicationID"),
                        new XElement("PSNServiceID"),
                        new XElement("PSNEntitlementRequired", 0),
                        new XElement("PSNActivationRequired", 0),
                        new XElement("PSNSubscriptionRequired", 0),
                        new XElement("PSNNewRulesEnabled", 0),
                        new XElement("PSNTicketCacheEnabled", 0),
                        new XElement("Digest", "local-ps3"),
                        new XElement("LaunchArgs", ""),
                        new XElement("Launch", "true")))))));

    private static async Task<IResult> AccountInfoAsync(DatabaseContext db)
    {
        var user = await GetOrCreatePs3UserAsync(db);
        var characters = await db.Characters.AsNoTracking()
            .Where(x => x.UserId == user.Id).ToListAsync();
        characters.Sort((left, right) => left.Id.CompareTo(right.Id));

        // These element and attribute names follow the NPUA30048 account parser
        // at game.elf:0xD9B6F8. Keep cloned PC characters out of this response.
        return Xml(new XElement("AccountInfoReply",
            new XAttribute("Status", characters.Count == 0 ? 2 : 1),
            new XAttribute("StatusMessage", characters.Count == 0 ? "Test" : "Success"),
            new XElement("Account",
                new XAttribute("Member", user.IsMember ? 1 : 0),
                new XAttribute("MaxCharacters", user.MaxCharacters),
                new XAttribute("Admin", user.IsAdmin ? 1 : 0)),
            new XElement("Characters", characters.Select(character =>
                new XElement("Character",
                    new XAttribute("HeadshotUrl", ""),
                    new XElement("Details",
                        new XAttribute("Guid", GuidHelper.GetPlayerGuid(character.Id)),
                        new XAttribute("Name", string.Join(' ', new[] { character.FirstName, character.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)))),
                        new XAttribute("Model", character.Model),
                        new XAttribute("Head", character.Head),
                        new XAttribute("Hair", character.Hair),
                        new XAttribute("ModelCustomization", character.ModelCustomization ?? ""),
                        new XAttribute("FacePaint", character.FacePaint ?? ""),
                        new XAttribute("SkinTone", character.SkinTone),
                        new XAttribute("EyeColor", character.EyeColor),
                        new XAttribute("HairColor", character.HairColor),
                        new XElement("Attachments")))))));
    }

    private static IResult ServerList() => Xml(new XElement("ServerListReply",
        new XAttribute("Status", 1),
        new XElement("StatusMessage", "Success"),
        new XElement("Servers", new XElement("Server",
            new XAttribute("Name", "PS3 Sanctuary"),
            new XAttribute("Online", 1),
            new XAttribute("Locked", 0)))));

    private static async Task<IResult> CharacterLoginAsync(HttpRequest request, DatabaseContext db, IConfiguration config, ILoggerFactory loggerFactory)
    {
        // The archived PHP sample logged POST bodies but did not document the request field names.
        // Record field names for protocol analysis while never logging PSN tickets or values.
        ulong requestedGuid = 0;
        var logger = loggerFactory.CreateLogger(nameof(Ps3Endpoints));
        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync();
            logger.LogInformation("PS3 character/login form fields: {Fields}; Guid={Guid}", string.Join(",", form.Keys), form["Guid"].ToString());
            ulong.TryParse(form["Guid"], out requestedGuid);
        }

        ulong? characterId = null;
        try
        {
            characterId = GuidHelper.GetPlayerId(requestedGuid);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Some PS3 flows post the database ID rather than Sanctuary's tagged GUID.
            if (requestedGuid != 0)
                characterId = requestedGuid;
        }

        var character = characterId.HasValue
            ? await db.Characters.FirstOrDefaultAsync(x => x.Id == characterId.Value && x.User.Username == LocalPs3Account)
            : null;
        if (character is null && requestedGuid == 0)
        {
            // The local PS3 account has no selection ambiguity with one character.
            var localCharacters = await db.Characters.Where(x => x.User.Username == LocalPs3Account).Take(2).ToListAsync();
            if (localCharacters.Count == 1)
            {
                character = localCharacters[0];
                logger.LogWarning("PS3 character/login did not provide a GUID; using sole local character {CharacterId}.", character.Id);
            }
        }
        if (character is null)
            return Xml(new XElement("CharacterLoginReply", new XAttribute("Status", 6), new XAttribute("StatusMessage", "No local character")));

        var ticket = Guid.NewGuid();
        character.Ticket = ticket;
        character.LastLogin = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        var gatewayAddress = config["PS3:GatewayAddress"] ?? "127.0.0.1:21260";
        return Xml(new XElement("CharacterLoginReply",
            new XAttribute("Status", 1),
            new XAttribute("StatusMessage", "Success"),
            new XAttribute("GatewayAddress", gatewayAddress),
            new XAttribute("GatewayTicket", ticket.ToString("N")),
            new XAttribute("Key", ticket.ToString("N"))));
    }

    private static IResult CheckName() => Xml(new XElement("CheckNameReply",
        new XAttribute("Status", 1), new XAttribute("StatusMessage", "Success"),
        new XElement("Suggestions")));

    private static async Task<IResult> CharacterCreate(HttpRequest request, DatabaseContext db, IResourceManager resources, ILoggerFactory loggerFactory)
    {
        if (!request.HasFormContentType)
            return CreateFailure("Invalid request");

        var form = await request.ReadFormAsync();
        var logger = loggerFactory.CreateLogger(nameof(Ps3Endpoints));
        logger.LogInformation("PS3 character/create form fields: {Fields}", string.Join(",", form.Keys));

        static bool Number(IFormCollection fields, string key, out int value) =>
            int.TryParse(fields[key], out value);

        if (!Number(form, "Model", out var modelId) ||
            !Number(form, "Head", out var headId) ||
            !Number(form, "Hair", out var hairId) ||
            !Number(form, "SkinTone", out var skinToneId) ||
            !Number(form, "EyeColor", out var eyeColor) ||
            !Number(form, "HairColor", out var hairColor) ||
            !resources.Models.TryGetValue(modelId, out var model) ||
            !resources.HeadMappings.TryGetValue(headId, out var head) ||
            !resources.HairMappings.TryGetValue(hairId, out var hair) ||
            !resources.SkinToneMappings.TryGetValue(skinToneId, out var skinTone))
        {
            logger.LogWarning("PS3 character/create has an invalid model or appearance ID. Model={Model} Head={Head} Hair={Hair} SkinTone={SkinTone}",
                form["Model"].ToString(), form["Head"].ToString(), form["Hair"].ToString(), form["SkinTone"].ToString());
            return CreateFailure("Invalid appearance");
        }

        var firstName = form["FirstName"].ToString().Trim();
        var lastName = form["LastName"].ToString().Trim();
        if (firstName.Length is 0 or > 16 || lastName.Length > 16)
            return CreateFailure("Invalid name");

        var user = await GetOrCreatePs3UserAsync(db);

        Number(form, "ModelCustomization", out var customizationId);
        Number(form, "FacePaint", out var facePaintId);
        resources.ModelCustomizationMappings.TryGetValue(customizationId, out var customization);
        resources.FacePaintMappings.TryGetValue(facePaintId, out var facePaint);

        var character = new DbCharacter
        {
            FirstName = firstName,
            LastName = lastName,
            Model = modelId,
            Head = head,
            HeadId = headId,
            Hair = hair,
            HairId = hairId,
            SkinTone = skinTone,
            SkinToneId = skinToneId,
            FacePaint = facePaint,
            FacePaintId = facePaintId,
            ModelCustomization = customization,
            ModelCustomizationId = customizationId,
            EyeColor = eyeColor,
            HairColor = hairColor,
            Gender = model.Gender,
            MembershipStatus = user.IsMember ? 2 : 0,
            UserId = user.Id,
            ActiveProfileId = 1,
            Coins = 1000
        };

        var profile = new DbProfile { Id = 1, Level = 1 };
        character.Profiles.Add(profile);

        foreach (var (itemField, tintField) in new (string, string)[]
        {
            ("ItemHands", "ItemHandsTint"),
            ("ItemFeet", "ItemFeetTint"),
            ("ItemLegs", "ItemLegsTint"),
            ("ItemChest", "ItemChestTint"),
            ("ItemHead", "ItemHeadTint"),
            ("ItemShoulders", "ItemShouldersTint")
        })
        {
            if (!Number(form, itemField, out var itemId) || itemId <= 0)
                continue;
            if (!resources.ClientItemDefinitions.ContainsKey(itemId))
            {
                logger.LogWarning("PS3 character/create item {ItemId} is absent from server definitions.", itemId);
                continue;
            }
            Number(form, tintField, out var tint);
            var item = new DbItem { Id = character.Items.Count + 1, Definition = itemId, Tint = tint, Count = 1 };
            character.Items.Add(item);
            profile.Items.Add(item);
        }

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        logger.LogInformation("Created PS3 character {CharacterId}.", character.Id);

        return Xml(new XElement("CharacterCreateReply",
            new XAttribute("Status", 1), new XAttribute("StatusMessage", "Success"),
            new XElement("Guid", GuidHelper.GetPlayerGuid(character.Id))));
    }

    private static IResult CreateFailure(string reason) => Xml(new XElement("CharacterCreateReply",
        new XAttribute("Status", 2), new XAttribute("StatusMessage", reason)));

    private static async Task<DbUser> GetOrCreatePs3UserAsync(DatabaseContext db)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Username == LocalPs3Account);
        if (user is not null)
            return user;

        user = new DbUser
        {
            Username = LocalPs3Account,
            Password = Guid.NewGuid().ToString("N"),
            MaxCharacters = 10,
            IsMember = true,
            IsAdmin = true,
            Created = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
