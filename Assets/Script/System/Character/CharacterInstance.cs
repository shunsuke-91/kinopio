using System;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    public const int MaxLevel = 3;

    [SerializeField] private string instanceID;          // 個体ID（同キャラ複数所持の識別用）
    [SerializeField] private CharacterBlueprint blueprint;

    [Header("Upgrade (Instance)")]
    [SerializeField, Range(0, 3)] private int level = 0; // ★ Lv0開始（MAX=3）

    public string InstanceID => instanceID;
    public CharacterBlueprint Blueprint => blueprint;
    public int Level => level;
    public bool IsMaxLevel => level >= MaxLevel;

    public CharacterInstance(CharacterBlueprint characterBlueprint, int level = 0)
    {
        instanceID = Guid.NewGuid().ToString("N");
        blueprint = characterBlueprint;
        this.level = Mathf.Clamp(level, 0, MaxLevel); // ★ Lv0開始
    }

    // ===== 最終ステータス（計算結果） =====
    public float GetMaxHP()
    {
        if (blueprint == null) return 0f;
        float baseValue = blueprint.baseHP;
        float growth = blueprint.hpPerLevel;
        return baseValue + growth * level;
    }

    public float GetAttack()
    {
        if (blueprint == null) return 0f;
        float baseValue = blueprint.baseAttack;
        float growth = blueprint.attackPerLevel;
        return baseValue + growth * level;
    }

    public float GetAttackSpeed()
    {
        if (blueprint == null) return 1f;
        float baseValue = blueprint.baseAttackSpeed;
        float growth = blueprint.attackSpeedPerLevel;
        return baseValue + growth * level;
    }

    /// <summary>
    /// レベルを1上げます（MAX=3）。成功したら true を返します。
    /// </summary>
    public bool LevelUp()
    {
        if (level >= MaxLevel) return false;
        level++;
        return true;
    }
}