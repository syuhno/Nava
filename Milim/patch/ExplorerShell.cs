using HarmonyLib;
using SafeExamBrowser.WindowsApi;

namespace Milim.patch
{
    [HarmonyPatch(typeof(ExplorerShell), nameof(ExplorerShell.HideAllWindows))]
    public class UnhideWindow
    {
        static bool Prefix() => false;
    }

    [HarmonyPatch(typeof(ExplorerShell), "KillExplorerShell")]
    public class EdotenseiExplorerShell
    {
        static bool Prefix() => false;
    }
}
