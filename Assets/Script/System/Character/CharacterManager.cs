using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load(); // ★保存データを読み込む
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCharacter(CharacterBlueprint bp)
    {
        CharacterInstance ins = new CharacterInstance
        {
            instanceID = System.Guid.NewGuid().ToString(),
            blueprintID = bp.id
        };

        ownedCharacters.Add(ins);
        Save();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(new Wrapper { list = ownedCharacters });
        PlayerPrefs.SetString("CHAR_DATA", json);
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString("CHAR_DATA", "");
        if (json == "") return;

        ownedCharacters = JsonUtility.FromJson<Wrapper>(json).list;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<CharacterInstance> list;
    }
}