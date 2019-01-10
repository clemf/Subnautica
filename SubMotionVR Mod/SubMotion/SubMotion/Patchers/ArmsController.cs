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
    public class VRHandsController : MonoBehaviour
    {
        public GameObject rightController;
        public GameObject leftController;
        public ArmsController armsController;

        private static VRHandsController _instance;
        public static VRHandsController instance {
            get {
                if (_instance == null) {
                    _instance = new GameObject("VRHandsController");
                }
                return _instance;
            }
        }

        protected void Initialize(ArmsController _armsController)
        {
            this.armsController = _armsController;
            Transform parent = this.armsController.player.camRoot.transform;

            this.rightController = new GameObject("rightController");
            this.rightController.transform.parent = parent;

            this.leftController = new GameObject("leftController");
            this.leftController.transform.parent = parent;
        }

        public void UpdateHandPositions() {
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;

            this.rightController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) + new Vector3(0f, -0.02f, -0.2f);
            this.rightController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand) * Quaternion.Euler(35f, 190f, 270f);

            leftController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand) + new Vector3(0f, 0.02f, -0.2f);
            leftController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand) * Quaternion.Euler(270f, 90f, 0f);

            if (heldItem.item.GetComponent<PropulsionCannon>()) {
                __instance.ik.solver.leftHandEffector.target = null;
                __instance.ik.solver.rightHandEffector.target = null;
            } else if (heldItem.item.GetComponent<StasisRifle>()) {
                __instance.ik.solver.leftHandEffector.target = null;
                __instance.ik.solver.rightHandEffector.target = null;
            } else {
                __instance.ik.solver.leftHandEffector.target = leftController.transform;
                __instance.ik.solver.rightHandEffector.target = rightController.transform;
            }
        }
    }


    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Start")]
    internal class ArmsController_Start_Patch
    {
        [HarmonyPostfix]
        public void PostFix(ArmsController __instance)
        {
            if (!VRSettings.enabled) {
                return;
            }

            VRHandsController.instance.Initialize(__instance);
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    internal class ArmsController_Update_Patch
    {

        [HarmonyPostfix]
        public void Postfix(ArmsController __instance, FullBodyBipedChain ik, PlayerTool tool)
        {
            if (!VRSettings.enabled) {
                return;
            }

            if ((Player.main.motorMode != Player.MotorMode.Vehicle && !__instance.player.cinematicModeActive) || __instance.pda.isActiveAndEnabled)
            {
                VRHandsController.instance.UpdateHandPositions();
            }
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



