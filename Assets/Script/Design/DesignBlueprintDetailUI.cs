using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesignBlueprintDetailUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text unlockText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button craftButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private CharacterBlueprintDatabase blueprintDb;
    [SerializeField] private BlueprintUnlockDatabase unlockDb;
    [SerializeField] private GameState gameState;

    private CharacterBlueprint currentBlueprint;
    private DesignBlueprintListUI listUI;

    private void Start()
    {
        if (gameState == null)
        {
            gameState = FindFirstObjectByType<GameState>();
            if (gameState == null) Debug.LogWarning("[DetailUI] GameState not found in scene.");
        }

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnClickCraft);
        }
        else
        {
            Debug.LogWarning("[DetailUI] Craft button reference is missing.");
        }
    }

    public void AssignListUI(DesignBlueprintListUI list) => listUI = list;

    public void Show(CharacterBlueprint bp)
    {
        currentBlueprint = bp;

        if (bp == null)
        {
            Debug.LogWarning("[DetailUI] Show called with null blueprint.");
            return;
        }

        if (icon == null || nameText == null || unlockText == null || costText == null || resultText == null)
        {
            Debug.LogWarning("[DetailUI] missing references (icon/name/unlock/cost/result).");
            return;
        }

        icon.sprite = bp.icon;
        nameText.text = bp.characterName;
        resultText.text = string.Empty;

        bool unlocked = gameState != null && unlockDb != null && gameState.IsUnlocked(bp.blueprintID, unlockDb);
        int required = unlockDb != null ? unlockDb.GetUnlockStage(bp.blueprintID) : 9999;

        unlockText.text = unlockDb == null
            ? "Unlock data missing"
            : (unlocked ? "Unlocked" : $"Unlock: Clear Stage {required}");

        UpdateCostText(bp);
        UpdateCraftButton(unlocked);
    }

    private void UpdateCostText(CharacterBlueprint bp)
    {
        if (costText == null) return;
        if (unlockDb == null) { costText.text = ""; return; }

        var costs = unlockDb.GetCraftCosts(bp.blueprintID);
        if (costs.Count == 0) { costText.text = "No materials required"; return; }

        var sb = new StringBuilder();
        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            int owned = gameState != null ? gameState.Materials.GetCount(cost.materialId) : 0;
            sb.AppendLine($"{cost.materialId}: {owned}/{cost.count}");
        }
        costText.text = sb.ToString();
    }

    private void UpdateCraftButton(bool unlocked)
    {
        if (craftButton == null) return;

        if (gameState == null || unlockDb == null || currentBlueprint == null)
        {
            craftButton.interactable = false;
            return;
        }

        craftButton.interactable = unlocked && gameState.CanCraft(currentBlueprint.blueprintID, unlockDb);
    }

    private void OnClickCraft()
    {
        Debug.Log("[DetailUI] OnClickCraft fired."); // ★これが出ないなら「クリックが届いてない」

        if (currentBlueprint == null)
        {
            Debug.LogWarning("[DetailUI] No blueprint selected to craft.");
            return;
        }

        Debug.Log($"[DetailUI] Try Craft blueprintID={currentBlueprint.blueprintID}");

        if (resultText == null)
        {
            Debug.LogWarning("[DetailUI] Result text reference is missing.");
            return;
        }

        if (gameState == null)
        {
            Debug.LogWarning("[DetailUI] GameState missing.");
            resultText.text = "Game state missing";
            return;
        }

        if (blueprintDb == null || unlockDb == null)
        {
            Debug.LogWarning($"[DetailUI] Database missing. blueprintDb={(blueprintDb==null)} unlockDb={(unlockDb==null)}");
            resultText.text = "Database missing";
            return;
        }

        bool success = gameState.Craft(currentBlueprint.blueprintID, blueprintDb, unlockDb);
        resultText.text = success ? "Crafted!" : "Craft failed";

        UpdateCostText(currentBlueprint);
        UpdateCraftButton(gameState.IsUnlocked(currentBlueprint.blueprintID, unlockDb));

        if (listUI != null) listUI.Refresh();
    }
}