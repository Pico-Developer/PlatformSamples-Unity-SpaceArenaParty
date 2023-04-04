using System;
using SpaceArenaParty.Utils;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    [Serializable]
    public class MapTransform
    {
        public Transform vrTarget;
        public Transform ikTarget;
        public Vector3 trackingPositionOffset;
        public Vector3 trackingRotationOffset;

        public MapTransform(Transform _vrTarget, Transform _ikTarget, Vector3 _offsetPosition, Vector3 _offsetRotation)
        {
            vrTarget = _vrTarget;
            ikTarget = _ikTarget;
            trackingPositionOffset = _offsetPosition;
            trackingRotationOffset = _offsetRotation;
        }

        public void MapVRAvatar()
        {
            ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
            ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        }
    }

    public class AvatarIKController : MonoBehaviour
    {
        [SerializeField] public MapTransform head;
        [SerializeField] public MapTransform leftHand;
        [SerializeField] public MapTransform rightHand;

        [SerializeField] private float turnSmoothness;

        public Transform IKHead;


        private VRRig vrRig;

        private void Start()
        {
            vrRig = FindObjectOfType<VRRig>();
            if (vrRig == null) return;
            SetLayerRecursively(gameObject, 13);
            head.vrTarget = vrRig.Head;
            leftHand.vrTarget = vrRig.LeftHand;
            rightHand.vrTarget = vrRig.RightHand;
        }

        private void Update()
        {
            Move();
            MapIK();
        }


        private void SetLayerRecursively(GameObject go, int layerNumber)
        {
            if (go == null) return;
            foreach (var trans in go.GetComponentsInChildren<Transform>(true)) trans.gameObject.layer = layerNumber;
        }

        private void MapIK()
        {
            head.MapVRAvatar();
            leftHand.MapVRAvatar();
            rightHand.MapVRAvatar();
        }

        private void Move()
        {
            transform.forward = Vector3.Lerp(transform.forward,
                Vector3.ProjectOnPlane(IKHead.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
            ;

            transform.position = vrRig.gameObject.transform.position - transform.forward * 0.1f;
        }
    }
}