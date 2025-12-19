using UnityEngine;

[CreateAssetMenu(fileName = "CraftCostDatabase", menuName = "Characters/CraftCostDatabase")]
public class CraftCostDatabase : ScriptableObject
{
    public CraftCostEntry[] entries;

    public MaterialCost[] GetCosts(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId) || entries == null) return null;
        foreach (var entry in entries)
        {
            if (entry != null && entry.blueprintId == blueprintId)
            {
                return entry.costs;
            }
        }
        return null;
    }
}

[System.Serializable]
public class CraftCostEntry
{
    public string blueprintId;
    public MaterialCost[] costs;
}

[System.Serializable]
public class MaterialCost
{
    public string materialId;
    public int amount;
}
