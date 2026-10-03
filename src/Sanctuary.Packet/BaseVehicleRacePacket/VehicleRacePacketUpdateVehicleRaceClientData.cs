using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketUpdateVehicleRaceClientData : BaseVehicleRacePacket, ISerializablePacket
{
    public new const short OpCode = 5;

    public byte[] Data = [];

    public VehicleRacePacketUpdateVehicleRaceClientData() : base(OpCode)
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
