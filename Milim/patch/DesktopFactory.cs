using HarmonyLib;
using SafeExamBrowser.WindowsApi.Desktops;

namespace Milim.patch
{
    [HarmonyPatch(typeof(DesktopFactory), "GenerateRandomDesktopName")]
    class GenerateDefaultDesktop
    {
        static bool Prefix(ref string __result)
        {
            __result = "Default";
            return false;
        }
    }
}
