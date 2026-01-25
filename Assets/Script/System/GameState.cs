using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [Header("Databases")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDb;

    public PlayerSaveData CurrentSave { get; private set; }
    public MaterialInventory Materials { get; private set; } = new MaterialInventory();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentSave = SaveSystem.LoadOrCreate();

        // null 安全化
        if (CurrentSave.progress == null) CurrentSave.progress = new ProgressData();
        if (CurrentSave.materials == null) CurrentSave.materials = new MaterialInventoryData();
        if (CurrentSave.materials.stacks == null) CurrentSave.materials.stacks = new List<MaterialStack>();
        if (CurrentSave.ownedCharacters == null) CurrentSave.ownedCharacters = new List<CharacterInstance>();
        if (CurrentSave.selectedTeamInstanceIds == null) CurrentSave.selectedTeamInstanceIds = Array.Empty<string>();

        // 素材をロード
        Materials = new MaterialInventory();
        Materials.LoadFromList(CurrentSave.materials.stacks);

        // チーム配列の初期化
        EnsureTeamIdsArray();

        // NonSerialized 対策：BlueprintDB を再注入
        RebindOwnedCharactersBlueprintDb();

        // セーブの編成IDからランタイムを復元
        RestoreRuntimeTeamFromSave();
    }

    // =========================================================
    // Progress
    // =========================================================
    public int HighestClearedStage
    {
        get => CurrentSave != null && CurrentSave.progress != null ? CurrentSave.progress.highestClearedStage : 0;
        set
        {
            if (CurrentSave == null || CurrentSave.progress == null) return;
            CurrentSave.progress.highestClearedStage = value;
            Save();
        }
    }

    // =========================================================
    // Unlock
    // =========================================================
    public bool IsUnlocked(string blueprintID, BlueprintUnlockDatabase db)
    {
        if (string.IsNullOrEmpty(blueprintID) || db == null) return false;
        int requiredStage = db.GetUnlockStage(blueprintID);
        return HighestClearedStage >= requiredStage;
    }

    // =========================================================
    // Materials
    // =========================================================
    public bool HasMaterials(List<MaterialStack> costs)
    {
        if (costs == null || costs.Count == 0) return true;

        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            if (Materials.GetCount(cost.materialId) < cost.count) return false;
        }
        return true;
    }

    public bool ConsumeMaterials(List<MaterialStack> costs)
    {
        if (costs == null || costs.Count == 0) return true;
        if (!HasMaterials(costs)) return false;

        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            if (!Materials.TryConsume(cost.materialId, cost.count)) return false;
        }

        return true;
    }

    // =========================================================
    // Craft (Design)
    // =========================================================
    public bool CanCraft(string blueprintID, BlueprintUnlockDatabase unlockDb)
    {
        if (string.IsNullOrEmpty(blueprintID)) return false;
        if (unlockDb == null) return false;

        if (!IsUnlocked(blueprintID, unlockDb)) return false;

        var costs = unlockDb.GetCraftCosts(blueprintID);
        return HasMaterials(costs);
    }

    /// <summary>
    /// 設計(クラフト)：素材消費→CharacterInstance追加→保存
    /// 同じキャラを複数作れる仕様のまま
    /// </summary>
    public bool Craft(string blueprintID, CharacterBlueprintDatabase runtimeBlueprintDb, BlueprintUnlockDatabase unlockDb)
    {
        Debug.Log($"[GameState] Craft called. blueprintID={blueprintID}");

        if (string.IsNullOrEmpty(blueprintID)) return false;
        if (runtimeBlueprintDb == null)
        {
            Debug.LogWarning("[GameState] Craft failed: runtimeBlueprintDb is null.");
            return false;
        }
        if (unlockDb == null)
        {
            Debug.LogWarning("[GameState] Craft failed: unlockDb is null.");
            return false;
        }
        if (CurrentSave == null)
        {
            Debug.LogWarning("[GameState] Craft failed: save missing.");
            return false;
        }
        if (CurrentSave.ownedCharacters == null) CurrentSave.ownedCharacters = new List<CharacterInstance>();

        var blueprint = runtimeBlueprintDb.GetByID(blueprintID);
        if (blueprint == null)
        {
            Debug.LogWarning($"[GameState] Craft failed: blueprint not found. id={blueprintID}");
            return false;
        }

        if (!IsUnlocked(blueprintID, unlockDb))
        {
            Debug.LogWarning("[GameState] Craft failed: not unlocked.");
            return false;
        }

        var costs = unlockDb.GetCraftCosts(blueprintID);
        if (!HasMaterials(costs))
        {
            Debug.LogWarning("[GameState] Craft failed: not enough materials.");
            return false;
        }
        if (!ConsumeMaterials(costs))
        {
            Debug.LogWarning("[GameState] Craft failed: consume materials failed.");
            return false;
        }

        // ★ CharacterInstance に統一（DBも入れる）
        var instance = new CharacterInstance(blueprintID, 0, runtimeBlueprintDb);
        CurrentSave.ownedCharacters.Add(instance);

        Save();
        Debug.Log($"[GameState] Craft success. Added={blueprintID} ownedCharacters={CurrentSave.ownedCharacters.Count}");
        return true;
    }

    // =========================================================
    // Save
    // =========================================================
    public void Save()
    {
        if (CurrentSave == null) return;
        if (CurrentSave.materials == null) CurrentSave.materials = new MaterialInventoryData();

        CurrentSave.materials.stacks = Materials.ToSerializableList();
        SaveSystem.Save(CurrentSave);
    }

    // =========================================================
    // Team Sync
    // =========================================================
    public void EnsureTeamIdsArray()
    {
        int n = TeamSetupData.MaxSlots;

        if (CurrentSave.selectedTeamInstanceIds == null || CurrentSave.selectedTeamInstanceIds.Length != n)
        {
            var newArr = new string[n];
            if (CurrentSave.selectedTeamInstanceIds != null)
            {
                int copyLen = Mathf.Min(CurrentSave.selectedTeamInstanceIds.Length, n);
                for (int i = 0; i < copyLen; i++) newArr[i] = CurrentSave.selectedTeamInstanceIds[i];
            }
            CurrentSave.selectedTeamInstanceIds = newArr;
        }

        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != n)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[n];
        }
    }

    public void RestoreRuntimeTeamFromSave()
    {
        EnsureTeamIdsArray();
        if (CurrentSave == null || CurrentSave.ownedCharacters == null) return;

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string id = (CurrentSave.selectedTeamInstanceIds != null && i < CurrentSave.selectedTeamInstanceIds.Length)
                ? CurrentSave.selectedTeamInstanceIds[i]
                : string.Empty;

            CharacterInstance found = null;
            if (!string.IsNullOrEmpty(id))
            {
                found = CurrentSave.ownedCharacters.Find(c => c != null && c.InstanceId == id);
            }

            TeamSetupData.SelectedTeam[i] = found;
        }
    }

    public void SaveTeamFromRuntime()
    {
        EnsureTeamIdsArray();

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            var inst = TeamSetupData.SelectedTeam != null && i < TeamSetupData.SelectedTeam.Length
                ? TeamSetupData.SelectedTeam[i]
                : null;

            CurrentSave.selectedTeamInstanceIds[i] = inst != null ? inst.InstanceId : string.Empty;
        }

        Save();
    }

    // =========================================================
    // Rebind DB (NonSerialized対策)
    // =========================================================
    public void RebindOwnedCharactersBlueprintDb()
    {
        if (blueprintDb == null) return;
        if (CurrentSave == null || CurrentSave.ownedCharacters == null) return;

        foreach (var c in CurrentSave.ownedCharacters)
        {
            if (c == null) continue;
            c.AssignBlueprintDatabase(blueprintDb);
        }
    }

    // =========================================================
    // Upgrade
    // =========================================================
    public bool TryUpgradeOwnedCharacter(string instanceId, UpgradeCostDatabase upgradeCostDb)
    {
        if (string.IsNullOrEmpty(instanceId)) return false;
        if (upgradeCostDb == null) return false;
        if (CurrentSave == null || CurrentSave.ownedCharacters == null) return false;

        var instance = CurrentSave.ownedCharacters.Find(c => c != null && c.InstanceId == instanceId);
        if (instance == null || instance.IsMaxLevel) return false;

        int nextLevel = instance.Level + 1;
        var costs = upgradeCostDb.GetCosts(instance.BlueprintId, nextLevel);
        if (costs == null) return false;

        if (!HasMaterials(costs)) return false;
        if (!ConsumeMaterials(costs)) return false;

        if (!instance.TryLevelUp()) return false;

        Save();
        return true;
    }
}