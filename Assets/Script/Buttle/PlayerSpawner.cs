using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private StageData stageData;

    private void Awake()
    {
        stageData = StageLoader.selectedStage;

        if (stageData == null)
        {
            Debug.LogError("PlayerSpawner: StageData が StageLoader から取得できていません！");
        }
    }

    /// <summary>
    /// UIボタンから index 番目の Player を出撃させる
    /// </summary>
 public void SpawnPlayer(int index)
    {
        if (stageData == null) return;

        // 出撃可能ユニットが設定されていない
        if (stageData.playerPrefabs == null || stageData.playerPrefabs.Length == 0)
        {
            Debug.LogError("StageData に playerPrefabs が設定されていません！");
            return;
        }

        // index が範囲外
        if (index < 0 || index >= stageData.playerPrefabs.Length)
        {
            Debug.LogError($"SpawnPlayer: index {index} が範囲外です");
            return;
        }

        GameObject prefab = stageData.playerPrefabs[index];

        // 🔥 OneWay 用ランダム座標（A方式）
        Vector2 pos;

        if (stageData.ruleType == StageRuleType.OneWay)
        {
            float y = Random.Range(stageData.minY, stageData.maxY);
            pos = new Vector2(stageData.playerX, y);
        }
        else
        {
            // BothSides / FreeField は従来の spawnPositions を使用
            if (stageData.playerSpawnPositions != null &&
                stageData.playerSpawnPositions.Length > 0)
            {
                pos = stageData.playerSpawnPositions[
                    Random.Range(0, stageData.playerSpawnPositions.Length)
                ];
            }
            else
            {
                pos = Vector2.zero; // fallback
            }
        }

        // Player生成
        GameObject playerObj = Instantiate(prefab, pos, Quaternion.identity);

        // Playerにステージルールを渡す
        var pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.Initialize(stageData.ruleType);
        }

        Debug.Log($"Player {index} を {pos} に出現");
    }
}