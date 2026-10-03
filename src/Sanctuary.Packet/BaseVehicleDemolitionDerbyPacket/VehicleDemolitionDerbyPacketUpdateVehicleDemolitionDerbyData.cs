using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData : BaseVehicleDemolitionDerbyPacket, IDeserializable<VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData>, ISerializablePacket
{
    public new const short OpCode = 5;

    public byte[] Data = [];

    public VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.WritePayload(Data);

        return writer.Buffer;
    }

    public static bool TryDeserialize(ReadOnlySpan<byte> data, out VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData value)
    {
        value = new VehicleDemolitionDerbyPacketUpdateVehicleDemolitionDerbyData();

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
