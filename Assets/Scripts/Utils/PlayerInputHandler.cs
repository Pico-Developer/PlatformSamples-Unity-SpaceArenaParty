using SpaceArenaParty.Player;
using SpaceArenaParty.UI;
using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public void OnChangeColor()
        {
            var player = NetworkManager.Singleton.LocalClient.PlayerObject;
            var playerAvatarState = player.GetComponent<PlayerAvatarState>();
            playerAvatarState.OnChangeColor();
        }

        public void OnToggleLaunchPad()
        {
            var launchpadLocator = FindObjectOfType<UILaunchPadLocator>();
            launchpadLocator.OnToggleLaunchPad();
        }
    }
}