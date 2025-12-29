using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesignBlueprintButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] public Button button;

    [Header("Locked Visual")]
    [SerializeField, Range(0f, 1f)] private float lockedBrightness = 0.55f;
    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.35f;

    // -----------------------------
    // Blueprint が存在しない場合のみ使用
    // -----------------------------
    public void SetUnknown(Sprite unknownSprite)
    {
        if (icon == null || nameText == null)
        {
            Debug.LogWarning("DesignBlueprintButtonUI is missing references.");
            return;
        }

        icon.sprite = unknownSprite;
        icon.color = Color.white; // Unknown は普通に表示
        nameText.text = "???";
    }

    // -----------------------------
    // Blueprint 表示（★常に icon を差し替える）
    // -----------------------------
    public void SetBlueprint(CharacterBlueprint bp)
    {
        if (icon == null || nameText == null)
        {
            Debug.LogWarning("DesignBlueprintButtonUI is missing references.");
            return;
        }

        if (bp == null)
        {
            Debug.LogWarning("SetBlueprint called with null blueprint.");
            return;
        }

        icon.sprite = bp.icon;
        nameText.text = bp.characterName;

        // デフォルトは「解放状態の見た目」
        SetLockedVisual(false);
    }

    // -----------------------------
    // ロック / 解放 表現（見た目だけ）
    // -----------------------------
    public void SetLockedVisual(bool locked)
    {
        if (icon == null) return;

        if (locked)
        {
            icon.color = new Color(
                lockedBrightness,
                lockedBrightness,
                lockedBrightness,
                lockedAlpha
            );
        }
        else
        {
            icon.color = Color.white;
        }
    }

    // -----------------------------
    // ボタン有効 / 無効
    // -----------------------------
    public void SetInteractable(bool on)
    {
        if (button == null)
        {
            Debug.LogWarning("DesignBlueprintButtonUI button reference is missing.");
            return;
        }

        button.interactable = on;
    }
}