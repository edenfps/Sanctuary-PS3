using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class BaseVehicleDemolitionDerbyPacket
{
    public const short OpCode = 34;

    private short SubOpCode;

    public BaseVehicleDemolitionDerbyPacket(short subOpCode)
    {
        SubOpCode = subOpCode;
    }

    public virtual void Write(PacketWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(SubOpCode);
    }

    public virtual bool TryRead(ref PacketReader reader)
    {
        if (!reader.TryRead(out short opCode) || opCode != OpCode)
            return false;

        if (!reader.TryRead(out short subOpCode) || subOpCode != SubOpCode)
            return false;

        return true;
    }

    public virtual bool TryReadBody(ref PacketReader reader) => true;
}
