using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.ApplicationLifeCycle;
using SpaceArenaParty.ConnectionManagement;
using SpaceArenaParty.UI.Base;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace SpaceArenaParty.UI
{
    public class UILaunchPadManager : MonoBehaviour
    {
        public enum RoomVisibility
        {
            Everyone,
            Friends
        }

        public UICreateRoomPanel CreateOrUpdateRoomPanel;

        public UIConfirmDialog ConfirmDialog;

        private readonly List<User> _friends;
        private readonly List<UserRoom> _friendsRooms;
        private ApplicationController _applicationController;
        private bool _initialized;
        private bool _isOutDated;
        private DateTime _lastUpdate;
        private UILaunchPadLocator _locator;
        private UILogPanelManager _logPanelManager;
        private MultiPlayerManager _multiplayerManager;
        private bool _showLogPanel = true;
        private UIFriendEntriesPresenter _uiFriendEntriesPresenter;
        private UIRoomManagerPresenter _uiRoomManagerPresenter;
        private UIRoomEntriesPresenter _uiRoomsPresenter;

        private UILaunchPadManager()
        {
            _friendsRooms = new List<UserRoom>();
            _friends = new List<User>();
        }

        public Room CurrentRoom { get; private set; }

        private void Update()
        {
            if (_initialized == false)
                return;
            var now = DateTime.Now;
            if (now - _lastUpdate > UIConfig.DataFetchInterval) _isOutDated = true;

            if (_isOutDated)
            {
                _isOutDated = false;
                StartFetchDataCoroutines();
            }
        }

        private void OnEnable()
        {
            _locator = GetComponent<UILaunchPadLocator>();
            _logPanelManager = FindObjectOfType<UILogPanelManager>();

            UpdateLogPanel();

            _multiplayerManager = FindObjectOfType<MultiPlayerManager>();
            _multiplayerManager.OnRoomUpdate += OnRoomUpdate;

            _uiRoomsPresenter = FindObjectOfType<UIRoomEntriesPresenter>();
            _uiRoomManagerPresenter = FindObjectOfType<UIRoomManagerPresenter>();
            _uiFriendEntriesPresenter = FindObjectOfType<UIFriendEntriesPresenter>();

            _applicationController = FindObjectOfType<ApplicationController>();
            _applicationController.SwitchRoomCallback += OnRoomSwitched;

            DontDestroyOnLoad(this);
            ToggleCreateRoomPanelActive(false);
        }

        public void Init()
        {
            _initialized = true;
        }


        private void UpdateLogPanel()
        {
            _locator.SetCanvasTransformPivot(_showLogPanel ? new Vector2(0.8f, 0.5f) : new Vector2(0.5f, 0.5f));
            _logPanelManager.gameObject.transform.localScale = _showLogPanel ? Vector3.one : Vector3.zero;
        }

        public void ToggleLogPanel()
        {
            _showLogPanel = !_showLogPanel;
            UpdateLogPanel();
        }

        private void StartFetchDataCoroutines()
        {
            _lastUpdate = DateTime.Now;
            UpdateFriends();
            UpdateFriendRooms();
        }

        private async void UpdateFriendRooms()
        {
            var hasNextPage = true;
            UserRoomList userRoomList = null;
            _friendsRooms.RemoveAll(item => true);

            while (hasNextPage)
            {
                if (userRoomList == null)
                {
                    var result = await UserService.GetFriendsAndRooms().Async();
                    userRoomList = result.Data;
                }
                else
                {
                    var result = await UserService.GetNextUserAndRoomListPage(userRoomList).Async();
                    userRoomList = result.Data;
                }

                foreach (var room in userRoomList) _friendsRooms.Add(room);

                hasNextPage = userRoomList.HasNextPage;
            }

            var notNullFriendRooms = _friendsRooms.FindAll(userRoom => userRoom.Room != null);

            _uiRoomsPresenter.SetEntries(notNullFriendRooms);
        }

        private async void UpdateFriends()
        {
            var hasNextPage = true;
            _friends.RemoveAll(item => true);
            while (hasNextPage)
            {
                var message = await UserService.GetFriends().Async();
                if (message.IsError == false)
                {
                    foreach (var friend in message.Data) _friends.Add(friend);

                    hasNextPage = message.Data.HasNextPage;
                }
                else
                {
                    throw new Exception(message.Error.Message);
                }
            }

            // Debug.Log($"_friends {_friends.Count}");
            _uiFriendEntriesPresenter.SetEntries(_friends);
        }

        public void OnCreateRoomClick()
        {
            ToggleCreateRoomPanelActive(true);
        }

        private async void UpdateCurrentRoom()
        {
            CreateOrUpdateRoomPanel.OnComplete -= UpdateCurrentRoom;
            var message = await RoomService.Get(CurrentRoom.RoomId).Async();
            if (message.IsError == false && message.Data.RoomId == CurrentRoom.RoomId)
            {
                CurrentRoom = message.Data;
                _uiRoomManagerPresenter.SetRoom(CurrentRoom);
            }
            else
            {
                throw new Exception(message.Error.Message);
            }
        }

        public void OnUpdateRoomSettingClick(Room room)
        {
            CreateOrUpdateRoomPanel.Init(room);
            CreateOrUpdateRoomPanel.OnComplete += UpdateCurrentRoom;
            CreateOrUpdateRoomPanel.gameObject.SetActive(true);
        }

        public void ToggleCreateRoomPanelActive(bool visible)
        {
            if (visible) CreateOrUpdateRoomPanel.Init();
            CreateOrUpdateRoomPanel.gameObject.SetActive(visible);
        }

        public void LaunchInvitePanel()
        {
            PresenceService.LaunchInvitePanel().OnComplete(message => { Debug.Log("LaunchInvitePanel complete"); });
        }

        public void ReturnToLobby()
        {
            Sprite sprite = null;
            sprite = Resources.Load<Sprite>("UI/Icons/Home Icon");
            ConfirmDialog.Show(sprite, "Leave Room", "Are you sure you want to leave the room and return to the lobby?",
                () => { _applicationController.ReturnToLobby(); },
                () => { });
        }

        public Task CreateRoomAndJoin2(RoomVisibility visibility, ApplicationSceneManager.Scenes scene)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            RoomJoinPolicy roomJoinPolicy;
            if (visibility == RoomVisibility.Everyone)
                roomJoinPolicy = RoomJoinPolicy.Everyone;
            else
                roomJoinPolicy = RoomJoinPolicy.FriendsOfMembers;
            _applicationController.CreateRoomAndJoin2(scene, roomJoinPolicy);
            return taskCompletionSource.Task;
        }

        public void OnJoin2RoomClick(Room room)
        {
            _applicationController.JoinToRoom(room);
        }

        public void Join2RoomByUserPresence(User user)
        {
            _applicationController.JoinToRoom(user.PresenceDestinationApiName,
                ulong.Parse(user.PresenceMatchSessionId));
        }

        public void OnExitGameClick()
        {
            Sprite sprite = null;
            sprite = Resources.Load<Sprite>("UI/Icons/Game Icon");
            ConfirmDialog.Show(sprite, "Exit Game", "Are you sure you want to exit?",
                () => { _applicationController.RequestQuitGame(); },
                () => { });
        }


        public void OnInviteButtonClick()
        {
            PresenceService.LaunchInvitePanel();
        }

        public Task SendFriendRequest(User user)
        {
            return UserService.LaunchFriendRequestFlow(user.ID).Async();
        }


        private void OnRoomSwitched()
        {
            _locator.OnToggleLaunchPad(false);
            ToggleCreateRoomPanelActive(false);
        }

        private void OnRoomUpdate(Room room)
        {
            CurrentRoom = room;
            if (room != null)
            {
                Debug.Log($"on room update{room.Description}");
                _uiRoomManagerPresenter.SetRoom(room);
            }
        }
    }
}