using System;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    [SerializeField] private CharacterBlueprint blueprint;
    [SerializeField, Range(0, 3)] private int level;

    public CharacterBlueprint Blueprint => blueprint;
    public int Level => level;

    public CharacterInstance(CharacterBlueprint characterBlueprint, int level = 0)
    {
        blueprint = characterBlueprint;
        this.level = Mathf.Clamp(level, 0, 3);
    }

    public float GetMaxHP()
    {
        if (blueprint == null) return 0f;
        return blueprint.baseHP + blueprint.hpPerLevel * level;
    }

    public float GetAttack()
    {
        if (blueprint == null) return 0f;
        return blueprint.baseAttack + blueprint.attackPerLevel * level;
    }

    public float GetAttackSpeed()
    {
        if (blueprint == null) return 1f;
        return blueprint.baseAttackSpeed + blueprint.attackSpeedPerLevel * level;
    }
}
