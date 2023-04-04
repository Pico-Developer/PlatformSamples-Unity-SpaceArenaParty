using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    public class AvatarName : NetworkBehaviour
    {
        public TMP_Text username;

        private Transform _cameraRig;

        private void Start()
        {
            if (IsOwner == false)
            {
                _cameraRig = Camera.main.GetComponentInParent<Transform>();
                username.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (IsOwner == false)
                username.gameObject.transform.rotation =
                    Quaternion.LookRotation(username.gameObject.transform.position - _cameraRig.position);
        }
    }
}