using System;
using System.Collections.Generic;
using UnityEngine;

namespace Netcode.Transports.Pico
{
    /// <summary>
    ///     in addition to the room sync service, pico developer will definitely also need other pico platform function, as
    ///     well as the full control of the room join procedure.
    ///     that's why we have 'AlreadyInRoomProvider' here, which takes a passed in PicoRoomWrapper, instead of calling pico
    ///     service internally.
    /// </summary>
    public class AlreadyInRoomProvider : IMatchRoomProvider
    {
        private const string NOTICE =
            "AlreadyInRoomProvider should be used only if room join procedure is finished separately out of the pico transport";

        private IMatchRoomEventHandler _eventHandler;

        private readonly Status _status = new();

        public bool UninitProvider()
        {
            _status.PicoRoomWrapper.OnTransportShutdown();
            return true;
        }

        public bool Login(IMatchRoomEventHandler eventHandler, string host, ushort port, string accessToken,
            string appID, int regionID)
        {
            _eventHandler = eventHandler;
            _ = host;
            _ = port;
            _ = accessToken;
            _ = appID;
            _ = regionID;
            if (!IsInRoom())
            {
                Debug.LogError("PicoLibLog: Login:" + NOTICE);
                return false;
            }

            _eventHandler.OnLoginResult(true, _status.SelfUID);
            return true;
        }

        public bool HostCheck()
        {
            if (!IsInRoom())
            {
                Debug.LogError("PicoLibLog: HostCheck:" + NOTICE);
                return false;
            }

            var selfType = _status.SelfUID == _status.TransportRoomInfoOfUIDs.OwnerUID
                ? NetcodeMode.Host
                : NetcodeMode.Client;
            Debug.LogWarning(
                $"------, host or client: ownerUID:{_status.TransportRoomInfoOfUIDs.OwnerUID}, selfUID:{_status.SelfUID}, selfType:{selfType}");
            _eventHandler.OnHostCheckResult(true, selfType);
            return true;
        }

        public void UpdateProvider()
        {
            //PicoSDKWrapper.Singleton.UpdateMatchroomProvider();
        }

        public ulong GetSelfLoggedInUID()
        {
            return _status.SelfUID;
        }

        public bool MatchmakingCreateAndEnqueue(string poolName)
        {
            if (!IsInRoom())
            {
                Debug.LogError("PicoLibLog: MatchmakingCreateAndEnqueue:" + NOTICE);
                return false;
            }

            _eventHandler.OnMatchEnqueueResult(true);
            _eventHandler.OnMatchFoundResult(true, NetcodeMode.Host, _status.TransportRoomInfoOfUIDs.RoomID);
            _eventHandler.OnPlayerEnterRoom(_status.SelfUID, _status.TransportRoomInfoOfUIDs);
            return true;
        }

        public bool MatchmakingEnqueue(string poolName)
        {
            _ = poolName;
            if (!IsInRoom())
            {
                Debug.LogError("PicoLibLog: MatchmakingEnqueue:" + NOTICE);
                return false;
            }

            _eventHandler.OnMatchEnqueueResult(true);
            _eventHandler.OnMatchFoundResult(true, NetcodeMode.Client, _status.TransportRoomInfoOfUIDs.RoomID);
            return true;
        }

        public bool RoomJoin(ulong roomID)
        {
            _ = roomID;
            if (!IsInRoom())
            {
                Debug.LogError("PicoLibLog: RoomJoin:" + NOTICE);
                return false;
            }

            _eventHandler.OnPlayerEnterRoom(_status.SelfUID, _status.TransportRoomInfoOfUIDs);
            return true;
        }

        public bool RoomKickUserByID(ulong roomID, ulong clientID)
        {
            string clientOpenID;
            if (!_status.UID2OpenIDs.TryGetValue(clientID, out clientOpenID))
            {
                Debug.LogError($"PicoLibLog: RoomKickUserByID, {clientID} is not in this room");
                return false;
            }

            _status.PicoRoomWrapper.KickUser(roomID, clientOpenID);
            return true;
        }

        public bool RoomLeave(ulong roomID)
        {
            if (roomID != _status.TransportRoomInfoOfUIDs.RoomID)
            {
                Debug.LogError($"PicoLibLog: RoomLeave, current is not in room {roomID}, skip this leave request");
                return false;
            }

            Debug.Log("transport leaving room");
            _status.PicoRoomWrapper.LeaveRoom(roomID);
            return true;
        }

