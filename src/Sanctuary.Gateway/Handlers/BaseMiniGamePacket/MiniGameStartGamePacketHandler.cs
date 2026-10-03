using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Gateway.Fishing;
using Sanctuary.Gateway.Racing;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class MiniGameStartGamePacketHandler
{
    private static ILogger _logger = null!;

    private static readonly Dictionary<int, (string Name, Vector4 Position)> RacingZoneConfigs = new()
    {
        [62] = ("bw_kt_track", new Vector4(576f, 50f, 544f, 1f)),
        [63] = ("sh_kt_track", new Vector4(512f, 50f, 512f, 1f)),
        [64] = ("sg_kt_track", new Vector4(512f, 50f, 512f, 1f)),
        [65] = ("bw_dm_arena", new Vector4(256f, 50f, 288f, 1f)),
        [67] = ("sh_dm_arena", new Vector4(384f, 50f, 256f, 1f)),
        [68] = ("sg_dm_arena", new Vector4(256f, 50f, 256f, 1f)),
        [231] = ("ss_kt_track", new Vector4(512f, 50f, 512f, 1f)),
        [1047] = ("bw_dm_arena", new Vector4(256f, 50f, 288f, 1f)),
        [1048] = ("bw_kt_track", new Vector4(576f, 50f, 544f, 1f)),
    };

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(MiniGameStartGamePacketHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!MiniGameStartGamePacket.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(MiniGameStartGamePacket));
            return false;
        }

        _logger.LogTrace("Received {name} packet. ( {packet} )", nameof(MiniGameStartGamePacket), packet);

        var miniGameGameStarted = new MiniGameGameStartPacket(packet.StateId, packet.GroupId, packet.GameId);

        connection.SendTunneled(miniGameGameStarted);

        // Mining Practice
        if (packet.StateId == 1113)
        {
            var commandPacketStartFlashGame = new CommandPacketStartFlashGame()
            {
                LuaClass = "MiniGameFlash",
                Swf = "game_hidden.gfx"
            };

            connection.SendTunneled(commandPacketStartFlashGame);
        }
        else if (FishingActivityZones.TryGet(packet.StateId, out var fishingZone))
        {
            connection.Player.IsInFishingActivity = true;
            _logger.LogWarning("SET IsInFishingActivity=true for player {guid}, activity {id}", connection.Player.Guid, packet.StateId);

            // Skip PacketClientBeginZoning — its handler (sub_93D180) unconditionally
            // calls sub_B6BB90(fishing, 0) which disables fishing for the local player.
            // Instead just set the character state to enable the fishing UI + interactions.

            connection.SendTunneled(new PlayerUpdatePacketUpdateCharacterState
            {
                Guid = connection.Player.Guid,
                State = FishingActivityZones.FishingCharacterState
            });

            _logger.LogInformation(
                "Started fishing minigame activity {activityId} (no zone teleport)",
                packet.StateId);
        }
        // Kart Racing
        else if (RacingActivityZones.IsRacingActivity(packet.StateId))
        {
            _logger.LogInformation(
                "Started racing minigame activity {activityId}",
                packet.StateId);

            // Step 1: Set game state to 0 (prerequisite for vehicle creation, must be < 4)
            connection.SendTunneled(new VehicleRacePacketUpdateVehicleGameState
            {
                Data = [0x00, 0x00, 0x00, 0x00]
            });

            // Step 2: Create proxied vehicle for the player
            // Format: base(4) + VehicleConfig(~600 zeros) + GUID(8) + 4 floats(16) + bool(1) + byte(1)
            var vehicleData = new byte[630];
            var playerGuid = connection.Player.Guid;
            var guidBytes = BitConverter.GetBytes(playerGuid);
            Array.Copy(guidBytes, 0, vehicleData, 600, 8);
            connection.SendTunneled(new VehicleRacePacketCreateProxiedVehicleRace
            {
                Data = vehicleData
            });

            // Step 3: Set game state to 5 (racing start)
            connection.SendTunneled(new VehicleRacePacketUpdateVehicleGameState
            {
                Data = [0x05, 0x00, 0x00, 0x00]
            });
        }
        // Demo Derby
        else if (RacingActivityZones.IsDemoDerbyActivity(packet.StateId))
        {
            _logger.LogInformation(
                "Started demo derby minigame activity {activityId}",
                packet.StateId);

            if (RacingZoneConfigs.TryGetValue(packet.StateId, out var config))
            {
                var beginZoning = new PacketClientBeginZoning
                {
                    Name = config.Name,
                    Type = 2,
                    Position = config.Position,
                    Rotation = Quaternion.Identity,
                    Id = 0,
                    GeometryId = 214,
                    OverrideUpdateRadius = true
                };
                connection.SendTunneled(beginZoning);
                _logger.LogInformation("Sent zone teleport to {name} at ({x},{y},{z})",
                    config.Name, config.Position.X, config.Position.Y, config.Position.Z);
            }

            connection.SendTunneled(new VehicleDemolitionDerbyPacketUpdateVehicleGameState
            {
                Data = [0x05, 0x00, 0x00, 0x00]
            });
        }

        return true;
    }
}