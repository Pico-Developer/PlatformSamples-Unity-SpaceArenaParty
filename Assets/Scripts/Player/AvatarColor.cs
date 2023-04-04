using UnityEngine;

namespace SpaceArenaParty.Player
{
    public class AvatarColor : MonoBehaviour
    {
        [SerializeField] private Renderer meshRenderer;

        public void UpdateColor(Color color)
        {
            foreach (var mat in meshRenderer.materials) mat.color = color;
        }
    }
}