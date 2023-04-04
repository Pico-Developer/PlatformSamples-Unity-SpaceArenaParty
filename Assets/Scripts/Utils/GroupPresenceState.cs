using System.Collections;
using Pico.Platform;
using UnityEngine;

namespace SpaceArenaParty.Utils
{
    public class GroupPresenceState
    {
        public IEnumerator Set(string dest, string lobbyID, string matchID, string extra, bool joinable)
        {
            var groupPresenceOptions = new PresenceOptions();
            groupPresenceOptions.SetDestinationApiName(dest);
            groupPresenceOptions.SetLobbySessionId(lobbyID);
            groupPresenceOptions.SetMatchSessionId(matchID);
            groupPresenceOptions.SetIsJoinable(joinable);
            groupPresenceOptions.SetExtra(extra);

            var setCompleted = false;
            PresenceService.Set(groupPresenceOptions).OnComplete(message =>
            {
                if (message.IsError)
                    Debug.Log($"Failed to setup Group Presence {message.GetError()}");
                else
                    Debug.Log("Group Presence set successfully");

                setCompleted = true;
            });

            yield return new WaitUntil(() => setCompleted);
        }
    }
}