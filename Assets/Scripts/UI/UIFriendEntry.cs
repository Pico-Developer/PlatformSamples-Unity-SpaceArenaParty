using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.UI.Base;
using TMPro;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIFriendEntry : UIUserEntry
    {
        public TMP_Text userPresenceText;
        public Button joinButton;
        private UILaunchPadManager _launchPadManager;
        private Room _room;
        private User _user;

        private void Start()
        {
            _launchPadManager = FindObjectOfType<UILaunchPadManager>();
        }

        public new void Init(User user)
        {
            _user = user;
            base.Init(user);
            joinButton.gameObject.SetActive(false);
            if (_user == null) return;

            userNameText.text = _user.DisplayName;

            var userPresence = "";
            userPresence += _user.PresenceStatus == UserPresenceStatus.OnLine ? "Online" : "Offline";
            userPresence += "|";

            if (_user.PresenceDestinationApiName != "") userPresence += $"{_user.PresenceDestinationApiName}|";
            if (_user.PresenceMatchSessionId != "")
            {
                userPresence += $"{_user.PresenceMatchSessionId}|";
                userPresenceText.text = _user.PresenceMatchSessionId;
                if (_launchPadManager && _launchPadManager.CurrentRoom != null &&
                    _launchPadManager.CurrentRoom.RoomId.ToString() != _user.PresenceMatchSessionId)
                {
                    joinButton.gameObject.SetActive(true);
                    joinButton.onClick.RemoveAllListeners();
                    joinButton.onClick.AddListener(() => { _launchPadManager.Join2RoomByUserPresence(user); });
                }
            }

            userPresenceText.SetText(userPresence);
        }
    }
}