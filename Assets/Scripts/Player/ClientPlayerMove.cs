using SpaceArenaParty.Player;
using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    [RequireComponent(typeof(ServerPlayerMove))]
    [DefaultExecutionOrder(1)]
    public class ClientPlayerMove : NetworkBehaviour
    {
        [SerializeField] private AvatarIKController m_AvatarIKController;

        [SerializeField] private AvatarAnimationController m_AvatarAnimationController;

        [SerializeField] private Collider m_Collider;

        private void Awake()
        {
            m_AvatarIKController.enabled = false;
            m_AvatarAnimationController.enabled = false;
            m_Collider.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            enabled = IsClient;
            if (!IsOwner)
            {
                enabled = false;
                m_Collider.enabled = true;
                return;
            }


            m_AvatarIKController.enabled = true;
            m_AvatarAnimationController.enabled = true;
        }
    }
}