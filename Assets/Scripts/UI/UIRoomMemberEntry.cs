using System;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.UI.Base;
using TMPro;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIRoomMemberEntry : UIUserEntry
    {
        public TMP_Text userRelationStatusText;
        public Button friendRequestButton;
        private Room _room;
        private User _user;
        public Action<User> OnFriendRequestSendClick;
        private UserRelationType userRelationResult;

        private void Start()
        {
            friendRequestButton.gameObject.SetActive(false);
        }

        public void Init(User user, UserRelationType userRelation)
        {
            _user = user;
            base.Init(user);
            if (_user == null) return;
            SetUserRelation(userRelation);
            userNameText.text = _user.DisplayName;
            userAvatar.Init(_user.ImageUrl);
        }

        public void SetUserRelation(UserRelationType relation)
        {
            if (relation == UserRelationType.NotFriend)
            {
                friendRequestButton.gameObject.SetActive(true);
                friendRequestButton.onClick.RemoveAllListeners();
                friendRequestButton.onClick.AddListener(() => { OnFriendRequestSendClick?.Invoke(_user); });
            }
            else
            {
                friendRequestButton.gameObject.SetActive(false);
            }

            userRelationResult = relation;
            userRelationStatusText.text = relation.ToString();
        }
    }
}