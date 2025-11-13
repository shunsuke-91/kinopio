using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージデータ一覧")]
    [SerializeField] private StageData[] stageList;

    /// <summary>
    /// UIボタンから呼び出される
    /// indexは stageList の何番を押したか
    /// </summary>
    public void OnStageButtonPressed(int index)
    {
        if (index < 0 || index >= stageList.Length)
        {
            Debug.LogError("ステージ番号が不正です: " + index);
            return;
        }

        // 選ばれたステージを StageLoader に保存
        StageLoader.selectedStage = stageList[index];

        // バトルシーンへ遷移
        SceneManager.LoadScene("BattleScene");
    }
}