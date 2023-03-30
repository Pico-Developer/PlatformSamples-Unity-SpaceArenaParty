using System;
using System.Collections.Generic;
using System.Linq;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.ApplicationLifeCycle;
using SpaceArenaParty.UI.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UICreateRoomPanel : MonoBehaviour
    {
        public UIRadioManager sceneRadio;

        // public UIRadioManager visibilityRadio;
        public TMP_Text roomNameText;
        public Button submitButton;
        public Button cancelButton;
        public Button closeButton;

        private readonly Dictionary<string, ApplicationSceneManager.Scenes> _sceneMap = new()
        {
            { "Blue Room", ApplicationSceneManager.Scenes.BlueRoom },
            { "Orange Room", ApplicationSceneManager.Scenes.RedRoom }
        };

        private readonly Dictionary<string, UILaunchPadManager.RoomVisibility> _visibilityMap = new()
        {
            { "Public", UILaunchPadManager.RoomVisibility.Everyone },
            { "Private", UILaunchPadManager.RoomVisibility.Friends }
        };

        private Room _currentRoom;

        private UILaunchPadManager _launchPadManager;
        public Action OnComplete;

        private void Start()
        {
            _launchPadManager = FindObjectOfType<UILaunchPadManager>();
            sceneRadio.Init(_sceneMap.Keys.ToArray());
            // visibilityRadio.Init(_visibilityMap.Keys.ToArray());

            sceneRadio.OnValueChanged += OnSceneRadioValueChanged;
        }

        private void OnSceneRadioValueChanged(string value)
        {
            UpdateRoomTitle(Utils.Utils.GenerateRoomTitle(value));
        }

        private void UpdateRoomTitle(string value)
        {
            roomNameText.text = value;
        }

        public void Init()
        {
            EnableButtons();

            sceneRadio.gameObject.SetActive(true);

            submitButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();

            UpdateRoomTitle(Utils.Utils.GenerateRoomTitle(sceneRadio.currentValue));
            submitButton.gameObject.GetComponentInChildren<TMP_Text>().SetText("Create Room");
            submitButton.onClick.AddListener(OnCreateRoomButtonClick);

            cancelButton.onClick.AddListener(OnCancelButtonClick);
            closeButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void DisableButtons()
        {
            cancelButton.interactable = false;
            submitButton.interactable = false;
            closeButton.interactable = false;
        }

        private void EnableButtons()
        {
            cancelButton.interactable = true;
            submitButton.interactable = true;
            closeButton.interactable = true;
        }

        public void Init(Room room)
        {
            EnableButtons();
            _currentRoom = room;
            submitButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveAllListeners();

            sceneRadio.gameObject.SetActive(false);
            submitButton.gameObject.GetComponentInChildren<TMP_Text>().SetText("Update");
            submitButton.onClick.AddListener(OnUpdateRoomOptions);

            var title = Utils.Utils.GetRoomTitle(room);
            UpdateRoomTitle(title);

            // visibilityRadio.currentValue =
            //     _visibilityMap.FirstOrDefault(x => x.Value == UILaunchPadManager.RoomVisibility.Everyone).Key;

            cancelButton.onClick.AddListener(OnCancelButtonClick);
            closeButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void OnCancelButtonClick()
        {
            _launchPadManager.ToggleCreateRoomPanelActive(false);
        }

        private async void OnCreateRoomButtonClick()
        {
            var scene = sceneRadio.currentValue;
            // var visibility = visibilityRadio.currentValue;
            DisableButtons();
            submitButton.GetComponentInChildren<TMP_Text>().SetText("loading");
            await _launchPadManager.CreateRoomAndJoin2(UILaunchPadManager.RoomVisibility.Everyone,
                _sceneMap[scene]);
            OnComplete?.Invoke();
        }

        private async void OnUpdateRoomOptions()
        {
            // var visibility = visibilityRadio.currentValue;
            submitButton.GetComponentInChildren<TMP_Text>().SetText("loading");
            DisableButtons();
            await RoomService.UpdatePrivateRoomJoinPolicy(_currentRoom.RoomId,
                RoomJoinPolicy.Everyone).Async();
            gameObject.SetActive(false);
            OnComplete?.Invoke();
        }
    }
}