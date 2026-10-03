using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyDataHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyDataHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData));
            return false;
        }

        _logger.LogTrace("Received {name} packet. Data length: {len}", nameof(VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData), packet.Data.Length);

        return true;
    }
}
