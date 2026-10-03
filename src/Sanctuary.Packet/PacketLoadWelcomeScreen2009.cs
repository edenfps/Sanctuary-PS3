using System.Collections.Generic;
using Sanctuary.Core.IO;
using Sanctuary.Packet.Common;

namespace Sanctuary.Packet;

/// <summary>The September 2009 client uses opcode 94 and ends after the two arrays.</summary>
public sealed class PacketLoadWelcomeScreen2009 : ISerializablePacket
{
    public List<ContentInfo> Contents { get; } = new();
    public List<ClaimCodeInfo> ClaimCodes { get; } = new();

    public byte[] Serialize()
    {
        using var writer = new PacketWriter();
        writer.Write((short)94);
        writer.Write(true);
        writer.Write(Contents);
        writer.Write(ClaimCodes.Count);
        foreach (var claimCode in ClaimCodes)
        {
            writer.Write(claimCode.Code);
            writer.Write(claimCode.NameId);
            writer.Write(claimCode.DescriptionId);
            writer.Write(claimCode.IconId);
        }
        return writer.Buffer;
    }
}
