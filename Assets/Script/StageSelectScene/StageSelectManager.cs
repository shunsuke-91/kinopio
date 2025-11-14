using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージデータ一覧")]
    [SerializeField] private StageData[] stageList;

    [Header("難易度選択パネル")]
    [SerializeField] private GameObject difficultyPanel;

    private int selectedStageIndex = -1;


    // ========================================================
    // ステージボタンを押した瞬間の処理
    // ========================================================
    public void OnStageButtonPressed(int index)
    {
        if (index < 0 || index >= stageList.Length)
        {
            Debug.LogError("ステージ番号が不正です: " + index);
            return;
        }

        selectedStageIndex = index;

        // パネルを開く
        difficultyPanel.SetActive(true);

        // 難易度の解放状態を反映
        UpdateDifficultyButtons(index);
    }


    // ========================================================
    // 難易度ボタンを押した時の処理（Normal/Hard/Hell）
    // diffIndex → 0=Normal, 1=Hard, 2=Hell
    // ========================================================
    public void OnDifficultySelected(int diffIndex)
    {
        if (selectedStageIndex < 0)
        {
            Debug.LogError("ステージ未選択のまま難易度選択が押された");
            return;
        }

        // 選んだステージデータを BattleScene に渡す
        StageLoader.selectedStage = stageList[selectedStageIndex];

        // 選んだ難易度を保存
        StageLoader.selectedDifficulty = (DifficultyType)diffIndex;

        // バトルシーンへ
        SceneManager.LoadScene("BattleScene");
    }


    // ========================================================
    // 難易度ボタン（Normal/Hard/Hell）の解放状況を反映
    // Normalクリア → Hard解放
    // Hardクリア → Hell解放
    // ========================================================
    private void UpdateDifficultyButtons(int stageIndex)
    {
        // パネル内のボタンを探す（名前一致前提）
        Transform normalBtn = difficultyPanel.transform.Find("NormalButton");
        Transform hardBtn   = difficultyPanel.transform.Find("HardButton");
        Transform hellBtn   = difficultyPanel.transform.Find("HellButton");

        if (normalBtn == null || hardBtn == null || hellBtn == null)
        {
            Debug.LogError("DifficultyPanel 内に NormalButton / HardButton / HellButton が見つかりません");
            return;
        }

        // Normal は常に ON
        normalBtn.gameObject.SetActive(true);

        // PlayerPrefs のクリア状況
        bool normalCleared = PlayerPrefs.GetInt($"Stage{stageIndex}_NormalClear", 0) == 1;
        bool hardCleared   = PlayerPrefs.GetInt($"Stage{stageIndex}_HardClear", 0) == 1;

        // Hard → Normalクリアで解放
        hardBtn.gameObject.SetActive(normalCleared);

        // Hell → Hardクリアで解放
        hellBtn.gameObject.SetActive(hardCleared);
    }


    // ========================================================
    // 「close」ボタンでパネルを閉じる
    // ========================================================
    public void CloseDifficultyPanel()
    {
        difficultyPanel.SetActive(false);
        selectedStageIndex = -1; // 念のためリセット
    }
}