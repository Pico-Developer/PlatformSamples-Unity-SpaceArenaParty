using Pico.Platform.Models;
using SpaceArenaParty.Player;

namespace SpaceArenaParty.Utils
{
    public class Utils
    {
        private static LocalPlayerState LocalPlayerState => LocalPlayerState.Instance;

        public static string GenerateRoomTitle(User user, string sceneName)
        {
            return $"{user.DisplayName}'s {sceneName}";
        }

        public static string GenerateRoomTitle(string sceneName)
        {
            return $"{LocalPlayerState.picoUser.DisplayName}'s {sceneName}";
        }

        public static string GetRoomTitle(Room room)
        {
            if (room.DataStore.TryGetValue("title", out var title))
                return title;

            return "Untitled Room";
        }
    }
}