using UnityEngine;
using UnityEngine.UI;

public class BattleTeamSlotUI : MonoBehaviour
{
    [Header("左から Slot1〜Slot5 の順で登録")]
    [SerializeField] private Button[] slotButtons = new Button[5];

    [Header("ボタンの見た目（アイコン用）※ボタンと同じ数")]
    [SerializeField] private Image[] slotIcons = new Image[5];

    [Header("空スロット時の表示（任意）")]
    [SerializeField] private Sprite emptySprite;

    private void Start()
    {
        Apply();
    }

    public void Apply()
    {
        var team = TeamSetupData.SelectedTeam;

        for (int i = 0; i < 5; i++)
        {
            CharacterInstance instance = null;
            CharacterBlueprint bp = null;

            if (team != null && i < team.Length)
            {
                instance = team[i];
                bp = instance != null ? instance.Blueprint : null;
            }

            // アイコン反映
            if (slotIcons != null && i < slotIcons.Length && slotIcons[i] != null)
            {
                if (bp != null && bp.icon != null)
                {
                    slotIcons[i].sprite = bp.icon;
                    slotIcons[i].color = Color.white; // 透明になってる事故対策
                }
                else
                {
                    slotIcons[i].sprite = emptySprite;
                    // emptySprite が無いなら、とりあえず薄くする
                    slotIcons[i].color = (emptySprite == null) ? new Color(1f, 1f, 1f, 0.25f) : Color.white;
                }
            }

            // 空スロットは押せない（仕様に合わせてON/OFF調整してください）
            if (slotButtons != null && i < slotButtons.Length && slotButtons[i] != null)
            {
                slotButtons[i].interactable = (instance != null && bp != null);
            }
        }
    }
}