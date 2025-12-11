using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// チーム編成画面の UI をまとめて管理するクラス
/// ・スロットボタンをクリック → 選択中スロットを変更
/// ・キャラボタンをクリック → 選択中スロットにキャラをセット
/// </summary>
public class TeamSetupUI : MonoBehaviour
{
    [Header("チームスロットボタン（左から順に 0〜）")]
    [SerializeField] private Button[] teamSlotButtons;

    [Header("スロットに表示するアイコン画像（各ボタンの子の Image を指定）")]
    [SerializeField] private Image[] teamSlotImages;

    [Header("現在選択中スロットの表示ラベル（例：\"スロット1選択中\"）")]
    [SerializeField] private TextMeshProUGUI selectedSlotLabel;

    // 現在どのスロットを選んでいるか（-1 = 未選択）
    private int currentSelectedSlotIndex = -1;

    private void Start()
    {
        // スロットボタンにクリック処理を登録
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            int slotIndex = i; // クロージャ対策
            teamSlotButtons[i].onClick.AddListener(() => OnClickTeamSlot(slotIndex));
        }

        // 初期表示
        UpdateSelectedSlotLabel();
        RefreshSlotIconsFromData();
    }

    /// <summary>
    /// スロットボタンを押した時に呼ばれる
    /// </summary>
    private void OnClickTeamSlot(int slotIndex)
    {
        currentSelectedSlotIndex = slotIndex;
        UpdateSelectedSlotLabel();
        Debug.Log($"スロット {slotIndex + 1} を選択しました");
    }

    /// <summary>
    /// キャラボタン側から呼んでもらうメソッド
    /// 現在選択中のスロットに、このキャラをセットする
    /// </summary>
    public void AssignCharacterToCurrentSlot(CharacterBlueprint blueprint)
    {
        if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning("スロットが選択されていません。先にスロットボタンを押してください。");
            return;
        }

        // データ側に記録
        TeamSetupData.SelectedTeam[currentSelectedSlotIndex] = blueprint;

        // 見た目を更新
        if (teamSlotImages != null &&
            currentSelectedSlotIndex < teamSlotImages.Length &&
            teamSlotImages[currentSelectedSlotIndex] != null)
        {
            teamSlotImages[currentSelectedSlotIndex].sprite = blueprint.icon;
            teamSlotImages[currentSelectedSlotIndex].enabled = (blueprint.icon != null);
        }

        Debug.Log($"スロット {currentSelectedSlotIndex + 1} に {blueprint.characterName} をセットしました。");
    }

    /// <summary>
    /// ラベル更新
    /// </summary>
    private void UpdateSelectedSlotLabel()
    {
        if (selectedSlotLabel == null) return;

        if (currentSelectedSlotIndex < 0)
        {
            selectedSlotLabel.text = "スロット未選択";
        }
        else
        {
            selectedSlotLabel.text = $"スロット {currentSelectedSlotIndex + 1} 選択中";
        }
    }

    /// <summary>
    /// 既に TeamSetupData に入っている情報からスロットアイコンを復元する
    /// （将来的にシーンをまたぐ場合を想定）
    /// </summary>
    private void RefreshSlotIconsFromData()
    {
        if (teamSlotImages == null) return;

        for (int i = 0; i < teamSlotImages.Length && i < TeamSetupData.SelectedTeam.Length; i++)
        {
            var img = teamSlotImages[i];
            if (img == null) continue;

            var bp = TeamSetupData.SelectedTeam[i];
            if (bp != null)
            {
                img.sprite = bp.icon;
                img.enabled = (bp.icon != null);
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
            }
        }
    }
}