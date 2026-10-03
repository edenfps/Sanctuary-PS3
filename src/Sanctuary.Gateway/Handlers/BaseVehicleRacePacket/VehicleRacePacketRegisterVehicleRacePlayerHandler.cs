using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class VehicleRacePacketRegisterVehicleRacePlayerHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(VehicleRacePacketRegisterVehicleRacePlayerHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!VehicleRacePacketRegisterVehicleRacePlayer.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(VehicleRacePacketRegisterVehicleRacePlayer));
            return false;
        }

        _logger.LogInformation("Player {guid} registered for VehicleRace", connection.Player.Guid);

        return true;
    }
}
