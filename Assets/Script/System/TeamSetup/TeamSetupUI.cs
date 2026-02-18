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

    [Header("未設定なら自動検索")]
    [SerializeField] private GameState gameState;

    private int currentSelectedSlotIndex = -1;

    private void Awake()
    {
        if (gameState == null)
        {
            gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
        }
    }

    private void Start()
    {
        // ボタン押下 → スロット選択
        if (teamSlotButtons != null)
        {
            for (int i = 0; i < teamSlotButtons.Length; i++)
            {
                int slotIndex = i;
                if (teamSlotButtons[i] == null) continue;

                teamSlotButtons[i].onClick.RemoveAllListeners();
                teamSlotButtons[i].onClick.AddListener(() => OnClickTeamSlot(slotIndex));
            }
        }

        UpdateSelectedSlotLabel();
        RefreshSlotIconsFromRuntime();
    }

    private void OnClickTeamSlot(int slotIndex)
    {
        currentSelectedSlotIndex = slotIndex;
        UpdateSelectedSlotLabel();
        Debug.Log($"[TeamSetupUI] Selected slot {slotIndex + 1}");
    }

    // =========================================================
    // ★互換メソッド（既存コードが呼んでいる名前）
    // CharacterScrollView が teamSetupUI.OnCharacterSelected(inst) を呼ぶ想定に合わせる
    // =========================================================
    public void OnCharacterSelected(CharacterInstance instance)
    {
        AssignCharacterToCurrentSlot(instance);
    }

    /// <summary>
    /// 「このキャラを今選んでるスロットに入れる」
    /// </summary>
    public void AssignCharacterToCurrentSlot(CharacterInstance instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[TeamSetupUI] Assign failed: instance is null.");
            return;
        }

        if (currentSelectedSlotIndex < 0 || currentSelectedSlotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning("[TeamSetupUI] Assign failed: slot not selected.");
            return;
        }

        // ランタイム配列の安全化
        EnsureRuntimeTeamArray();

        // 同じ「インスタンス」を複数スロットに入れない（同キャラ3体OK、同インスタンス重複だけNG）
        for (int i = 0; i < TeamSetupData.SelectedTeam.Length; i++)
        {
            if (i == currentSelectedSlotIndex) continue;
            if (TeamSetupData.SelectedTeam[i] == null) continue;

            if (ReferenceEquals(TeamSetupData.SelectedTeam[i], instance))
            {
                TeamSetupData.SelectedTeam[i] = null;
                ClearSlotIcon(i);
            }
        }

        // セット
        TeamSetupData.SelectedTeam[currentSelectedSlotIndex] = instance;
        SetSlotIcon(currentSelectedSlotIndex, instance);

        // セーブ側へ同期（GameState に統一している前提）
        if (gameState == null)
        {
            gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
        }
        if (gameState != null)
        {
            gameState.SaveTeamFromRuntime();
        }

        Debug.Log($"[TeamSetupUI] Assigned instanceId={instance.InstanceId} to slot {currentSelectedSlotIndex + 1}");
    }

    private void UpdateSelectedSlotLabel()
    {
        if (selectedSlotLabel == null) return;

        if (currentSelectedSlotIndex < 0)
            selectedSlotLabel.text = "スロット未選択";
        else
            selectedSlotLabel.text = $"スロット {currentSelectedSlotIndex + 1} 選択中";
    }

    /// <summary>
    /// 画面表示を「TeamSetupData.SelectedTeam」の現在値で描画し直す
    /// </summary>
    public void RefreshSlotIconsFromRuntime()
    {
        EnsureRuntimeTeamArray();

        if (teamSlotImages == null) return;

        for (int i = 0; i < teamSlotImages.Length && i < TeamSetupData.SelectedTeam.Length; i++)
        {
            var inst = TeamSetupData.SelectedTeam[i];
            if (inst != null)
                SetSlotIcon(i, inst);
            else
                ClearSlotIcon(i);
        }
    }

    private void SetSlotIcon(int index, CharacterInstance inst)
    {
        if (teamSlotImages == null) return;
        if (index < 0 || index >= teamSlotImages.Length) return;

        var img = teamSlotImages[index];
        if (img == null) return;

        var bp = inst.Blueprint;
        if (bp == null || bp.icon == null)
        {
            img.sprite = null;
            img.enabled = false;
            return;
        }

        img.sprite = bp.icon;
        img.enabled = true;
    }

    private void ClearSlotIcon(int index)
    {
        if (teamSlotImages == null) return;
        if (index < 0 || index >= teamSlotImages.Length) return;

        var img = teamSlotImages[index];
        if (img == null) return;

        img.sprite = null;
        img.enabled = false;
    }

    private void EnsureRuntimeTeamArray()
    {
        int n = TeamSetupData.MaxSlots;

        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != n)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[n];
        }
    }
}