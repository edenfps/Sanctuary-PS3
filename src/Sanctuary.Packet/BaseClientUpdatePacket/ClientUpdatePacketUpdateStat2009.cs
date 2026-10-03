using System.Collections.Generic;

using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

public class ClientUpdatePacketUpdateStat2009 : BaseClientUpdatePacket, ISerializablePacket
{
    public new const short OpCode = 7;

    public List<CharacterStat> Stats = new();

    public ClientUpdatePacketUpdateStat2009() : base(OpCode)
    {
    }

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();
        Write(writer);
        writer.Write(Stats);
        return writer.Buffer;
    }
}
