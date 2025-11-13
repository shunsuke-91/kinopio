using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 戦闘の全体進行を管理する司令塔クラス
/// ・StageData の読み込み
/// ・背景設定、BGM再生
/// ・敵スポーン（ルール別）
/// ・バトル開始／終了の管理
/// 
/// ※敵の移動／攻撃／AI は EnemyController など別スクリプトが担当
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("ステージデータ")]
    [SerializeField] private StageData currentStage;  // ← インスペクタでStageData.assetを設定

    [Header("UI / 演出関連")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AudioSource bgmSource;

    // 必要ならUI（Wave表示など）をここに追加  
    // [SerializeField] private Text waveText;


    // ==========================================
    //  Unityの基本イベント
    // ==========================================

    private void Awake()
    {
        // StageSelect から渡されたステージを受け取る
        if (StageLoader.selectedStage != null)
            currentStage = StageLoader.selectedStage;
    }
    private void Start()
    {
        LoadStageSettings();
        StartCoroutine(SpawnEnemies()); // 敵出現開始
        // TODO: バトル開始演出（フェードインなど）が必要ならここに書く
    }


    // ==========================================
    //  ステージ設定の読み込み
    // ==========================================
    private void LoadStageSettings()
    {
        if (currentStage == null)
        {
            Debug.LogError("StageData が設定されていません！");
            return;
        }

        // 背景
        if (backgroundImage != null && currentStage.background != null)
            backgroundImage.sprite = currentStage.background;

        // BGM
        if (bgmSource != null && currentStage.bgm != null)
        {
            bgmSource.clip = currentStage.bgm;
            bgmSource.Play();
        }

        // ルール確認ログ
        Debug.Log($"ステージ開始：{currentStage.stageName} / ルール：{currentStage.ruleType}");
    }


    // ==========================================
    //  敵出現処理（ステージのルールごとに切り替え）
    // ==========================================
    private IEnumerator SpawnEnemies()
    {
        // ============================
        // ① OneWay（横スクロール式）
        // ============================
if (currentStage.ruleType == StageRuleType.OneWay)
{
    while (true) // ← ★無限ループで常に湧かせる
    {
        // 待ち時間（5秒）
        yield return new WaitForSeconds(5f);

        // 出す敵をランダムに選択（既存の enemyPrefabs を利用）
        int index = Random.Range(0, currentStage.enemyPrefabs.Length);

        // 出現位置（今は StageData の spawnPositions をそのまま利用）
        Vector2 spawnPos = currentStage.spawnPositions[
            index % currentStage.spawnPositions.Length
        ];

        var enemyObj = Instantiate(
            currentStage.enemyPrefabs[index],
            spawnPos,
            Quaternion.identity
        );

        // 初期化（既存の処理）
        enemyObj.GetComponent<EnemyController>()
                .Initialize(currentStage.ruleType);
    }
}

        // ============================
        // ② BothSides（左右交互に出現）
        // ============================
        else if (currentStage.ruleType == StageRuleType.BothSides)
        {
            for (int i = 0; i < currentStage.enemyPrefabs.Length; i++)
            {
                yield return new WaitForSeconds(currentStage.spawnDelays[i]);

                Vector2 spawnPos = (i % 2 == 0)
                    ? new Vector2(8, 0)     // 右側
                    : new Vector2(-8, 0);   // 左側

                Instantiate(currentStage.enemyPrefabs[i], spawnPos, Quaternion.identity);
            }
        }

        // ============================
        // ③ FreeField（複数地点からランダム）
        // ============================
        else if (currentStage.ruleType == StageRuleType.FreeField)
        {
            for (int i = 0; i < currentStage.enemyPrefabs.Length; i++)
            {
                yield return new WaitForSeconds(currentStage.spawnDelays[i]);

                Vector2 spawnPos = currentStage.spawnPositions[
                    Random.Range(0, currentStage.spawnPositions.Length)
                ];

                Instantiate(currentStage.enemyPrefabs[i], spawnPos, Quaternion.identity);
            }
        }

        // ===================================
        // TODO: ここで "敵出現終了" を通知
        // Wave制を作りたい場合はここから続ける
        // ===================================
    }


    // ==========================================
    //  バトル終了管理（クリア／敗北）
    // ==========================================
    // ※まだ仕様が固まってないのでコメントだけ
    // ==========================================
    private void CheckBattleEnd()
    {
        // TODO: 敵の残数をカウントしてクリア判定
        // TODO: プレイヤーHPが0になったら敗北
        // TODO: リザルト画面へ遷移
    }


    // ==========================================
    //  Debug用の強制終了
    // ==========================================
    public void ForceClear()
    {
        // TODO: デバッグ目的でステージクリアさせる
    }

    public void ForceGameOver()
    {
        // TODO: デバッグ目的でゲームオーバー扱いにする
    }
}