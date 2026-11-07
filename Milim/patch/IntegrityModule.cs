using HarmonyLib;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Milim.patch
{
    [HarmonyPatch]
    public class IntegrityModulePatch
    {
        static MethodBase TargetMethod()
        {
            var targetType = AccessTools.TypeByName("SafeExamBrowser.Configuration.Integrity.IntegrityModule")
                          ?? AccessTools.TypeByName("SafeExamBrowser.Integrity.IntegrityModule");

            return AccessTools.Method(targetType, "LogRuntimeComponentsViolation", new[] { typeof(IntPtr), typeof(int) });
        }

        static bool Prefix(IntPtr data, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var pointer = Marshal.ReadIntPtr(data, index * IntPtr.Size);
                if (pointer != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(pointer);
                }
            }

            if (data != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(data);
            }

            return false;
        }
    }
}
