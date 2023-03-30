using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class LimitFrameRate : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 72;
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}