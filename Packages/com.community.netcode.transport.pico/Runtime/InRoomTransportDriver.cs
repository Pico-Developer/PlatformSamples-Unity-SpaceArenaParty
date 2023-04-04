using System;
using System.Collections.Generic;
using System.Text;
using Pico.Platform.Models;
using Unity.Netcode;
using UnityEngine;
// using static Netcode.Transports.Pico.AlreadyInRoomProvider;
using PicoPlatform = Pico.Platform;

namespace Netcode.Transports.Pico
{
    /// <summary>
    ///     This is a helper class, it combines PicoTransport with outer pico matchmaking/room service to allow developer to
    ///     use Netcode in Pico PlatformServices.
    ///     It drive the transport for Netcode with info/event from pico(room) service, as follows:
    ///     when something happened in room(such as player leave):
    ///     pico room notification > this driver > PicoTransport > appropriate event to Netcode
    ///     when netcode logic send msg(such as ServerRpc):
    ///     netcode > PicoTransport > this driver > pico room send
    ///     [receiver side] > pico room recv > this driver > PicoTransport > data event to Netcode
    ///     For developer:
    ///     1. execute matchmaking and room joining procedure to join a pico room, using pico platform's game service api;
    ///     2. call InRoomTransportDriver.Init with room info to prepare PicoTransport;
    ///     3. call HandleRoomInfoUpdate on any pico room info change;
    ///     4. use Netcode's facility;
    /// </summary>
    public class InRoomTransportDriver
    {
        public enum TransportDriverEvent
        {
            Error,
            Shutdown,
            BeforeReenter,
            AfterReenter
        }

        private readonly AlreadyInRoomProvider _matchroomProvider = new AlreadyInRoomProvider();
        private bool _inited;
        private Dictionary<ulong, string> _networkid2OpenID;
        private AlreadyInRoomProvider.PicoRoomWrapper _picoRoomWrapper;
        private PicoTransport _picoTransport;

        private string _selfOpenID;
        public event Action<NetworkEvent, ulong, string> OnClientEvent;
        public event Action<TransportDriverEvent, int, string> OnDriverEvent;

        public bool Init(bool autoRestartNetcode, PicoTransport picoTransport, string selfOpenID, Room roomInfo)
        {
            _selfOpenID = selfOpenID;
            _picoTransport = picoTransport;
            if (autoRestartNetcode)
            {
                _picoTransport.SetRestartFlagOnHostLeave();
            }

            _networkid2OpenID = new Dictionary<ulong, string>();
            if (roomInfo.OwnerOptional == null)
            {
                PicoTransport.Log("owner is not in room now, postpone transport init ...");
                _inited = false;
                return true;
            }

            return InnerStart(selfOpenID, GetPicoRoomInfo(roomInfo));
        }

        public void Uninit()
        {
            if (!_inited)
            {
                PicoTransport.Log("duplicated uninit transport driver");
                return;
            }

            PicoTransport.Log("Uninit transport driver now ...");
            _inited = false;
            _picoTransport.UninitPicoTransport();
            if (!_picoTransport.GetRestartNetcodeFlag())
            {
                _selfOpenID = null;
                _picoTransport = null;
                if (NetworkManager.Singleton)
                {
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                    NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
                }
            }
        }

        private bool InnerStart(string selfOpenID, PicoRoomInfo roomInfo)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
            _picoRoomWrapper = new AlreadyInRoomProvider.PicoRoomWrapper();
            _picoRoomWrapper.KickUser = PicoKickUser;
            _picoRoomWrapper.LeaveRoom = PicoLeaveRoom;
            _picoRoomWrapper.SendMsgToUID = PicoSendMsgToUID;
            _picoRoomWrapper.OnTransportShutdown = HandleTransportShutdown;
            _picoRoomWrapper.SelfOpenID = selfOpenID;
            _picoRoomWrapper.PicoRoomInfo = roomInfo;

            _networkid2OpenID = new Dictionary<ulong, string>();
            if (!_matchroomProvider.Init(_picoRoomWrapper))
            {
                PicoTransport.LogError("init picoProvider failed");
                return false;
            }

