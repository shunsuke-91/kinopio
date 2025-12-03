using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterScrollView : MonoBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Transform contentParent;

    private void Awake()
    {
        if (characterManager == null)
        {
            characterManager = FindObjectOfType<CharacterManager>();
        }
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || characterButtonPrefab == null || characterManager == null)
        {
            return;
        }

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        foreach (var character in characterManager.ownedCharacters)
        {
            if (character?.Blueprint == null)
            {
                continue;
            }

            var buttonObject = Instantiate(characterButtonPrefab, contentParent);
            var image = buttonObject.GetComponentInChildren<Image>();
            var text = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            var button = buttonObject.GetComponent<Button>();

            if (image != null)
            {
                image.sprite = character.Blueprint.icon;
            }

            if (text != null)
            {
                text.text = character.Blueprint.characterName;
            }

            if (button != null)
            {
                string nameCopy = character.Blueprint.characterName;
                button.onClick.AddListener(() => Debug.Log($"Selected character: {nameCopy}"));
            }
        }
    }
}
