using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    public class AvatarIKHeadTransform : NetworkBehaviour
    {
        public Transform followHead;

        private void Update()
        {
            if (IsOwner)
            {
                transform.position = followHead.TransformPoint(Vector3.zero) - followHead.forward * 0.5f;
                transform.rotation = followHead.rotation;
            }
            else
            {
                transform.position = followHead.TransformPoint(Vector3.zero);
                transform.rotation = followHead.rotation;
            }
        }
    }
}