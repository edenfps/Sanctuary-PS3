using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class PacketClientFinishedLoadingHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(PacketClientFinishedLoadingHandler));
    }

    public static bool HandlePacket(GatewayConnection connection)
    {
        _logger.LogTrace("Received {name} packet.", nameof(PacketClientFinishedLoading));

        var player = connection.Player;

        player.Visible = true;

        player.UpdatePosition(player.Position, player.Rotation);

        var mount = player.Mount;

        if (mount is not null)
        {
            mount.Visible = true;

            mount.UpdatePosition(player.Position, player.Rotation);

            var packetMountResponse = new PacketMountResponse();

            packetMountResponse.RiderGuid = mount.Rider.Guid;
            packetMountResponse.MountGuid = mount.Guid;

            packetMountResponse.Seat = mount.Seat;

            packetMountResponse.QueuePosition = mount.QueuePosition;

            packetMountResponse.Unknown = 1;

            packetMountResponse.CompositeEffectId = 46;

            connection.Player.SendTunneled(packetMountResponse);
        }

        player.Zone.OnClientFinishedLoading(player);

        return true;
    }
}