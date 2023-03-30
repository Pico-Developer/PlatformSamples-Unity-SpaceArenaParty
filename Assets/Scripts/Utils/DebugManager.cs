using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Management;

namespace SpaceArenaParty.Utils
{
    public class DebugManager : MonoBehaviour
    {
        public XRDeviceSimulator simulator;


        private void Start()
        {
#if UNITY_EDITOR
            if (XRGeneralSettings.Instance.AssignedSettings.activeLoaders.Count == 0)
                simulator.gameObject.SetActive(true);
#endif
        }
    }
}