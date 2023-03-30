using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI.Base
{
    public class UITabHeader : MonoBehaviour
    {
        public GameObject ButtonsContainer;
        public Button ButtonPrefab;
        public Color ActiveButtonTextColor;
        public Color InactiveButtonTextColor;
        private readonly List<Button> _buttons = new();

        private readonly List<ButtonData> _buttonsData = new()
        {
            new ButtonData(UITabManager.Tabs.World, "World"),
            new ButtonData(UITabManager.Tabs.Room, "Room"),
            new ButtonData(UITabManager.Tabs.Social, "Friends")
        };

        private int _activeButtonIndex = -1;
        private UITabManager _tabManager;

        private void Start()
        {
            _tabManager = FindObjectOfType<UITabManager>();
            UpdateButtons();

            SetActiveButton(0);
        }

        private void OnTabButtonClick(int index)
        {
            SetActiveButton(index);
            _tabManager.OnTabSelect(_buttonsData[index].tab);
        }

        private void SetActiveButton(int index)
        {
            if (_activeButtonIndex >= 0)
            {
                var lastButton = _buttons[_activeButtonIndex];
                SetButtonInactive(lastButton);
            }

            _activeButtonIndex = index;
            SetButtonActive(_buttons[index]);
        }

        private void SetButtonActive(Button button)
        {
            button.gameObject.transform.GetChild(1).gameObject.SetActive(true);
            button.gameObject.GetComponentInChildren<TMP_Text>().color = new Color(ActiveButtonTextColor.r,
                ActiveButtonTextColor.g, ActiveButtonTextColor.b, 1);
        }

        private void SetButtonInactive(Button button)
        {
            button.gameObject.transform.GetChild(1).gameObject.SetActive(false);
            button.gameObject.GetComponentInChildren<TMP_Text>().color = new Color(InactiveButtonTextColor.r,
                InactiveButtonTextColor.g, InactiveButtonTextColor.b, 1);
        }

        private void UpdateButtons()
        {
            for (var i = 0; i < ButtonsContainer.transform.childCount; ++i)
                Destroy(ButtonsContainer.transform.GetChild(i).gameObject);
            _buttons.Clear();
            for (var i = 0; i < _buttonsData.Count; ++i)
            {
                var data = _buttonsData[i];
                var button = Instantiate(ButtonPrefab, ButtonsContainer.transform);
                button.gameObject.GetComponentInChildren<TMP_Text>().SetText(data.text);
                SetButtonInactive(button);
                _buttons.Add(button);
                var i1 = i;
                button.onClick.AddListener(() => OnTabButtonClick(i1));
            }
        }

        private struct ButtonData
        {
            public readonly UITabManager.Tabs tab;
            public readonly string text;

            public ButtonData(UITabManager.Tabs tab, string text = "")
            {
                this.tab = tab;
                this.text = text;
            }
        }
    }
}