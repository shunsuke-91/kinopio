using UnityEngine;
using UnityEngine.UI;


public class BaseUI : MonoBehaviour
{
    [SerializeField] Text hpText;
    [SerializeField] HitPoint hitpoint;

    int maxHP;
    int nowHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxHP = hitpoint.hp;
        nowHP = maxHP;
        UpdateUI();
    }

    public void UpdateUI()
    {
        nowHP = hitpoint.hp;
        if (nowHP <= 0)
        {
            nowHP = 0;
        }
        hpText.text = nowHP.ToString() + "/" + maxHP.ToString();
    }   


}
