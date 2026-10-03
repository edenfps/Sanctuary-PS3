using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketUpdateVehicleRaceServerData : BaseVehicleRacePacket, IDeserializable<VehicleRacePacketUpdateVehicleRaceServerData>, ISerializablePacket
{
    public new const short OpCode = 6;

    public byte[] Data = [];

    public VehicleRacePacketUpdateVehicleRaceServerData() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.WritePayload(Data);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out VehicleRacePacketUpdateVehicleRaceServerData value)
    {
        value = new VehicleRacePacketUpdateVehicleRaceServerData();

        var reader = new PacketReader(data);

        if (!value.TryRead(ref reader))
            return false;

        if (reader.RemainingLength > 0)
        {
            if (!reader.TryReadExact(reader.RemainingLength, out var remaining))
                return false;

            value.Data = remaining.ToArray();
        }

        return true;
    }
}
