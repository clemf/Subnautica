using Harmony;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.VR;

namespace SubMotion
{
 
    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Start")]
    class ArmsController_Start_Patch
    {
        [HarmonyPostfix]
        public static void PostFix(ArmsController __instance)
        {
            if (!VRSettings.enabled)
            {
                return;
            }

            VRHandsController.main.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    class ArmsController_Update_Patch
    {

        [HarmonyPostfix]
        public static void Postfix(ArmsController __instance)
        {
            if (!VRSettings.enabled) {
                return;
            }

            PDA pda = VRHandsController.main.pda;
            Player player = VRHandsController.main.player;
            if ((Player.main.motorMode != Player.MotorMode.Vehicle && !player.cinematicModeActive) || pda.isActiveAndEnabled)
            {
                VRHandsController.main.UpdateHandPositions();
            }
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Reconfigure")]
    class ArmsController_Reconfigure_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ArmsController __instance, PlayerTool tool)
        {
            FullBodyBipedIK ik = VRHandsController.main.ik;

            ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.leftHandElbow;
            ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 1f;
            if (tool == null)
            {
                Traverse tInstance = Traverse.Create(__instance);
                tInstance.Field("leftAim").Field("shouldAim").SetValue(false);
                tInstance.Field("rightAim").Field("shouldAim").SetValue(false);

                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
                if (!VRHandsController.main.pda.isActiveAndEnabled)
                {
                    Transform leftWorldTarget = tInstance.Field<Transform>("leftWorldTarget").Value;
                    if (leftWorldTarget)
                    {
                        ik.solver.leftHandEffector.target = leftWorldTarget;
                        ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = null;
                        ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 0f;
                    }

                    Transform rightWorldTarget = tInstance.Field<Transform>("rightWorldTarget").Value;
                    if (rightWorldTarget)
                    {
                        ik.solver.rightHandEffector.target = rightWorldTarget;
                        return;
                    }
                }
            }

        }

    }
}