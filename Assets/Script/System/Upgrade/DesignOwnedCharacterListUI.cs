using UnityEngine;

public class DesignOwnedCharacterListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private DesignOwnedCharacterButtonUI buttonPrefab;
    [SerializeField] private DesignUpgradeDetailUI detailUI;
    [SerializeField] private GameState gameState;

    private void Start()
    {
        if (gameState == null)
        {
            gameState = FindFirstObjectByType<GameState>();
            if (gameState == null) Debug.LogWarning("GameState not found in scene.");
        }

        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || buttonPrefab == null)
        {
            Debug.LogWarning("DesignOwnedCharacterListUI is missing references.");
            return;
        }
        if (gameState == null || gameState.CurrentSave == null || gameState.CurrentSave.ownedCharacters == null)
        {
            Debug.LogWarning("No save / ownedCharacters found.");
            ClearChildren();
            return;
        }

        ClearChildren();

        var list = gameState.CurrentSave.ownedCharacters;
        foreach (var inst in list)
        {
            if (inst == null) continue;

            // 重要：Blueprint参照が取れる状態にする（GameState側が管理するならここは不要でもOK）
            // inst.AssignBlueprintDatabase(blueprintDb); ←今回はGameStateに寄せるので、必要ならGameState側で一括実行

            bool isMax = inst.IsMaxLevel;
            bool selectable = !isMax;

            var item = Instantiate(buttonPrefab, contentParent);
            item.Set(inst, selectable);

            if (item.button != null)
            {
                item.button.onClick.RemoveAllListeners();

                if (selectable)
                {
                    var captured = inst;
                    item.button.onClick.AddListener(() =>
                    {
                        if (detailUI != null) detailUI.Show(captured);
                        else Debug.LogWarning("DesignUpgradeDetailUI is not assigned.");
                    });
                }
            }
        }
    }

    private void ClearChildren()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}