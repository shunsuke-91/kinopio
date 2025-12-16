using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private StageData stageData;
    [SerializeField, Range(0, 3)] private int spawnLevel = 0;

    private void Awake()
    {
        stageData = StageLoader.selectedStage;

        if (stageData == null)
        {
            Debug.LogError("PlayerSpawner: StageData が StageLoader から取得できていません！");
        }
    }

    /// <summary>
    /// UIボタンから index 番目の編成キャラを出撃させる（slot 0〜4）
    /// </summary>
    public void SpawnPlayer(int index)
    {
        if (stageData == null) return;

        var team = TeamSetupData.SelectedTeam;
        if (team == null || team.Length == 0)
        {
            Debug.LogWarning("PlayerSpawner: TeamSetupData.SelectedTeam が空です（編成シーンを経由していない可能性）");
            return;
        }

        if (index < 0 || index >= team.Length)
        {
            Debug.LogError($"SpawnPlayer: index {index} が編成スロット範囲外です");
            return;
        }

        CharacterBlueprint bp = team[index];
        if (bp == null)
        {
            Debug.LogWarning($"SpawnPlayer: スロット {index} にキャラがセットされていません");
            return;
        }

        if (bp.prefab == null)
        {
            Debug.LogError($"SpawnPlayer: {bp.characterName} の prefab が設定されていません");
            return;
        }

        GameObject prefab = bp.prefab;

        // 出撃座標
        Vector2 pos;

        if (stageData.ruleType == StageRuleType.OneWay)
        {
            float y = Random.Range(stageData.minY, stageData.maxY);
            pos = new Vector2(stageData.playerX, y);
        }
        else
        {
            if (stageData.playerSpawnPositions != null &&
                stageData.playerSpawnPositions.Length > 0)
            {
                pos = stageData.playerSpawnPositions[
                    Random.Range(0, stageData.playerSpawnPositions.Length)
                ];
            }
            else
            {
                pos = Vector2.zero;
            }
        }

        // Player生成
        GameObject playerObj = Instantiate(prefab, pos, Quaternion.identity);

        // ルール・ステータスを渡す
        var pc = playerObj.GetComponent<PlayerController>();
        if (pc != null)
        {
            var instance = new CharacterInstance(bp, spawnLevel);
            pc.ApplyInstance(instance);
            pc.Initialize(stageData.ruleType);
        }

        Debug.Log($"[PlayerSpawner] Slot {index} ({bp.characterName}) を {pos} に出現");
    }
}
