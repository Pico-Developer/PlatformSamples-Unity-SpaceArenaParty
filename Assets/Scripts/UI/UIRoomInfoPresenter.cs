using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceArenaParty.UI
{
    public class UIRoomInfoPresenter : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text description;

        public Button configButton;

        private LocalPlayerState LocalPlayerState => LocalPlayerState.Instance;

        public void Init(Room room)
        {
            title.SetText(Utils.Utils.GetRoomTitle(room));

            var descriptionText = "";
            descriptionText += room.RoomJoinPolicy == RoomJoinPolicy.Everyone ? "Public" : "Private";

            if (room.DataStore.TryGetValue("scene", out var scene))
            {
                descriptionText += " | ";
                descriptionText += scene;
            }

            description.SetText(descriptionText);

            configButton.onClick.RemoveAllListeners();
            configButton.onClick.AddListener(() =>
            {
                var launchPadManager = FindObjectOfType<UILaunchPadManager>();
                launchPadManager.OnUpdateRoomSettingClick(room);
            });

            if (room.OwnerOptional != null && LocalPlayerState.picoUser.ID == room.OwnerOptional.ID)
                configButton.gameObject.SetActive(true);
            else
                configButton.gameObject.SetActive(false);
        }
    }
}