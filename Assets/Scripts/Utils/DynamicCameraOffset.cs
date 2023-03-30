using System;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class DynamicCameraOffset : MonoBehaviour
    {
        private VRRig _vrRig;

        private void Start()
        {
            _vrRig = FindObjectOfType<VRRig>();
        }

        private void Update()
        {
            var position = _vrRig.CameraOffset.position;
            var targetHeadY = transform.position.y + 1.65f;
            var headY = _vrRig.Head.position.y;
            var offset = targetHeadY - headY;
            if (Math.Abs(offset) > 0.1f)
                _vrRig.CameraOffset.transform.position = new Vector3(position.x, position.y + offset, position.z);
        }
    }
}