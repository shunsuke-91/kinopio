using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public ProgressData progress = new ProgressData();
    public MaterialInventoryData materials = new MaterialInventoryData();
    public List<CharacterInstanceData> ownedCharacters = new List<CharacterInstanceData>();
    public string[] selectedTeamInstanceIds = Array.Empty<string>();

    public static PlayerSaveData CreateNew()
    {
        return new PlayerSaveData
        {
            progress = new ProgressData(),
            materials = new MaterialInventoryData(),
            ownedCharacters = new List<CharacterInstanceData>(),
            selectedTeamInstanceIds = Array.Empty<string>()
        };
    }
}
