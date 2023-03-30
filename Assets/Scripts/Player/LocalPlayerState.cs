using System;
using Pico.Platform;
using Pico.Platform.Models;
using SpaceArenaParty.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpaceArenaParty.Player
{
    public class LocalPlayerState : MonoBehaviour
    {
        public static LocalPlayerState Instance;

        [HideInInspector] public Color color;

        [HideInInspector] public string username;

        [HideInInspector] public string id;

        [HideInInspector] public string lobbyID;

        public User picoUser;

        public event Action OnChange;

        private void Awake()
        {
            Debug.Assert(Instance == null, $"LocalPlayerState has been instantiated more than once.");
            Instance = this;
            lobbyID = GenerateUniqID();
        }

        private void OnEnable()
        {
            DontDestroyOnLoad(this);
        }


        public void Init(Message<User> message)
        {
            Debug.Log($"Init using {message.Data.ID}");
            color = Random.ColorHSV();
            id = message.Data.ID;
            username = message.Data.DisplayName;
            picoUser = message.Data;
            OnChange?.Invoke();
        }

        private string GenerateUniqID()
        {
            var id = (uint)(Random.value * uint.MaxValue);
            return id.ToString("X").ToLower();
        }
    }
}