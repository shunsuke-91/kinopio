using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBlueprintDatabase", menuName = "Characters/BlueprintDatabase", order = 1)]
public class CharacterBlueprintDatabase : ScriptableObject
{
    public CharacterBlueprint[] blueprints;

    public CharacterBlueprint GetByID(string id)
    {
        if (string.IsNullOrEmpty(id) || blueprints == null) return null;

        for (int i = 0; i < blueprints.Length; i++)
        {
            var bp = blueprints[i];
            if (bp != null && bp.blueprintID == id) return bp;
        }
        return null;
    }
}