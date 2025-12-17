using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// チーム編成画面の UI をまとめて管理するクラス
/// ・スロットボタンをクリック → 選択中スロットを変更
/// ・キャラボタンをクリック → 現在選択中スロットにキャラをセット
/// ・TeamSetupData を正として、CharacterManager で保存/復元する
/// </summary>
public class TeamSetupUI : MonoBehaviour
{
    [Header("チームスロットボタン（左から順に 0〜）")]
    [SerializeField] private Button[] teamSlotButtons;

    [Header("スロットに表示するアイコン画像（各ボタンの子の Image を指定）")]
    [SerializeField] private Image[] teamSlotImages;

    [Header("現在選択中スロットの表示ラベル（例：\"スロット1選択中\"）")]
    [SerializeField] private TextMeshProUGUI selectedSlotLabel;

    [Header("保存/復元に使う（未設定なら自動検索）")]
    [SerializeField] private CharacterManager characterManager;

    // 現在どのスロットを選んでいるか（-1 = 未選択）
    private int currentSelectedSlotIndex = -1;

    private void Awake()
    {
        if (characterManager == null)
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
        }
    }

    private void Start()
    {
        // スロットボタンにクリック処理を登録
        if (teamSlotButtons != null)
        {
            for (int i = 0; i < teamSlotButtons.Length; i++)
            {
                int slotIndex = i; // クロージャ対策
                if (teamSlotButtons[i] != null)
                {
                    teamSlotButtons[i].onClick.AddListener(() => OnClickTeamSlot(slotIndex));
                }
            }
        }

        // ★ 重要：保存済み → TeamSetupData に復元（事故防止）
        if (characterManager != null)
        {
            characterManager.GetTeamInstances();
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
    /// 現在選択中のスロットに、このキャラをセットする（A：空スロットOK）
    /// </summary>
    public void AssignCharacterToCurrentSlot(CharacterBlueprint blueprint)
    {
        if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning("スロットが選択されていません。先にスロットボタンを押してください。");
            return;
        }

        if (characterManager == null)
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
        }

        CharacterInstance foundInstance = null;
        if (characterManager != null)
        {
            foundInstance = characterManager.ownedCharacters.FirstOrDefault(o => o != null && o.Blueprint == blueprint);
            if (foundInstance == null && blueprint != null)
            {
                foundInstance = new CharacterInstance(blueprint);
                characterManager.ownedCharacters.Add(foundInstance);
            }
            characterManager.SetTeamSlot(currentSelectedSlotIndex, foundInstance);
        }

        // 2) 見た目を更新
        if (teamSlotImages != null &&
            currentSelectedSlotIndex < teamSlotImages.Length &&
            teamSlotImages[currentSelectedSlotIndex] != null)
        {
            var img = teamSlotImages[currentSelectedSlotIndex];

            if (blueprint != null && blueprint.icon != null)
            {
                img.sprite = blueprint.icon;
                img.enabled = true;
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
            }
        }

        string name = (blueprint != null) ? blueprint.characterName : "未設定";
        Debug.Log($"スロット {currentSelectedSlotIndex + 1} に {name} をセットしました");
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
    /// TeamSetupData に入っている情報からスロットアイコンを復元する
    /// </summary>
    public void RefreshSlotIconsFromData()
    {
        if (teamSlotImages == null) return;

        CharacterInstance[] team = characterManager != null
            ? characterManager.GetTeamInstances()
            : null;

        if (team == null)
        {
            if (characterManager != null)
            {
                characterManager.GetTeamBlueprints();
            }
            return;
        }

        int len = Mathf.Min(teamSlotImages.Length, team.Length);

        for (int i = 0; i < len; i++)
        {
            var img = teamSlotImages[i];
            if (img == null) continue;

            var inst = team[i];
            var bp = inst != null ? inst.Blueprint : null;
            if (bp != null && bp.icon != null)
            {
                img.sprite = bp.icon;
                img.enabled = true;
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
            }
        }
    }
}
