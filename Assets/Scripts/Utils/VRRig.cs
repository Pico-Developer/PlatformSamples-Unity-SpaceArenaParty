using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class VRRig : MonoBehaviour
    {
        public Transform Head;

        public Transform LeftHand;

        public Transform RightHand;

        public Transform CharacterOffset;

        public Transform CameraOffset;

        private void Start()
        {
            // DontDestroyOnLoad(this);
        }
    }
}