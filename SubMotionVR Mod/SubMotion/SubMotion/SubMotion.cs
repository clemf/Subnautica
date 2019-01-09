using System.Reflection;
using Harmony;

namespace SubMotion
{
    public class MainPatcher
    {
        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("com.musashi.subnautica.submotion.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
