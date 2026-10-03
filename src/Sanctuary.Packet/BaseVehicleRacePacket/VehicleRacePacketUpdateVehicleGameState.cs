using System;
using System.Numerics;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class VehicleRacePacketUpdateVehicleGameState : BaseVehicleRacePacket, ISerializablePacket
{
    public new const short OpCode = 1;

    public byte[] Data = [];

    public VehicleRacePacketUpdateVehicleGameState() : base(OpCode)
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
