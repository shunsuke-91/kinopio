using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static PlayerSaveData LoadOrCreate()
    {
        if (!File.Exists(SavePath))
        {
            var created = PlayerSaveData.CreateNew();
            Save(created);
            return created;
        }

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<PlayerSaveData>(json);
            if (data == null)
            {
                return HandleCorrupt();
            }

            EnsureDefaults(data);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to load save data: {ex.Message}");
            return HandleCorrupt();
        }
    }

    public static void Save(PlayerSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveSystem.Save called with null data.");
            return;
        }

        EnsureDefaults(data);
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    private static PlayerSaveData HandleCorrupt()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var backupName = $"save_corrupt_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var backupPath = Path.Combine(Application.persistentDataPath, backupName);
                File.Move(SavePath, backupPath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to backup corrupt save: {ex.Message}");
        }

        var created = PlayerSaveData.CreateNew();
        Save(created);
        return created;
    }

    private static void EnsureDefaults(PlayerSaveData data)
    {
        if (data.progress == null) data.progress = new ProgressData();
        if (data.materials == null) data.materials = new MaterialInventoryData();
        if (data.materials.stacks == null) data.materials.stacks = new System.Collections.Generic.List<MaterialStack>();
        if (data.ownedCharacters == null) data.ownedCharacters = new System.Collections.Generic.List<CharacterInstanceData>();
        if (data.selectedTeamInstanceIds == null) data.selectedTeamInstanceIds = Array.Empty<string>();
    }
}
