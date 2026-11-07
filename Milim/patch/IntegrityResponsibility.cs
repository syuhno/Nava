using HarmonyLib;
using System;
using System.Reflection;

namespace Milim.patch
{
    public static class IntegrityResponsibility
    {
        public static void Apply(Harmony harmony)
        {
            ApplyClient(harmony);
            ApplyRuntime(harmony);
        }

        private static void ApplyClient(Harmony harmony)
        {
            var type = AccessTools.TypeByName("SafeExamBrowser.Client.Responsibilities.IntegrityResponsibility");
            if (type == null)
            {
                return;
            }

            var method = AccessTools.Method(type, "HandleRuntimeIntegrityStatus");
            if (method == null)
            {
                return;
            }

            var prefix = typeof(IntegrityResponsibility).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
        }

        private static void ApplyRuntime(Harmony harmony)
        {
            var type = AccessTools.TypeByName("SafeExamBrowser.Runtime.Responsibilities.IntegrityResponsibility");
            if (type == null)
            {
                return;
            }

            var method = AccessTools.Method(type, "HandleRuntimeIntegrityStatus");
            if (method == null)
            {
                return;
            }

            var prefix = typeof(IntegrityResponsibility).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
        }

        private static bool Prefix()
        {
            return false;
        }
    }
}
