using UnityEngine;

/// <summary>
/// ステージごとの設定データをまとめる ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    [Header("ステージの基本情報")]
    public string stageName = "ステージ名";

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


    [Header("難易度")]
    public DifficultyType difficulty = DifficultyType.Normal;

    [Header("難易度設定（Normal / Hard / Hell）")]
    public DifficultySettings normalSettings = new DifficultySettings
    {
        attackPowerMultiplier = 1f,
        attackSpeedMultiplier = 1f,
        hpMultiplier = 1f
    };

    public DifficultySettings hardSettings = new DifficultySettings
    {
        attackPowerMultiplier = 1.2f,
        attackSpeedMultiplier = 1.2f,
        hpMultiplier = 2f
    };

    public DifficultySettings hellSettings = new DifficultySettings
    {
        attackPowerMultiplier = 1.5f,
        attackSpeedMultiplier = 1.5f,
        hpMultiplier = 3f
    };

    // 現在選択されている難易度の設定を返す
    public DifficultySettings GetDifficultySettings()
    {
        switch (difficulty)
        {
            case DifficultyType.Hard:
                return hardSettings;
            case DifficultyType.Hell:
                return hellSettings;
            default:
                return normalSettings;
        }
    }
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

public enum DifficultyType
{
    Normal,
    Hard,
    Hell
}

[System.Serializable]
public class DifficultySettings
{
    [Header("攻撃力倍率（敵の攻撃力に掛ける）")]
    public float attackPowerMultiplier = 1f;

    [Header("攻撃スピード倍率（Animator.speed に使う）")]
    public float attackSpeedMultiplier = 1f;

    [Header("HP倍率（敵のHPに掛ける）")]
    public float hpMultiplier = 1f;
}
