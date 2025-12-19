using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public ProgressData progress = new ProgressData();
    public MaterialInventoryData materials = new MaterialInventoryData();
    public List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();
    public string[] selectedTeamInstanceIds = Array.Empty<string>();

    public static PlayerSaveData CreateNew()
    {
        return new PlayerSaveData
        {
            progress = new ProgressData(),
            materials = new MaterialInventoryData(),
            ownedCharacters = new List<CharacterInstance>(),
            selectedTeamInstanceIds = Array.Empty<string>()
        };
    }
}
