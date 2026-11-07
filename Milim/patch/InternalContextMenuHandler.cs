using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace Milim.patch
{
    public static class InternalContextMenuHandler
    {
        private static Harmony _harmony;

        public static void Apply(Harmony harmony)
        {
            _harmony = harmony;

            var targetAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "SafeExamBrowser.Browser", StringComparison.OrdinalIgnoreCase));

            if (targetAssembly != null)
            {
                Patch(targetAssembly);
            }
            else
            {
                AppDomain.CurrentDomain.AssemblyLoad += OnDllLoad;
            }
        }

        private static void OnDllLoad(object sender, AssemblyLoadEventArgs ea)
        {
            if (string.Equals(ea.LoadedAssembly.GetName().Name, "SafeExamBrowser.Browser", StringComparison.OrdinalIgnoreCase))
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnDllLoad;
                Patch(ea.LoadedAssembly);
            }
        }

        private static void Patch(Assembly targetAssembly)
        {
            var type = targetAssembly.GetType("SafeExamBrowser.Browser.Handlers.ContextMenuHandler");
            var targetMethod = type?.GetMethod("OnBeforeContextMenu", BindingFlags.Public | BindingFlags.Instance);

            if (targetMethod == null)
            {
                return;
            }

            var prefix = typeof(InternalContextMenuHandler).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            _harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));
        }

        private static bool Prefix() => false;
    }
}
