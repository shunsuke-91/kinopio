using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesignOwnedCharacterButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] public Button button;

    [Header("見た目（任意）")]
    [SerializeField] private CanvasGroup canvasGroup; // 無くてもOK。あるとグレーアウトが楽

    public void Set(CharacterInstance instance, bool selectable)
    {
        if (instance == null) return;

        var bp = instance.Blueprint; // AssignBlueprintDatabase 済み前提
        if (icon != null) icon.sprite = bp != null ? bp.icon : null;
        if (nameText != null) nameText.text = bp != null ? bp.characterName : instance.BlueprintId;
        if (levelText != null) levelText.text = $"Lv {instance.Level}";

        if (button != null) button.interactable = selectable;

        // グレーアウト
        if (canvasGroup != null)
        {
            canvasGroup.alpha = selectable ? 1f : 0.45f;
        }
        else
        {
            // CanvasGroupが無い場合：アイコンと文字を薄くする（最低限）
            if (icon != null)
            {
                var c = icon.color;
                c.a = selectable ? 1f : 0.45f;
                icon.color = c;
            }
            if (nameText != null)
            {
                var c = nameText.color;
                c.a = selectable ? 1f : 0.45f;
                nameText.color = c;
            }
            if (levelText != null)
            {
                var c = levelText.color;
                c.a = selectable ? 1f : 0.45f;
                levelText.color = c;
            }
        }
    }
}