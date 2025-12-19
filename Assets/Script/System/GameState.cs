using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

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

        Materials = new MaterialInventory();

        // CurrentSave.materials が MaterialInventoryData 前提（stacks を持つ）
        if (CurrentSave != null && CurrentSave.materials != null)
        {
            Materials.LoadFromList(CurrentSave.materials.stacks);
        }
        else
        {
            Materials.LoadFromList(null);
        }
    }

    public int HighestClearedStage
    {
        get => CurrentSave != null && CurrentSave.progress != null ? CurrentSave.progress.highestClearedStage : 0;
        set
        {
            if (CurrentSave == null || CurrentSave.progress == null)
            {
                Debug.LogWarning("GameState not initialized; cannot set HighestClearedStage.");
                return;
            }

            CurrentSave.progress.highestClearedStage = value;
            SaveMaterials();
            SaveSystem.Save(CurrentSave);
        }
    }

    public bool IsUnlocked(string blueprintID, BlueprintUnlockDatabase db)
    {
        if (string.IsNullOrEmpty(blueprintID)) return false;
        if (db == null)
        {
            Debug.LogWarning("BlueprintUnlockDatabase is not assigned.");
            return false;
        }

        int requiredStage = db.GetUnlockStage(blueprintID);
        return HighestClearedStage >= requiredStage;
    }

    public bool CanCraft(string blueprintID, BlueprintUnlockDatabase db)
    {
        if (!IsUnlocked(blueprintID, db)) return false;
        var costs = db.GetCraftCosts(blueprintID);
        return HasMaterials(costs);
    }

    /// <summary>
    /// ここは「設計(クラフト)」の実処理。
    /// 素材を消費し、CharacterInstance を ownedCharacters に追加する。
    /// </summary>
    public bool Craft(string blueprintID, CharacterBlueprintDatabase blueprintDb, BlueprintUnlockDatabase unlockDb)
    {
        if (string.IsNullOrEmpty(blueprintID)) return false;

        if (blueprintDb == null)
        {
            Debug.LogWarning("CharacterBlueprintDatabase is not assigned.");
            return false;
        }
        if (unlockDb == null)
        {
            Debug.LogWarning("BlueprintUnlockDatabase is not assigned.");
            return false;
        }
        if (CurrentSave == null)
        {
            Debug.LogWarning("GameState save data missing.");
            return false;
        }
        if (CurrentSave.ownedCharacters == null)
        {
            CurrentSave.ownedCharacters = new List<CharacterInstance>();
        }

        var blueprint = blueprintDb.GetByID(blueprintID);
        if (blueprint == null)
        {
            Debug.LogWarning($"Blueprint not found for id: {blueprintID}");
            return false;
        }

        if (!IsUnlocked(blueprintID, unlockDb)) return false;

        var costs = unlockDb.GetCraftCosts(blueprintID);
        if (!HasMaterials(costs)) return false;
        if (!ConsumeMaterials(costs)) return false;

        // ★修正点：CharacterInstanceData ではなく CharacterInstance に統一
        var instance = new CharacterInstance(blueprintID, 0, blueprintDb);

        CurrentSave.ownedCharacters.Add(instance);

        SaveMaterials();
        SaveSystem.Save(CurrentSave);
        return true;
    }

    private bool HasMaterials(List<MaterialStack> costs)
    {
        if (costs == null || costs.Count == 0) return true;
        if (Materials == null)
        {
            Debug.LogWarning("MaterialInventory is not initialized.");
            return false;
        }

        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            int owned = Materials.GetCount(cost.materialId);
            if (owned < cost.count) return false;
        }

        return true;
    }

    private bool ConsumeMaterials(List<MaterialStack> costs)
    {
        if (costs == null || costs.Count == 0) return true;
        if (!HasMaterials(costs)) return false;

        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId)) continue;
            if (!Materials.TryConsume(cost.materialId, cost.count))
            {
                Debug.LogWarning("Failed to consume materials after passing availability check.");
                return false;
            }
        }

        return true;
    }

    private void SaveMaterials()
    {
        if (CurrentSave == null || CurrentSave.materials == null) return;
        CurrentSave.materials.stacks = Materials.ToSerializableList();
    }
}