using System;
using System.Collections.Generic;
using System.Linq;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.Player;
using SpaceArenaParty.UI.Base;
using UnityEngine;

namespace SpaceArenaParty.UI
{
    public class UIRoomMemberEntriesPresenter : UIEntriesPresenter<User, UIRoomMemberEntry>
    {
        private readonly Dictionary<string, UserRelationType> _userRelationResults = new();
        private DateTime _lastUpdate;
        private bool _outdated;
        private LocalPlayerState LocalPlayerState => LocalPlayerState.Instance;

        private void Update()
        {
            var now = DateTime.Now;
            if (now - _lastUpdate > UIConfig.DataFetchInterval) _outdated = true;

            if (_outdated)
            {
                _outdated = false;
                UpdateUserRelations();
            }
        }


#pragma warning disable CS1998
        private async void UpdateUserRelations()
#pragma warning restore CS1998
        {
            var userIds = _entryComponentDictionary.Keys.ToArray();
            _lastUpdate = DateTime.Now;
            var userIdsExceptLocalPlayer = userIds.Where(x => x != LocalPlayerState.picoUser.ID).ToArray();
            if (userIdsExceptLocalPlayer.Length == 0)
                return;
            // At this point, the pc debug for GetUserRelations is not supported
#if !UNITY_EDITOR
        var message = await UserService.GetUserRelations(userIdsExceptLocalPlayer).Async();
        if (message.IsError)
        {
            Debug.LogError(message.Error);
            return;
        }

        var userRelationResults = message.Data;
        for (var i = 0; i < userIdsExceptLocalPlayer.Length; i++)
        {
            var userId = userIdsExceptLocalPlayer[i];
            var userRelation = userRelationResults[userId];
            _userRelationResults[userId] = userRelation;
            _entryComponentDictionary[userId]?.SetUserRelation(userRelation);
        }
#endif
        }

        protected override string ExtractEntryID(User entry)
        {
            return entry.ID;
        }

        protected override List<string> ExtractEntryIDs(List<User> newEntries)
        {
            return newEntries.Select(x => x.ID).ToList();
        }

        protected override void InitEntry(User entry, UIRoomMemberEntry entryComponent)
        {
            if (_userRelationResults.TryGetValue(entry.ID, out var userRelation))
                entryComponent.SetUserRelation(userRelation);
            entryComponent.Init(entry, userRelation);
            entryComponent.OnFriendRequestSendClick += OnFriendRequestSendClicked;
        }

        private async void OnFriendRequestSendClicked(User user)
        {
            var launchPadManager = FindObjectOfType<UILaunchPadManager>();
            try
            {
                await launchPadManager.SendFriendRequest(user);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            _outdated = true;
        }

        public override void SetEntries(List<User> newEntries)
        {
            base.SetEntries(newEntries);
            _outdated = true;
        }
    }
}