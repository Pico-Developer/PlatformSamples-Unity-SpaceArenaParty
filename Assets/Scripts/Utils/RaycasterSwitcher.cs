using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceArenaParty.Utils
{
    public class RaycasterSwitcher : MonoBehaviour
    {
        public GameObject TeleportRaycaster;
        public XRRayInteractor LeftRayInteractor;
        public XRRayInteractor RightRayInteractor;

        private bool _isTeleportRaycasterActive;


        // Update is called once per frame
        private void Update()
        {
            var leftUIRaycastResult = LeftRayInteractor.TryGetCurrentUIRaycastResult(out var raycastResult);
            var rightUIRaycastResult = RightRayInteractor.TryGetCurrentUIRaycastResult(out var raycastResult2);
            if (leftUIRaycastResult || rightUIRaycastResult)
            {
                if (_isTeleportRaycasterActive) DisableTeleportRaycaster();
            }

            else
            {
                if (_isTeleportRaycasterActive == false) EnableTeleportRaycaster();
            }
        }

        private void DisableTeleportRaycaster()
        {
            _isTeleportRaycasterActive = false;
            TeleportRaycaster.SetActive(false);
        }

        private void EnableTeleportRaycaster()
        {
            _isTeleportRaycasterActive = true;
            TeleportRaycaster.SetActive(true);
        }
    }
}