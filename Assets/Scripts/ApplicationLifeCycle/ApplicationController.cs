using System;
using System.Collections;
using System.IO;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.ConnectionManagement;
using SpaceArenaParty.Player;
using SpaceArenaParty.UI;
using SpaceArenaParty.Utils;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
#if UNITY_EDITOR
using ParrelSync;
#endif

namespace SpaceArenaParty.ApplicationLifeCycle
{
    public class ApplicationController : MonoBehaviour
    {
        public ApplicationSceneManager applicationSceneManager;

        public Spawner spawner;

        public bool debugSkipSceneLoad;
        public bool debugSkipSpawn;

        private GroupPresenceState _groupPresenceState;

        private UILaunchPadManager _launchPadManager;
        private LaunchType _launchType;

        private MultiPlayerManager _networkLayer;
        private Session _session;

        public Action SwitchRoomCallback;

        private LocalPlayerState LocalPlayerState => LocalPlayerState.Instance;

        private void Start()
        {
            _launchPadManager = FindObjectOfType<UILaunchPadManager>();
            _networkLayer = FindObjectOfType<MultiPlayerManager>();
            _networkLayer.OnStatusChange += OnMultiplayerStatusChange;
            StartCoroutine(Init());
        }

        private void OnEnable()
        {
            DontDestroyOnLoad(this);
        }

        private void OnMultiplayerStatusChange(MultiPlayerManager.GameState lastState,
            MultiPlayerManager.GameState state, string desc, string openId)
        {
            // Debug.Log($"OnMultiplayerStatusChange {lastState} {state} {desc} {openId}");
            switch (state)
            {
                case MultiPlayerManager.GameState.InRoom:
                {
                    NetworkManager.Singleton.OnServerStarted += OnHostStarted;
                    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                    break;
                }
                case MultiPlayerManager.GameState.RoomLeaving:
                {
                    NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                    break;
                }
                case MultiPlayerManager.GameState.Error:
                {
                    if (NetworkManager.Singleton == null) break;
                    NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                    if (lastState == MultiPlayerManager.GameState.InRoom) StartCoroutine(LaunchToPrivateLobby());
                    break;
                }
            }
        }

        private void OnHostStarted()
        {
            if (debugSkipSpawn) return;
            Debug.Log($"On Host Started spawner {spawner}");
            _session = spawner.SpawnSession().GetComponent<Session>();
            var xrOrigin = FindObjectOfType<XROrigin>();
            var spawnPoint = SpawnPoint.singleton.GetSpawnPoint();
            xrOrigin.gameObject.transform.position = new Vector3(spawnPoint.x, 0f, spawnPoint.z);

            spawner.SpawnPlayer(
                NetworkManager.Singleton.LocalClientId,
                spawnPoint,
                SpawnPoint.singleton.SpawnRotation
            );
        }

        private void OnClientConnected(ulong clientId)
        {
            if (debugSkipSpawn) return;
            if (NetworkManager.Singleton.IsHost) return;
            if (NetworkManager.Singleton.IsClient && clientId == NetworkManager.Singleton.LocalClientId)
            {
                _session = FindObjectOfType<Session>();
                var spawnPoint = SpawnPoint.singleton.GetSpawnPoint();
                var xrOrigin = FindObjectOfType<XROrigin>();
                xrOrigin.gameObject.transform.position = new Vector3(spawnPoint.x, 0f, spawnPoint.z);
                _session.RequestSpawnServerRpc(
                    clientId,
                    spawnPoint,
                    SpawnPoint.singleton.SpawnRotation
                );
            }
        }


