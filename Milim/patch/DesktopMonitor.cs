using HarmonyLib;
using SafeExamBrowser.WindowsApi.Desktops;

namespace Milim.patch
{
    [HarmonyPatch(typeof(DesktopMonitor), "Timer_Elapsed")]
    public class UnMonitorDesktop
    {
        static bool Prefix() => false;
    }

    [HarmonyPatch(typeof(DesktopMonitor), nameof(DesktopMonitor.Start))]
    public class Start_Hook
    {
        static bool Prefix() => false;
    }
}
