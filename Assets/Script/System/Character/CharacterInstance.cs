using System;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    public const int MaxLevel = 3;

    [SerializeField] private string instanceId;
    [SerializeField] private string blueprintId;
    [SerializeField, Range(0, MaxLevel)] private int level = 0;

    [NonSerialized] private CharacterBlueprintDatabase blueprintDatabase;

    public string InstanceId => instanceId;
    public string BlueprintId => blueprintId;
    public int Level => Mathf.Clamp(level, 0, MaxLevel);
    public bool IsMaxLevel => Level >= MaxLevel;
    public CharacterBlueprint Blueprint => GetBlueprint();

    public CharacterInstance()
    {
    }

    public CharacterInstance(string blueprintId, int level = 0, CharacterBlueprintDatabase blueprintDatabase = null)
    {
        instanceId = Guid.NewGuid().ToString("N");
        this.blueprintId = blueprintId;
        this.level = Mathf.Clamp(level, 0, MaxLevel);
        AssignBlueprintDatabase(blueprintDatabase);
    }

    public void AssignBlueprintDatabase(CharacterBlueprintDatabase database)
    {
        blueprintDatabase = database;
    }

    public float GetMaxHP()
    {
        var bp = GetBlueprint();
        if (bp == null) return 0f;
        return bp.baseHP + bp.hpPerLevel * Level;
    }

    public float GetAttack()
    {
        var bp = GetBlueprint();
        if (bp == null) return 0f;
        return bp.baseAttack + bp.attackPerLevel * Level;
    }

    public float GetAttackSpeed()
    {
        var bp = GetBlueprint();
        if (bp == null) return 1f;
        return bp.baseAttackSpeed + bp.attackSpeedPerLevel * Level;
    }

    public bool TryLevelUp()
    {
        if (level >= MaxLevel) return false;
        level = Mathf.Clamp(level + 1, 0, MaxLevel);
        return true;
    }

    public CharacterBlueprint GetBlueprint()
    {
        if (blueprintDatabase == null || string.IsNullOrEmpty(blueprintId)) return null;
        return blueprintDatabase.GetByID(blueprintId);
    }
}
