using Harmony;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

namespace SubMotion
{
    public class VRHandsController : MonoBehaviour
    {
        public GameObject rightController;
        public GameObject leftController;
        public ArmsController armsController;
        public Player player;
        public FullBodyBipedIK ik;
        public PDA pda;

        static VRHandsController mInstance;
        public static VRHandsController Instance
        {
            get
            {
                if (mInstance == null)
                {
                    GameObject go = new GameObject();
                    mInstance = go.AddComponent<VRHandsController>();
                }
                return mInstance;
            }
        }

        public void Initialize(ArmsController controller)
        {
            armsController = controller;
            player = Utils.GetLocalPlayerComp();
            ik = controller.GetComponent<FullBodyBipedIK>();
            pda = player.GetPDA();

            rightController = new GameObject("rightController");
            rightController.transform.parent = player.camRoot.transform;

            leftController = new GameObject("leftController");
            leftController.transform.parent = player.camRoot.transform;
        }

        readonly List<XRNodeState> nodeStatesCache = new List<XRNodeState>();
        public void UpdateHandPositions()
        {
            InputTracking.GetNodeStates(nodeStatesCache);
            Vector3 rightHandPosition = new Vector3();
            Quaternion rightHandRotation = Quaternion.identity;
            Vector3 leftHandPosition = new Vector3();
            Quaternion leftHandRotation = Quaternion.identity;
            for (int i = 0; i < nodeStatesCache.Count; i++)
            {
                XRNodeState nodeState = nodeStatesCache[i];
                if (nodeState.nodeType == XRNode.RightHand)
                {
                    nodeState.TryGetPosition(out rightHandPosition);
                    nodeState.TryGetRotation(out rightHandRotation);
                }
                if (nodeState.nodeType == XRNode.LeftHand)
                {
                    nodeState.TryGetPosition(out leftHandPosition);
                    nodeState.TryGetRotation(out leftHandRotation);
                }
            }

            rightController.transform.localPosition = rightHandPosition + new Vector3(0f, -0.13f, -0.14f);
            rightController.transform.localRotation = rightHandRotation * Quaternion.Euler(35f, 190f, 270f);

            leftController.transform.localPosition = leftHandPosition + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = leftHandRotation * Quaternion.Euler(270f, 90f, 0f);

            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
            if (heldItem?.item.GetComponent<PropulsionCannon>())
            {
                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
            }
            else if (heldItem?.item.GetComponent<StasisRifle>())
            {
                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
            }
            else
            {
                ik.solver.leftHandEffector.target = leftController.transform;
                ik.solver.rightHandEffector.target = rightController.transform;
            }
        }

        [HarmonyPatch(typeof(ArmsController))]
        [HarmonyPatch("Start")]
        class ArmsController_Start_Patch
        {
            [HarmonyPostfix]
            public static void PostFix(ArmsController __instance)
            {
                if (!XRSettings.enabled)
                {
                    return;
                }

                Instance.Initialize(__instance);
            }
        }

        [HarmonyPatch(typeof(ArmsController))]
        [HarmonyPatch("Update")]
        class ArmsController_Update_Patch
        {

            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!XRSettings.enabled)
                {
                    return;
                }

                PDA pda = Instance.pda;

                Player player = Instance.player;

                if ((Player.main.motorMode != Player.MotorMode.Vehicle && !player.cinematicModeActive) || pda.isActiveAndEnabled)
                {
                    Instance.UpdateHandPositions();
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
                FullBodyBipedIK ik = Instance.ik;

                ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).bendGoal = __instance.leftHandElbow;
                ik.solver.GetBendConstraint(FullBodyBipedChain.LeftArm).weight = 1f;
                if (tool == null)
                {
                    Traverse tInstance = Traverse.Create(__instance);
                    tInstance.Field("leftAim").Field("shouldAim").SetValue(false);
                    tInstance.Field("rightAim").Field("shouldAim").SetValue(false);

                    ik.solver.leftHandEffector.target = null;
                    ik.solver.rightHandEffector.target = null;
                    if (!Instance.pda.isActiveAndEnabled)
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
}



