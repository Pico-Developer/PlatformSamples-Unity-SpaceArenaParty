using System;
using Netcode.Transports.Pico;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.ConnectionManagement.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.ConnectionManagement
{
    public partial class MultiPlayerManager
    {
        // Update is called once per frame
        private void Update()
        {
            if (CurState == GameState.InRoom)
                if (_transportDriver != null)
                    _transportDriver.Update();
        }

        private void RegisterNotificationCallbacks()
        {
            NetworkService.SetNotification_Game_ConnectionEventCallback(OnGameConnectionEvent);
            NetworkService.SetNotification_Game_Request_FailedCallback(OnRequestFailed);
            NetworkService.SetNotification_Game_StateResetCallback(OnGameStateReset);

            RoomService.SetLeaveNotificationCallback(OnRoomLeaveNotification);
            RoomService.SetJoin2NotificationCallback(OnRoomJoin2Notification);
            RoomService.SetKickUserNotificationCallback(OnRoomKickUserNotification);
            RoomService.SetUpdateOwnerNotificationCallback(OnRoomUpdateOwnerNotification);
            RoomService.SetUpdateNotificationCallback(ProcessRoomUpdate);
        }

        private void ProcessRoomUpdate(Message<Room> message)
        {
            CommonProcess("ProcessRoomUpdate", message, () =>
            {
                RoomData = message.Data;
                var room = message.Data;
                Debug.Log($"ProcessRoomUpdate room id {room.RoomId}");
                if (_transportDriver != null) _transportDriver.HandleRoomInfoUpdate(room);
            });
        }

        private void ProcessRoomJoin2(Message<Room> message)
        {
            CommonProcess("ProcessRoomJoin2", message, () =>
            {
                if (message.IsError)
                {
                    var err = message.GetError();
                    Debug.LogError($"Join room error {err.Message} code={err.Code}");
                    CallOnStatusChange(GameState.Error, "Join room failed");
                    return;
                }

                var room = message.Data;
                Debug.Log($"in ProcessRoomJoin2, roomData: {GameUtils.GetRoomLogData(room)}");
                OnRoomJoined(room);
            });
        }

        private void ProcessRoomLeave(Message<Room> message)
        {
            CommonProcess("ProcessRoomLeave", message, () =>
            {
                if (message.IsError)
                {
                    var err = message.GetError();
                    Debug.LogError($"Leave room error {err.Message} code={err.Code}");
                    CallOnStatusChange(GameState.Error, "Leave room failed");
                    return;
                }

                var room = message.Data;
                Debug.Log($"in ProcessRoomLeave, roomLeave: {GameUtils.GetRoomLogData(room)}");

                OnRoomLeft(room);
            });
        }

        private void HandleDriverEvent(InRoomTransportDriver.TransportDriverEvent type, int errorCode, string errorInfo)
        {
            Debug.Log($"HandleDriverEvent, event {type}");
            if (type == InRoomTransportDriver.TransportDriverEvent.Error
                || type == InRoomTransportDriver.TransportDriverEvent.Shutdown)
            {
                if (type == InRoomTransportDriver.TransportDriverEvent.Shutdown)
                    Debug.LogWarning($"shutdowned: ({errorCode}, {errorInfo}), stop transport driver now");
                else
                    Debug.LogWarning($"error: ({errorCode}, {errorInfo}), stop transport driver now");
                _transportDriver.OnDriverEvent -= HandleDriverEvent;
                _transportDriver.OnClientEvent -= HandleClientEvent;
                _transportDriver.Uninit();
                _transportDriver = null;
                if (CurState != GameState.RoomLeaving) CallOnStatusChange(GameState.Error, "pico transport shutdown");
            }
        }

        private void HandleClientEvent(NetworkEvent type, ulong clientID, string openID)
        {
            Debug.Log($"!!!, event from transport driver: (event:{type}, clientID:{clientID}, openID:{openID})");
            if (type == NetworkEvent.Connect)
            {
                // CallOnStatusChange(GameState.Connected, "connected to server");
            }
            else
            {
                Debug.Log($"!!!, TODO: dicsonnect event from transport driver: (event:{type}, clientID:{clientID})");
            }
        }

        private void OnRoomJoined(Room room)
        {
            Debug.Log($"room {room.RoomId} join succeed, enter fighting scene ...");
            RoomID = room.RoomId;
            RoomData = room;
            CallOnStatusChange(GameState.InRoom, "in room now");
            StartPicoTransport();
        }

        private void OnRoomLeft(Room room)
        {
            Debug.Log($"room {room.RoomId} leave succeed, leave fighting scene ...");
            if (RoomID != room.RoomId)
            {
                Debug.Log($"room {room.RoomId} leave succeed, but current is not in this room now, skip this request");
                return;
            }

            RoomID = 0;
            RoomData = null;
            CallOnStatusChange(GameState.Idle, "room left");
        }

        private void OnRoomKickUserNotification(Message<Room> message)
        {
            CommonProcess("OnRoomKickUserNotification", message, () =>
            {
                var room = message.Data;
                Debug.Log(GameUtils.GetRoomLogData(room));
            });
        }

        private void OnRoomUpdateOwnerNotification(Message message)
        {
            CommonProcess("OnRoomUpdateOwnerNotification", message,
                () => { Debug.Log("OnRoomUpdateOwnerNotification"); });
        }

        private void OnRoomLeaveNotification(Message<Room> message)
        {
            CommonProcess("OnRoomLeaveNotification", message, () =>
            {
                var room = message.Data;
                Debug.Log(GameUtils.GetRoomLogData(room));
                OnRoomLeft(room);
            });
        }

        private void OnRoomJoin2Notification(Message<Room> message)
        {
            CommonProcess("OnRoomJoin2Notification", message, () =>
            {
                var room = message.Data;
                Debug.Log(GameUtils.GetRoomLogData(room));
            });
        }

        private void CommonProcess(string funName, Message message, Action action)
        {
            Debug.Log($"message.Type: {message.Type}");
            if (!message.IsError)
            {
                Debug.Log($"{funName} no error");
            }
            else
            {
                var error = message.GetError();
                Debug.Log($"{funName} error: {error.Message}");
            }

            action();
        }

        private void ProcessCreatePrivate(Message<Room> message)
        {
            CommonProcess("ProcessCreatePrivate", message, () =>
            {
                if (message.IsError)
                {
                    CallOnStatusChange(GameState.Error, "create private room failed");
                    return;
                }

                RoomData = message.Data;
                RoomID = RoomData.RoomId;
                if (RoomID == 0)
                {
                    Debug.LogError("unexpected RoomID 0");
                    CallOnStatusChange(GameState.Error, "unexpected RoomID 0");
                    return;
                }

                OnRoomJoined(RoomData);
            });
        }
    }
}