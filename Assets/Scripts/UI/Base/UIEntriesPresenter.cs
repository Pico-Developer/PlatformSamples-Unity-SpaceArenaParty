using System.Collections.Generic;
using UnityEngine;

namespace SpaceArenaParty.UI.Base
{
    public class UIEntriesPresenter<TEntryType, TEntryComponentType> : MonoBehaviour
        where TEntryComponentType : MonoBehaviour
    {
        public TEntryComponentType EntryPrefab;
        protected readonly List<TEntryType> _entries = new();
        protected readonly Dictionary<string, TEntryComponentType> _entryComponentDictionary = new();

        private void Start()
        {
            RemoveAllEntries();
        }

        protected void RemoveAllEntries()
        {
            for (var i = 0; i < transform.childCount; ++i) Destroy(transform.GetChild(i).gameObject);
        }

        protected virtual string ExtractEntryID(TEntryType entry)
        {
            return "";
        }

        protected virtual List<string> ExtractEntryIDs(List<TEntryType> newEntries)
        {
            return new List<string>();
        }

        protected virtual void InitEntry(TEntryType entry, TEntryComponentType entryComponent)
        {
        }

        protected void RemoveUnusedEntries(List<TEntryType> newEntries)
        {
            var newKeys = ExtractEntryIDs(newEntries);

            for (var i = 0; i < _entries.Count; i++)
            {
                var key = ExtractEntryID(_entries[i]);
                if (newKeys.Contains(key) == false)
                {
                    Destroy(_entryComponentDictionary[key].gameObject);
                    _entryComponentDictionary.Remove(key);
                }
            }
        }


        public virtual void SetEntries(List<TEntryType> newEntries)
        {
            RemoveUnusedEntries(newEntries);
            _entries.Clear();

            for (var i = 0; i < newEntries.Count; i++)
            {
                var entry = newEntries[i];
                _entries.Add(entry);

                TEntryComponentType entryGameObject;
                if (_entryComponentDictionary.TryGetValue(ExtractEntryID(entry), out entryGameObject))
                {
                    entryGameObject.gameObject.transform.SetSiblingIndex(i);
                    InitEntry(entry, entryGameObject);
                }
                else
                {
                    entryGameObject = Instantiate(EntryPrefab, transform);
                    _entryComponentDictionary.Add(ExtractEntryID(entry), entryGameObject);
                    entryGameObject.gameObject.transform.SetSiblingIndex(i);
                    InitEntry(entry, entryGameObject);
                }
            }
        }
    }
}