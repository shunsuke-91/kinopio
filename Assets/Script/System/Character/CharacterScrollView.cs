using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterScrollView : MonoBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private TeamSetupUI teamSetupUI;

    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Transform contentParent;

    private void Awake()
    {
        if (characterManager == null)
            characterManager = FindFirstObjectByType<CharacterManager>();

        if (teamSetupUI == null)
            teamSetupUI = FindFirstObjectByType<TeamSetupUI>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || characterButtonPrefab == null || characterManager == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        foreach (var ins in characterManager.ownedCharacters)
        {
            if (ins == null || ins.Blueprint == null) continue;

            var buttonObject = Instantiate(characterButtonPrefab, contentParent);
            var image = buttonObject.GetComponentInChildren<Image>();
            var text = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            var button = buttonObject.GetComponent<Button>();

            if (image != null) image.sprite = ins.Blueprint.icon;
            if (text != null) text.text = ins.Blueprint.characterName;

            if (button != null)
            {
                var insCopy = ins; // クロージャ対策
                button.onClick.AddListener(() =>
                {
                    if (teamSetupUI != null)
                        teamSetupUI.AssignCharacterToCurrentSlot(insCopy);
                });
            }
        }
    }
}