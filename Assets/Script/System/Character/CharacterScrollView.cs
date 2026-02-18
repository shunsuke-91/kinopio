using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScrollView : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button characterButtonPrefab;

    [Header("Databases")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDatabase;

    [Header("Optional UI")]
    [SerializeField] private TeamSetupUI teamSetupUI;

    [Header("Runtime")]
    [SerializeField] private GameState gameState;

    private void Start()
    {
        if (gameState == null)
        {
            gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
        }

        Refresh();
    }

    private void OnEnable()
    {
        if (Application.isPlaying) Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || characterButtonPrefab == null)
        {
            Debug.LogWarning("CharacterScrollView: references missing (contentParent / characterButtonPrefab).");
            return;
        }

        if (gameState == null || gameState.CurrentSave == null || gameState.CurrentSave.ownedCharacters == null)
        {
            Debug.LogWarning("CharacterScrollView: GameState or save data missing.");
            Clear();
            return;
        }

        // NonSerialized対策（保険）
        if (blueprintDatabase != null)
        {
            foreach (var c in gameState.CurrentSave.ownedCharacters)
            {
                if (c == null) continue;
                c.AssignBlueprintDatabase(blueprintDatabase);
            }
        }

        Clear();

        var list = gameState.CurrentSave.ownedCharacters;
        Debug.Log("ownedCharacters count = " + list.Count);

        for (int i = 0; i < list.Count; i++)
        {
            var inst = list[i];
            if (inst == null) continue;

            var btn = Instantiate(characterButtonPrefab, contentParent);

            // ★重要：背景Imageではなく「Icon」だけを取る
            var iconTr = btn.transform.Find("Icon");
            var iconImg = iconTr != null ? iconTr.GetComponent<Image>() : null;

            // テキストも名前で取る（無ければ従来通り子から拾う）
            var textTr = btn.transform.Find("NameText");
            var txt = textTr != null ? textTr.GetComponent<TMP_Text>() : btn.GetComponentInChildren<TMP_Text>(true);

            var bp = inst.Blueprint;

            if (iconImg != null)
            {
                iconImg.sprite = bp != null ? bp.icon : null;

                // 潰れ対策（ここで必ず有効化）
                iconImg.preserveAspect = true;
                iconImg.type = Image.Type.Simple;
            }
            else
            {
                Debug.LogWarning("CharacterScrollView: Button prefab has no child Image named 'Icon'.");
            }

            if (txt != null)
            {
                string name = bp != null ? bp.characterName : inst.BlueprintId;
                txt.text = $"{name}  Lv.{inst.Level}";
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (teamSetupUI != null) teamSetupUI.OnCharacterSelected(inst);
            });
        }
    }

    private void Clear()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}