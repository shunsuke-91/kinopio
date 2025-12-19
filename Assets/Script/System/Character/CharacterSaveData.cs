using System;
using System.Collections.Generic;

[Serializable]
public class CharacterSaveData
{
    public int version = 1;
    public List<MaterialStack> materials = new List<MaterialStack>();
    public List<CharacterInstance> ownedCharacters = new List<CharacterInstance>();
    public List<string> teamInstanceIds = new List<string>();
}
