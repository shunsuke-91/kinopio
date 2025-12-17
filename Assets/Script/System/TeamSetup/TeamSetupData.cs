using UnityEngine;

/// <summary>
/// チーム編成情報を一時的に保持する静的クラス
/// （シーンをまたいでも参照できるようにするため）
/// </summary>
public static class TeamSetupData
{
    // 例えば 5 スロット想定
    public const int MaxSlots = 5;

    // 各スロットに選ばれたキャラクターの Instance を保持
    public static CharacterInstance[] SelectedTeam = new CharacterInstance[MaxSlots];
}