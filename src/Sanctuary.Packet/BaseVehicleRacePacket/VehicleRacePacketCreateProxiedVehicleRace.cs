using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketCreateProxiedVehicleRace : BaseVehicleRacePacket, ISerializablePacket
{
    public new const short OpCode = 11;

    public byte[] Data = [];

    public VehicleRacePacketCreateProxiedVehicleRace() : base(OpCode)
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
