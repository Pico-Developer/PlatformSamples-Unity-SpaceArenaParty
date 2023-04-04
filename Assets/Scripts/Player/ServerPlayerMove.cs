using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    [DefaultExecutionOrder(0)]
    public class ServerPlayerMove : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }

            OnServerSpawnPlayer();

            base.OnNetworkSpawn();
        }

        private void OnServerSpawnPlayer()
        {
            var spawnPosition = new Vector3(0f, 1f, 0f);
            transform.position = spawnPosition;
        }
    }
}