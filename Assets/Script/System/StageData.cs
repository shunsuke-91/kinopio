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

    [Header("敵プレハブ")]
    public GameObject[] enemyPrefabs;      // 敵の種類
    public float[] spawnDelays;            // 出現タイミング（秒）

    [Header("Player 出撃設定")]
    public GameObject[] playerPrefabs;               // 出撃可能ユニット


    [Header("OneWay 出現範囲（A方式）")]
    public float enemyX = 10f;      // 敵の固定X（右側）
    public float playerX = -10f;    // プレイヤーの固定X（左側）
    public float minY = -5f;        // ランダムY最小
    public float maxY = 5f;         // ランダムY最大

    [Header("BothSides / FreeField 用")]
    public Vector2[] enemySpawnPositions;
    public Vector2[] playerSpawnPositions;
 


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