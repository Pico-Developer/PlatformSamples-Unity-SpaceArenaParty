using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace SpaceArenaParty.Player
{
    public class PlayerAvatarState : NetworkBehaviour
    {
        public NetworkVariable<Color> color = new();
        public NetworkVariable<FixedString128Bytes> username = new();
        private AvatarColor playerColor;
        private AvatarName playerName;

        private LocalPlayerState LocalPlayerState => IsOwner ? LocalPlayerState.Instance : null;

        private void Start()
        {
            OnColorChanged(color.Value, color.Value);
            OnUsernameChanged(username.Value, username.Value);

            if (!LocalPlayerState) return;

            LocalPlayerState.OnChange += UpdateData;

            UpdateData();
        }

        private void OnEnable()
        {
            playerColor = GetComponent<AvatarColor>();
            playerName = GetComponent<AvatarName>();

            color.OnValueChanged += OnColorChanged;
            username.OnValueChanged += OnUsernameChanged;
        }

        private void OnDisable()
        {
            color.OnValueChanged -= OnColorChanged;
            username.OnValueChanged -= OnUsernameChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (!LocalPlayerState) return;

            LocalPlayerState.transform.position = transform.position;
            LocalPlayerState.transform.rotation = transform.rotation;

            LocalPlayerState.OnChange -= UpdateData;
        }

        private void OnColorChanged(Color oldColor, Color newColor)
        {
            playerColor.UpdateColor(newColor);
        }

        private void OnUsernameChanged(FixedString128Bytes oldName, FixedString128Bytes newName)
        {
            playerName.username.text = newName.ConvertToString();
        }

        public void OnChangeColor()
        {
            if (!LocalPlayerState) return;

            LocalPlayerState.color = Random.ColorHSV();
            SetColorServerRpc(LocalPlayerState.color);
        }

        private void UpdateData()
        {
            SetStateServerRpc(LocalPlayerState.color, LocalPlayerState.username);
        }

        [ServerRpc]
        private void SetStateServerRpc(Color color_, string username_)
        {
            color.Value = color_;
            username.Value = username_;
        }

        [ServerRpc]
        private void SetColorServerRpc(Color color_)
        {
            color.Value = color_;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (LocalPlayerState) SetStateServerRpc(LocalPlayerState.color, LocalPlayerState.username);
        }
    }
}