using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBlueprint", menuName = "Characters/Blueprint", order = 0)]
public class CharacterBlueprint : ScriptableObject
{
    [Header("Character Info")]
    public Sprite icon;
    public string characterName;
    public string blueprintID;
}
