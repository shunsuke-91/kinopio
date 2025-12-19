using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlueprintUnlockDatabase", menuName = "Characters/BlueprintUnlockDatabase", order = 2)]
public class BlueprintUnlockDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string blueprintID;
        public int unlockStage;
        public List<MaterialStack> craftCosts = new List<MaterialStack>();
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    public int GetUnlockStage(string blueprintID)
    {
        if (string.IsNullOrEmpty(blueprintID)) return 9999;
        if (entries == null) return 9999;

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.blueprintID)) continue;
            if (entry.blueprintID == blueprintID) return entry.unlockStage;
        }

        return 9999;
    }

    public List<MaterialStack> GetCraftCosts(string blueprintID)
    {
        if (string.IsNullOrEmpty(blueprintID)) return new List<MaterialStack>();
        if (entries == null) return new List<MaterialStack>();

        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.blueprintID)) continue;
            if (entry.blueprintID != blueprintID) continue;
            return entry.craftCosts ?? new List<MaterialStack>();
        }

        return new List<MaterialStack>();
    }
}
