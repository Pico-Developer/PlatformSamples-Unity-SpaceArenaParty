using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceArenaParty.ApplicationLifeCycle
{
    public class ApplicationSceneManager : MonoBehaviour
    {
        public enum Scenes
        {
            None,
            Lobby,
            BlueRoom,
            RedRoom
        }

        public static Scenes currentScene;

        [HideInInspector] public bool sceneLoaded;

        public Dictionary<string, Scenes> scenes { get; private set; }

        private void Start()
        {
            scenes = new Dictionary<string, Scenes>
            {
                { "Lobby", Scenes.Lobby },
                { "BlueRoom", Scenes.BlueRoom },
                { "RedRoom", Scenes.RedRoom }
            };
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentScene = (Scenes)scene.buildIndex;
            sceneLoaded = true;
        }


        public void LoadScene(Scenes scene)
        {
            if (scene == currentScene) return;

            sceneLoaded = false;
            SceneManager.LoadSceneAsync((int)scene);
        }
    }
}