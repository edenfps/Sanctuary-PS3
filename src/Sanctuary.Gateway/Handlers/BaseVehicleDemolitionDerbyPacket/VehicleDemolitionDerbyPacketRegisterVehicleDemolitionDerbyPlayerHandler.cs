using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class VehicleDemolitionDerbyPacketRegisterVehicleDemolitionDerbyPlayerHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(VehicleDemolitionDerbyPacketRegisterVehicleDemolitionDerbyPlayerHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!VehicleDemolitionDerbyPacketRegisterVehicleDemolitionDerbyPlayer.TryDeserialize(data, out var packet))
        {
            _logger.LogError("Failed to deserialize {packet}.", nameof(VehicleDemolitionDerbyPacketRegisterVehicleDemolitionDerbyPlayer));
            return false;
        }

        _logger.LogInformation("Player {guid} registered for DemoDerby", connection.Player.Guid);

        return true;
    }
}
