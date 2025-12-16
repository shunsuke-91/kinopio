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

        // ===== ★ここが「最小1行追加」 =====
        CharacterManager.Instance.SyncTeamToRuntimeData();
        // ==================================

        SceneManager.LoadScene("BattleScene");
    }
}