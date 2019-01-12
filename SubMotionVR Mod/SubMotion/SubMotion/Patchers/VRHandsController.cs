using Harmony;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.VR;

namespace SubMotion
{
    public class VRHandsController : MonoBehaviour
    {
        public GameObject rightController;
        public GameObject leftController;
        public ArmsController Component;
        public Player player;
        public FullBodyBipedIK ik;
        public PDA pda;

        private static VRHandsController _main;
        public static VRHandsController main
        {
            get
            {
                if (_main == null)
                {
                    _main = new VRHandsController();
                }
                return _main;
            }
        }

        public void Initialize(ArmsController Component)
        {

            player = global::Utils.GetLocalPlayerComp();
            ik = Component.GetComponent<FullBodyBipedIK>();
            pda = player.GetPDA();

            rightController = new GameObject("rightController");
            rightController.transform.parent = player.camRoot.transform;

            leftController = new GameObject("leftController");
            leftController.transform.parent = player.camRoot.transform;
        }

        public void UpdateHandPositions()
        {
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;

            rightController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.RightHand) + new Vector3(0f, -0.13f, -0.14f);
            rightController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.RightHand) * Quaternion.Euler(35f, 190f, 270f);

            leftController.transform.localPosition = InputTracking.GetLocalPosition(VRNode.LeftHand) + new Vector3(0f, -0.13f, -0.14f);
            leftController.transform.localRotation = InputTracking.GetLocalRotation(VRNode.LeftHand) * Quaternion.Euler(270f, 90f, 0f);

            if (heldItem.item.GetComponent<PropulsionCannon>())
            {
                ik.solver.leftHandEffector.target = null;
                ik.solver.rightHandEffector.target = null;
            }
            else if (heldItem.item.GetComponent<StasisRifle>())
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
    }
}
