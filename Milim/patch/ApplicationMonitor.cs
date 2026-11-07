using HarmonyLib;
using SafeExamBrowser.Monitoring.Applications;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.WindowsApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Milim.patch
{
    [HarmonyPatch(typeof(ApplicationMonitor), "SystemEvent_WindowChanged")]
    class SystemEvent_WindowChangedPatch
    {
        private static readonly string[] IgnoredKeywords = { "snippingtool", "chrome" };

        static bool Prefix(IntPtr handle, INativeMethods ___nativeMethods)
        {
            if (___nativeMethods == null)
            {
                return true;
            }

            string winTitle = ___nativeMethods.GetWindowTitle(handle);
            if (string.IsNullOrWhiteSpace(winTitle))
            {
                return false;
            }

            string normalizedTitle = winTitle.Replace(" ", "");
            foreach (var prog in IgnoredKeywords)
            {
                if (normalizedTitle.IndexOf(prog, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ApplicationMonitor), "AddForTermination")]
    class AddForTermination
    {
        static bool Prefix(IProcess process, IList<BlacklistApplication> ___blacklist)
        {
            if (process == null || ___blacklist == null)
            {
                return true;
            }

            var changeAbleName = process.Name?.Replace(" ", "") ?? string.Empty;
            var originalName = process.OriginalName?.Replace(" ", "") ?? string.Empty;

            foreach (var b in ___blacklist)
            {
                var bOri = b.OriginalName?.Replace(" ", "") ?? string.Empty;
                var bExe = b.ExecutableName?.Replace(" ", "") ?? string.Empty;

                if (!string.IsNullOrEmpty(bOri) && string.Equals(bOri, originalName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(bExe) && string.Equals(bExe, changeAbleName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ApplicationMonitor), "IsWhitelisted")]
    class IsWhitelistedPatch
    {
        static bool Prefix(ref bool __result, out Guid? __state)
        {
            __state = Guid.NewGuid();
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(ApplicationMonitor), "Timer_Elapsed")]
    public class CallPerSecond
    {
        private static readonly string[] ExcludedNames = { "milim", "chrome", "snipping", "dllhost" };

        static bool Prefix(ref IList<IProcess> ___processes)
        {
            if (___processes == null)
            {
                return true;
            }

            ___processes = ___processes.Where(proc =>
            {
                var name = proc?.OriginalName;
                if (string.IsNullOrEmpty(name))
                {
                    return true;
                }

                var clean = name.Replace(" ", "");
                foreach (var ex in ExcludedNames)
                {
                    if (clean.IndexOf(ex, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return false;
                    }
                }
                return true;
            }).ToList();

            return true;
        }
    }
}
