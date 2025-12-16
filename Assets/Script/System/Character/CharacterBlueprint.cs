using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBlueprint", menuName = "Characters/Blueprint", order = 0)]
public class CharacterBlueprint : ScriptableObject
{
    [Header("Character Info")]
    public Sprite icon;
    public string characterName;
    public string blueprintID;

    [Header("Battle")]
    public GameObject prefab;

    [Header("基礎ステータス（Lv0）")]
    public float baseHP = 100f;
    public float baseAttack = 10f;
    public float baseAttackSpeed = 1f;

    [Header("レベル毎の上昇量")]
    public float hpPerLevel = 20f;
    public float attackPerLevel = 2f;
    public float attackSpeedPerLevel = 0.1f;
}
