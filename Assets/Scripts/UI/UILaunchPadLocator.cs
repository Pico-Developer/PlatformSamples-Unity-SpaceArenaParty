using System;
using SpaceArenaParty.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UILaunchPadLocator : MonoBehaviour
    {
        public GameObject towardObject;
        public bool defaultOpen;
        private readonly bool _updateSpatialPosition = true;

        private Canvas _canvas;
        private RectTransform _canvasRectTransform;
        private CanvasScaler _canvasScaler;
        private Vector2 _canvasTransformPivot;

        private Vector3 _forward;

        private Vector3 _originalLocalScale;
        private bool _show;
        private Vector3 _targetForward;

        private void Start()
        {
            _canvas = GetComponentInChildren<Canvas>();
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();
            _show = defaultOpen;
            _originalLocalScale = transform.localScale + Vector3.zero;
            if (_show == false) transform.localScale = Vector3.zero;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void Update()
        {
            if (_show && _updateSpatialPosition && towardObject != null)
            {
                var targetPositionAndRotation = GetTargetPositionAndRotation();

                transform.position = targetPositionAndRotation.position;
                transform.rotation = targetPositionAndRotation.rotation;

                var angle = Vector3.Angle(_forward, towardObject.transform.forward);
                if (angle > 30f) ResetTargetForward();

                AnimateToNewForward();
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            towardObject = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var vrRig = FindObjectOfType<VRRig>();
            towardObject = vrRig.Head.gameObject;
            // if (scene.name == "LaunchPad")
            // {
            //     _show = true;
            //     transform.localScale = _originalLocalScale;
            // }
            // else
            // {
            //     _show = false;
            //     transform.localScale = Vector3.zero;
            // }
        }

        private PositionAndRotation GetTargetPositionAndRotation()
        {
            if (towardObject == null) throw new Exception();
            var towardObjectPosition = towardObject.transform.position;

            var direction = _forward;
            var xzDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            var newPosition = towardObjectPosition + xzDirection;

            var relativePos = newPosition - towardObjectPosition;
            relativePos.y = 0f;
            var newRotation = Quaternion.LookRotation(relativePos, Vector3.up);

            return new PositionAndRotation
            {
                position = newPosition,
                rotation = newRotation
            };
        }

        private void AnimateToNewForward()
        {
            _forward = Vector3.Lerp(_forward, _targetForward, 5f * Time.deltaTime);
            _canvasRectTransform.pivot =
                Vector2.Lerp(_canvasRectTransform.pivot, _canvasTransformPivot, 20f * Time.deltaTime);
        }


        public void SetCanvasTransformPivot(Vector2 pivot)
        {
            _canvasTransformPivot = pivot;
        }

        public void OnToggleLaunchPad()
        {
            OnToggleLaunchPad(!_show);
        }

        private void ResetTargetForward()
        {
            _targetForward = towardObject.transform.forward;
        }

        public void OnToggleLaunchPad(bool show)
        {
            _show = show;
            if (_show && towardObject != null)
            {
                ResetTargetForward();
                _forward = _targetForward;
                var targetPositionAndRotation = GetTargetPositionAndRotation();
                transform.position = targetPositionAndRotation.position;
                transform.rotation = targetPositionAndRotation.rotation;
                transform.localScale = _originalLocalScale + Vector3.zero;
            }
            else
            {
                transform.localScale = Vector3.zero;
            }
        }

        private struct PositionAndRotation
        {
            public Vector3 position;
            public Quaternion rotation;
        }
    }
}