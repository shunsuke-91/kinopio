using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("データベース")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDatabase;
    [SerializeField] private BlueprintUnlockDatabase blueprintUnlockDatabase;
    [SerializeField] private UpgradeCostDatabase upgradeCostDatabase;

    private GameState GS => GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // 所持キャラ参照
    // =========================================================
    public IReadOnlyList<CharacterInstance> OwnedCharacters
    {
        get
        {
            var gs = GS;
            if (gs == null || gs.CurrentSave == null || gs.CurrentSave.ownedCharacters == null)
                return new List<CharacterInstance>();
            return gs.CurrentSave.ownedCharacters;
        }
    }

    // =========================================================
    // 解放判定
    // =========================================================
    public bool IsBlueprintUnlocked(string blueprintId)
    {
        var gs = GS;
        if (gs == null) return false;
        if (blueprintUnlockDatabase == null) return false;
        return gs.IsUnlocked(blueprintId, blueprintUnlockDatabase);
    }

    // =========================================================
    // 設計（クラフト）
    // =========================================================
    public bool TryCraft(string blueprintId)
    {
        var gs = GS;
        if (gs == null || gs.CurrentSave == null) return false;
        if (string.IsNullOrEmpty(blueprintId)) return false;

        if (blueprintDatabase == null) return false;
        var blueprint = blueprintDatabase.GetByID(blueprintId);
        if (blueprint == null) return false;

        if (!IsBlueprintUnlocked(blueprintId)) return false;

        var costs = blueprintUnlockDatabase != null
            ? blueprintUnlockDatabase.GetCraftCosts(blueprintId)
            : new List<MaterialStack>();

        if (!gs.HasMaterials(costs)) return false;
        if (!gs.ConsumeMaterials(costs)) return false;

        if (gs.CurrentSave.ownedCharacters == null) gs.CurrentSave.ownedCharacters = new List<CharacterInstance>();

        var instance = new CharacterInstance(blueprintId, 0, blueprintDatabase);
        gs.CurrentSave.ownedCharacters.Add(instance);

        gs.Save();
        return true;
    }

    // =========================================================
    // 強化（instance 単位）
    // =========================================================
    public bool TryUpgrade(string instanceId)
    {
        var gs = GS;
        if (gs == null || gs.CurrentSave == null) return false;
        if (string.IsNullOrEmpty(instanceId)) return false;

        var list = gs.CurrentSave.ownedCharacters;
        if (list == null) return false;

        var instance = list.Find(c => c != null && c.InstanceId == instanceId);
        if (instance == null || instance.IsMaxLevel) return false;

        int nextLevel = instance.Level + 1;

        var costs = upgradeCostDatabase != null
            ? upgradeCostDatabase.GetCosts(instance.BlueprintId, nextLevel)
            : new List<MaterialStack>();

        if (!gs.HasMaterials(costs)) return false;
        if (!gs.ConsumeMaterials(costs)) return false;

        if (!instance.TryLevelUp()) return false;

        gs.Save();
        return true;
    }

    // =========================================================
    // 編成（同じ「インスタンス」を複数スロットに入れない）
    // ※同じ Blueprint は複数体作れる想定なので禁止しない
    // =========================================================
    public void SetTeamSlot(int slotIndex, CharacterInstance instance)
    {
        var gs = GS;
        if (gs == null || gs.CurrentSave == null) return;

        gs.EnsureTeamIdsArray();
        EnsureRuntimeTeamArray();

        if (slotIndex < 0 || slotIndex >= TeamSetupData.MaxSlots) return;

        // 同じ instance を他スロットから外す（同一インスタンス重複禁止）
        if (instance != null)
        {
            for (int i = 0; i < gs.CurrentSave.selectedTeamInstanceIds.Length; i++)
            {
                if (i == slotIndex) continue;
                if (gs.CurrentSave.selectedTeamInstanceIds[i] == instance.InstanceId)
                {
                    gs.CurrentSave.selectedTeamInstanceIds[i] = string.Empty;
                    TeamSetupData.SelectedTeam[i] = null;
                }
            }
        }

        // セーブ側
        gs.CurrentSave.selectedTeamInstanceIds[slotIndex] = instance != null ? instance.InstanceId : string.Empty;

        // ランタイム側（UI用）
        TeamSetupData.SelectedTeam[slotIndex] = instance;

        gs.Save();
    }

    public CharacterInstance[] GetTeamInstances()
    {
        var gs = GS;
        EnsureRuntimeTeamArray();
        if (gs == null || gs.CurrentSave == null)
        {
            return TeamSetupData.SelectedTeam;
        }

        gs.EnsureTeamIdsArray();

        // id → ownedCharacters で引き直して返す（整合性維持）
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string id = gs.CurrentSave.selectedTeamInstanceIds[i];
            TeamSetupData.SelectedTeam[i] = FindOwnedByInstanceId(id);
        }

        return TeamSetupData.SelectedTeam;
    }

    private CharacterInstance FindOwnedByInstanceId(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        var gs = GS;
        var list = gs != null && gs.CurrentSave != null ? gs.CurrentSave.ownedCharacters : null;
        if (list == null) return null;

        var inst = list.Find(c => c != null && c.InstanceId == id);
        if (inst != null) inst.AssignBlueprintDatabase(blueprintDatabase);
        return inst;
    }

    private void EnsureRuntimeTeamArray()
    {
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[TeamSetupData.MaxSlots];
        }
    }
}