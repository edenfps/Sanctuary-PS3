using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleDemolitionDerbyPacketSetDerbyConfig : BaseVehicleDemolitionDerbyPacket, ISerializablePacket
{
    public new const short OpCode = 19;

    public byte[] Data = [];

    public VehicleDemolitionDerbyPacketSetDerbyConfig() : base(OpCode)
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
