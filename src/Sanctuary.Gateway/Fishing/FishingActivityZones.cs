using System.Collections.Generic;
using System.Numerics;

namespace Sanctuary.Gateway.Fishing;

public readonly record struct FishingZoneConfig(
    string ZoneName,
    string? Sky,
    Vector4 SpawnPosition,
    Quaternion SpawnRotation);

public static class FishingActivityZones
{
    public const int FishingCharacterState = 0x400000;

    private static readonly Dictionary<int, FishingZoneConfig> ZonesByActivityId = new()
    {
        // Darklit Lagoon
        [560] = new("bw_fishing_medpond", null, new Vector4(200f, 0f, 200f, 1f), Quaternion.Identity),
        // Brambleback's Bayou
        [561] = new("bw_fishing_stream", null, new Vector4(200f, 0f, 200f, 1f), Quaternion.Identity),
        // Rainbow Lake
        [562] = new("bw_fishing_medpond", null, new Vector4(200f, 0f, 200f, 1f), Quaternion.Identity),
        // Sacred Grove Shallows
        [563] = new("sg_fishing_medpond", null, new Vector4(435.05676f, -64.46508f, 370.70682f, 1f), Quaternion.Identity),
        // Wintery Basin
        [564] = new("sh_fishing_medpond", null, new Vector4(200f, 0f, 200f, 1f), Quaternion.Identity),
        // Frostbitten Banks
        [565] = new("sh_fishing_stream", null, new Vector4(200f, 0f, 200f, 1f), Quaternion.Identity),
    };

    public static bool IsFishingActivity(int activityId) => ZonesByActivityId.ContainsKey(activityId);

    public static bool TryGet(int activityId, out FishingZoneConfig config) =>
        ZonesByActivityId.TryGetValue(activityId, out config);
}