        public bool SendPacket2UID(ulong clientID, byte[] dataArray)
        {
            string tgtOpenID;
            if (!_status.UID2OpenIDs.TryGetValue(clientID, out tgtOpenID))
            {
                Debug.LogError(
                    $"PicoLibLog: SendPacket2UID, target({clientID}) is not in room, skip this send packet request");
                return false;
            }

            // Debug.Log($"send msg to: {tgtOpenID}, msg size: {dataArray.Length}");
            _status.PicoRoomWrapper.SendMsgToUID(tgtOpenID, dataArray);
            return true;
        }

        public void SetLogDelegate(DebugDelegate logDelegate)
        {
            //PicoSDKWrapper.Singleton.SetLogDelegate(logDelegate);
        }

        public void ResetLogDelegate()
        {
            //PicoSDKWrapper.Singleton.ResetLogDelegate();
        }

        public bool Init(PicoRoomWrapper picoRoomWrapper)
        {
            _status.PicoRoomWrapper = picoRoomWrapper;
            _status.ParseUIDInfo();
            return true;
        }

        public void HandleRoomInfoUpdate(PicoRoomInfo roomInfo)
        {
            _status.PicoRoomWrapper.PicoRoomInfo = roomInfo;
            _status.ParseUIDInfo();
            _eventHandler.OnRoomInfoUpdate(_status.TransportRoomInfoOfUIDs);
        }

        public void HandleMsgFromRoom(string senderOpenID, byte[] msg)
        {
            ulong senderUID;
            if (!_status.OpenID2UIDs.TryGetValue(senderOpenID, out senderUID))
            {
                Debug.LogError($"PicoLibLog: got msg from developer with unkown sender {senderOpenID}");
                return;
            }

            //Debug.Log($"got msg from: {senderOpenID}, msg size: {msg.Length}");
            var payload = new ArraySegment<byte>(msg, 0, msg.Length);
            _eventHandler.OnPkgRecved(senderUID, payload);
        }

        public bool IsInRoom()
        {
            return 0 != _status.SelfUID && 0 != _status.TransportRoomInfoOfUIDs.RoomID;
        }

        public class PicoRoomWrapper
        {
            public delegate int KickUserHandler(ulong roomID, string userOpenId);

            public delegate int LeaveRoomHandler(ulong roomID);

            public delegate int SendMsgToUIDHandler(string tgtOpenID, byte[] message);

            public delegate int TransportShutdownHandler();

            public KickUserHandler KickUser;
            public LeaveRoomHandler LeaveRoom;
            public TransportShutdownHandler OnTransportShutdown;
            public PicoRoomInfo PicoRoomInfo;
            public string SelfOpenID;
            public SendMsgToUIDHandler SendMsgToUID;
        }

        private class Status
        {
            public readonly Dictionary<string, ulong> OpenID2UIDs = new();
            public PicoRoomWrapper PicoRoomWrapper;
            public ulong SelfUID;
            public readonly TransportRoomInfo TransportRoomInfoOfUIDs = new();
            public readonly Dictionary<ulong, string> UID2OpenIDs = new();

            public void ParseUIDInfo()
            {
                TransportRoomInfoOfUIDs.RoomID = PicoRoomWrapper.PicoRoomInfo.RoomID;
                SelfUID = (ulong)PicoRoomWrapper.SelfOpenID.GetHashCode();
                Debug.Log($"PicoLibLog: selfUID {SelfUID} from selfOpenID {PicoRoomWrapper.SelfOpenID}");
                TransportRoomInfoOfUIDs.OwnerUID = (ulong)PicoRoomWrapper.PicoRoomInfo.OwnerOpenID.GetHashCode();
                TransportRoomInfoOfUIDs.CurRoomUIDs.Clear();
                OpenID2UIDs.Clear();
                UID2OpenIDs.Clear();
                Debug.Log("PicoLibLog: <uid, openid> mapping ...");
                foreach (var playerName in PicoRoomWrapper.PicoRoomInfo.CurRoomOpenIDs)
                {
                    var hashCode = (ulong)playerName.GetHashCode();
                    TransportRoomInfoOfUIDs.CurRoomUIDs.Add(hashCode);
                    OpenID2UIDs.Add(playerName, hashCode);
                    UID2OpenIDs.Add(hashCode, playerName);
                    Debug.Log($"PicoLibLog: uid({hashCode})<->openID({playerName})");
                }

                Debug.Log("PicoLibLog: ... uid openid mapping");
            }
        } //Status
    } //PicoMatchRoomProvider
}