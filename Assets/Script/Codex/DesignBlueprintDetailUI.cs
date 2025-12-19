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
            if (gameState == null)
            {
                Debug.LogWarning("GameState not found in scene.");
            }
        }

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnClickCraft);
        }
        else
        {
            Debug.LogWarning("Craft button reference is missing.");
        }
    }

    public void AssignListUI(DesignBlueprintListUI list)
    {
        listUI = list;
    }

    public void Show(CharacterBlueprint bp)
    {
        currentBlueprint = bp;
        if (bp == null)
        {
            Debug.LogWarning("DesignBlueprintDetailUI.Show called with null blueprint.");
            return;
        }
        if (icon == null || nameText == null || unlockText == null || costText == null || resultText == null)
        {
            Debug.LogWarning("DesignBlueprintDetailUI is missing references.");
            return;
        }

        icon.sprite = bp.icon;
        nameText.text = bp.characterName;
        resultText.text = string.Empty;

        bool unlocked = gameState != null && unlockDb != null && gameState.IsUnlocked(bp.blueprintID, unlockDb);
        if (unlockDb == null)
        {
            unlockText.text = "Unlock data missing";
        }
        else
        {
            int required = unlockDb.GetUnlockStage(bp.blueprintID);
            unlockText.text = unlocked ? "Unlocked" : $"Unlock: Clear Stage {required}";
        }

        UpdateCostText(bp);
        UpdateCraftButton(unlocked);
    }

    private void UpdateCostText(CharacterBlueprint bp)
    {
        if (costText == null)
        {
            Debug.LogWarning("Cost text reference is missing.");
            return;
        }

        if (unlockDb == null)
        {
            costText.text = "";
            return;
        }

        var costs = unlockDb.GetCraftCosts(bp.blueprintID);
        if (costs.Count == 0)
        {
            costText.text = "No materials required";
            return;
        }

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
        if (currentBlueprint == null)
        {
            Debug.LogWarning("No blueprint selected to craft.");
            return;
        }
        if (resultText == null)
        {
            Debug.LogWarning("Result text reference is missing.");
            return;
        }

        if (gameState == null)
        {
            resultText.text = "Game state missing";
            return;
        }
        if (blueprintDb == null || unlockDb == null)
        {
            resultText.text = "Database missing";
            return;
        }

        bool success = gameState.Craft(currentBlueprint.blueprintID, blueprintDb, unlockDb);
        resultText.text = success ? "Crafted!" : "Craft failed";

        UpdateCostText(currentBlueprint);
        UpdateCraftButton(gameState.IsUnlocked(currentBlueprint.blueprintID, unlockDb));
        if (listUI != null)
        {
            listUI.Refresh();
        }
    }
}
