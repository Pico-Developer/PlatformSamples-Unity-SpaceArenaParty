using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceArenaParty.Player
{
    public class AvatarAnimationController : MonoBehaviour
    {
        [SerializeField] private InputActionReference move;

        [SerializeField] private Animator animator;

        private void OnEnable()
        {
            move.action.started += AnimateLegs;
            move.action.canceled += StopAnimation;
        }

        private void OnDisable()
        {
            move.action.started -= AnimateLegs;
            move.action.canceled -= StopAnimation;
        }

        private void AnimateLegs(InputAction.CallbackContext obj)
        {
            var isWalkingFoward = move.action.ReadValue<Vector2>().y > 0;

            if (isWalkingFoward)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("animSpeed", 1.0f);
            }
            else
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("animSpeed", -1.0f);
            }
        }

        private void OnFootstep()
        {
        }

        private void StopAnimation(InputAction.CallbackContext obj)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("animSpeed", 0.0f);
        }
    }
}