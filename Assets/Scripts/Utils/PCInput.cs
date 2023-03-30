using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceArenaParty.Utils
{
    public class PCInput : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _move;
        [SerializeField] private InputActionProperty _look;

        [SerializeField] private Transform _rootTransform;
        [SerializeField] private Transform _cameraTransform;

        [SerializeField] private Vector2 _lookSpeed = new(0.08f, -0.08f);
        [SerializeField] private Vector2 _moveSpeed = new(3f, 3f);
        [SerializeField] private float _acceleration = 4f;

        private Vector2 _currentMoveDelta;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Application.isFocused == false) return;

            var moveDelta = _move.action.ReadValue<Vector2>();

            _currentMoveDelta = Vector2.MoveTowards(_currentMoveDelta, moveDelta, Time.deltaTime * _acceleration);

            _rootTransform.Translate(_currentMoveDelta.x * _moveSpeed.x * Time.deltaTime, 0f,
                _currentMoveDelta.y * _moveSpeed.y * Time.deltaTime, Space.Self);

            var lookDelta = _look.action.ReadValue<Vector2>();
            _rootTransform.Rotate(0f, lookDelta.x * _lookSpeed.x, 0f, Space.Self);
            _cameraTransform.Rotate(lookDelta.y * _lookSpeed.y, 0f, 0f, Space.Self);
        }

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}