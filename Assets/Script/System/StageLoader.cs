using UnityEngine;

/// <summary>
/// ステージ選択画面 → バトルシーン への橋渡し役。
/// 選ばれた StageData を一時保持するだけのクラス。
/// </summary>
public static class StageLoader
{
    public static StageData selectedStage;
    public static DifficultyType selectedDifficulty = DifficultyType.Normal;
}