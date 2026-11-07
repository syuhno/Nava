using HarmonyLib;
using SafeExamBrowser.Monitoring.Keyboard;

namespace Milim.patch
{
    [HarmonyPatch(typeof(KeyboardInterceptor), "Start")]
    static class KeyboardInterceptorPatch
    {
        static bool Prefix() => false;
    }
}
