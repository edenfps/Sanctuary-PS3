using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class PacketSendZoneDetails2009 : ISerializablePacket
{
    public const short OpCode = 43;

    public required string Name;
    public int Type = 2;
    public bool Tutorial;
    public bool Unknown2;
    public string? Sky;
    public bool IsInArena;

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        writer.Write(OpCode);
        writer.Write(Name);
        writer.Write(Type);
        writer.Write(Tutorial);
        writer.Write(Unknown2);
        writer.Write(Sky);
        writer.Write(IsInArena);

        return writer.Buffer;
    }
}
