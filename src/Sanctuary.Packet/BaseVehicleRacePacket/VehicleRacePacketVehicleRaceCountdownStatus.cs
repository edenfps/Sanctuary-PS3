using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketVehicleRaceCountdownStatus : BaseVehicleRacePacket, ISerializablePacket
{
    public new const short OpCode = 10;

    public byte[] Data = [];

    public VehicleRacePacketVehicleRaceCountdownStatus() : base(OpCode)
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
