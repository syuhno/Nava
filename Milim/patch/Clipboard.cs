using System.Timers;
using HarmonyLib;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Monitoring;
using SafeExamBrowser.Settings.Security;

namespace Milim.patch
{
    [HarmonyPatch(typeof(Clipboard), nameof(Clipboard.Initialize))]
    public class ClipboardInitPatch
    {
        static bool Prefix(ClipboardPolicy policy, ref ClipboardPolicy ___policy, ILogger ___logger, Timer ___timer)
        {
            ___policy = ClipboardPolicy.Allow;
            ___logger?.Debug("Cleared clipboard.");

            if (policy != ClipboardPolicy.Allow && ___timer != null)
            {
                ___logger?.Debug($"Started clipboard monitoring with interval {___timer.Interval} ms.");
            }
            else
            {
                ___logger?.Debug("Clipboard is allowed, not starting monitoring.");
            }

            ___logger?.Info($"Initialized clipboard for policy '{policy}'.");
            return false;
        }
    }
}
