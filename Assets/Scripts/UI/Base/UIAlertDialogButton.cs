using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceArenaParty.UI.Base
{
    public class UIAlertDialogButton : Button
    {
        public Sprite hoverSprite;
        public Color hoverTextColor;
        public Color normalTextColor;
        private Image _image;
        private TMP_Text _text;

        protected override void Start()
        {
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<TMP_Text>();
            InitStyle();
        }

        private void Update()
        {
            if (IsHighlighted() == false) InitStyle();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _image.sprite = hoverSprite;
            _text.color = hoverTextColor;
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            InitStyle();
            base.OnPointerExit(eventData);
        }

        private void InitStyle()
        {
            _image.sprite = null;
            _text.color = normalTextColor;
        }
    }
}