            _inited = _picoTransport.InitPicoTransport(_matchroomProvider, (bool initResult, bool isServer) =>
            {
                if (!initResult)
                {
                    PicoTransport.LogError("init pico transport driver failed");
                    return;
                }

                PicoTransport.Log($"!!!transport driver init succeed, isServer {isServer}, selfOpenID {_selfOpenID}");
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                if (isServer)
                {
                    NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
                    NetworkManager.Singleton.StartHost();
                }
                else
                {
                    NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(_selfOpenID);
                    NetworkManager.Singleton.StartClient();
                }
            });
            return _inited;
        }

        public string GetOpenIDOfNetworkID(ulong networkID)
        {
            string openID = "";
            bool got = _networkid2OpenID.TryGetValue(networkID, out openID);
            return got ? openID : "no player openID";
        }

        public void HandleRoomInfoUpdate(Room roomInfo)
        {
            if (roomInfo.RoomId == 0)
            {
                Debug.Log("OnRoomInfoUpdate, room_id is 0(leave room notification)");
                OnDriverEvent?.Invoke(TransportDriverEvent.Error, -1, "room left");
                return;
            }

            if (!_inited)
            {
                if (roomInfo.OwnerOptional == null)
                {
                    Debug.Log("owner is not in room now, postpone (again) transport init ...");
                    return;
                }

                Debug.Log("owner entered room now, issue transport init ...");
                InnerStart(_selfOpenID, GetPicoRoomInfo(roomInfo));
                return;
            }

            _picoRoomWrapper.PicoRoomInfo = GetPicoRoomInfo(roomInfo);
            _matchroomProvider.HandleRoomInfoUpdate(_picoRoomWrapper.PicoRoomInfo);
        }

        private void HandleMsgFromRoom(Packet pkgFromRoom)
        {
            byte[] message = new byte[pkgFromRoom.Size];
            ulong pkgSize = pkgFromRoom.GetBytes(message);
            if (pkgSize <= 0)
            {
                PicoTransport.LogError($"OnMsgFromRoom, error pkgSize: {pkgSize}");
                OnDriverEvent?.Invoke(TransportDriverEvent.Error, -1, "error pkg size from room");
                return;
            }

            _matchroomProvider.HandleMsgFromRoom(pkgFromRoom.SenderId, message);
        }

        private void OnClientConnectedCallback(ulong clientID)
        {
            string openID;
            bool gotOpenID = _networkid2OpenID.TryGetValue(clientID, out openID);
            if (!gotOpenID)
            {
                PicoTransport.LogWarning($"unknown client connected in, its netcode ID {clientID}");
            }

            PicoTransport.Log($"!!!, OnClientConnectedCallback, clientID {clientID}, clientOpenID {openID}");
            OnClientEvent?.Invoke(NetworkEvent.Connect, clientID, openID);
        }

        private void OnClientDisconnectedCallback(ulong clientID)
        {
            string openID;
            bool gotOpenID = _networkid2OpenID.TryGetValue(clientID, out openID);
            if (!gotOpenID)
            {
                //will be here, if got pico room player which has not finish approval procedure, and leave now.
                PicoTransport.LogWarning($"unknown client disconnected, its netcode ID {clientID}");
            }

            PicoTransport.Log($"!!!, OnClientDisconnectedCallback, clientID {clientID}, clientOpenID {openID}");
            OnClientEvent?.Invoke(NetworkEvent.Disconnect, clientID, openID);
        }

        public void Update()
        {
            CheckRestartNetcode();
            RecvRoomPackage();
        }

        private void CheckRestartNetcode()
        {
            if (_picoTransport && _picoTransport.GetRestartNetcodeFlag())
            {
                PicoTransport.Log($"got restart flag, reinit picoTransport ...");
                OnDriverEvent?.Invoke(TransportDriverEvent.BeforeReenter, 0, "");
                InnerStart(_picoRoomWrapper.SelfOpenID, _picoRoomWrapper.PicoRoomInfo);
                OnDriverEvent?.Invoke(TransportDriverEvent.AfterReenter, 0, "");
            }
        }

