using System;
using Pico.Platform.Models;
using SpaceArenaParty.ApplicationLifeCycle;
using SpaceArenaParty.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIRoomManagerPresenter : MonoBehaviour
    {
        public Button InviteButton;
        public Button QuitButton;

        private DateTime _lastUpdate;
        private UILaunchPadManager _launchPadManager;
        private UIRoomInfoPresenter _roomInfoPresenter;
        private UIRoomMemberEntriesPresenter _userEntriesPresenter;
        private bool _userRelationOutdated;

        private void Start()
        {
            _launchPadManager = FindObjectOfType<UILaunchPadManager>();

            _roomInfoPresenter = GetComponentInChildren<UIRoomInfoPresenter>();
            _userEntriesPresenter = GetComponentInChildren<UIRoomMemberEntriesPresenter>();

            InviteButton.onClick.AddListener(() => { _launchPadManager.LaunchInvitePanel(); });
            QuitButton.onClick.AddListener(() => { _launchPadManager.ReturnToLobby(); });
        }

        public void SetRoom(Room room)
        {
            _roomInfoPresenter.Init(room);

            if (room.OwnerOptional != null &&
                ApplicationSceneManager.currentScene == ApplicationSceneManager.Scenes.Lobby &&
                room.OwnerOptional.ID == LocalPlayerState.Instance.picoUser.ID)
                QuitButton.gameObject.SetActive(false);
            else
                QuitButton.gameObject.SetActive(true);
            if (room.UsersOptional != null)
            {
                var entries = room.UsersOptional;
                // Debug.Log($"set room member entries {entries.Count}");
                _userEntriesPresenter.SetEntries(entries);
            }
        }
    }
}