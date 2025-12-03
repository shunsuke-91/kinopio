using System;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    [SerializeField] private CharacterBlueprint blueprint;

    public CharacterBlueprint Blueprint => blueprint;

    public CharacterInstance(CharacterBlueprint characterBlueprint)
    {
        blueprint = characterBlueprint;
    }
}
