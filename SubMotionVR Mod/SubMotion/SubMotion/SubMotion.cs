using Harmony;
using System;
using System.Collections;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.VR;x
using UWE;

namespace SubMotion
{
    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    internal class ArmsController_Update_Patch
    {
        
         [HarmonyPostfix]
        public void Postfix(ArmsController __instance, FullBodyBipedChain ik)
        {
            if (Player.main.motorMode != Player.MotorMode.Vehicle)
            {
                GameObject rightController = new GameObject("rightController");
                rightController.transform.parent = __instance.player.camRoot.transform;
                rightController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) + new Vector3(0f, -0.02f, -0.2f);
                rightController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand);
                rightController.transform.localRotation *= Quaternion.Euler(35f, 190f, 270f);
                __instance.ik.solver.rightHandEffector.target = rightController.transform;
                GameObject leftController = new GameObject("leftController");
                leftController.transform.parent = __instance.player.camRoot.transform;
                leftController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand) + new Vector3(0f, 0.02f, -0.2f);
                leftController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand);
                leftController.transform.localRotation *= Quaternion.Euler(270f, 90f, 0f);
                __instance.ik.solver.leftHandEffector.target = leftController.transform;
            }
        }
    }
}

namespace SubMotion
{
    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Reconfigure")]
    internal class ArmsController_Reconfigure_Patch
    {
       
        [HarmonyPrefix]
        public void Prefix(ArmsController __instance, PlayerTool tool)
        {
            __instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.leftHandElbow;
            __instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 1f;
            if (tool == null)
            {
                __instance.leftAim.shouldAim = false;
                __instance.rightAim.shouldAim = false;
                __instance.ik.solver.leftHandEffector.target = null;
                __instance.ik.solver.rightHandEffector.target = null;
                if (!__instance.pda.isActiveAndEnabled)
                {
                    if (__instance.leftWorldTarget)
                    {
                        __instance.ik.solver.leftHandEffector.target = __instance.leftWorldTarget;
                        __instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = null;
                        __instance.ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 0f;
                    }
                    if (__instance.rightWorldTarget)
                    {
                        __instance.ik.solver.rightHandEffector.target = __instance.rightWorldTarget;
                        return;
                    }
                }
            }

        }

    }
}



