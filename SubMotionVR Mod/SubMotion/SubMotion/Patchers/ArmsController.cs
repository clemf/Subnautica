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
                    _instance = new VRHandsController();
                }
                return _instance;
            }
        }

        public void Initialize(Transform parent)
        {
            this.rightController = new GameObject("rightController");
            this.rightController.transform.parent = parent;

            this.leftController = new GameObject("leftController");
            this.leftController.transform.parent = parent;
        }

        public void UpdateHandPositions(FullBodyBipedIK ik) {
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;

            this.rightController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) + new Vector3(0f, -0.13f, -0.14f);
            this.rightController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand) * Quaternion.Euler(35f, 190f, 270f);

            leftController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand) + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand) * Quaternion.Euler(270f, 90f, 0f);

            if (heldItem.item.GetComponent<PropulsionCannon>()) {
                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
            } else if (heldItem.item.GetComponent<StasisRifle>()) {
                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
            } else {
                ik.solver.leftHandEffector.target = leftController.transform;
                ik.solver.rightHandEffector.target = rightController.transform;
            }
        }
    }


    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Start")]
    internal class ArmsController_Start_Patch
    {
        [HarmonyPostfix]
        public void PostFix(ArmsController __instance, Player ___player)
        {
            if (!VRSettings.enabled) {
                return;
            }

            VRHandsController.instance.Initialize(___player.camRoot.transform);
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    internal class ArmsController_Update_Patch
    {

        [HarmonyPostfix]
        public void Postfix(ArmsController __instance, FullBodyBipedIK ___ik, PDA ___pda, Player ___player)
        {
            if (!VRSettings.enabled) {
                return;
            }

            if ((Player.main.motorMode != Player.MotorMode.Vehicle && !___player.cinematicModeActive) || ___pda.isActiveAndEnabled)
            {
                VRHandsController.instance.UpdateHandPositions(___ik);
            }
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Reconfigure")]
    internal class ArmsController_Reconfigure_Patch
    {
        [HarmonyPrefix]
        public void Prefix(ArmsController __instance, PlayerTool tool, FullBodyBipedIK ___ik, PDA ___pda, Transform ___leftWorldTarget, Transform ___rightWorldTarget)
        {
            ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.leftHandElbow;
            ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 1f;
            if (tool == null)
            {
                Traverse.Create(__instance).Field("leftAim").Field("shouldAim").SetValue(false);
                Traverse.Create(__instance).Field("rightAim").Field("shouldAim").SetValue(false);

                ___ik.solver.leftHandEffector.target = null;
                ___ik.solver.rightHandEffector.target = null;
                if (!___pda.isActiveAndEnabled)
                {
                    if (___leftWorldTarget)
                    {
                        ___ik.solver.leftHandEffector.target = ___leftWorldTarget;
                        ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = null;
                        ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 0f;
                    }
                    if (___rightWorldTarget)
                    {
                        ___ik.solver.rightHandEffector.target = ___rightWorldTarget;
                        return;
                    }
                }
            }

        }

    }
}