        private void RecvRoomPackage()
        {
            // read packet
            var packet = PicoPlatform.NetworkService.ReadPacket();
            while (packet != null)
            {
                HandleMsgFromRoom(packet);
                packet.Dispose();
                packet = PicoPlatform.NetworkService.ReadPacket();
            }
        }

        private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var playerOpenID = Encoding.ASCII.GetString(request.Payload);
            if (NetworkManager.ServerClientId == request.ClientNetworkId)
            {
                PicoTransport.Log(
                    $"!!!, ClientID(host) {request.ClientNetworkId} pre approval, use m_selfOpenID {_selfOpenID}!");
                playerOpenID = _selfOpenID;
            }

            _networkid2OpenID.Add(request.ClientNetworkId, playerOpenID);
            PicoTransport.Log($"!!!, ClientID {request.ClientNetworkId} start approval, It's openID {playerOpenID}!");
            response.Approved = true;
            response.CreatePlayerObject = NetworkManager.Singleton.NetworkConfig.PlayerPrefab != null;
        }

        private int PicoSendMsgToUID(string tgtOpenID, byte[] message)
        {
            // Debug.Log($"PicoSendMsgToUID, tgtOpenID: {tgtOpenID}, size:{message.Length}");
            PicoPlatform.NetworkService.SendPacket(tgtOpenID, message, true);
            return 0;
        }

        private int PicoKickUser(ulong roomID, string userOpenId)
        {
            PicoTransport.LogWarning($"PicoKickUser, roomID: {roomID}, userOpenId:{userOpenId}");
            PicoPlatform.RoomService.KickUser(roomID, userOpenId, -1).OnComplete(HandleKickPlayerResponse);
            return 0;
        }

        private int PicoLeaveRoom(ulong roomID)
        {
            Debug.Log($"PicoLeaveRoom, roomID: {roomID}");
            PicoTransport.Log($"PicoLeaveRoom, roomID: {roomID}");
            PicoPlatform.RoomService.Leave(roomID).OnComplete(HandleLeaveRoomResponse);
            return 0;
        }

        private int HandleTransportShutdown()
        {
            PicoTransport.Log($"driver go event: pico transport shutdown");
            OnDriverEvent?.Invoke(TransportDriverEvent.Shutdown, -1, "transport uninited");
            return 0;
        }

        private PicoRoomInfo GetPicoRoomInfo(Room room)
        {
            PicoTransport.Log($"in GetPicoRoomInfo, {room}");
            PicoRoomInfo roomInfo = new PicoRoomInfo();
            roomInfo.RoomID = room.RoomId;
            if (room.OwnerOptional != null)
            {
                roomInfo.OwnerOpenID = room.OwnerOptional.ID;
            }
            else
            {
                roomInfo.OwnerOpenID = "";
            }

            if (room.UsersOptional != null)
            {
                foreach (User user in room.UsersOptional)
                {
                    roomInfo.CurRoomOpenIDs.Add(user.ID);
                }
            }
            else
            {
                roomInfo.CurRoomOpenIDs.Clear();
            }

            return roomInfo;
        }

        void CommonProcess(string funName, PicoPlatform.Message message, Action action)
        {
            PicoTransport.Log($"message.Type: {message.Type}");
            if (!message.IsError)
            {
                Debug.Log($"{funName} no error");
                action();
            }
            else
            {
                var error = message.GetError();
                Debug.Log($"{funName} error: {error.Message}");
            }
        }

        void HandleLeaveRoomResponse(PicoPlatform.Message<Room> message)
        {
            CommonProcess("HandleLeaveRoom", message, () =>
            {
                var result = message.Data;
                HandleRoomInfoUpdate(result);
            });
        }

        void HandleKickPlayerResponse(PicoPlatform.Message<Room> message)
        {
            CommonProcess("HandleKickPlayerResponse", message, () =>
            {
                var result = message.Data;
                HandleRoomInfoUpdate(result);
            });
        }
    }
}