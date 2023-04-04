using System.Collections;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class FPSCounter : MonoBehaviour
    {
        private float count;

        private IEnumerator Start()
        {
            DontDestroyOnLoad(this);
            GUI.depth = 2;
            while (true)
            {
                count = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(count));
        }
    }
}