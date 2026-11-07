using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Milim
{
    public static class InitMilim
    {
        public static int Nava(string unused)
        {
            try
            {
                var milim = new Harmony("lord.of.wrath");
                milim.PatchAll(Assembly.GetExecutingAssembly());

                patch.IntegrityResponsibility.Apply(milim);

                using (var currentProcess = Process.GetCurrentProcess())
                {
                    if (string.Equals(currentProcess.ProcessName, "safeexambrowser.client", StringComparison.OrdinalIgnoreCase))
                    {
                        patch.InternalContextMenuHandler.Apply(milim);
                    }
                }

                return 7;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Milim::Error] Initialization failed: {ex.Message}");
                return -1;
            }
        }
    }
}
