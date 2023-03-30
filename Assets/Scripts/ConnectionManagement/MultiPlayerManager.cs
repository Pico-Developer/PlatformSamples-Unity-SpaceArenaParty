using System;
using System.Collections;
using Netcode.Transports.Pico;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.ConnectionManagement.Utils;
using SpaceArenaParty.Player;
using Unity.Netcode;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace SpaceArenaParty.ConnectionManagement
{
    [RequireComponent(typeof(PicoTransport))]
    [DefaultExecutionOrder(-2000)]
    public partial class MultiPlayerManager : MonoBehaviour
    {
        public enum GameState
        {
            NotInitialised,
            Initializing,
            Idle,
            Error,
            RoomJoining,
            RoomLeaving,
            InRoom
        }

        private bool _autoRestartNetcodeOnHostLeave;

        private Room _roomData;
        private InRoomTransportDriver _transportDriver;

        public GameState CurState { get; private set; }
        public ulong RoomID { get; private set; }

        private Room RoomData
        {
            get => _roomData;
            set
            {
                _roomData = value;
                OnRoomUpdate?.Invoke(value);
            }
        }

        private LocalPlayerState LocalPlayerState => LocalPlayerState.Instance;

        private void Start()
        {
            CurState = GameState.NotInitialised;
            RoomID = 0;
            SetAutoRestartFlag();
            RegisterNotificationCallbacks();
        }

        private void OnDestroy()
        {
            StopPicoGame();
        }

        public event Action<GameState, GameState, string, string> OnStatusChange;
        public event Action<Room> OnRoomUpdate;

        private void CallOnStatusChange(GameState curState, string statusDesc)
        {
            var prevState = CurState;
            CurState = curState;


            OnStatusChange?.Invoke(prevState, CurState, statusDesc, LocalPlayerState.picoUser?.ID);
            if (curState == GameState.Error) ErrorHandler(prevState);
        }

        private void ErrorHandler(GameState prevState)
        {
            Debug.Log($"error handler prevState:{prevState}");
            switch (prevState)
            {
                case GameState.Idle:
                    break;
                case GameState.InRoom:
                    break;
                case GameState.RoomJoining:
                    StopAllCoroutines();
                    break;
                case GameState.RoomLeaving:
                    StopAllCoroutines();
                    break;
            }

            CallOnStatusChange(GameState.Idle, "error restored");
        }

        private IEnumerator InitGameService(string accessToken)
        {
            yield return null;
            Debug.Log($"got accessToken {accessToken}, GameInitialize begin");
            CoreService.GameUninitialize();
            Task<GameInitializeResult> request;
            request = CoreService.GameInitialize(accessToken);

            CallOnStatusChange(CurState, "game init started, requestID:" + request.TaskId);
            if (request.TaskId != 0)
            {
                request.OnComplete(OnGameInitialize);
            }
            else
            {
                Debug.Log("Core.GameInitialize requestID is 0! Repeated initialization or network error");
                CallOnStatusChange(GameState.NotInitialised, "game init start failed!");
            }
        }

        public void SetAutoRestartFlag()
        {
            _autoRestartNetcodeOnHostLeave = true;
        }

        public void StartPicoTransport()
        {
            if (null == _transportDriver)
            {
                _transportDriver = new InRoomTransportDriver();
                _transportDriver.OnClientEvent += HandleClientEvent;
                _transportDriver.OnDriverEvent += HandleDriverEvent;
            }

            var selfOpenID = LocalPlayerState.picoUser.ID;
            var result = _transportDriver.Init(_autoRestartNetcodeOnHostLeave, GetComponent<PicoTransport>(),
                selfOpenID,
                RoomData);
            if (!result) Debug.LogError("init pico driver failed");
        }

        public void StopPicoTransport()
        {
            if (null == _transportDriver) return;
            _transportDriver.Uninit();
            _transportDriver = null;
            NetworkManager.Singleton.Shutdown();
        }

        public void Init(string accessToken)
        {
            if (CurState < GameState.Idle)
            {
                CallOnStatusChange(GameState.Initializing, "pico SDK init started ...");
                StartCoroutine(InitGameService(accessToken));
            }
            else
            {
                CallOnStatusChange(CurState, "StartPicoGame in initialised State, skip request");
            }
        }

        public void CreatePrivateRoom(RoomJoinPolicy joinPolicy, RoomOptions roomOptions, Action<Room> callback = null)
        {
            StartCoroutine(StartCreatePrivateRoom(joinPolicy, roomOptions, callback));
        }

        private IEnumerator StartCreatePrivateRoom(RoomJoinPolicy joinPolicy, RoomOptions roomOptions,
            Action<Room> callback = null)
        {
            if (CurState == GameState.InRoom) StartCoroutine(StartLeaveRoom());

            yield return new WaitUntil(() => CurState == GameState.Idle);

            RoomService.CreateAndJoinPrivate2(joinPolicy, 20, roomOptions).OnComplete(message =>
            {
                if (message.IsError == false) callback?.Invoke(message.Data);
                ProcessCreatePrivate(message);
            });
        }

        public async Task JoinRoomByRoomID(ulong roomID)
        {
            Debug.Log("start join room by room id");
            Room roomToJoin = null;
            Debug.Log("validate room");
            var roomResult = await RoomService.Get(roomID).Async();
            if (roomResult.IsError == false)
            {
                var room = roomResult.Data;
                if (room.RoomJoinability == RoomJoinability.CanJoin) roomToJoin = room;
                Debug.Log($"Room joinability {room.RoomJoinability}");
            }
            else
            {
                Debug.LogError("fail to get room data");
            }

            Debug.Log("room validation done");

            if (roomToJoin == null) throw new Exception("failed to validate room");

            StartCoroutine(StartJoinRoomByRoomID(roomID));
            // return roomToJoin;
        }

        private IEnumerator StartJoinRoomByRoomID(ulong roomID)
        {
            if (CurState == GameState.InRoom) StartCoroutine(StartLeaveRoom());

            yield return new WaitUntil(() => CurState == GameState.Idle);

            RoomID = roomID;
            StartCoroutine(StartJoinRoom());
        }

        private IEnumerator StartJoinRoom()
        {
            if (RoomID == 0)
                if (RoomID == 0)
                {
                    Debug.LogError("no valid room to join");
                    yield break;
                }

            if (CurState < GameState.Idle)
            {
                Debug.LogError($"StartJoinRoom, but game state {CurState} is invalid");
                yield break;
            }

            CallOnStatusChange(GameState.RoomJoining, $"request to join room {RoomID} ...");
            var roomOptions = GameUtils.GetRoomOptions(RoomID.ToString(), null, null, null, null, null);
            RoomService.Join2(RoomID, roomOptions).OnComplete(ProcessRoomJoin2);
        }

        public IEnumerator StartLeaveRoom()
        {
            Debug.Log("StartLeaveRoom");
            if (RoomID == 0)
            {
                Debug.LogError("no valid room to leave");
                CallOnStatusChange(GameState.Idle, "No room to leave");
                yield break;
            }

            if (CurState < GameState.RoomJoining)
            {
                Debug.LogError("not InRoom, skip this request");
                CallOnStatusChange(GameState.Idle, "No room to leave");
                yield break;
            }

            CallOnStatusChange(GameState.RoomLeaving, $"request to Leaving room {RoomID} ...");
            StopPicoTransport();
            RoomService.Leave(RoomID).OnComplete(ProcessRoomLeave);
        }

        public ulong GetRoomID()
        {
            if (CurState != GameState.InRoom) return 0;
            return RoomID;
        }

        private void StopPicoGame()
        {
            CoreService.GameUninitialize();
            CallOnStatusChange(GameState.NotInitialised, "Not Initialised");
        }

        private void OnGameInitialize(Message<GameInitializeResult> msg)
        {
            CallOnStatusChange(CurState, "game init finished");
            if (msg == null)
            {
                Debug.Log("OnGameInitialize: fail, message is null");
                CallOnStatusChange(CurState, "game init finished with null msg");
                return;
            }

            if (msg.IsError)
            {
                Debug.LogError($"GameInitialize Failed: {msg.Error.Code}, {msg.Error.Message}");
                CallOnStatusChange(CurState, "game init finished with error:" + msg.Error.Message);
            }
            else
            {
                Debug.Log($"OnGameInitialize: {msg.Data}");
                if (msg.Data == GameInitializeResult.Success)
                {
                    CallOnStatusChange(GameState.Idle, "game init succeed");
                }
                else
                {
                    CallOnStatusChange(GameState.NotInitialised, "game init failed, data:" + msg.Data);
                    StopPicoGame();
                    Debug.Log("GameInitialize: failed please re-initialize");
                }
            }
        }

        private void OnGameConnectionEvent(Message<GameConnectionEvent> msg)
        {
            var state = msg.Data;
            Debug.Log($"OnGameConnectionEvent: {state}");
            if (state == GameConnectionEvent.Connected)
            {
                Debug.Log("GameConnection: success");
            }
            else if (state == GameConnectionEvent.Closed)
            {
                StopPicoGame();
                Debug.Log("GameConnection: failed Please re-initialize");
            }
            else if (state == GameConnectionEvent.GameLogicError)
            {
                StopPicoGame();
                Debug.Log(
                    "GameConnection: failed After successful reconnection, the logic state is found to be wrong, please re-initialize��");
            }
            else if (state == GameConnectionEvent.Lost)
            {
                Debug.Log("GameConnection: Reconnecting, waiting ...");
            }
            else if (state == GameConnectionEvent.Resumed)
            {
                Debug.Log("GameConnection: successfully reconnected");
            }
            else if (state == GameConnectionEvent.KickedByRelogin)
            {
                StopPicoGame();
                Debug.Log("GameConnection: login in other device, try reinitialize later");
            }
            else if (state == GameConnectionEvent.KickedByGameServer)
            {
                StopPicoGame();
                Debug.Log("GameConnection: be kicked by server, try reinitialize later");
            }
            else
            {
                Debug.Log("GameConnection: unknown error");
            }
        }

        private void OnRequestFailed(Message<GameRequestFailedReason> msg)
        {
            Debug.Log($"OnRequestFailed: {msg.Data}");
        }

        private void OnGameStateReset(Message msg)
        {
            Debug.LogError("OnGameStateReset");
            StopPicoGame();
        }
    }
}