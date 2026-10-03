using System.Collections.Concurrent;

using Sanctuary.Core.IO;
using Sanctuary.Packet;

namespace Sanctuary.Gateway.Handlers;

public static class PacketInGamePurchase2009Handler
{
    private static readonly ConcurrentDictionary<ulong, byte> _walletSent = new();

    public static bool HandlePacket(GatewayConnection connection, PacketReader reader)
    {
        if (!reader.TryRead(out short subOpCode))
            return false;

        if (subOpCode != 6 || reader.RemainingLength != 0)
            return false;

        if (_walletSent.TryAdd(connection.Player.Guid, 0))
            connection.SendTunneled(new PacketInGamePurchaseWalletInfoResponse2009
            {
                StationCash = connection.Player.StationCash
            });

        return true;
    }
}
