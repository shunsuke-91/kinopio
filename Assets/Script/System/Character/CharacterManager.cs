using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("最初に所持するキャラの Blueprint")]
    [SerializeField] private CharacterBlueprint defaultBlueprint;

    public List<CharacterInstance> ownedCharacters = new();

    private void Awake()
    {
        Instance = this;

        // ★ゲーム開始時に最初の1体を自動所持
        if (ownedCharacters.Count == 0)
        {
            ownedCharacters.Add(new CharacterInstance(defaultBlueprint));
        }
    }
}