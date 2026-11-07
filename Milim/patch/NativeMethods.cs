using HarmonyLib;
using SafeExamBrowser.WindowsApi;

namespace Milim.patch
{
    [HarmonyPatch(typeof(NativeMethods), nameof(NativeMethods.HideWindow))]
    public class HideWindowPatch
    {
        static bool Prefix() => false;
    }
}
