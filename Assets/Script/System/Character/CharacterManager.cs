using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("保存ファイル名")]
    [SerializeField] private string saveFileName = "player_save.json";

    [Header("データベース")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDatabase;
    [SerializeField] private BlueprintUnlockDatabase blueprintUnlockDatabase;
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

    // =========================================================
    // 解放判定
    // =========================================================
    public bool IsBlueprintUnlocked(string blueprintId)
    {
        if (blueprintUnlockDatabase == null) return false;

        var gameState = GameState.Instance != null ? GameState.Instance : FindFirstObjectByType<GameState>();
        if (gameState == null)
        {
            Debug.LogWarning("GameState が見つからないため、解放判定ができません。");
            return false;
        }

        return gameState.IsUnlocked(blueprintId, blueprintUnlockDatabase);
    }

    // =========================================================
    // 設計（クラフト）
    // =========================================================
    public bool TryCraft(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId)) return false;
        if (!IsBlueprintUnlocked(blueprintId)) return false;

        var blueprint = blueprintDatabase != null ? blueprintDatabase.GetByID(blueprintId) : null;
        if (blueprint == null) return false;

        // BlueprintUnlockDatabase に統合済み：クラフトコストは List<MaterialStack>
        var costs = blueprintUnlockDatabase != null
            ? blueprintUnlockDatabase.GetCraftCosts(blueprintId)
            : new List<MaterialStack>();

        if (!HasEnoughMaterials(costs)) return false;

        ConsumeMaterials(costs);

        var instance = new CharacterInstance(blueprintId, 0, blueprintDatabase);
        ownedCharacters.Add(instance);

        SaveGame();
        return true;
    }

    // =========================================================
    // 強化
    // =========================================================
    public bool TryUpgrade(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return false;

        var instance = ownedCharacters.Find(c => c != null && c.InstanceId == instanceId);
        if (instance == null || instance.IsMaxLevel) return false;

        int nextLevel = instance.Level + 1;

        // UpgradeCostDatabase は List<MaterialStack> を返す前提
        var costs = upgradeCostDatabase != null
            ? upgradeCostDatabase.GetCosts(instance.BlueprintId, nextLevel)
            : new List<MaterialStack>();

        if (!HasEnoughMaterials(costs)) return false;

        ConsumeMaterials(costs);

        if (instance.TryLevelUp())
        {
            SaveGame();
            return true;
        }

        return false;
    }

    // =========================================================
    // 編成
    // =========================================================
    public void SetTeamSlot(int slotIndex, CharacterInstance instance)
    {
        if (slotIndex < 0 || slotIndex >= TeamSetupData.MaxSlots) return;
        EnsureTeamArray();

        teamSlots[slotIndex] = instance;

        // TeamSetupData 側も同期（あなたの現状は CharacterInstance[] 前提）
        if (TeamSetupData.SelectedTeam != null && slotIndex < TeamSetupData.SelectedTeam.Length)
        {
            TeamSetupData.SelectedTeam[slotIndex] = instance;
        }

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

    // =========================================================
    // 素材チェック / 消費（List<MaterialStack>）
    // =========================================================
    private bool HasEnoughMaterials(List<MaterialStack> costs)
    {
        if (costs == null || costs.Count == 0) return false;

        foreach (var cost in costs)
        {
            if (cost == null) return false;
            if (string.IsNullOrEmpty(cost.materialId)) return false;
            if (cost.count <= 0) return false;

            if (materialInventory.GetCount(cost.materialId) < cost.count)
                return false;
        }

        return true;
    }

    private void ConsumeMaterials(List<MaterialStack> costs)
    {
        if (costs == null) return;

        foreach (var cost in costs)
        {
            if (cost == null) continue;
            if (string.IsNullOrEmpty(cost.materialId) || cost.count <= 0) continue;

            materialInventory.TryConsume(cost.materialId, cost.count);
        }
    }

    // =========================================================
    // セーブ / ロード（PlayerSaveData に統一）
    // =========================================================
    private void LoadSaveData()
    {
        EnsureTeamArray();

        var data = ReadSaveFromFile<PlayerSaveData>();
        ownedCharacters.Clear();

        // ownedCharacters 復元
        var savedOwned = GetFieldOrPropertyValue(data, "ownedCharacters") as IList;
        if (savedOwned != null)
        {
            foreach (var obj in savedOwned)
            {
                var inst = obj as CharacterInstance;
                if (inst == null) continue;
                if (string.IsNullOrEmpty(inst.BlueprintId)) continue;

                inst.AssignBlueprintDatabase(blueprintDatabase);
                ownedCharacters.Add(inst);
            }
        }

        // materials 復元（PlayerSaveData.materials の型が揺れても拾う）
        var savedMaterials = ExtractMaterialStacksFromPlayerSave(data);
        materialInventory.LoadFromList(savedMaterials);

        // team 復元（selectedTeamInstanceIds / teamInstanceIds 両対応）
        var teamIds = ExtractTeamIdsFromPlayerSave(data);
        ApplyTeamIds(teamIds);

        // 初回セーブ（ファイルが無い場合）
        if (data == null)
        {
            SaveGame();
        }
    }

    private void SaveGame()
    {
        EnsureTeamArray();

        PlayerSaveData data = CreateOrNewPlayerSaveData();

        // ownedCharacters
        SetFieldOrPropertyValue(data, "ownedCharacters", new List<CharacterInstance>(ownedCharacters));

        // materials（materials の型が List<MaterialStack> / MaterialInventoryData どちらでも入れる）
        InjectMaterialStacksToPlayerSave(data, materialInventory.ToSerializableList());

        // team ids
        var idsArray = BuildTeamInstanceIdArray();
        if (!TrySetTeamIdsToPlayerSave(data, idsArray))
        {
            // どうしても入らなければ、最低限ログだけ（クラッシュさせない）
            Debug.LogWarning("PlayerSaveData に team ids を格納できませんでした。フィールド名を確認してください（selectedTeamInstanceIds 等）。");
        }

        // 書き込み
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
            // 例外は投げない
        }
    }

    // =========================================================
    // チーム適用
    // =========================================================
    private void ApplyTeamIds(string[] ids)
    {
        EnsureTeamArray();

        if (ids == null || ids.Length == 0)
        {
            ClearTeamSlots();
            return;
        }

        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            string id = i < ids.Length ? ids[i] : string.Empty;
            var instance = ownedCharacters.Find(c => c != null && c.InstanceId == id);

            teamSlots[i] = instance;

            if (TeamSetupData.SelectedTeam != null && i < TeamSetupData.SelectedTeam.Length)
            {
                TeamSetupData.SelectedTeam[i] = instance;
            }
        }
    }

    private void ClearTeamSlots()
    {
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            teamSlots[i] = null;

            if (TeamSetupData.SelectedTeam != null && i < TeamSetupData.SelectedTeam.Length)
            {
                TeamSetupData.SelectedTeam[i] = null;
            }
        }
    }

    private void EnsureTeamArray()
    {
        // TeamSetupData.SelectedTeam は CharacterInstance[] 前提
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterInstance[TeamSetupData.MaxSlots];
        }

        if (teamSlots == null || teamSlots.Length != TeamSetupData.MaxSlots)
        {
            teamSlots = new CharacterInstance[TeamSetupData.MaxSlots];
        }
    }

    private string[] BuildTeamInstanceIdArray()
    {
        EnsureTeamArray();

        var result = new string[TeamSetupData.MaxSlots];
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            var instance = teamSlots != null && i < teamSlots.Length ? teamSlots[i] : null;
            result[i] = instance != null ? instance.InstanceId : string.Empty;
        }
        return result;
    }

    // =========================================================
    // ファイルIO（汎用）
    // =========================================================
    private T ReadSaveFromFile<T>() where T : class
    {
        var path = SavePath;
        if (!File.Exists(path)) return null;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<T>(json);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private PlayerSaveData CreateOrNewPlayerSaveData()
    {
        // PlayerSaveData に CreateNew() があれば使う
        var type = typeof(PlayerSaveData);
        var mi = type.GetMethod("CreateNew", BindingFlags.Public | BindingFlags.Static);
        if (mi != null)
        {
            try
            {
                var obj = mi.Invoke(null, null) as PlayerSaveData;
                if (obj != null) return obj;
            }
            catch { }
        }

        // 無ければ new
        try
        {
            return new PlayerSaveData();
        }
        catch
        {
            // ここに来るなら PlayerSaveData 側が new 不可（private ctor など）
            // その場合は落とさず警告だけ
            Debug.LogError("PlayerSaveData の生成に失敗しました。CreateNew() または public なコンストラクタを用意してください。");
            return null;
        }
    }

    // =========================================================
    // PlayerSaveData から materials / team ids を吸い出す（型揺れ対策）
    // =========================================================
    private List<MaterialStack> ExtractMaterialStacksFromPlayerSave(PlayerSaveData data)
    {
        // 1) materials が List<MaterialStack> の場合
        var direct = GetFieldOrPropertyValue(data, "materials") as List<MaterialStack>;
        if (direct != null) return direct;

        // 2) materials が MaterialInventoryData 等で、その中に List<MaterialStack> がある場合
        var materialsObj = GetFieldOrPropertyValue(data, "materials");
        if (materialsObj != null)
        {
            // よくある名前を順に探す
            var listA = GetFieldOrPropertyValue(materialsObj, "items") as List<MaterialStack>;
            if (listA != null) return listA;

            var listB = GetFieldOrPropertyValue(materialsObj, "stacks") as List<MaterialStack>;
            if (listB != null) return listB;

            var listC = GetFieldOrPropertyValue(materialsObj, "list") as List<MaterialStack>;
            if (listC != null) return listC;
        }

        return new List<MaterialStack>();
    }

    private void InjectMaterialStacksToPlayerSave(PlayerSaveData data, List<MaterialStack> stacks)
    {
        if (data == null) return;

        // 1) materials が List<MaterialStack> として直接入るならそれで終了
        if (TrySetFieldOrPropertyValue(data, "materials", stacks)) return;

        // 2) materials が MaterialInventoryData 等の場合：その中の items/stacks に入れる
        var materialsObj = GetFieldOrPropertyValue(data, "materials");
        if (materialsObj == null)
        {
            // materials フィールドが null の場合、生成できるなら生成
            var field = typeof(PlayerSaveData).GetField("materials", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType != typeof(List<MaterialStack>))
            {
                try
                {
                    materialsObj = Activator.CreateInstance(field.FieldType);
                    field.SetValue(data, materialsObj);
                }
                catch { }
            }
        }

        if (materialsObj != null)
        {
            if (TrySetFieldOrPropertyValue(materialsObj, "items", stacks)) return;
            if (TrySetFieldOrPropertyValue(materialsObj, "stacks", stacks)) return;
            if (TrySetFieldOrPropertyValue(materialsObj, "list", stacks)) return;
        }

        Debug.LogWarning("PlayerSaveData.materials に MaterialStack リストを格納できませんでした。MaterialInventoryData 側のフィールド名（items等）を確認してください。");
    }

    private string[] ExtractTeamIdsFromPlayerSave(PlayerSaveData data)
    {
        if (data == null) return Array.Empty<string>();

        // 1) selectedTeamInstanceIds : string[]
        var a = GetFieldOrPropertyValue(data, "selectedTeamInstanceIds") as string[];
        if (a != null) return a;

        // 2) teamInstanceIds : List<string>（旧名）
        var b = GetFieldOrPropertyValue(data, "teamInstanceIds") as List<string>;
        if (b != null) return b.ToArray();

        // 3) selectedTeamIds 等の別名
        var c = GetFieldOrPropertyValue(data, "selectedTeamIds") as string[];
        if (c != null) return c;

        return Array.Empty<string>();
    }

    private bool TrySetTeamIdsToPlayerSave(PlayerSaveData data, string[] idsArray)
    {
        if (data == null) return false;

        // selectedTeamInstanceIds : string[]
        if (TrySetFieldOrPropertyValue(data, "selectedTeamInstanceIds", idsArray)) return true;

        // teamInstanceIds : List<string>
        if (TrySetFieldOrPropertyValue(data, "teamInstanceIds", new List<string>(idsArray))) return true;

        // selectedTeamIds : string[]
        if (TrySetFieldOrPropertyValue(data, "selectedTeamIds", idsArray)) return true;

        return false;
    }

    // =========================================================
    // 反射ユーティリティ
    // =========================================================
    private static object GetFieldOrPropertyValue(object obj, string name)
    {
        if (obj == null || string.IsNullOrEmpty(name)) return null;

        var t = obj.GetType();

        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null)
        {
            try { return p.GetValue(obj); } catch { }
        }

        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            try { return f.GetValue(obj); } catch { }
        }

        return null;
    }

    private static void SetFieldOrPropertyValue(object obj, string name, object value)
    {
        if (obj == null || string.IsNullOrEmpty(name)) return;

        var t = obj.GetType();

        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.CanWrite)
        {
            try { p.SetValue(obj, value); return; } catch { }
        }

        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            try { f.SetValue(obj, value); return; } catch { }
        }
    }

    private static bool TrySetFieldOrPropertyValue(object obj, string name, object value)
    {
        if (obj == null || string.IsNullOrEmpty(name)) return false;

        var t = obj.GetType();

        var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.CanWrite)
        {
            try
            {
                if (value == null || p.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    p.SetValue(obj, value);
                    return true;
                }
            }
            catch { }
        }

        var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null)
        {
            try
            {
                if (value == null || f.FieldType.IsAssignableFrom(value.GetType()))
                {
                    f.SetValue(obj, value);
                    return true;
                }
            }
            catch { }
        }

        return false;
    }
}