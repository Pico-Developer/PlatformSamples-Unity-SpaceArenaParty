using Pico.Platform;
using Pico.Platform.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIRoomEntry : MonoBehaviour
    {
        public TMP_Text roomName;
        public TMP_Text capacity;
        public Button joinButton;

        private Room _room;
        private Room _roomComplete;

        public void Init(Room room)
        {
            _room = room;
            if (_room.UsersOptional != null)
            {
                SetRoom(room);
            }
            else
            {
                roomName.text = "loading...";
                capacity.text = "";
            }
        }

        public void SetRoom(Room room)
        {
            if (room.RoomId != _room.RoomId) return;
            _roomComplete = room;
            roomName.text = Utils.Utils.GetRoomTitle(room);
            if (room.UsersOptional != null) capacity.text = $"{room.UsersOptional.Count}/{room.MaxUsers}";


            joinButton.onClick.RemoveAllListeners();
            // Debug.Log($"_userRoom.Room.RoomJoinability ${room.RoomJoinability}");
            if (room.RoomJoinability == RoomJoinability.CanJoin)
            {
                joinButton.gameObject.SetActive(true);
                joinButton.onClick.AddListener(() =>
                {
                    var launchPadManager = FindObjectOfType<UILaunchPadManager>();
                    launchPadManager.OnJoin2RoomClick(room);
                });
            }
            else
            {
                joinButton.gameObject.SetActive(false);
            }
        }
    }
}