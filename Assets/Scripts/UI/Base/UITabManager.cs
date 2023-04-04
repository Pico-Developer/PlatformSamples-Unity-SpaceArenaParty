using UnityEngine;

namespace SpaceArenaParty.UI.Base
{
    public class UITabManager : MonoBehaviour
    {
        public enum Tabs
        {
            World = 0,
            Room = 1,
            Social = 2
        }

        public RectTransform tabsContentTransform;

        private Tabs _activeTab;
        private Vector3 tabsContentTargetPosition;

        private void Start()
        {
            UpdateTabsContentTransformTargetPosition();
        }

        private void Update()
        {
            tabsContentTransform.anchoredPosition3D = Vector3.Lerp(tabsContentTransform.anchoredPosition3D,
                tabsContentTargetPosition, 10 * Time.deltaTime);
        }

        private void UpdateTabsContentTransformTargetPosition()
        {
            var index = (int)_activeTab;
            tabsContentTargetPosition = new Vector3((float)-index * 1140, 0f, 0f);
        }

        public void OnTabSelect(Tabs tab)
        {
            _activeTab = tab;
            UpdateTabsContentTransformTargetPosition();
        }
    }
}