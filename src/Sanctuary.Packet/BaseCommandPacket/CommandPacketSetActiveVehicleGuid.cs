using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class CommandPacketSetActiveVehicleGuid : BaseCommandPacket, ISerializablePacket
{
    public new const short OpCode = 31;

    public ulong VehicleGuid;

    public CommandPacketSetActiveVehicleGuid() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();

        Write(writer);

        writer.Write(VehicleGuid);

        return writer.Buffer;
    }
}
