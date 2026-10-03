using Sanctuary.Core.IO;

namespace Sanctuary.Packet.Common;

public class UnderwaterFishSpawnInfo : ISerializableType
{
    public int Unknown;

    public int ModelId;

    public string? TintAlias;
    public string? TextureAlias;

    public int Unknown5;
    public int Unknown6;

    public bool Unknown7;

    public int Unknown8;
    public int Unknown9;
    public int Unknown10;
    public int Unknown11;
    public int Unknown12;
    public int Unknown13;
    public int Unknown14;
    public int Unknown15;
    public int Unknown16;
    public int Unknown17;

    public float Unknown18;

    public void Serialize(PacketWriter writer)
    {
        writer.Write(Unknown);

        writer.Write(ModelId);

        writer.Write(TintAlias);
        writer.Write(TextureAlias);

        writer.Write(Unknown5);
        writer.Write(Unknown6);

        writer.Write(Unknown7);

        writer.Write(Unknown8);
        writer.Write(Unknown9);
        writer.Write(Unknown10);
        writer.Write(Unknown11);
        writer.Write(Unknown12);
        writer.Write(Unknown13);
        writer.Write(Unknown14);
        writer.Write(Unknown15);
        writer.Write(Unknown16);
        writer.Write(Unknown17);

        writer.Write(Unknown18);
    }
}