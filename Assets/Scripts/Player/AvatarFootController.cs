using UnityEngine;

namespace SpaceArenaParty.Player
{
    public class AvatarFootController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [SerializeField] [Range(0, 1)] private float leftFootPosWeight;
        [SerializeField] [Range(0, 1)] private float rightFootPosWeight;

        [SerializeField] [Range(0, 1)] private float leftFootRotWeight;
        [SerializeField] [Range(0, 1)] private float rightFootRotWeight;

        [SerializeField] private Vector3 footOffset;
        [SerializeField] private Vector3 raycastOffsetLeft;
        [SerializeField] private Vector3 raycastOffsRight;

        private void OnAnimatorIK(int layerIndex)
        {
            var leftFootPos = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            var rightFootPos = animator.GetIKPosition(AvatarIKGoal.RightFoot);

            RaycastHit hitLeftFoot;
            RaycastHit hitRightFoot;

            var isLeftFootDown = Physics.Raycast(leftFootPos + raycastOffsetLeft, Vector3.down, out hitLeftFoot);
            var isRightFootDown = Physics.Raycast(rightFootPos + raycastOffsRight, Vector3.down, out hitRightFoot);

            if (isLeftFootDown)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPosWeight);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitLeftFoot.point + footOffset);

                var leftFootRotation =
                    Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitLeftFoot.normal),
                        hitLeftFoot.normal);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotWeight);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            }

            if (isRightFootDown)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootPosWeight);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, hitRightFoot.point + footOffset);

                var rightFootRotation =
                    Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hitRightFoot.normal),
                        hitRightFoot.normal);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootRotWeight);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            }
        }
    }
}