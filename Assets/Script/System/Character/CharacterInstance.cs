using System;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    public const int MaxLevel = 3;

    [SerializeField] private string instanceID;          // 個体ID（同キャラ複数所持の識別用）
    [SerializeField] private CharacterBlueprint blueprint;

    [Header("Upgrade (Instance)")]
    [SerializeField] private int level = 0;              // ★ Lv0開始（MAX=3）

    // 強化の伸び率（必要なら後で調整）
    [SerializeField] private float hpPerLevel = 0.10f;        // 1レベルごとに +10%
    [SerializeField] private float attackPerLevel = 0.10f;    // 1レベルごとに +10%
    [SerializeField] private float speedPerLevel = 0.03f;     // 1レベルごとに +3%

    public string InstanceID => instanceID;
    public CharacterBlueprint Blueprint => blueprint;
    public int Level => level;
    public bool IsMaxLevel => level >= MaxLevel;

    public CharacterInstance(CharacterBlueprint characterBlueprint)
    {
        instanceID = Guid.NewGuid().ToString("N");
        blueprint = characterBlueprint;
        level = 0; // ★ Lv0開始
    }

    // ===== 最終ステータス（計算結果） =====
    public float CurrentHP
    {
        get
        {
            if (blueprint == null) return 0f;
            float rate = 1f + level * hpPerLevel; // ★ Lv0でも1.0
            return blueprint.baseHP * rate;
        }
    }

    public float CurrentAttack
    {
        get
        {
            if (blueprint == null) return 0f;
            float rate = 1f + level * attackPerLevel;
            return blueprint.baseAttack * rate;
        }
    }

    public float CurrentAttackSpeed
    {
        get
        {
            if (blueprint == null) return 1f;
            float rate = 1f + level * speedPerLevel;
            return blueprint.baseAttackSpeed * rate;
        }
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