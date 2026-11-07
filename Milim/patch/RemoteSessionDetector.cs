using HarmonyLib;
using SafeExamBrowser.Monitoring;

namespace Milim.patch
{
    [HarmonyPatch(typeof(RemoteSessionDetector), nameof(RemoteSessionDetector.IsRemoteSession))]
    public class RemoteSessionDetectorPatch
    {
        static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
