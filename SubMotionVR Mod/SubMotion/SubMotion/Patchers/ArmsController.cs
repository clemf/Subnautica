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
        

        private static VRHandsController __instance;
        public static VRHandsController instance
        {
            get {
                if (__instance == null)
                {
                    __instance = new VRHandsController();
                }
                return __instance;
            }
        }

        public void Initialize(Transform parent)
        {
            this.rightController = new GameObject("rightController");
            this.rightController.transform.parent = parent;

            this.leftController = new GameObject("leftController");
            this.leftController.transform.parent = parent;
        }

        public void UpdateHandPositions(FullBodyBipedIK ik)
        {
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
        private static Player _player;
        public static Player player
        {
            get
            {
                if (_player == null)
                {
                    _player = global::Utils.GetLocalPlayerComp();
                }
                return _player;
            }
        }
        [HarmonyPostfix]
        public static void PostFix(ArmsController __instance)
        {
            
            if (!VRSettings.enabled)
            {
                return;
            }
            {
                VRHandsController.instance.Initialize(player.camRoot.gameObject.transform);
            }
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Update")]
    internal class ArmsController_Update_Patch
    {
        private static Player _player;
        public static Player player
        {
            get
            {
                if (_player == null)
                {
                    _player = global::Utils.GetLocalPlayerComp();
                }
                return _player;
            }
        }
        private static Component _component;
        public static Component component
        {
            get
            {
                if (_component == null)
                {
                    _component = new Component();
                }
                return _component;
            }
        }
        private static FullBodyBipedIK ___ik;
        public static FullBodyBipedIK ik
        {
            get
            {
                if (___ik == null)
                {
                    ___ik = _component.GetComponent<FullBodyBipedIK>();
                }
                return ___ik;
            }
        }

        private static PDA ___pda;
        public static PDA pda
        {
            get
            {
                if (___pda == null)
                {
                    ___pda = player.GetPDA();
                }
                return ___pda;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ArmsController __instance)
        {
            if (!VRSettings.enabled) {
                return;
            }

            if ((Player.main.motorMode != Player.MotorMode.Vehicle && !player.cinematicModeActive) || pda.isActiveAndEnabled)
            {
                VRHandsController.instance.UpdateHandPositions(ik);
            }
        }
    }

    [HarmonyPatch(typeof(ArmsController))]
    [HarmonyPatch("Reconfigure")]
    internal class ArmsController_Reconfigure_Patch
    {
        private static Player _player;
        public static Player player
        {
            get
            {
                if (_player == null)
                {
                    _player = global::Utils.GetLocalPlayerComp();
                }
                return _player;
            }
        }
        private static PDA ___pda;
        public static PDA pda
        {
            get
            {
                if (___pda == null)
                {
                    ___pda = player.GetPDA();
                }
                return ___pda;
            }
        }
        private static FullBodyBipedIK ___ik;
        public static FullBodyBipedIK ik
        {
            get
            {
                if (___ik == null)
                {
                    ___ik = ik.GetComponent<FullBodyBipedIK>();
                }
                return ___ik;
            }
        }

        private static Transform leftWorldTarget;
        public static Transform leftTarget
        {
            get
            {
                if (leftWorldTarget == null)
                {
                    leftWorldTarget = leftTarget;
                }
                return leftWorldTarget;
            }
        }

        private static Transform rightWorldTarget;
        public static Transform rightTarget
        {
            get
            {
                if (rightWorldTarget == null)
                {
                    rightWorldTarget = rightTarget;
                }
                return rightWorldTarget;
            }
        }

        [HarmonyPrefix]
        public static void Prefix(ArmsController __instance, PlayerTool tool)
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
                    if (leftWorldTarget)
                    {
                        ___ik.solver.leftHandEffector.target = leftTarget;
                        ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = null;
                        ___ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 0f;
                    }
                    if (rightWorldTarget)
                    {
                        ___ik.solver.rightHandEffector.target = rightTarget;
                        return;
                    }
                }
            }

        }

    }
}



