using Pico.Platform.Models;
using TMPro;
using UnityEngine;

namespace SpaceArenaParty.UI.Base
{
    public class UIUserEntry : MonoBehaviour
    {
        public UIAvatarPresenter userAvatar;
        public TMP_Text userNameText;
        private User _user;

        protected void Init(User user)
        {
            _user = user;
            if (_user == null) return;
            userNameText.text = _user.DisplayName;
            userAvatar.Init(_user.ImageUrl);
        }
    }
}