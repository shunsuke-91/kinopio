using UnityEngine;
using UnityEngine.UI;

public class BaseHPUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider playerBaseSlider;
    [SerializeField] private Slider enemyBaseSlider;

    private BaseController playerBase;
    private BaseController enemyBase;

    public void Setup(BaseController pb, BaseController eb)
    {
        playerBase = pb;
        enemyBase = eb;

        // 初期値
        playerBaseSlider.value = 1f;
        enemyBaseSlider.value = 1f;

        // 基地のHPが変化したらUI更新
        playerBase.OnHpChanged += UpdatePlayerBaseHP;
        enemyBase.OnHpChanged += UpdateEnemyBaseHP;
    }

    private void UpdatePlayerBaseHP(float current, float max)
    {
        playerBaseSlider.value = current / max;
    }

    private void UpdateEnemyBaseHP(float current, float max)
    {
        enemyBaseSlider.value = current / max;
    }
}