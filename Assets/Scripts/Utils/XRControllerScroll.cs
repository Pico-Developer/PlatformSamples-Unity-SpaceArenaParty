using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpaceArenaParty.Utils
{
    public class XRControllerScroll : Selectable
    {
        private bool _isPointerInside;
        private InputActionReference _scrollActionReference;
        private ScrollRect _scrollRect;
        private XRUIInputModule _xrUiInputModule;

        private void Start()
        {
            _xrUiInputModule = FindObjectOfType<XRUIInputModule>();
            _scrollRect = GetComponent<ScrollRect>();
            _scrollActionReference = _xrUiInputModule.scrollWheelAction;
        }

        private void Update()
        {
            if (!_isPointerInside) return;
            var scrollValue = _scrollActionReference.action.ReadValue<Vector2>();
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.scrollDelta = scrollValue;
            _scrollRect.OnScroll(pointerEventData);
        }


        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            base.OnPointerExit(eventData);
        }
    }
}