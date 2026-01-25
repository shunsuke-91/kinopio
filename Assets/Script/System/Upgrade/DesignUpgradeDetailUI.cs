using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesignUpgradeDetailUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text resultText;

    [SerializeField] private UpgradeCostDatabase upgradeCostDb;
    [SerializeField] private GameState gameState;

    private CharacterInstance current;

    private void Start()
    {
        if (gameState == null)
        {
            gameState = FindFirstObjectByType<GameState>();
            if (gameState == null) Debug.LogWarning("GameState not found in scene.");
        }

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }
    }

    public void Show(CharacterInstance instance)
    {
        current = instance;

        if (resultText != null) resultText.text = "";

        if (current == null)
        {
            Debug.LogWarning("DesignUpgradeDetailUI.Show called with null.");
            return;
        }

        var bp = current.Blueprint;
        if (icon != null) icon.sprite = bp != null ? bp.icon : null;
        if (nameText != null) nameText.text = bp != null ? bp.characterName : current.BlueprintId;
        if (levelText != null) levelText.text = $"Lv {current.Level}";

        UpdateCostAndButton();
    }

    private void UpdateCostAndButton()
    {
        if (costText == null || upgradeButton == null) return;

        if (current == null || current.IsMaxLevel)
        {
            costText.text = "Max Level";
            upgradeButton.interactable = false;
            return;
        }

        if (gameState == null || upgradeCostDb == null)
        {
            costText.text = "Database missing";
            upgradeButton.interactable = false;
            return;
        }

        int nextLevel = current.Level + 1;
        var costs = upgradeCostDb.GetCosts(current.BlueprintId, nextLevel); // List<MaterialStack>前提

        if (costs == null || costs.Count == 0)
        {
            costText.text = "No cost data";
            upgradeButton.interactable = false;
            return;
        }

        var sb = new StringBuilder();
        bool enough = true;

        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            int owned = gameState.Materials != null ? gameState.Materials.GetCount(cost.materialId) : 0;

            sb.AppendLine($"{cost.materialId}: {owned}/{cost.count}");
            if (owned < cost.count) enough = false;
        }

        costText.text = sb.ToString();
        upgradeButton.interactable = enough;
    }

    private void OnClickUpgrade()
    {
        if (current == null) return;

        if (current.IsMaxLevel)
        {
            if (resultText != null) resultText.text = "Already Max Level";
            return;
        }

        if (gameState == null)
        {
            if (resultText != null) resultText.text = "GameState missing";
            return;
        }

        bool ok = gameState.TryUpgradeOwnedCharacter(current.InstanceId, upgradeCostDb);
        if (resultText != null) resultText.text = ok ? "Upgrade Success" : "Upgrade Failed";

        // 表示更新
        Show(current);
    }
}