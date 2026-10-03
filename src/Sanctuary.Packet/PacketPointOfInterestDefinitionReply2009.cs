using System;

using Sanctuary.Core.IO;

namespace Sanctuary.Packet;

public class PacketPointOfInterestDefinitionReply2009 : ISerializablePacket
{
    public const short OpCode = 58;

    public byte[] Payload = Array.Empty<byte>();

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();
        writer.Write(OpCode);
        writer.WritePayload(Payload);
        return writer.Buffer;
    }
}
