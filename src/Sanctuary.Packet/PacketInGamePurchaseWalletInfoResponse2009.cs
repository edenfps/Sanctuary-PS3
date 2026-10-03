using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

// September 2009 uses base opcode 67, subopcode 7, and a shorter WalletInfo.
public class PacketInGamePurchaseWalletInfoResponse2009 : ISerializablePacket
{
    public int ErrorCode = 1;
    public int StationCash;

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();
        writer.Write((short)67);
        writer.Write((short)7);
        writer.Write(ErrorCode);
        writer.Write(true);
        writer.Write(StationCash);
        writer.Write(0); // Credit card ID
        writer.Write("SOE", 8);
        writer.Write(false); // International
        return writer.Buffer;
    }
}
