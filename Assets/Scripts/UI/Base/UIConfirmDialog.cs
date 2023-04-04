using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI.Base
{
    public class UIConfirmDialog : MonoBehaviour
    {
        public Button ConfirmButton;
        public Button CancelButton;
        public Button CloseButton;
        public Image IconImage;
        public TMP_Text TitleText;
        public TMP_Text MessageText;

        public void Start()
        {
            CloseButton.onClick.AddListener(Hide);
        }

        public void Show(Sprite icon, string title, string message, Action onConfirm, Action onCancel)
        {
            gameObject.SetActive(true);

            ConfirmButton.onClick.RemoveAllListeners();
            CancelButton.onClick.RemoveAllListeners();

            IconImage.sprite = icon;
            TitleText.text = title;
            MessageText.text = message;

            ConfirmButton.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                Hide();
            });
            CancelButton.onClick.AddListener(() =>
            {
                Hide();
                onCancel?.Invoke();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}