using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace SpaceArenaParty.Utils
{
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint singleton;
        public Vector3 SpawnPosition { get; private set; }
        public Quaternion SpawnRotation { get; private set; }

        public static void Reset()
        {
            singleton.SpawnPosition = new Vector3(0.0f, 0f, 0.0f);
            singleton.SpawnRotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
        }

        private void OnEnable()
        {
            if (singleton && singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            singleton = this;
            Reset();
        }

        private bool IsSpawnPointAvailable(Vector3 spawnPosition, List<Vector3> positions)
        {
            return positions.TrueForAll(position =>
            {
                var delta = position - spawnPosition;
                return delta.magnitude > 1f;
            });
        }

        public Vector3 GetSpawnPoint()
        {
            var occupiedSpawnPoints = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList
                .Where(spawnedObject => spawnedObject.CompareTag("Player"))
                .Select(spawnedObject => spawnedObject.transform.position)
                .ToList();

            var iteration = 0;
            var scaleLimit = 500;
            var iterationLimit = 100;
            var step = scaleLimit / iterationLimit;
            var rnd = new Random();
            var currentRange = 0;

            var spawnPoint = Vector3.zero;

            while (iteration < iterationLimit)
            {
                spawnPoint = new Vector3(rnd.Next(-currentRange, currentRange) / 100f, 0.1f,
                    rnd.Next(-currentRange, currentRange) / 100f);
                if (IsSpawnPointAvailable(spawnPoint, occupiedSpawnPoints)) break;

                currentRange += step;
                iteration++;
            }

            return spawnPoint;
        }
    }
}