using Sanctuary.Core.IO;

namespace Sanctuary.Packet.Common;

public class FishingZoneConfig : ISerializableType
{
    public int Unknown6;

    public string? Unknown;

    public int Unknown2;
    public float Unknown3; // stored as float in client
    public int Unknown4;
    public int Unknown5;

    public void Serialize(PacketWriter writer)
    {
        writer.Write(Unknown);

        writer.Write(Unknown2);
        writer.Write(Unknown3);
        writer.Write(Unknown4);
        writer.Write(Unknown5);
        writer.Write(Unknown6);
    }
}
