using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public struct PlayerUpdatePacketCameraUpdate2009 : IDeserializable<PlayerUpdatePacketCameraUpdate2009>
{
    public const short OpCode = 126;

    public ulong Guid;
    public Vector3 Position;
    public Vector3 Direction;
    public byte State;
    public byte Unknown;

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out PlayerUpdatePacketCameraUpdate2009 value)
    {
        value = default;
        var reader = new PacketReader(data);

        if (!reader.TryRead(out short opCode) || opCode != OpCode ||
            !reader.TryRead(out value.Guid) ||
            !reader.TryRead(out value.Position.X) ||
            !reader.TryRead(out value.Position.Y) ||
            !reader.TryRead(out value.Position.Z) ||
            !reader.TryRead(out value.Direction.X) ||
            !reader.TryRead(out value.Direction.Y) ||
            !reader.TryRead(out value.Direction.Z) ||
            !reader.TryRead(out value.State) ||
            !reader.TryRead(out value.Unknown))
            return false;

        return reader.RemainingLength == 0;
    }
}
