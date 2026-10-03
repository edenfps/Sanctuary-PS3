using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Gateway.Fishing;
using Sanctuary.Packet;
using Sanctuary.Packet.Common;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class FishingPacketRegisterPlayerRequestHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(FishingPacketRegisterPlayerRequestHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        _logger.LogTrace("Received fishing register player request. Payload: {payload}", Convert.ToHexString(data));

        var player = connection.Player;
        var spawnPos = player.Position;

        connection.SendTunneled(new FishingPacketRegisterPlayerResponse
        {
            FishingPlayerConfig = new FishingPlayerConfig
            {
                Unknown = 4,
                Unknown2 = 1,
                Unknown3 = 1,
                Unknown4 = 1,
                Unknown5 = 1,
                Unknown6 = 10,
                Unknown7 = 21,
                Unknown8 = 1,
                Unknown9 = 1,
                Unknown10 = 6.0f,
                Unknown11 = 0.444f,
                Unknown12 = 1.85f,
                Unknown13 = 0.2f,
                Unknown14 = 1,
                Unknown15 = 1,
                Unknown16 = 1
            },
            FishModelIds = [],
            FishingZoneConfig = new Sanctuary.Packet.Common.FishingZoneConfig
            {
                Unknown = null,
                Unknown2 = 1,
                Unknown3 = 1.0f,
                Unknown4 = 1,
                Unknown5 = 1,
                Unknown6 = 1
            }
        });

        connection.SendTunneled(new FishingPacketUpdateData
        {
            Guid = player.Guid,
            Position = player.Position
        });

        connection.SendTunneled(new FishingPacketFishInfoUpdate
        {
            ClientFishEntries = new List<ClientFishEntryInfo>
            {
                new() { Type = 418, NameId = 1, IconId = 1, FishCatchable = true }
            }
        });

        connection.SendTunneled(new FishingPacketSpawnProxiedFishingSchool
        {
            SchoolId = 1,
            Position = new Vector4(spawnPos.X + 5f, spawnPos.Y - 5f, spawnPos.Z + 5f, 1f),
            Rotation = new Vector4(0, 0, 0, 1),
            Fish = [new() { ModelId = 418, Unknown2 = 1, Unknown3 = 0x100 }],
            ModelIds = [418]
        });

        connection.SendTunneled(new FishingPacketSpawnProxiedFishingSchool
        {
            SchoolId = 2,
            Position = new Vector4(spawnPos.X + 15f, spawnPos.Y - 5f, spawnPos.Z - 5f, 1f),
            Rotation = new Vector4(0, 0, 0, 1),
            Fish = [new() { ModelId = 418, Unknown2 = 1, Unknown3 = 0x100 }],
            ModelIds = [418]
        });

        _logger.LogInformation("Player {guid} registered for fishing at {pos}", player.Guid, spawnPos);

        // Re-send fishing state after registration — the response processing
        // may have side effects that clear the flag on the client.
        connection.SendTunneled(new PlayerUpdatePacketUpdateCharacterState
        {
            Guid = player.Guid,
            State = FishingActivityZones.FishingCharacterState
        });

        return true;
    }
}
