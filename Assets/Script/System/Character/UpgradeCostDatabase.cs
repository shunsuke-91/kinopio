using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UpgradeCostDatabase", menuName = "Characters/UpgradeCostDatabase")]
public class UpgradeCostDatabase : ScriptableObject
{
    public UpgradeCostEntry[] entries;

    /// <summary>
    /// 指定キャラ・次レベルに必要な素材コストを取得
    /// </summary>
    public List<MaterialStack> GetCosts(string blueprintId, int nextLevel)
    {
        if (string.IsNullOrEmpty(blueprintId) || entries == null)
            return new List<MaterialStack>();

        foreach (var entry in entries)
        {
            if (entry == null) continue;
            if (entry.blueprintId == blueprintId && entry.nextLevel == nextLevel)
            {
                return entry.costs ?? new List<MaterialStack>();
            }
        }

        return new List<MaterialStack>();
    }
}

[System.Serializable]
public class UpgradeCostEntry
{
    public string blueprintId;
    public int nextLevel;

    // ★ MaterialCost → MaterialStack に統一
    public List<MaterialStack> costs = new List<MaterialStack>();
}