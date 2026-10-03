using System.Collections.Concurrent;
using System.Numerics;

using Sanctuary.Packet;
using Sanctuary.Packet.Common;

namespace Sanctuary.Gateway.Fishing;

public sealed class FishingSessionState
{
    public ulong PlayerGuid { get; init; }

    public Vector4 LastCastPosition { get; set; }

    public bool HasActiveCast { get; set; }

    public int CastRequestCount { get; set; }
}

public static class FishingSessions
{
    public const int ThinFishModelId = 1670;
    public const int MediumFishModelId = 1671;
    public const int FatFishModelId = 1672;

    private static readonly ConcurrentDictionary<ulong, FishingSessionState> Sessions = new();

    public static FishingSessionState GetOrCreate(ulong playerGuid) =>
        Sessions.GetOrAdd(playerGuid, static guid => new FishingSessionState
        {
            PlayerGuid = guid,
            LastCastPosition = Vector4.Zero
        });

    public static FishingPacketSpawnFishRun CreateSpawnFishRun(Vector4 center)
    {
        return new FishingPacketSpawnFishRun
        {
            Unknown = true,
            Unknown2 = 1,
            Unknown3 = string.Empty,
            UnderwaterFishSpawns =
            [
                CreateFishSpawn(1, ThinFishModelId, 0.85f),
                CreateFishSpawn(2, MediumFishModelId, 1.0f),
                CreateFishSpawn(3, FatFishModelId, 1.2f)
            ]
        };
    }

    public static FishingPacketUpdateData CreateUpdateData(ulong playerGuid, Vector4 position)
    {
        if (position == Vector4.Zero)
            position = new Vector4(0f, 0f, 0f, 1f);

        position.W = 1f;

        return new FishingPacketUpdateData
        {
            Guid = playerGuid,
            Position = position
        };
    }

    private static UnderwaterFishSpawnInfo CreateFishSpawn(int id, int modelId, float scale)
    {
        return new UnderwaterFishSpawnInfo
        {
            Unknown = id,
            ModelId = modelId,
            TintAlias = string.Empty,
            TextureAlias = string.Empty,
            Unknown5 = 1,
            Unknown6 = 1,
            Unknown7 = true,
            Unknown8 = 1,
            Unknown9 = 1,
            Unknown10 = 1,
            Unknown11 = 1,
            Unknown12 = 1,
            Unknown13 = 1,
            Unknown14 = 1,
            Unknown15 = 1,
            Unknown16 = 1,
            Unknown17 = 1,
            Unknown18 = scale
        };
    }
}
