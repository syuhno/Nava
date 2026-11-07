using HarmonyLib;
using SafeExamBrowser.WindowsApi.Contracts;
using SafeExamBrowser.WindowsApi.Processes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Milim.patch
{
    [HarmonyPatch(typeof(ProcessFactory), nameof(ProcessFactory.GetAllRunning))]
    public class FilterProcess
    {
        private static readonly string[] ExcludedNames = { "nava", "chrome", "snipping", "dllhost" };

        static void Postfix(ref IList<IProcess> __result)
        {
            if (__result == null)
            {
                return;
            }

            __result = __result.Where(proc =>
            {
                var name = proc?.Name;
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
        }
    }
}
