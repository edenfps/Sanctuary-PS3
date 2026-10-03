using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class WallOfDataPlayerKeyboardPacket : WallOfDataBasePacket, IDeserializable<WallOfDataPlayerKeyboardPacket>
{
    public new const byte OpCode = 2;

    public int Unknown1;
    public int KeyCode;
    public int Unknown3;

    public WallOfDataPlayerKeyboardPacket() : base(OpCode)
    {
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out WallOfDataPlayerKeyboardPacket value)
    {
        value = new WallOfDataPlayerKeyboardPacket();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (!reader.TryRead(out value.Unknown1))
            return false;

        if (!reader.TryRead(out value.KeyCode))
            return false;

        if (!reader.TryRead(out value.Unknown3))
            return false;

        return reader.RemainingLength == 0;
    }

    public override string ToString()
    {
        return $"Unknown1: {Unknown1}, KeyCode: {KeyCode}, Unknown3: {Unknown3}";
    }
}
