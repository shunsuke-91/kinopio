using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamSettingManager : MonoBehaviour
{
    public void OnClickStartBattle()
    {
        var team = TeamSetupData.SelectedTeam;

        if (team == null || team.Length == 0)
        {
            Debug.LogWarning("編成が空のため、バトルを開始できません");
            return;
        }

        bool hasAny = false;
        for (int i = 0; i < team.Length; i++)
        {
            if (team[i] != null)
            {
                hasAny = true;
                break;
            }
        }

        if (!hasAny)
        {
            Debug.LogWarning("編成スロットにキャラが1体もセットされていません");
            return;
        }

        // ★方針変更：SyncTeamToRuntimeData は廃止。
        // 編成変更時に CharacterManager.SetTeamSlot() が GameState.Save() まで行っている前提。
        // ここで余計な同期処理は不要。

        SceneManager.LoadScene("BattleScene");
    }
}