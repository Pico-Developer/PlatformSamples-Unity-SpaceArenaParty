using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class Session : NetworkBehaviour
    {
        private Spawner spawner;

        private void Awake()
        {
            spawner = FindObjectOfType<Spawner>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnServerRpc(ulong clientId, Vector3 position, Quaternion rotation)
        {
            spawner.SpawnPlayer(clientId, position, rotation);
        }
    }
}