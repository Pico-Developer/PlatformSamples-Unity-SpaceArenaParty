using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace SpaceArenaParty.Utils
{
    public class AutoHideRaycaster : MonoBehaviour
    {
        private ActionBasedController controller;
        private XRInteractorLineVisual raycaster;

        private void OnEnable()
        {
            controller = GetComponent<ActionBasedController>();
            raycaster = GetComponent<XRInteractorLineVisual>();
            controller.activateAction.action.performed += ShowRaycaster;
            controller.activateAction.action.canceled += HideRaycaster;
        }

        private void OnDisable()
        {
            controller.activateAction.action.performed -= ShowRaycaster;
            controller.activateAction.action.canceled -= HideRaycaster;
        }

        private static Gradient GetValidColorGradient()
        {
            Gradient gradient = new();

            var colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.white, 0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 1f, 1f, 0.5f), 1f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(0.5f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0.5f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        private static Gradient GetInvalidColorGradient()
        {
            Gradient gradient = new();

            var colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.white, 0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 1f, 1f, 0.5f), 1f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(0f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);

            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        private void ShowRaycaster(InputAction.CallbackContext context)
        {
            raycaster.validColorGradient = GetValidColorGradient();
        }

        private void HideRaycaster(InputAction.CallbackContext context)
        {
            raycaster.validColorGradient = GetInvalidColorGradient();
        }
    }
}