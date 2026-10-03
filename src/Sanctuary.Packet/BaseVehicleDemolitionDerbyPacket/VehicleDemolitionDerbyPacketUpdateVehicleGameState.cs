using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleDemolitionDerbyPacketUpdateVehicleGameState : BaseVehicleDemolitionDerbyPacket, ISerializablePacket
{
    public new const short OpCode = 1;

    public byte[] Data = [];

    public VehicleDemolitionDerbyPacketUpdateVehicleGameState() : base(OpCode)
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
