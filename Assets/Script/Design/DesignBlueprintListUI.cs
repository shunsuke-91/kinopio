using UnityEngine;

public class DesignBlueprintListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private DesignBlueprintButtonUI buttonPrefab;
    [SerializeField] private CharacterBlueprint[] allBlueprints;
    [SerializeField] private Sprite unknownSprite; // bp.icon が null の時だけ使う
    [SerializeField] private CharacterBlueprintDatabase blueprintDb;
    [SerializeField] private BlueprintUnlockDatabase unlockDb;
    [SerializeField] private DesignBlueprintDetailUI detailUI;
    [SerializeField] private GameState gameState;

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

        if (detailUI != null)
        {
            detailUI.AssignListUI(this);
        }

        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || buttonPrefab == null)
        {
            Debug.LogWarning("DesignBlueprintListUI is missing references.");
            return;
        }
        if (allBlueprints == null)
        {
            Debug.LogWarning("AllBlueprints is not assigned.");
            return;
        }

        // 既存の生成物を削除
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        foreach (var blueprint in allBlueprints)
        {
            if (blueprint == null) continue;

            var item = Instantiate(buttonPrefab, contentParent);

            bool unlocked = gameState != null && unlockDb != null
                && gameState.IsUnlocked(blueprint.blueprintID, unlockDb);

            // ★ロックでも「本来のアイコン」を表示する
            // ただし bp.icon が未設定なら unknown にする
            if (blueprint.icon != null)
            {
                item.SetBlueprint(blueprint);
            }
            else
            {
                item.SetUnknown(unknownSprite);
            }

            // ★見た目だけロック表現
            item.SetLockedVisual(!unlocked);

            // ★操作はロック中は不可
            item.SetInteractable(unlocked);

            // ★クリックで詳細表示（ロックでも詳細は見せたいなら unlocked を外す）
            if (item.button != null)
            {
                var captured = blueprint;

                item.button.onClick.RemoveAllListeners();
                item.button.onClick.AddListener(() =>
                {
                    if (!unlocked) return; // ロック中は押せない想定の保険

                    if (detailUI != null)
                    {
                        detailUI.Show(captured);
                    }
                    else
                    {
                        Debug.LogWarning("DesignBlueprintDetailUI is not assigned.");
                    }
                });
            }
            else
            {
                Debug.LogWarning("DesignBlueprintButtonUI button reference is missing.");
            }
        }
    }
}