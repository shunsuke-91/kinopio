using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        for (int i = 0; i < teamSlotButtons.Length; i++)
        {
            int slotIndex = i;
            teamSlotButtons[i].onClick.AddListener(() => OnClickTeamSlot(slotIndex));
        }

        UpdateSelectedSlotLabel();
        RefreshSlotIconsFromData();
    }

    private void OnClickTeamSlot(int slotIndex)
    {
        currentSelectedSlotIndex = slotIndex;
        UpdateSelectedSlotLabel();
        Debug.Log($"スロット {slotIndex + 1} を選択しました");
    }

    // ★ 変更：Blueprintではなく Instance を受け取る
    public void AssignCharacterToCurrentSlot(CharacterInstance instance)
    {
        if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning("スロットが選択されていません。先にスロットボタンを押してください。");
            return;
        }

        if (instance == null || instance.Blueprint == null)
        {
            Debug.LogWarning("無効なキャラクターです（instance または Blueprint が null）");
            return;
        }

        if (characterManager == null)
        {
            characterManager = FindFirstObjectByType<CharacterManager>();
        }

        // ★ 重複禁止：同じ Instance は他スロットに入れられない
        for (int i = 0; i < TeamSetupData.SelectedTeam.Length; i++)
        {
            if (i == currentSelectedSlotIndex) continue;

            var other = TeamSetupData.SelectedTeam[i];
            if (other == null) continue;

            if (ReferenceEquals(other, instance))
            {
                // データから外す
                TeamSetupData.SelectedTeam[i] = null;

                // 保存側も外す（SetTeamSlot が CharacterInstance 前提）
                if (characterManager != null)
                {
                    characterManager.SetTeamSlot(i, null);
                }

                // 見た目も消す
                if (teamSlotImages != null && i < teamSlotImages.Length && teamSlotImages[i] != null)
                {
                    teamSlotImages[i].sprite = null;
                    teamSlotImages[i].enabled = false;
                }

                Debug.Log($"重複防止：スロット {i + 1} から {instance.Blueprint.characterName} を外しました。");
            }
        }

        // 1) データに入れる
        TeamSetupData.SelectedTeam[currentSelectedSlotIndex] = instance;

        // 2) 保存側にも同期
        if (characterManager != null)
        {
            characterManager.SetTeamSlot(currentSelectedSlotIndex, instance);
        }

        // 見た目更新
        if (teamSlotImages != null &&
            currentSelectedSlotIndex < teamSlotImages.Length &&
            teamSlotImages[currentSelectedSlotIndex] != null)
        {
            var bp = instance.Blueprint;
            teamSlotImages[currentSelectedSlotIndex].sprite = bp.icon;
            teamSlotImages[currentSelectedSlotIndex].enabled = (bp.icon != null);
        }

        Debug.Log($"スロット {currentSelectedSlotIndex + 1} に {instance.Blueprint.characterName} をセットしました。");
    }

    private void UpdateSelectedSlotLabel()
    {
        if (selectedSlotLabel == null) return;

        if (currentSelectedSlotIndex < 0)
            selectedSlotLabel.text = "スロット未選択";
        else
            selectedSlotLabel.text = $"スロット {currentSelectedSlotIndex + 1} 選択中";
    }

    private void RefreshSlotIconsFromData()
    {
        if (teamSlotImages == null) return;

        for (int i = 0; i < teamSlotImages.Length && i < TeamSetupData.SelectedTeam.Length; i++)
        {
            var img = teamSlotImages[i];
            if (img == null) continue;

            var ins = TeamSetupData.SelectedTeam[i];
            if (ins != null && ins.Blueprint != null)
            {
                img.sprite = ins.Blueprint.icon;
                img.enabled = (ins.Blueprint.icon != null);
            }
            else
            {
                img.sprite = null;
                img.enabled = false;
            }
        }
    }
}