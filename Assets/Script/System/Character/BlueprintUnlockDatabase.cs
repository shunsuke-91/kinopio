using UnityEngine;

[CreateAssetMenu(menuName = "Characters/BlueprintUnlockDatabase")]
public class BlueprintUnlockDatabase : ScriptableObject
{
    public BlueprintUnlockEntry[] entries;

    public bool IsUnlocked(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId) || entries == null) return false;
        foreach (var entry in entries)
        {
            if (entry != null && entry.blueprintID == blueprintId)
            {
                return entry.unlocked;
            }
        }
        return false;
    }
}

[System.Serializable]
public class BlueprintUnlockEntry
{
    public string blueprintID;
    public bool unlocked;
}
