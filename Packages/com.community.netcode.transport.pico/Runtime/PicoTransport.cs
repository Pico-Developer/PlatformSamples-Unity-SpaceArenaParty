using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Netcode.Transports.Pico
{
    [RequireComponent(typeof(NetworkManager))]
    [DefaultExecutionOrder(-1000)]
    public partial class PicoTransport : NetworkTransport, IMatchRoomEventHandler
    {
        public delegate void InitResultCallback(bool initResult, bool isServer);

        public enum PicoServiceState
        {
            Stopped = 0,
            InInit,
            Inited,
            InMatching,
            MatchEnqueued,
            MatchFound,
            RoomJoining,
            InRoom,
            RoomLeaving
        }

        public enum TransportState
        {
            Stopped = 0,
            Started
        }

        private static string _picoAccount;
        private static string _serverIp;
        private static ushort _serverPort;
        private readonly string _appID = "";
        private readonly string _poolName = "test_pool_ad2";
        private readonly PicoServiceInfo _serviceInfo = new();
        private InitResultCallback _initCallback;

        private bool _isSelfServer;
        private IMatchRoomProvider _matchRoomProvider;
        private TransportState _transportState = TransportState.Stopped;

        private PicoServiceState CurrentState
        {
            get => _serviceInfo.CurState;
            set => _serviceInfo.CurState = value;
        }

        public override ulong ServerClientId => 0;

        private void Update()
        {
            if (CurrentState == PicoServiceState.Stopped) return;
            _matchRoomProvider.UpdateProvider();
            CheckNetcodeEvent(_serviceInfo.CurRoomUIDs);
        }

        private void OnDestroy()
        {
            UninitPicoTransport();
        }

        // public static bool SetLoginInfo(string account, string hostIP, ushort hostPort, bool isAuth)
        // {
        //     _picoAccount = account;
        //     _serverIp = hostIP;
        //     _serverPort = hostPort;
        //     return true;
        // }

        public bool GetRestartNetcodeFlag()
        {
            return _serviceInfo.RestartNetcoodeFlag;
        }

        public void SetRestartFlagOnHostLeave()
        {
            _serviceInfo.SetRestartFlag = true;
        }

        public bool InitPicoTransport(IMatchRoomProvider matchRoomProvider, InitResultCallback initCallback)
        {
            if (CurrentState != PicoServiceState.Stopped && !_serviceInfo.RestartNetcoodeFlag)
            {
                LogError("duplicated pico service init");
                return false;
            }

            _serviceInfo.RestartNetcoodeFlag = false;
            CurrentState = PicoServiceState.InInit;
            _serviceInfo.CancelFlag = false;
            if (initCallback != null) _initCallback = initCallback;
            if (matchRoomProvider != null) _matchRoomProvider = matchRoomProvider;

            if (null == _initCallback || null == matchRoomProvider)
            {
                LogError("invalid matchroomProvider or invalid _initCallback");
                return false;
            }

            _matchRoomProvider.SetLogDelegate(MyLog);
            return _matchRoomProvider.Login(this, _serverIp, _serverPort, _picoAccount, _appID, -1);
        }

        public void UninitPicoTransport()
        {
            Debug.Log($"UninitPicoTransport be called, CurrentState {CurrentState}");
            if (CurrentState >= PicoServiceState.InRoom)
                //TryIssueTransoprtEvent(NetworkEvent.Disconnect, m_serviceInfo.selfUID);
                if (NetworkManager.Singleton != null)
                {
                    Debug.Log("in UninitPicoService, NetworkManger shutdown ...");
                    NetworkManager.Singleton.Shutdown();
                }

            _transportState = TransportState.Stopped;
            _serviceInfo.ResetServiceInfo();
            if (_matchRoomProvider != null)
            {
                _matchRoomProvider.ResetLogDelegate();
                _ = _matchRoomProvider.UninitProvider();
            }
        }

        public bool MatchCreateAndWait()
        {
            CurrentState = PicoServiceState.InMatching;
            return _matchRoomProvider.MatchmakingCreateAndEnqueue(_poolName);
        }

        public bool MatchWait()
        {
            CurrentState = PicoServiceState.InMatching;
            return _matchRoomProvider.MatchmakingEnqueue(_poolName);
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload,
            out float receiveTime)
        {
            clientId = 0;
            receiveTime = Time.realtimeSinceStartup;
            payload = default;
            return NetworkEvent.Nothing;
        }

        private void PicoOnLoginResult(bool loginResult)
        {
            if (_serviceInfo.CancelFlag)
            {
                LogError($"PicoOnLoginResult, initResult:{loginResult}, cancel pico procedure due to cancelFlag");
                loginResult = false;
            }

            CurrentState = loginResult ? PicoServiceState.Inited : PicoServiceState.Stopped;
            if (!loginResult)
            {
                LogError("login failed, uninit now");
                UninitPicoTransport();
                _initCallback?.Invoke(false, false);
                return;
            }

            //start querying if self is host or client
            if (!_matchRoomProvider.HostCheck())
            {
                LogError("HostCheck failed, uninit now");
                UninitPicoTransport();
                _initCallback?.Invoke(false, false);
            }
        }

        public override bool StartClient()
        {
            if (_serviceInfo.SelfUID == _serviceInfo.OwnerUID)
            {
                LogError("self is room owner, but StartClient be called");
                return false;
            }

            _transportState = TransportState.Started;
            _isSelfServer = false;
            if (CurrentState == PicoServiceState.Stopped)
            {
                InitPicoTransport(null, null);
                return true;
            }

            return true;
        }

        public override bool StartServer()
        {
            if (_serviceInfo.SelfUID != _serviceInfo.OwnerUID)
            {
                LogError("self is not room owner, but StartServer be called");
                return false;
            }

            _transportState = TransportState.Started;
            _isSelfServer = true;
            if (CurrentState == PicoServiceState.Stopped)
            {
                InitPicoTransport(null, null);
                return true;
            }

            return true;
        }

        public override void DisconnectRemoteClient(ulong clientID)
        {
            Log(
                $"DisconnectRemoteClient, selfUID:{_serviceInfo.SelfUID}, ownerUID{_serviceInfo.OwnerUID}, remoteUID:{clientID}");
            if (!_isSelfServer)
            {
                LogError("DisconnectRemoteClient, self is not server, skip this request");
                return;
            }

            if (CurrentState != PicoServiceState.InRoom)
            {
                LogWarning($"DisconnectRemoteClient, CurrentState {CurrentState} not in game now, skip this request");
                return;
            }

            if (!_serviceInfo.CurRoomUIDs.Contains(clientID))
                LogError($"DisconnectRemoteClient, targetId({clientID}) is not in game now, skip this request");
            _ = _matchRoomProvider.RoomKickUserByID(_serviceInfo.RoomID, clientID);
        }

        public override void DisconnectLocalClient()
        {
            Log($"DisconnectLocalClient, selfUID:{_serviceInfo.SelfUID}, ownerUID{_serviceInfo.OwnerUID}");
            if (CurrentState != PicoServiceState.InRoom)
            {
                LogWarning($"DisconnectLocalClient, curState({CurrentState}), not in game now, skip this request");
                return;
            }

            if (!_serviceInfo.RestartNetcoodeFlag)
            {
                CurrentState = PicoServiceState.RoomLeaving;
                Log(
                    $"DisconnectLocalClient, issue self leave room, selfUID:{_serviceInfo.SelfUID}, ownerUID{_serviceInfo.OwnerUID}");
                if (!_matchRoomProvider.RoomLeave(_serviceInfo.RoomID))
                    Log(
                        $"DisconnectLocalClient, issue self leave room failed, selfUID:{_serviceInfo.SelfUID}, ownerUID{_serviceInfo.OwnerUID}");
            }
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            return 0;
        }

        public override void Send(ulong clientId, ArraySegment<byte> data, NetworkDelivery delivery)
        {
            // Log($"send be called, tgt clientId {clientId}, send size {data.Count}");
            var dataArray = data.ToArray();
            if (clientId == ServerClientId) clientId = _serviceInfo.OwnerUID;
            // Log($"send to server, change tgt clientId to {clientId}, send size {data.Count}");
            _ = _matchRoomProvider.SendPacket2UID(clientId, dataArray);
        }

        public override void Shutdown()
        {
            Log($"PicoTransport:Shutdown, selfId:{_serviceInfo.SelfUID}, curState: {CurrentState}");
            _serviceInfo.CancelFlag = true;
            if (CurrentState < PicoServiceState.RoomJoining)
            {
                Log($"Shutdown, selfId:{_serviceInfo.SelfUID}, curState: {CurrentState}, call UninitPicoService");
                UninitPicoTransport();
                return;
            }

            //以下InRoom或者RoomLeaving
            if (CurrentState == PicoServiceState.InRoom)
            {
                Log($"Shutdown, selfId:{_serviceInfo.SelfUID}, curState: {CurrentState}, call DisconnectLocalClient");
                DisconnectLocalClient();
            }
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
            Log($"pico transport initialize be called, current LocalClientID {NetworkManager.Singleton.LocalClientId}");
        }

        private class PicoServiceInfo
        {
            public bool CancelFlag;
            public HashSet<ulong> CurRoomUIDs = new();
            public PicoServiceState CurState = PicoServiceState.Stopped;
            public HashSet<ulong> NewRoomUIDs = new();
            public PicoServiceState NotifiedState = PicoServiceState.Stopped;
            public ulong OwnerUID;
            public bool RestartNetcoodeFlag;
            public ulong RoomID;
            public ulong SelfUID;
            public bool SetRestartFlag;

            public void ResetServiceInfo()
            {
                CancelFlag = false;
                SelfUID = 0;
                RoomID = 0;
                OwnerUID = 0;
                CurState = PicoServiceState.Stopped;
                NotifiedState = PicoServiceState.Stopped;
                CurRoomUIDs.Clear();
                NewRoomUIDs.Clear();
            }
        }
    } //PicoTransport
}