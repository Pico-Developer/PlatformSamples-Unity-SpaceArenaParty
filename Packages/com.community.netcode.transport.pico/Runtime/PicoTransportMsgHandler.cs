using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Netcode.Transports.Pico
{
    public partial class PicoTransport
    {
//         public void OnPicoNetworkEvent(PicoNetworkEvent picoNetworkEvent)
//         {
// #if HANDLE_LOST_RESUME
//             NetworkEvent notifyEvent = NetworkEvent.Nothing;
//             bool notifyNetcode = false;
//             switch (picoNetworkEvent)
//             {
//                 case PicoNetworkEvent.Lost:
//                     notifyEvent = NetworkEvent.NetworkLost;
//                     notifyNetcode = true;
//                     break;
//                 case PicoNetworkEvent.Resumed:
//                     notifyEvent = NetworkEvent.NetworkResumed;
//                     notifyNetcode = true;
//                     break;
//                 default:
//                     break;
//             }
//             if (notifyNetcode)
//             {
//                 LogWarning($"notify netcode event {notifyEvent}");
//                 TryIssueTransoprtEvent(notifyEvent, 0);
//         }
// #endif
//             bool isLogout = (picoNetworkEvent == PicoNetworkEvent.Closed || picoNetworkEvent == PicoNetworkEvent.Unknown);
//             if (isLogout)
//             {
//                 LogWarning($"network error {picoNetworkEvent}, uninit now");
//                 UninitPicoService();
//             }
//             return;
//         }

        public void OnLoginResult(bool result, ulong selfUID)
        {
            if (result) _serviceInfo.SelfUID = selfUID;
            PicoOnLoginResult(result);
        }

        public void OnHostCheckResult(bool initResult, NetcodeMode peerType)
        {
            Log("multi-player game modules ready");
            if (!initResult)
            {
                LogError("session init failed, uninit now");
                UninitPicoTransport();
                _initCallback?.Invoke(false, false);
                return;
            }

            TryStartNetcodeProcedure(peerType);
        }

        public void OnMatchRoomProviderError(int errorCode)
        {
            LogError($"error from match_room_provider: {errorCode}");
            UninitPicoTransport();
        }

        public void OnMatchEnqueueResult(bool result)
        {
            if (!result)
            {
                LogError("match enqueue failed");
                UninitPicoTransport();
                return;
            }

            CurrentState = PicoServiceState.MatchEnqueued;
            Log("enqueue succeed");
        }

        public void OnMatchFoundResult(bool result, NetcodeMode peerType, ulong roomID)
        {
            if (!result)
            {
                LogError("got failed match found result");
                UninitPicoTransport();
                return;
            }

            var isServer = peerType == NetcodeMode.Host;
            if (isServer)
            {
                CurrentState = PicoServiceState.InRoom;
                if (CheckCancelFlag()) return;
            }
            else
            {
                Log($"match found, call join room {roomID} ...");
                CurrentState = PicoServiceState.RoomJoining;
                if (CheckCancelFlag())
                {
                    Log("match found, bug session has canceled");
                    return;
                }

                if (!_matchRoomProvider.RoomJoin(roomID))
                {
                    LogError($"call RoomJoin({roomID}) failed");
                    UninitPicoTransport();
                    return;
                }
            }

            if (isServer) _serviceInfo.OwnerUID = _serviceInfo.SelfUID;
            _initCallback?.Invoke(true, isServer);
        }

        public void OnPlayerEnterRoom(ulong userID, TransportRoomInfo transportRoomInfo)
        {
            if (userID == _serviceInfo.SelfUID)
            {
                CurrentState = PicoServiceState.InRoom;
                _serviceInfo.RoomID = transportRoomInfo.RoomID;
                _serviceInfo.OwnerUID = transportRoomInfo.OwnerUID;
                Log($"room join succeed, room {_serviceInfo.RoomID}");
            }

            if (CheckCancelFlag()) return;
            OnRoomInfoUpdate(transportRoomInfo);
        }

        public void OnPlayerLeaveRoom(ulong userUID, TransportRoomInfo transportRoomInfo)
        {
            if (transportRoomInfo.RoomID != _serviceInfo.RoomID)
            {
                LogError($"room leave response, not in this room(${transportRoomInfo.RoomID}) now, skip this response");
                return;
            }

            if (userUID == _serviceInfo.SelfUID)
            {
                CurrentState = PicoServiceState.Stopped;
                TryIssueTransoprtEvent(NetworkEvent.Disconnect, _serviceInfo.SelfUID);
                Shutdown();
            }
        }

        public void OnRoomInfoUpdate(TransportRoomInfo transportRoomInfo)
        {
            Log($"OnRoomInfoUpdate 1, got roomInfo, player num: {transportRoomInfo.CurRoomUIDs.Count}");
            if (null == transportRoomInfo)
            {
                LogError("OnRoomInfoUpdate, roomInfo is null");
                return;
            }

            if (transportRoomInfo.RoomID != _serviceInfo.RoomID)
            {
                LogError($"got non-current room info, roomID:{transportRoomInfo.RoomID} vs {_serviceInfo.RoomID}");
                return;
            }

            _serviceInfo.NewRoomUIDs = new HashSet<ulong>(transportRoomInfo.CurRoomUIDs);
            Log($"OnRoomInfoUpdate 2, got roomInfo, player num: {_serviceInfo.NewRoomUIDs.Count}");
            var oldOwnerUID = _serviceInfo.OwnerUID;
            _serviceInfo.OwnerUID = transportRoomInfo.OwnerUID;
            if (oldOwnerUID != transportRoomInfo.OwnerUID && oldOwnerUID != 0)
            {
                Log($"owner changed from {oldOwnerUID} to {transportRoomInfo.OwnerUID}");
                if (NetworkManager.Singleton != null)
                {
                    //重连条件: 1.有新房主; 2.自己不是旧房主; 3.自己还是房间内
                    _serviceInfo.RestartNetcoodeFlag = _serviceInfo.SetRestartFlag && transportRoomInfo.OwnerUID != 0 &&
                                                       _serviceInfo.SelfUID != oldOwnerUID &&
                                                       _serviceInfo.NewRoomUIDs.Contains(_serviceInfo.SelfUID);
                    LogError(
                        $"owner changed, shutdown networkmanager now!, is restartNetcoodeFlag {_serviceInfo.RestartNetcoodeFlag}, selfUID {_serviceInfo.SelfUID}, oldOwnerUID {oldOwnerUID}");
                    NetworkManager.Singleton.Shutdown();
                }
            }

            if (_transportState == TransportState.Started)
            {
                CheckNetcodeEvent(_serviceInfo.NewRoomUIDs);
                //find out those left && new entered
                foreach (var olduid in _serviceInfo.CurRoomUIDs)
                {
                    Log($"check event of olduid:{olduid}");
                    if (!_serviceInfo.NewRoomUIDs.Contains(olduid))
                    {
                        //left player
                        Log(
                            $"try notify client disconnect event, uid in event:{olduid}, ownerUID:{_serviceInfo.OwnerUID}");
                        TryIssueTransoprtEvent(NetworkEvent.Disconnect, olduid);
                    }
                }

                foreach (var newuid in _serviceInfo.NewRoomUIDs)
                {
                    Log($"check event of newuid:{newuid}");
                    if (!_serviceInfo.CurRoomUIDs.Contains(newuid))
                    {
                        //new entered player
                        Log(
                            $"try notify client connect event, uid in event:{newuid}, ownerUID:{_serviceInfo.OwnerUID}");
                        TryIssueTransoprtEvent(NetworkEvent.Connect, newuid);
                    }
                }
            }

            var tmp = _serviceInfo.CurRoomUIDs;
            _serviceInfo.CurRoomUIDs = _serviceInfo.NewRoomUIDs;
            _serviceInfo.NewRoomUIDs = tmp;
            Log(
                $"OnRoomInfoUpdate 3, transport {_transportState}, newest roomInfo, player num in roomInfo:{_serviceInfo.CurRoomUIDs.Count}");
        }

        public void OnPkgRecved(ulong senderUID, ArraySegment<byte> pkg)
        {
            InvokePicoTransportEvent(NetworkEvent.Data, senderUID, pkg);
        }

        private void InvokePicoTransportEvent(NetworkEvent networkEvent, ulong userId = 0,
            ArraySegment<byte> payload = default)
        {
            // if (networkEvent != NetworkEvent.Data)
            // {
            //     Log($"InvokePicoTransportEvent, networkEvent:{networkEvent}, userId:{userId}");
            // } else
            // {
            //     Log($"network payload recved: from userId({userId}), size({payload.Count})");
            // }
            switch (networkEvent)
            {
                case NetworkEvent.Nothing:
                    // do nothing
                    break;
                case NetworkEvent.Disconnect:
                    // need no extra handling now
                    goto default;
                default:
                    InvokeOnTransportEvent(networkEvent, userId, payload, Time.realtimeSinceStartup);
                    break;
            }
        }

        private void TryIssueTransoprtEvent(NetworkEvent networkEvent, ulong uid)
        {
            var isSelf = uid == _serviceInfo.SelfUID;
            var isServerEvent = uid == _serviceInfo.OwnerUID || uid == 0;
            if (_isSelfServer)
            {
                if (!isSelf)
                {
                    Log(
                        $"(server) send others player event '{networkEvent}' to netcode, uid in event:{uid}, ownerUID:{_serviceInfo.OwnerUID}");
                    InvokePicoTransportEvent(networkEvent, uid);
                }
            }
            else
            {
                if (isSelf)
                {
                    LogWarning(
                        $"(client) send self event '{networkEvent}' to netcode, uid in event:{uid}, ownerUID:{_serviceInfo.OwnerUID}");
                    InvokePicoTransportEvent(networkEvent);
                }
                else if (isServerEvent)
                {
                    Log(
                        $"(client) send server's event '{networkEvent}' to netcode, uid in event:{uid}, ownerUID:{_serviceInfo.OwnerUID}");
                    InvokePicoTransportEvent(networkEvent, uid);
                }
            }
        }

        private bool CheckCancelFlag()
        {
            if (!_serviceInfo.CancelFlag) return false;
            Shutdown();
            return true;
        }

        private void TryStartNetcodeProcedure(NetcodeMode peerType)
        {
            if (CurrentState != PicoServiceState.Inited)
            {
                LogError($"TryStartNetcodeProcedure, but CurrentState is {CurrentState}");
                return;
            }

            if (peerType == NetcodeMode.Host)
                MatchCreateAndWait();
            else
                MatchWait();
        }

        private void CheckNetcodeEvent(HashSet<ulong> newestRoomUIDs)
        {
            if (_transportState != TransportState.Started)
                //wait StartClient/StartHost to be called
                return;
            var notied_state = _serviceInfo.NotifiedState;
            var cur_state = _serviceInfo.CurState;
            if (notied_state != cur_state)
            {
                Log($"notied_state {notied_state} disaccord with cur_state {cur_state}");
                if (cur_state == PicoServiceState.InRoom)
                {
                    //in room, and haven't notified netcode yet.
                    Log($"self new enter room, self uid:{_serviceInfo.SelfUID}, serverUID:{_serviceInfo.OwnerUID}");
                    if (!_isSelfServer)
                    {
                        //non-server, notify(client online)
                        Log(
                            $"(clietn) self is client: try notify server connect event, self uid:{_serviceInfo.SelfUID}, serverUID:{_serviceInfo.OwnerUID}");
                        TryIssueTransoprtEvent(NetworkEvent.Connect, _serviceInfo.OwnerUID);
                    }
                    else
                    {
                        //server, notify(other clients online)
                        foreach (var uid in newestRoomUIDs)
                        {
                            Log(
                                $"(server) self is server: try notify client connect event, client uid:{uid}, serverUID:{_serviceInfo.OwnerUID}");
                            if (uid != _serviceInfo.SelfUID) TryIssueTransoprtEvent(NetworkEvent.Connect, uid);
                        }
                    }
                }
                else
                {
                    //not in room now
                    if (notied_state == PicoServiceState.InRoom)
                    {
                        Log($"self disconnect, self uid:{_serviceInfo.OwnerUID}, serverUID:{_serviceInfo.OwnerUID}");
                        TryIssueTransoprtEvent(NetworkEvent.Disconnect, _serviceInfo.OwnerUID);
                    }
                }

                _serviceInfo.NotifiedState = cur_state;
                _serviceInfo.CurRoomUIDs =
                    new HashSet<ulong>(newestRoomUIDs); // m_serviceInfo.curRoomUIDs;//已经是最新信息了，避免OnRoomInfoUpdate中的重复通知
            }
        }
    } //PicoTransport
} //Transport.Pico