using System;
using System.Collections.Generic;
using System.Linq;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.UI.Base;
using UnityEngine;

namespace SpaceArenaParty.UI
{
    public class UIRoomEntriesPresenter : UIEntriesPresenter<Room, UIRoomEntry>
    {
        private readonly Dictionary<string, Room> _completeRooms = new();
        private DateTime _lastUpdate;
        private bool _outdated;

        private void Update()
        {
            var now = DateTime.Now;
            if (now - _lastUpdate > UIConfig.DataFetchInterval) _outdated = true;

            if (_outdated)
            {
                _outdated = false;
                UpdateRooms();
            }
        }

        protected override string ExtractEntryID(Room entry)
        {
            return entry.RoomId.ToString();
        }

        protected override List<string> ExtractEntryIDs(List<Room> newEntries)
        {
            return newEntries.Select(x => x.RoomId.ToString()).ToList();
        }

        protected override void InitEntry(Room entry, UIRoomEntry entryComponent)
        {
            entryComponent.Init(entry);
        }

        private void UpdateRooms()
        {
            _lastUpdate = DateTime.Now;
            foreach (var roomId in _entryComponentDictionary.Keys)
                RoomService.Get(ulong.Parse(roomId)).OnComplete(message =>
                {
                    // Room may have been deleted
                    if (message.IsError)
                    {
                        Debug.LogWarning($"Failed to get room. {roomId},  Room may have been deleted");
                        return;
                    }

                    _completeRooms[roomId] = message.Data;

                    UIRoomEntry roomEntry;
                    if (_entryComponentDictionary.TryGetValue(roomId, out roomEntry)) roomEntry.SetRoom(message.Data);
                });
        }

        public void SetEntries(List<UserRoom> userRooms)
        {
            var rooms = userRooms.Select(x => x.Room).ToList();
            List<Room> distinctRooms = new();
            foreach (var roomSimple in rooms)
            {
                if (distinctRooms.Any(x => x.RoomId == roomSimple.RoomId)) continue;
                Room roomComplete;
                if (_completeRooms.TryGetValue(roomSimple.RoomId.ToString(), out roomComplete))
                    distinctRooms.Add(roomComplete);
                else
                    distinctRooms.Add(roomSimple);
            }

            base.SetEntries(distinctRooms);
            _outdated = true;
        }
    }
}