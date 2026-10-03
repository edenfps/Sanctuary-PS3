using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

/// Sub-opcode 14: ulong Guid + int + bool + int + int + string + int + int + int
///              + int + int + int + int + string + string + int + int + bool + int
public class FishingPacketFishingResult : BaseFishingPacket, ISerializablePacket
{
    public new const short OpCode = 14;

    public ulong Guid;
    public int ResultType;
    public bool Caught;
    public int FishId;
    public int Unknown1;
    public string? FishName;
    public int Unknown2;
    public int Unknown3;
    public int Unknown4;
    public int Unknown5;
    public int Unknown6;
    public int Unknown7;
    public int Unknown8;
    public string? UnknownStr1;
    public string? UnknownStr2;
    public int Unknown9;
    public int Unknown10;
    public bool Unknown11;
    public int Unknown12;

    public FishingPacketFishingResult() : base(OpCode) { }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();
        Write(writer);
        writer.Write(Guid);
        writer.Write(ResultType);
        writer.Write(Caught);
        writer.Write(FishId);
        writer.Write(Unknown1);
        writer.Write(FishName ?? "");
        writer.Write(Unknown2);
        writer.Write(Unknown3);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
        writer.Write(Unknown6);
        writer.Write(Unknown7);
        writer.Write(Unknown8);
        writer.Write(UnknownStr1 ?? "");
        writer.Write(UnknownStr2 ?? "");
        writer.Write(Unknown9);
        writer.Write(Unknown10);
        writer.Write(Unknown11);
        writer.Write(Unknown12);
        return writer.Buffer;
    }
}