        private IEnumerator Init()
        {
#if UNITY_EDITOR
            PCDebugAsyncInitialize().OnComplete(OnPlatformInitialized);
#else
        CoreService.AsyncInitialize().OnComplete(OnPlatformInitialized);
#endif
            yield return new WaitUntil(() => LocalPlayerState.username != "");

            Debug.Log($"launchType {_launchType} username {LocalPlayerState.username}");
            yield return new WaitUntil(() => _networkLayer.CurState >= MultiPlayerManager.GameState.Idle);

            _launchPadManager.Init();

            if (_launchType == LaunchType.Normal)
            {
                StartCoroutine(LaunchToPrivateLobby());
            }
            else if (_launchType == LaunchType.Deeplink)
            {
                var launchDetails = ApplicationService.GetLaunchDetails();
                SetGroupPresenceState(
                    launchDetails.DestinationApiName,
                    LocalPlayerState.lobbyID,
                    launchDetails.MatchSessionID
                );
                JoinToRoom(launchDetails.DestinationApiName, ulong.Parse(launchDetails.MatchSessionID));
            }
        }

        private IEnumerator LaunchToPrivateLobby()
        {
            Debug.Log("LaunchToPrivateLobby");

            var targetScene = ApplicationSceneManager.Scenes.Lobby;
            SwitchRoom(targetScene);
            yield return new WaitUntil(() => applicationSceneManager.sceneLoaded);

            yield return new WaitUntil(() => _networkLayer.CurState >= MultiPlayerManager.GameState.Idle);

            CreateRoomAndJoin2(targetScene, RoomJoinPolicy.Everyone);

            yield return new WaitUntil(() => _networkLayer.CurState >= MultiPlayerManager.GameState.InRoom);
        }

        public void SetGroupPresenceState(string dest, string lobbyId, string matchId = "", string extra = "",
            bool joinable = true)
        {
            _groupPresenceState = new GroupPresenceState();
            StartCoroutine(_groupPresenceState.Set(
                    dest,
                    lobbyId,
                    matchId,
                    extra,
                    joinable
                )
            );
        }

        public void RequestQuitGame()
        {
            PresenceService.Clear().OnComplete(message => { Application.Quit(); });
        }


        private void HandleJoinIntentReceived(Message<PresenceJoinIntent> message)
        {
            var details = message.Data;
            SetGroupPresenceState(
                details.DestinationApiName,
                LocalPlayerState.lobbyID,
                details.MatchSessionId
            );
            JoinToRoom(details.DestinationApiName, ulong.Parse(details.MatchSessionId));
        }

        private async void OnPlatformInitialized(Message<PlatformInitializeResult> message)
        {
            Debug.Log("OnPlatformInitialized");
            if (message.IsError)
            {
                LogError("Failed to initialize PICO Platform SDK", message.GetError());
                return;
            }


#if UNITY_EDITOR
            var userAccessTokenResult = await UserService.GetAccessToken().Async();
            var accessToken = userAccessTokenResult.Data;
            if (userAccessTokenResult.IsError)
            {
                LogError("Failed to get user access token", userAccessTokenResult.GetError());
                Application.Quit();
                return;
            }
#else
        Debug.Log("RequestUserPermissions");
        string[] permissions = { "user_info", "friend_relation" };
        var permissionResult = await UserService.RequestUserPermissions(permissions).Async();
        var accessToken = permissionResult.Data.AccessToken;
        Debug.Log("request user permissions result");
        Debug.Log(string.Join(",", permissionResult.Data.AuthorizedPermissions));
        if (permissionResult.IsError)
        {
            LogError("Failed to request user permissions", permissionResult.GetError());
            Application.Quit();
            return;
        }
#endif

            Debug.Log("PICO Platform SDK initialized successfully");
#if UNITY_EDITOR
            _launchType = LaunchType.Normal;
#else
        _launchType = ApplicationService.GetLaunchDetails().LaunchType;
#endif

            Debug.Log($"Get Launch Type successfully {_launchType}");

            PresenceService.SetJoinIntentReceivedNotificationCallback(HandleJoinIntentReceived);
            _networkLayer.Init(accessToken);

            Debug.Log($"init using access token {accessToken}");

            var userReqMessage = await UserService.GetLoggedInUser().Async();
            if (userReqMessage.IsError)
            {
                LogError("Failed to get user info", userReqMessage.GetError());
                Application.Quit();
                return;
            }

            OnLoggedInUser(userReqMessage);
        }

        private void OnLoggedInUser(Message<User> message)
        {
            Debug.Log("OnLoggedInUser");

            if (message.IsError)
            {
                LogError("Cannot get user info", message.GetError());
                return;
            }

            LocalPlayerState.Init(message);
        }

