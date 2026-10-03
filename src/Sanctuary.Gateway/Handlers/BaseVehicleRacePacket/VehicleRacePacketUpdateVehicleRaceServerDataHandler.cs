using System;
using System.Numerics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class VehicleRacePacketUpdateVehicleRaceServerDataHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(VehicleRacePacketUpdateVehicleRaceServerDataHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!VehicleRacePacketUpdateVehicleRaceServerData.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(VehicleRacePacketUpdateVehicleRaceServerData));
            return false;
        }

        _logger.LogTrace("Received {name} packet. Data length: {len}", nameof(VehicleRacePacketUpdateVehicleRaceServerData), packet.Data.Length);

        return true;
    }
}
