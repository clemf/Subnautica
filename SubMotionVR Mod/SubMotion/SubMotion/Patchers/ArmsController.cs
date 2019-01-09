using Harmony;
using System;
using System.Collections;
using System.Reflection;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.VR;
using UWE;

namespace SubMotion
{
    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    internal class ArmsController_Update_Patch
    {

         [HarmonyPostfix]
        public void Postfix(ArmsController __instance, FullBodyBipedChain ik, PlayerTool tool)
        {
         if ((VRSettings.enabled && Player.main.motorMode != Player.MotorMode.Vehicle && !__instance.player.cinematicModeActive) || __instance.pda.isActiveAndEnabled)
		{
			GameObject rightController = new GameObject("rightController");
			GameObject leftController = new GameObject("leftController");
			InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
			rightController.transform.parent = __instance.player.camRoot.transform;
			rightController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) + new Vector3(0f, -0.02f, -0.2f);
			rightController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand) * Quaternion.Euler(35f, 190f, 270f);
                __instance.ik.solver.rightHandEffector.target = rightController.transform;
			leftController.transform.parent = __instance.player.camRoot.transform;
			leftController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand) + new Vector3(0f, 0.02f, -0.2f);
			leftController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand) * Quaternion.Euler(270f, 90f, 0f);
                __instance.ik.solver.leftHandEffector.target = leftController.transform;
			if (heldItem.item.GetComponent<PropulsionCannon>())
			{
                    __instance.ik.solver.leftHandEffector.target = null;
                    __instance.ik.solver.rightHandEffector.target = null;
				return;
			}
			if (heldItem.item.GetComponent<StasisRifle>())
			{
                    __instance.ik.solver.leftHandEffector.target = null;
                    __instance.ik.solver.rightHandEffector.target = null;
			}
		}


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



