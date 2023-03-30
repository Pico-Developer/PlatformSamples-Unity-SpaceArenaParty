using Unity.Netcode.Components;

namespace SpaceArenaParty.Player
{
    public class AvatarNetworkAnimatorOwner : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}