        public void CreateRoomAndJoin2(ApplicationSceneManager.Scenes scene, RoomJoinPolicy joinPolicy)
        {
            var title = Utils.Utils.GenerateRoomTitle(LocalPlayerState.picoUser, scene.ToString());
            CreateRoomAndJoin2(scene, title, joinPolicy);
        }

        public void CreateRoomAndJoin2(ApplicationSceneManager.Scenes scene, string title, RoomJoinPolicy joinPolicy)
        {
            var roomOptions = new RoomOptions();
            roomOptions.SetDataStore("destination", scene.ToString());
            roomOptions.SetDataStore("title", title);
            _networkLayer.CreatePrivateRoom(joinPolicy,
                roomOptions,
                delegate(Room room)
                {
                    SetGroupPresenceState(scene.ToString(), LocalPlayerState.lobbyID, room.RoomId.ToString());
                });
            SwitchRoom(scene);
        }

        public void JoinToRoom(Room room)
        {
            var roomId = room.RoomId;
            var destination = room.DataStore["destination"];
            JoinToRoom(destination, roomId);
        }

        public void JoinToRoom(string destination, ulong roomId)
        {
            JoinToRoom(Enum.Parse<ApplicationSceneManager.Scenes>(destination), roomId);
        }

        public async void JoinToRoom(ApplicationSceneManager.Scenes scene, ulong roomId)
        {
            try
            {
                await _networkLayer.JoinRoomByRoomID(roomId);

                SetGroupPresenceState(scene.ToString(), LocalPlayerState.lobbyID, roomId.ToString());
                SwitchRoom(scene);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void ReturnToLobby()
        {
            CreateRoomAndJoin2(ApplicationSceneManager.Scenes.Lobby, RoomJoinPolicy.Everyone);
        }

        private void SwitchRoom(ApplicationSceneManager.Scenes scene)
        {
            if (!debugSkipSceneLoad) applicationSceneManager.LoadScene(scene);
            SwitchRoomCallback?.Invoke();
        }

        private void LogError(string message, Error error)
        {
            Debug.LogError(message);
            Debug.LogError("ERROR MESSAGE:   " + error.Message);
            Debug.LogError("ERROR CODE:      " + error.Code);
        }

#if UNITY_EDITOR
        public static Task<PlatformInitializeResult> PCDebugAsyncInitialize(string appId = null)
        {
            appId = CoreService.GetAppID(appId);
            if (string.IsNullOrWhiteSpace(appId)) throw new UnityException("AppID cannot be null or empty");

            Task<PlatformInitializeResult> task;
            if (Application.platform == RuntimePlatform.Android)
            {
                var requestId = CLIB.ppf_UnityInitAsynchronousWrapper(appId);
                if (requestId == 0)
                    throw new Exception("PICO PlatformSDK failed to initialize");
                task = new Task<PlatformInitializeResult>(requestId);
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                     Application.platform == RuntimePlatform.WindowsEditor)
            {
                var config = Resources.Load<TextAsset>("PicoSdkPCConfig");
                var logDirectory = Path.GetFullPath("Logs");

                // == clone behavior start ==
                if (ClonesManager.IsClone())
                {
                    config = Resources.Load<TextAsset>("PicoSdkPCConfig_clone");
                    Debug.Log("is clone");
                }
                else
                {
                    config = Resources.Load<TextAsset>("PicoSdkPCConfig");
                    Debug.Log("is original");
                }
                // == clone behavior end ==

                if (config == null) throw new UnityException("cannot find PC config file Resources/PicoSdkPCConfig");

                if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

                var requestId = CLIB.ppf_PcInitAsynchronousWrapper(appId, config.text, logDirectory);
                if (requestId == 0)
                    throw new Exception("PICO PlatformSDK failed to initialize");
                task = new Task<PlatformInitializeResult>(requestId);
            }
            else
            {
                throw new NotImplementedException("PICO platform is not implemented on this platform yet.");
            }

            CoreService.Initialized = true;
            Runner.RegisterGameObject();
            return task;
        }
#endif
    }
}