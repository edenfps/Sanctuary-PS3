using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleDemolitionDerbyPacketVehicleHit : BaseVehicleDemolitionDerbyPacket, ISerializablePacket
{
    public new const short OpCode = 16;

    public byte[] Data = [];

    public VehicleDemolitionDerbyPacketVehicleHit() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.WritePayload(Data);

        return writer.Buffer;
    }
}
