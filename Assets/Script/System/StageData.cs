using UnityEngine;

/// <summary>
/// ステージごとの設定データをまとめる ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    [Header("ステージの基本情報")]
    public string stageName = "ステージ名";
    [TextArea] public string description = "ステージの説明文（任意）";

    [Header("ステージ演出")]
    public Sprite background;        // 背景画像
    public AudioClip bgm;            // BGM

    [Header("バトルルール")]
    public StageRuleType ruleType = StageRuleType.OneWay;

    [Header("敵の出現情報")]
    public GameObject[] enemyPrefabs;      // 敵の種類
    public Vector2[] spawnPositions;       // 出現位置
    public float[] spawnDelays;            // 出現タイミング（秒）

    [Header("報酬など（任意）")]
    public int rewardGold = 100;
    public int rewardExp = 10;
}

/// <summary>
/// ステージのルールタイプを指定
/// </summary>
public enum StageRuleType
{
    OneWay,      // 一方向（にゃんこ形式）
    BothSides,   // 両側から進攻
    FreeField    // 盤面全体で進攻
}