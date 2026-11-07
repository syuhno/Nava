using HarmonyLib;
using SafeExamBrowser.Settings;
using System;
using System.Linq;
using System.Reflection;

namespace Milim.patch
{
    [HarmonyPatch]
    public class DataProcessorPatch
    {
        static MethodBase TargetMethod()
        {
            Type type = null;
            try
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "SafeExamBrowser.Configuration", StringComparison.OrdinalIgnoreCase))
                    ?? Assembly.Load("SafeExamBrowser.Configuration");

                type = asm?.GetType("SafeExamBrowser.Configuration.ConfigurationData.DataProcessor");
            }
            catch
            {
                type = AccessTools.TypeByName("SafeExamBrowser.Configuration.ConfigurationData.DataProcessor");
            }

            return AccessTools.Method(type, "InitializeClipboardSettings");
        }

        static bool Prefix(AppSettings settings)
        {
            if (settings != null)
            {
                settings.Browser.UseIsolatedClipboard = false;
                settings.Keyboard.AllowCtrlC = false;
                settings.Keyboard.AllowCtrlV = false;
                settings.Keyboard.AllowCtrlX = false;
            }

            return false;
        }
    }
}
