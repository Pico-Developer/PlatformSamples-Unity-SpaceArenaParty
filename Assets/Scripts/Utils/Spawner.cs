using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class Spawner : MonoBehaviour
    {
        public NetworkObject playerPrefab;
        public NetworkObject sessionPrefab;

        public NetworkObject SpawnPlayer(ulong clientId, Vector3 position, Quaternion rotation)
        {
            var player = Instantiate(playerPrefab, position, rotation);
            player.SpawnAsPlayerObject(clientId);

            return player;
        }

        public NetworkObject SpawnSession()
        {
            var session = Instantiate(sessionPrefab);
            session.Spawn();

            return session;
        }
    }
}