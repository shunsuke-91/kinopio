using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeCostDatabase", menuName = "Characters/UpgradeCostDatabase")]
public class UpgradeCostDatabase : ScriptableObject
{
    public UpgradeCostEntry[] entries;

    public MaterialCost[] GetCosts(string blueprintId, int nextLevel)
    {
        if (string.IsNullOrEmpty(blueprintId) || entries == null) return null;
        foreach (var entry in entries)
        {
            if (entry != null && entry.blueprintId == blueprintId && entry.nextLevel == nextLevel)
            {
                return entry.costs;
            }
        }
        return null;
    }
}

[System.Serializable]
public class UpgradeCostEntry
{
    public string blueprintId;
    public int nextLevel;
    public MaterialCost[] costs;
}
