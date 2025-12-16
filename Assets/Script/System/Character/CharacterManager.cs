using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("所持キャラ（例：最初は1体だけ）")]
    public List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();

    [Header("全キャラの Blueprint データベース")]
    [SerializeField] private CharacterBlueprintDatabase blueprintDatabase;

    private const string OwnedKey = "OWNED_CHARACTERS";
    private const string TeamKey = "TEAM_CHARACTERS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (blueprintDatabase == null)
        {
            Debug.LogError("CharacterManager: blueprintDatabase が設定されていません。Inspector で設定してください。");
        }

        LoadOwnedCharacters();
        LoadTeamData();
    }

    public void AddOwnedCharacter(CharacterBlueprint bp)
    {
        if (bp == null) return;

        ownedCharacters.Add(new CharacterInstance(bp));
        SaveOwnedCharacters();
    }

    public void SetTeamSlot(int slotIndex, CharacterBlueprint blueprint)
    {
        if (slotIndex < 0 || slotIndex >= TeamSetupData.MaxSlots)
        {
            Debug.LogWarning($"CharacterManager: スロット {slotIndex} は範囲外です。");
            return;
        }

        EnsureTeamArray();
        TeamSetupData.SelectedTeam[slotIndex] = blueprint;
        SaveTeamData();
    }

    public CharacterBlueprint[] GetTeamBlueprints()
    {
        EnsureTeamArray();
        return TeamSetupData.SelectedTeam;
    }

    private void LoadOwnedCharacters()
    {
        ownedCharacters.Clear();

        string saved = PlayerPrefs.GetString(OwnedKey, string.Empty);
        if (string.IsNullOrEmpty(saved))
        {
            InitializeDefaultOwnedCharacter();
            return;
        }

        string[] ids = saved.Split('|');
        foreach (var id in ids)
        {
            if (string.IsNullOrEmpty(id)) continue;

            var bp = GetBlueprintById(id);
            if (bp != null)
            {
                ownedCharacters.Add(new CharacterInstance(bp));
            }
        }

        if (ownedCharacters.Count == 0)
        {
            InitializeDefaultOwnedCharacter();
        }
    }

    private void SaveOwnedCharacters()
    {
        var ids = ownedCharacters
            .Where(c => c != null && c.Blueprint != null)
            .Select(c => c.Blueprint.blueprintID);

        string joined = string.Join("|", ids);
        PlayerPrefs.SetString(OwnedKey, joined);
        PlayerPrefs.Save();
    }

    private void InitializeDefaultOwnedCharacter()
    {
        if (blueprintDatabase != null && blueprintDatabase.blueprints != null && blueprintDatabase.blueprints.Length > 0)
        {
            var first = blueprintDatabase.blueprints[0];
            if (first != null)
            {
                ownedCharacters.Add(new CharacterInstance(first));
                SaveOwnedCharacters();
            }
        }
    }

    private void LoadTeamData()
    {
        EnsureTeamArray();

        string saved = PlayerPrefs.GetString(TeamKey, string.Empty);
        if (string.IsNullOrEmpty(saved))
        {
            SaveTeamData();
            return;
        }

        string[] ids = saved.Split('|');
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            if (i < ids.Length && !string.IsNullOrEmpty(ids[i]))
            {
                TeamSetupData.SelectedTeam[i] = GetBlueprintById(ids[i]);
            }
            else
            {
                TeamSetupData.SelectedTeam[i] = null;
            }
        }
    }

    private void SaveTeamData()
    {
        EnsureTeamArray();

        string[] ids = new string[TeamSetupData.MaxSlots];
        for (int i = 0; i < TeamSetupData.MaxSlots; i++)
        {
            CharacterBlueprint bp = TeamSetupData.SelectedTeam[i];
            ids[i] = bp != null ? bp.blueprintID : string.Empty;
        }

        string joined = string.Join("|", ids);
        PlayerPrefs.SetString(TeamKey, joined);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 現在の編成内容をセーブデータに反映し、次のシーンで参照できるようにする
    /// </summary>
    public void SyncTeamToRuntimeData()
    {
        SaveTeamData();
    }

    private void EnsureTeamArray()
    {
        if (TeamSetupData.SelectedTeam == null || TeamSetupData.SelectedTeam.Length != TeamSetupData.MaxSlots)
        {
            TeamSetupData.SelectedTeam = new CharacterBlueprint[TeamSetupData.MaxSlots];
        }
    }

    private CharacterBlueprint GetBlueprintById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (blueprintDatabase == null)
        {
            Debug.LogError("CharacterManager: blueprintDatabase が未設定です。");
            return null;
        }

        return blueprintDatabase.GetByID(id);
    }
}
