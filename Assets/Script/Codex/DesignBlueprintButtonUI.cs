using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DesignBlueprintButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] public Button button;

    public void SetUnknown(Sprite unknownSprite)
    {
        if (icon == null || nameText == null)
        {
            Debug.LogWarning("DesignBlueprintButtonUI is missing references.");
            return;
        }

        icon.sprite = unknownSprite;
        nameText.text = "???";
    }

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
    }

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
