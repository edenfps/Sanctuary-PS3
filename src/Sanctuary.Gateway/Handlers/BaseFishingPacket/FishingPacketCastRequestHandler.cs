using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class FishingPacketCastRequestHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(FishingPacketCastRequestHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, ReadOnlySpan<byte> data)
    {
        if (!FishingPacketCastRequest.TryDeserialize(data, out var pkt))
        {
            _logger.LogError("Failed deserialize CastRequest: {data}", Convert.ToHexString(data));
            return false;
        }
        _logger.LogInformation("Player {g} cast at {p} flag={f}", pkt.Guid, pkt.Position, pkt.Flag);

        // Spawn bobber
        var bobber = new FishingPacketSpawnProxiedFishingBobber
        {
            Guid = (ulong)Random.Shared.NextInt64(),
            Unknown = 0,
            Position = pkt.Position,
            Rotation = new System.Numerics.Vector4(0, 0, 0, 1)
        };
        connection.SendTunneled(bobber);
        connection.Player.SendTunneledToVisible(bobber);

        return true;
    }
}
