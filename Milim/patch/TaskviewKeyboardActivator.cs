using HarmonyLib;
using SafeExamBrowser.UserInterface.Shared.Activators;

namespace Milim.patch
{
    [HarmonyPatch(typeof(TaskviewKeyboardActivator), "IsActivation")]
    public class IsActivation
    {
        static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
