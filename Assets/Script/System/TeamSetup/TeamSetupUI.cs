using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSetupUI : MonoBehaviour
{
    [Header("チームスロットボタン（左から順に 0〜）")]
    [SerializeField] private Button[] teamSlotButtons;

    [Header("スロットに表示するアイコン画像（各ボタンの子の Image を指定）")]
    [SerializeField] private Image[] teamSlotImages;

    [Header("現在選択中スロットの表示ラベル（例：\"スロット1選択中\"）")]
    [SerializeField] private TextMeshProUGUI selectedSlotLabel;

    [Header("Runtime (未設定なら自動検索)")]
    [SerializeField] private GameState gameState;

    private int currentSelectedSlotIndex = -1;

    private void Awake()
    {
        if (gameState == null)
            gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
    }

    private void Start()
    {
        if (teamSlotButtons != null)
        {
            for (int i = 0; i < teamSlotButtons.Length; i++)
            {
                int slotIndex = i;
                teamSlotButtons[i].onClick.RemoveAllListeners();
                teamSlotButtons[i].onClick.AddListener(() => OnClickTeamSlot(slotIndex));
            }
        }

        UpdateSelectedSlotLabel();
        RefreshSlotIconsFromData();
    }

    private void OnClickTeamSlot(int slotIndex)
    {
        currentSelectedSlotIndex = slotIndex;
        UpdateSelectedSlotLabel();
        Debug.Log($"[TeamSetupUI] slot {slotIndex + 1} selected");
    }

    // ★ CharacterScrollView から呼ばれる入口（あなたのエラー原因だったやつ）
    public void OnCharacterSelected(CharacterInstance instance)
    {
        AssignCharacterToCurrentSlot(instance);
    }

    public void AssignCharacterToCurrentSlot(CharacterInstance instance)
    {
        if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning("[TeamSetupUI] スロットが未選択です。先にスロットボタンを押してください。");
            return;
        }

        if (instance == null)
        {
            Debug.LogWarning("[TeamSetupUI] instance が null");
            return;
        }

        // TeamSetupData の配列を必ず準備
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[TeamSetupData.MaxSlots];
        }

        // ★重複禁止：「同じ instance」を他スロットに入れたら外す（Blueprint重複はOK）
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            if (i == currentSelectedSlotIndex) continue;

            var other = TeamSetupData.SelectedTeam[i];
            if (other == null) continue;

            if (ReferenceEquals(other, instance) || (!string.IsNullOrEmpty(other.InstanceId) && other.InstanceId == instance.InstanceId))
            {
                TeamSetupData.SelectedTeam[i] = null;

                if (teamSlotImages != null && i < teamSlotImages.Length && teamSlotImages[i] != null)
                {
                    teamSlotImages[i].sprite = null;
                    teamSlotImages[i].enabled = false;
                }

                Debug.Log($"[TeamSetupUI] duplicate instance removed from slot {i + 1} (InstanceId={instance.InstanceId})");
            }
        }

        // セット
        TeamSetupData.SelectedTeam[currentSelectedSlotIndex] = instance;

        // 見た目
        if (teamSlotImages != null &&
            currentSelectedSlotIndex < teamSlotImages.Length &&
            teamSlotImages[currentSelectedSlotIndex] != null)
        {
            var bp = instance.Blueprint;
            teamSlotImages[currentSelectedSlotIndex].sprite = bp != null ? bp.icon : null;
            teamSlotImages[currentSelectedSlotIndex].enabled = (teamSlotImages[currentSelectedSlotIndex].sprite != null);
        }

        Debug.Log($"[TeamSetupUI] slot {currentSelectedSlotIndex + 1} set InstanceId={instance.InstanceId} BlueprintId={instance.BlueprintId}");

        // ★ここが大事：ランタイム→セーブへ同期（BattleSceneでも同じになる）
        if (gameState != null)
        {
            gameState.SaveTeamFromRuntime();
        }
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

        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[TeamSetupData.MaxSlots];
        }

        for (int i = 0; i < teamSlotImages.Length && i < TeamSetupData.SelectedTeam.Length; i++)
        {
            var img = teamSlotImages[i];
            if (img == null) continue;

            var ins = TeamSetupData.SelectedTeam[i];
            var bp = ins != null ? ins.Blueprint : null;

            img.sprite = bp != null ? bp.icon : null;
            img.enabled = (img.sprite != null);
        }
    }
}