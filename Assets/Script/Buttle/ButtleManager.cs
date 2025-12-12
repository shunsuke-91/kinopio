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
    [SerializeField] private StageData currentStage;  

    [Header("UI / 演出関連")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AudioSource bgmSource;

    // ★ Base 実体（StageData から生成したものを保持）
    private BaseController playerBase;
    private BaseController enemyBase;

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
        if (currentStage == null)
        {
            Debug.LogError("BattleManager: currentStage が設定されていません！");
            return;
        }

        // 選んだ難易度をステージデータへ反映
        currentStage.difficulty = StageLoader.selectedDifficulty;
        
        LoadStageSettings();
        SpawnBases();          // 拠点の生成
        SpawnPlayersFromTeam(); // ★追加：編成キャラの出撃
        StartCoroutine(SpawnEnemies()); // 敵出現開始
        // TODO: バトル開始演出（フェードインなど）が必要ならここに書く
    }


    // ==========================================
    //  ステージ設定の読み込み
    // ==========================================
    private void LoadStageSettings()
    {
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
            while (true)
            {
                yield return new WaitForSeconds(5f);

                float y = Random.Range(currentStage.minY, currentStage.maxY);
                Vector2 spawnPos = new Vector2(currentStage.enemyX, y);

                int index = Random.Range(0, currentStage.enemyPrefabs.Length);

                GameObject enemyObj = Instantiate(
                    currentStage.enemyPrefabs[index],
                    spawnPos,
                    Quaternion.identity
                );

                // ステージの難易度設定を取得
                DifficultySettings diff = currentStage.GetDifficultySettings();

                // EnemyController を取得して難易度を渡す
                var ec = enemyObj.GetComponent<EnemyController>();
                if (ec != null)
                {
                    ec.Initialize(currentStage.ruleType, diff);
                }
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

                Vector2 spawnPos = currentStage.enemySpawnPositions[
                    Random.Range(0, currentStage.enemySpawnPositions.Length)
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
    //  プレイヤーおよびエネミーベースの生成
    // ==========================================

    private void SpawnBases()
    {
        DifficultySettings diff = currentStage.GetDifficultySettings();

        // PlayerBase
        if (currentStage.playerBasePrefab != null)
        {
            var pb = Instantiate(
                currentStage.playerBasePrefab,
                currentStage.playerBasePosition,
                Quaternion.identity
            );
            playerBase = pb.GetComponent<BaseController>();
            if (playerBase != null)
            {
                // Base の HP はステージ固有HP × 難易度倍率
                float hp = currentStage.playerBaseHP * diff.hpMultiplier;
                playerBase.Initialize(hp);
                playerBase.OnBaseDestroyed += OnPlayerBaseDestroyed;
            }
            else
            {
                Debug.LogError("BattleManager: PlayerBasePrefab に BaseController がついていません");
            }
        }

        // EnemyBase
        if (currentStage.enemyBasePrefab != null)
        {
            var eb = Instantiate(
                currentStage.enemyBasePrefab,
                currentStage.enemyBasePosition,
                Quaternion.identity
            );
            enemyBase = eb.GetComponent<BaseController>();
            if (enemyBase != null)
            {
                float hp = currentStage.enemyBaseHP * diff.hpMultiplier;
                enemyBase.Initialize(hp);
                enemyBase.OnBaseDestroyed += OnEnemyBaseDestroyed;
            }
            else
            {
                Debug.LogError("BattleManager: EnemyBasePrefab に BaseController がついていません");
            }
        }
        
        // UI に反映する
        var ui = FindFirstObjectByType<BaseHPUIController>();
        if (ui != null)
        {
            ui.Setup(playerBase, enemyBase);
        }
        else
        {
            Debug.LogError("BattleManager: BaseHPUIController がシーン内に見つかりません");
        }
    }

    // ==========================================
    //  編成されたプレイヤーユニットの出撃（★追加）
    // ==========================================
    private void SpawnPlayersFromTeam()
    {
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length == 0)
        {
            Debug.Log("BattleManager: TeamSetupData に編成情報がありません。");
            return;
        }

        for (int i = 0; i < TeamSetupData.SelectedTeam.Length; i++)
        {
            CharacterBlueprint bp = TeamSetupData.SelectedTeam[i];
            if (bp == null || bp.prefab == null)
            {
                // そのスロットにキャラがセットされていない場合はスキップ
                continue;
            }

            // ========== 出撃位置の決定 ==========
            Vector2 spawnPos = Vector2.zero;

            switch (currentStage.ruleType)
            {
                case StageRuleType.OneWay:
                    {
                        // 左側（playerX）にランダムなYで出撃
                        float y = Random.Range(currentStage.minY, currentStage.maxY);
                        spawnPos = new Vector2(currentStage.playerX, y);
                    }
                    break;

                case StageRuleType.BothSides:
                    {
                        // 仮実装：playerSpawnPositions があればそこを使う
                        if (currentStage.playerSpawnPositions != null &&
                            currentStage.playerSpawnPositions.Length > 0)
                        {
                            int index = Mathf.Min(i, currentStage.playerSpawnPositions.Length - 1);
                            spawnPos = currentStage.playerSpawnPositions[index];
                        }
                        else
                        {
                            spawnPos = new Vector2(currentStage.playerX, 0f);
                        }
                    }
                    break;

                case StageRuleType.FreeField:
                    {
                        // 仮実装：playerSpawnPositions からランダム
                        if (currentStage.playerSpawnPositions != null &&
                            currentStage.playerSpawnPositions.Length > 0)
                        {
                            int idx = Random.Range(0, currentStage.playerSpawnPositions.Length);
                            spawnPos = currentStage.playerSpawnPositions[idx];
                        }
                        else
                        {
                            spawnPos = new Vector2(currentStage.playerX, 0f);
                        }
                    }
                    break;
            }

            // ========== プレハブ生成 ==========
            GameObject playerObj = Instantiate(bp.prefab, spawnPos, Quaternion.identity);

            // PlayerController にルールを渡す
            var pc = playerObj.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Initialize(currentStage.ruleType);
            }

            Debug.Log($"BattleManager: スロット {i + 1} から {bp.characterName} を出撃させました。");
        }
    }

    private void OnPlayerBaseDestroyed()
    {
        Debug.Log("Game Over!");
        // TODO：リザルトへ遷移
    }

    private void OnEnemyBaseDestroyed()
    {
        Debug.Log("Stage Clear!");
        // TODO：リザルトへ遷移
    }


    // ==========================================
    //  バトル終了管理（クリア／敗北）
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