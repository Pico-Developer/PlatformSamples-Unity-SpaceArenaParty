using SpaceArenaParty.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceArenaParty.Player
{
    public class CustomCharacterController : MonoBehaviour
    {
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [SerializeField] private float turnSmoothness;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        public LocomotionProvider locomotionProvider;

        private readonly float _terminalVelocity = 53.0f;

        private CharacterController _characterController;

        private float _fallTimeoutDelta;
        private InputActions _input;
        private bool _isLocomotionMoving;

        private float _jumpTimeoutDelta;

        private float _verticalVelocity;

        private VRRig vrRig;


        private void Start()
        {
            locomotionProvider.beginLocomotion += StartLocomotion;
            locomotionProvider.endLocomotion += StopLocomotion;

            vrRig = FindObjectOfType<VRRig>();
            if (vrRig == null) return;
            _characterController = vrRig.GetComponent<CharacterController>();
            _input = GetComponent<InputActions>();
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
        }

        private void LateUpdate()
        {
            if (_isLocomotionMoving == false) Move();
        }

        private void StartLocomotion(LocomotionSystem system)
        {
            _isLocomotionMoving = true;
        }

        private void StopLocomotion(LocomotionSystem system)
        {
            _isLocomotionMoving = false;
        }


        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the FLafall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) _verticalVelocity = -2f;

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) _verticalVelocity += Gravity * Time.deltaTime;
        }

        private void GroundedCheck()
        {
            var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        private void Move()
        {
            var motion = vrRig.Head.localPosition + vrRig.CharacterOffset.localPosition;

            var targetXZPosition = new Vector3(motion.x, 0f, motion.z);
            transform.position += targetXZPosition;
            vrRig.CharacterOffset.localPosition =
                new Vector3(-vrRig.Head.localPosition.x, 0f, -vrRig.Head.localPosition.z);
        }
    }
}