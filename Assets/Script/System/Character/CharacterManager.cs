using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("所持キャラ（例：最初は1体だけ）")]
    public List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();

    [Header("Blueprintカタログ（全種類を登録してください）")]
    [SerializeField] private CharacterBlueprint[] blueprintCatalog;

    private const string TeamKeyPrefix = "TEAM_SLOT_"; // TEAM_SLOT_0 ～ TEAM_SLOT_4

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 起動時に「保存済み → TeamSetupData」へ復元
        SyncTeamToRuntimeData();
    }

    // ----------------------------
    // ★ ここが今回の要：保存 → TeamSetupData（復元）
    // ----------------------------
    public void SyncTeamToRuntimeData()
    {
        // TeamSetupData が未初期化の事故対策（念のため）
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterBlueprint[TeamSetupData.MaxSlots];
        }

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string key = TeamKeyPrefix + i;
            string savedId = PlayerPrefs.GetString(key, "");

            if (string.IsNullOrEmpty(savedId))
            {
                TeamSetupData.SelectedTeam[i] = null;
                continue;
            }

            CharacterBlueprint bp = GetBlueprintById(savedId);
            TeamSetupData.SelectedTeam[i] = bp; // 見つからなければ null になります
        }
    }

    // ----------------------------
    // ★ ここが今回の要：TeamSetupData → 保存（更新）
    // ----------------------------
    public void SaveTeamFromRuntimeData()
    {
        if (TeamSetupData.SelectedTeam == null)
        {
            Debug.LogWarning("CharacterManager: TeamSetupData.SelectedTeam が null です。保存を中断します。");
            return;
        }

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string key = TeamKeyPrefix + i;

            CharacterBlueprint bp = TeamSetupData.SelectedTeam[i];
            string id = (bp != null) ? bp.blueprintID : "";

            PlayerPrefs.SetString(key, id);
        }

        PlayerPrefs.Save();
    }

    // ----------------------------
    // BlueprintID → Blueprint を引く
    // ----------------------------
    private CharacterBlueprint GetBlueprintById(string id)
    {
        if (blueprintCatalog == null || blueprintCatalog.Length == 0)
        {
            Debug.LogWarning("CharacterManager: blueprintCatalog が未設定です。Inspectorで登録してください。");
            return null;
        }

        for (int i = 0; i < blueprintCatalog.Length; i++)
        {
            var bp = blueprintCatalog[i];
            if (bp == null) continue;
            if (bp.blueprintID == id) return bp;
        }

        Debug.LogWarning($"CharacterManager: blueprintID '{id}' が blueprintCatalog から見つかりませんでした。");
        return null;
    }
}