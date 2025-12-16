using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [Header("ステージデータ")]
    [SerializeField] private StageData currentStage;

    [Header("UI / 演出関連")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private AudioSource bgmSource;

    private BaseController playerBase;
    private BaseController enemyBase;

    private void Awake()
    {
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

        currentStage.difficulty = StageLoader.selectedDifficulty;

        LoadStageSettings();
        SpawnBases();                 // 拠点の生成
        // SpawnPlayersFromTeam();     // ★ここが自動生成の原因なので呼ばない
        StartCoroutine(SpawnEnemies());
    }

    private void LoadStageSettings()
    {
        if (backgroundImage != null && currentStage.background != null)
            backgroundImage.sprite = currentStage.background;

        if (bgmSource != null && currentStage.bgm != null)
        {
            bgmSource.clip = currentStage.bgm;
            bgmSource.Play();
        }

        Debug.Log($"ステージ開始：{currentStage.stageName} / ルール：{currentStage.ruleType}");
    }

    private IEnumerator SpawnEnemies()
    {
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

                DifficultySettings diff = currentStage.GetDifficultySettings();

                var ec = enemyObj.GetComponent<EnemyController>();
                if (ec != null)
                    ec.Initialize(currentStage.ruleType, diff);
            }
        }
        else if (currentStage.ruleType == StageRuleType.BothSides)
        {
            for (int i = 0; i < currentStage.enemyPrefabs.Length; i++)
            {
                yield return new WaitForSeconds(currentStage.spawnDelays[i]);

                Vector2 spawnPos = (i % 2 == 0) ? new Vector2(8, 0) : new Vector2(-8, 0);
                Instantiate(currentStage.enemyPrefabs[i], spawnPos, Quaternion.identity);
            }
        }
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
    }

    private void SpawnBases()
    {
        DifficultySettings diff = currentStage.GetDifficultySettings();

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
                float hp = currentStage.playerBaseHP * diff.hpMultiplier;
                playerBase.Initialize(hp);
                playerBase.OnBaseDestroyed += OnPlayerBaseDestroyed;
            }
            else
            {
                Debug.LogError("BattleManager: PlayerBasePrefab に BaseController がついていません");
            }
        }

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

        var ui = FindFirstObjectByType<BaseHPUIController>();
        if (ui != null) ui.Setup(playerBase, enemyBase);
    }

    // ==========================================================
    // ★ボタン押下で1体だけ出撃させる（slotIndex: 0〜4）
    // ==========================================================
    public void SpawnPlayerFromSlot(int slotIndex)
    {
        var team = TeamSetupData.SelectedTeam;
        if (team == null || slotIndex < 0 || slotIndex >= team.Length)
            return;

        CharacterBlueprint bp = team[slotIndex];
        if (bp == null || bp.prefab == null)
            return;

        Vector2 spawnPos = GetPlayerSpawnPos(slotIndex);

        GameObject playerObj = Instantiate(bp.prefab, spawnPos, Quaternion.identity);

        var pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
            pc.Initialize(currentStage.ruleType);
    }

    private Vector2 GetPlayerSpawnPos(int slotIndex)
    {
        switch (currentStage.ruleType)
        {
            case StageRuleType.OneWay:
                {
                    float y = Random.Range(currentStage.minY, currentStage.maxY);
                    return new Vector2(currentStage.playerX, y);
                }

            case StageRuleType.BothSides:
                {
                    if (currentStage.playerSpawnPositions != null && currentStage.playerSpawnPositions.Length > 0)
                    {
                        int idx = Mathf.Min(slotIndex, currentStage.playerSpawnPositions.Length - 1);
                        return currentStage.playerSpawnPositions[idx];
                    }
                    return new Vector2(currentStage.playerX, 0f);
                }

            case StageRuleType.FreeField:
                {
                    if (currentStage.playerSpawnPositions != null && currentStage.playerSpawnPositions.Length > 0)
                    {
                        int idx = Random.Range(0, currentStage.playerSpawnPositions.Length);
                        return currentStage.playerSpawnPositions[idx];
                    }
                    return new Vector2(currentStage.playerX, 0f);
                }
        }
        return Vector2.zero;
    }

    private void OnPlayerBaseDestroyed()
    {
        Debug.Log("Game Over!");
    }

    private void OnEnemyBaseDestroyed()
    {
        Debug.Log("Stage Clear!");
    }
}