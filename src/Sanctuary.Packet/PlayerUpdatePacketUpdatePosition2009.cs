using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public struct PlayerUpdatePacketUpdatePosition2009 : IDeserializable<PlayerUpdatePacketUpdatePosition2009>
{
    public const short OpCode = 127;

    public Vector3 Position;
    public Vector3 Direction;

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out PlayerUpdatePacketUpdatePosition2009 value)
    {
        value = default;
        var reader = new PacketReader(data);

        if (!reader.TryRead(out short opCode) || opCode != OpCode ||
            !reader.TryRead(out value.Position.X) ||
            !reader.TryRead(out value.Position.Y) ||
            !reader.TryRead(out value.Position.Z) ||
            !reader.TryRead(out value.Direction.X) ||
            !reader.TryRead(out value.Direction.Y) ||
            !reader.TryRead(out value.Direction.Z))
            return false;

        return reader.RemainingLength == 0;
    }
}
