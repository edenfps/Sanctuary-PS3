using System.Collections.Generic;

namespace Sanctuary.Gateway.Racing;

public static class RacingActivityZones
{
    private static readonly HashSet<int> RacingActivities = [62, 63, 64, 231, 1048];
    private static readonly HashSet<int> DemoDerbyActivities = [65, 67, 68, 1047];

    public static bool IsRacingActivity(int activityId) => RacingActivities.Contains(activityId);

    public static bool IsDemoDerbyActivity(int activityId) => DemoDerbyActivities.Contains(activityId);
}
