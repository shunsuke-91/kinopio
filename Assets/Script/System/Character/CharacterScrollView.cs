using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterScrollView : MonoBehaviour
{
    [SerializeField] private CharacterManager characterManager;
    [SerializeField] private GameObject characterButtonPrefab;
    [SerializeField] private Transform contentParent;

    [Header("チーム編成 UI への参照")]
    [SerializeField] private TeamSetupUI teamSetupUI;

    private void Awake()
    {
        if (characterManager == null)
        {
            // Unity 2022 以降推奨の API
            characterManager = Object.FindFirstObjectByType<CharacterManager>();
        }

        if (teamSetupUI == null)
        {
            teamSetupUI = Object.FindFirstObjectByType<TeamSetupUI>();
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
            Debug.LogWarning("CharacterScrollView: 必要な参照が足りません。");
            return;
        }

        // 既存のボタンを全削除
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        // 所持キャラをボタンとして並べる
        foreach (var character in characterManager.ownedCharacters)
        {
            if (character == null || character.Blueprint == null)
                continue;

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
                // ループ変数のキャラをローカル変数にキャプチャ
                CharacterBlueprint bp = character.Blueprint;

                button.onClick.AddListener(() =>
                {
                    if (teamSetupUI != null)
                    {
                        teamSetupUI.AssignCharacterToCurrentSlot(bp);
                    }
                    else
                    {
                        Debug.Log($"Selected character (UI 未設定): {bp.characterName}");
                    }
                });
            }
        }
    }
}