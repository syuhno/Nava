using HarmonyLib;
using SafeExamBrowser.UserInterface.Shared.Utilities;

namespace Milim.patch
{
    [HarmonyPatch(typeof(WindowExtensions), nameof(WindowExtensions.ExcludeFromCapture))]
    public class ExcludeFromCapture
    {
        static bool Prefix() => false;
    }
}
