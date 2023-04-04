using System.Collections.Generic;
using System.Linq;
using Pico.Platform.Models;
using SpaceArenaParty.UI.Base;

namespace SpaceArenaParty.UI
{
    public class UIFriendEntriesPresenter : UIEntriesPresenter<User, UIFriendEntry>
    {
        protected override string ExtractEntryID(User entry)
        {
            return entry.ID;
        }

        protected override List<string> ExtractEntryIDs(List<User> newEntries)
        {
            return newEntries.Select(x => x.ID).ToList();
        }

        protected override void InitEntry(User entry, UIFriendEntry entryComponent)
        {
            entryComponent.Init(entry);
        }
    }
}