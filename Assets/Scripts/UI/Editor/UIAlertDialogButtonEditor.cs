using SpaceArenaParty.UI.Base;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(UIAlertDialogButton))]
    public class MyButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            var targetMyButton = (UIAlertDialogButton)target;

            targetMyButton.hoverSprite =
                (Sprite)EditorGUILayout.ObjectField("Hover Sprite:", targetMyButton.hoverSprite, typeof(Sprite), true);
            targetMyButton.hoverTextColor =
                EditorGUILayout.ColorField("Hover Text Color:", targetMyButton.hoverTextColor);
            targetMyButton.normalTextColor =
                EditorGUILayout.ColorField("Normal Text Color:", targetMyButton.normalTextColor);

            base.OnInspectorGUI();
        }
    }
}