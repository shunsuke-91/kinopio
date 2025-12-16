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

    [Header("Base Stats")]
    public float baseHP = 100f;
    public float baseAttack = 10f;
    public float baseAttackSpeed = 1.0f; // 例：Animator.speed に掛ける用（1が標準）
}