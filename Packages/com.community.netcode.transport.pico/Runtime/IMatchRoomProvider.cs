using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Netcode.Transports.Pico
{
    public class PicoRoomInfo
    {
        public ulong RoomID;
        public string OwnerOpenID;
        public readonly HashSet<string> CurRoomOpenIDs = new HashSet<string>();
    }

    public class TransportRoomInfo
    {
        public ulong RoomID;
        public ulong OwnerUID;
        public HashSet<ulong> CurRoomUIDs = new HashSet<ulong>();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(int level, IntPtr strPtr);

    public enum NetcodeMode
    {
        Client,
        Host,
    };

    public interface IMatchRoomEventHandler
    {
        void OnLoginResult(bool result, ulong selfUID);
        void OnHostCheckResult(bool result, NetcodeMode peerType);
        void OnMatchEnqueueResult(bool result);
        void OnMatchFoundResult(bool result, NetcodeMode peerType, ulong roomID);
        void OnPlayerEnterRoom(ulong userID, TransportRoomInfo transportRoomInfo);
        void OnPlayerLeaveRoom(ulong userID, TransportRoomInfo transportRoomInfo);
        void OnRoomInfoUpdate(TransportRoomInfo transportRoomInfo);
        void OnPkgRecved(ulong senderUID, ArraySegment<byte> pkg);
        void OnMatchRoomProviderError(int errorCode);
    }

    public interface IMatchRoomProvider
    {
        bool Login(IMatchRoomEventHandler eventHandler, string host, ushort port, string fakeAccessToken, string appID, int regionID);
        bool HostCheck();

        bool UninitProvider();
        void UpdateProvider();
        ulong GetSelfLoggedInUID();
        bool MatchmakingCreateAndEnqueue(string poolName);
        bool MatchmakingEnqueue(string poolName);
        bool RoomJoin(ulong roomID);
        bool RoomKickUserByID(ulong roomID, ulong clientId);   
		bool RoomLeave(ulong roomID);
        bool SendPacket2UID(ulong clientId, byte[] dataArray);

        void SetLogDelegate(DebugDelegate logDelegate);
        void ResetLogDelegate();
    }
    
}
