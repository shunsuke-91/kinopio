using System.Collections.Generic;
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
        // シーン移動で戻ってきた時も最新で描画したい
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

        // BlueprintDBの再注入（NonSerialized対策）
        // ※GameState側でやってるなら無くてもOKだが、ここで保険をかける
        if (blueprintDatabase != null)
        {
            foreach (var c in gameState.CurrentSave.ownedCharacters)
            {
                if (c == null) continue;
                c.AssignBlueprintDatabase(blueprintDatabase);
            }
        }

        Clear();

        // ★ここが重要：GameStateの ownedCharacters（＝実体）を全部並べる
        var list = gameState.CurrentSave.ownedCharacters;
        for (int i = 0; i < list.Count; i++)
        {
            var inst = list[i];
            if (inst == null) continue;

            var btn = Instantiate(characterButtonPrefab, contentParent);

            // 表示（子の Image / TMP_Text を使う想定）
            var img = btn.GetComponentInChildren<Image>(true);
            var txt = btn.GetComponentInChildren<TMP_Text>(true);

            var bp = inst.Blueprint; // AssignBlueprintDatabase済なら取れる
            if (img != null) img.sprite = bp != null ? bp.icon : null;
            if (txt != null)
            {
                string name = bp != null ? bp.characterName : inst.BlueprintId;
                txt.text = $"{name}  Lv.{inst.Level}";
            }

            // クリックで編成UIへ渡す（同じキャラが複数いても、instance単位で別物）
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