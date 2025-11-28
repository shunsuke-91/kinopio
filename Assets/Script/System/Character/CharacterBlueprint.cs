using UnityEngine;

[CreateAssetMenu(menuName = "Game/CharacterBlueprint")]
public class CharacterBlueprint : ScriptableObject
{
    public string id;      // 固有ID
    public Sprite icon;    // 一覧に出すアイコン
    public GameObject prefab; // 戦闘で使うプレハブ
}