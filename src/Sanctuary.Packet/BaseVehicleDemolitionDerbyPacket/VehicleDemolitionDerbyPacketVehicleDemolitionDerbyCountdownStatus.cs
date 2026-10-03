using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleDemolitionDerbyPacketVehicleDemolitionDerbyCountdownStatus : BaseVehicleDemolitionDerbyPacket, ISerializablePacket
{
    public new const short OpCode = 7;

    public byte[] Data = [];

    public VehicleDemolitionDerbyPacketVehicleDemolitionDerbyCountdownStatus() : base(OpCode)
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
