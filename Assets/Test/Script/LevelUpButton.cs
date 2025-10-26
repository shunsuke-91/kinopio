using UnityEngine;
using UnityEngine.UI;



public class LevelUpButton : MonoBehaviour
{
    private Wallet wallet;
    private Button button;

    [SerializeField] Text levelText;
    [SerializeField] Text priceText;
    int level = 1;

    [SerializeField] int[] price;

    bool isMax = false;

    void Awake()
    {
        wallet = Wallet.instance;
        button = GetComponent<Button>();       
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelText.text = "Level" + level.ToString();
        priceText.text = price[level - 1].ToString() + "円";

    }

    // Update is called once per frame
    void Update()
    {
        if (!isMax)
        {
            if (wallet.nowCoin >= price[level - 1])
            {
                //ボタンを押せる
                button.interactable = true;
            }
            else
            {
                //ボタンを押せない
                button.interactable = false;
            }
        }

    }
    
    public void onclick()
    {
        if (level >= price.Length)
        {
            //MAXの時の処理
            isMax = true;
            priceText.text = "MAX";
            levelText.gameObject.SetActive(false);
            button.interactable = false;
            button.enabled = false;
        }
        else
        {

            //必要な金額の消費
            wallet.nowCoin -= price[level - 1];

            //レベルを上げる
            level++;

            //コインのスピードを上げる
            wallet.coinSpeed += 6;

            //テキストの表示を変更
            levelText.text = "Level" + level.ToString();
            priceText.text = price[level - 1].ToString() + "円";

            //財布のMAXコインを更新
            wallet.coinlevel++;

        }
    }
}
