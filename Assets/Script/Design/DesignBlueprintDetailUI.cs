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

    private CharacterManager CM =>
        CharacterManager.Instance != null ? CharacterManager.Instance : FindFirstObjectByType<CharacterManager>();

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

        bool unlocked = false;
        if (gameState != null && unlockDb != null)
        {
            unlocked = gameState.IsUnlocked(bp.blueprintID, unlockDb);
        }

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
        if (costs == null || costs.Count == 0)
        {
            costText.text = "No materials required";
            return;
        }

        var sb = new StringBuilder();
        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;

            int owned = 0;
            if (gameState != null && gameState.Materials != null)
                owned = gameState.Materials.GetCount(cost.materialId);

            sb.AppendLine($"{cost.materialId}: {owned}/{cost.count}");
        }

        costText.text = sb.ToString();
    }

    private void UpdateCraftButton(bool unlocked)
    {
        if (craftButton == null) return;

        // 今の方針：設計できるかの判定は CharacterManager.TryCraft の前段で行う
        // ここでは「素材が足りているか」だけ GameState で見て、ロックは unlock 判定
        if (gameState == null || unlockDb == null || currentBlueprint == null)
        {
            craftButton.interactable = false;
            return;
        }

        var costs = unlockDb.GetCraftCosts(currentBlueprint.blueprintID);
        bool hasMaterials = gameState.HasMaterials(costs);

        craftButton.interactable = unlocked && hasMaterials;
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

        if (CM == null)
        {
            resultText.text = "CharacterManager missing";
            return;
        }

        bool success = CM.TryCraft(currentBlueprint.blueprintID);
        resultText.text = success ? "Crafted!" : "Craft failed";

        UpdateCostText(currentBlueprint);

        bool unlocked = false;
        if (gameState != null && unlockDb != null)
            unlocked = gameState.IsUnlocked(currentBlueprint.blueprintID, unlockDb);

        UpdateCraftButton(unlocked);

        if (listUI != null)
        {
            listUI.Refresh();
        }
    }
}