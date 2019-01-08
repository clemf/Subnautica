using System.Reflection;
using Harmony;

// You can use this file almost as-is. Just change the marked lines below. This will be the main file of each mod that tells Harmony to load your changes.
namespace RunningWithTools     // Change this line to match your mod.
{
    public class MainPatcher
    {
        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("com.oldark.subnautica.runningwithtools.mod");   // Change this line to match your mod. 
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
