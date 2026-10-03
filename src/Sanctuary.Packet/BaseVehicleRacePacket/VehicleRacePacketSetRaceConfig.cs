using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketSetRaceConfig : BaseVehicleRacePacket, ISerializablePacket
{
    public new const short OpCode = 34;

    public byte[] Data = [];

    public VehicleRacePacketSetRaceConfig() : base(OpCode)
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
