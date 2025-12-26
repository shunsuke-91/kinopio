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

        // null 安全化
        if (CurrentSave.progress == null) CurrentSave.progress = new ProgressData();
        if (CurrentSave.materials == null) CurrentSave.materials = new MaterialInventoryData();
        if (CurrentSave.materials.stacks == null) CurrentSave.materials.stacks = new List<MaterialStack>();
        if (CurrentSave.ownedCharacters == null) CurrentSave.ownedCharacters = new List<CharacterInstance>();

        // 素材をロード
        Materials = new MaterialInventory();
        Materials.LoadFromList(CurrentSave.materials.stacks);

        // チーム配列の初期化（セーブ側も合わせる）
        EnsureTeamIdsArray();
    }

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

    public bool IsUnlocked(string blueprintID, BlueprintUnlockDatabase db)
    {
        if (string.IsNullOrEmpty(blueprintID) || db == null) return false;
        int requiredStage = db.GetUnlockStage(blueprintID);
        return HighestClearedStage >= requiredStage;
    }

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

    /// <summary>
    /// Materials → CurrentSave.materials に反映して保存
    /// </summary>
    public void Save()
    {
        if (CurrentSave == null) return;
        if (CurrentSave.materials == null) CurrentSave.materials = new MaterialInventoryData();
        CurrentSave.materials.stacks = Materials.ToSerializableList();
        SaveSystem.Save(CurrentSave);
    }

    /// <summary>
    /// selectedTeamInstanceIds を常に MaxSlots 長に揃える
    /// </summary>
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

        // ランタイム側も初期化
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != n)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[n];
        }
    }
}