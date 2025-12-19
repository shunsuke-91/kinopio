using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("保存ファイル名")]
    [SerializeField] private string saveFileName = "character_save.json";

    [Header("データベース")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDatabase;
    [SerializeField] private BlueprintUnlockDatabase blueprintUnlockDatabase;
    [SerializeField] private CraftCostDatabase craftCostDatabase;
    [SerializeField] private UpgradeCostDatabase upgradeCostDatabase;

    private readonly List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();
    private CharacterInstance[] teamSlots = new CharacterInstance[TeamSetupData.MaxSlots];
    private readonly MaterialInventory materialInventory = new MaterialInventory();

    public IReadOnlyList<CharacterInstance> OwnedCharacters => ownedCharacters;
    public MaterialInventory Materials => materialInventory;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSaveData();
    }

    public bool IsBlueprintUnlocked(string blueprintId)
    {
        if (blueprintUnlockDatabase == null) return false;
        var gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
        if (gameState == null)
        {
            Debug.LogWarning("GameState not found; cannot evaluate blueprint unlocks.");
            return false;
        }

        return gameState.IsUnlocked(blueprintId, blueprintUnlockDatabase);
    }

    public bool TryCraft(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId)) return false;
        if (!IsBlueprintUnlocked(blueprintId)) return false;

        var blueprint = blueprintDatabase != null ? blueprintDatabase.GetByID(blueprintId) : null;
        if (blueprint == null) return false;

        var costs = craftCostDatabase != null ? craftCostDatabase.GetCosts(blueprintId) : null;
        if (!HasEnoughMaterials(costs)) return false;

        ConsumeMaterials(costs);
        var instance = new CharacterInstance(blueprintId, 0, blueprintDatabase);
        ownedCharacters.Add(instance);
        SaveGame();
        return true;
    }

    public bool TryUpgrade(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return false;
        var instance = ownedCharacters.Find(c => c != null && c.InstanceId == instanceId);
        if (instance == null || instance.IsMaxLevel) return false;

        int nextLevel = instance.Level + 1;
        var costs = upgradeCostDatabase != null ? upgradeCostDatabase.GetCosts(instance.BlueprintId, nextLevel) : null;
        if (!HasEnoughMaterials(costs)) return false;

        ConsumeMaterials(costs);
        if (instance.TryLevelUp())
        {
            SaveGame();
            return true;
        }

        return false;
    }

    public void SetTeamSlot(int slotIndex, CharacterInstance instance)
    {
        if (slotIndex < 0 || slotIndex >= TeamSetupData.MaxSlots) return;
        EnsureTeamArray();

        teamSlots[slotIndex] = instance;
        TeamSetupData.SelectedTeam[slotIndex] = instance;
        SaveGame();
    }

    public CharacterInstance[] GetTeamInstances()
    {
        EnsureTeamArray();
        return teamSlots;
    }

    public void SyncTeamToRuntimeData()
    {
        SaveGame();
    }

    private bool HasEnoughMaterials(MaterialCost[] costs)
    {
        if (costs == null || costs.Length == 0) return false;
        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId) || cost.amount <= 0) return false;
            if (materialInventory.GetCount(cost.materialId) < cost.amount) return false;
        }
        return true;
    }

    private void ConsumeMaterials(MaterialCost[] costs)
    {
        if (costs == null) return;
        foreach (var cost in costs)
        {
            if (cost == null || string.IsNullOrEmpty(cost.materialId) || cost.amount <= 0) continue;
            materialInventory.TryConsume(cost.materialId, cost.amount);
        }
    }

    private void LoadSaveData()
    {
        EnsureTeamArray();
        CharacterSaveData data = ReadSaveFromFile();

        ownedCharacters.Clear();
        if (data != null && data.ownedCharacters != null)
        {
            foreach (var instance in data.ownedCharacters)
            {
                if (instance == null || string.IsNullOrEmpty(instance.BlueprintId)) continue;
                instance.AssignBlueprintDatabase(blueprintDatabase);
                ownedCharacters.Add(instance);
            }
        }

        materialInventory.LoadFromList(data != null ? data.materials : null);

        ApplyTeamData(data);

        if (data == null)
        {
            SaveGame();
        }
    }

    private CharacterSaveData ReadSaveFromFile()
    {
        var path = SavePath;
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<CharacterSaveData>(json);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private void SaveGame()
    {
        var data = new CharacterSaveData
        {
            materials = materialInventory.ToSerializableList(),
            ownedCharacters = new List<CharacterInstance>(ownedCharacters),
            teamInstanceIds = BuildTeamInstanceIdList(),
            version = 1
        };

        var path = SavePath;
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        catch (IOException)
        {
            // 何もしない（例外を投げない）
        }
    }

    private void ApplyTeamData(CharacterSaveData data)
    {
        EnsureTeamArray();
        if (data == null || data.teamInstanceIds == null)
        {
            ClearTeamSlots();
            return;
        }

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string id = i < data.teamInstanceIds.Count ? data.teamInstanceIds[i] : string.Empty;
            var instance = ownedCharacters.Find(c => c != null && c.InstanceId == id);
            teamSlots[i] = instance;
            TeamSetupData.SelectedTeam[i] = instance;
        }
    }

    private void ClearTeamSlots()
    {
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            teamSlots[i] = null;
            TeamSetupData.SelectedTeam[i] = null;
        }
    }

    private void EnsureTeamArray()
    {
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[TeamSetupData.MaxSlots];
        }

        if (teamSlots == null || teamSlots.Length != TeamSetupData.MaxSlots)
        {
            teamSlots = new CharacterInstance[TeamSetupData.MaxSlots];
        }
    }

    private List<string> BuildTeamInstanceIdList()
    {
        var result = new List<string>(TeamSetupData.MaxSlots);
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            var instance = teamSlots != null && i < teamSlots.Length ? teamSlots[i] : null;
            result.Add(instance != null ? instance.InstanceId : string.Empty);
        }
        return result;
    }
}
