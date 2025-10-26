using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ButtonData : MonoBehaviour
{
    private Wallet wallet;
    private Button button;

    [SerializeField] SpriteRenderer player;
    [SerializeField] int price;
    [SerializeField] Text priceText;
    [SerializeField] Slider gaugeBar;
    bool isclicked = false;

    private void Awake()
    {
        wallet = Wallet.instance;
        button = GetComponent<Button>();
    }

    private void Start()
    {
        priceText.text = price.ToString() + "円";
        if (wallet == null) wallet = Wallet.instance; // 念のため
    }

    private void Update()
    {
        if (wallet == null) return;

        if (wallet.nowCoin >= price && !isclicked)
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

    // 押した時に呼ばれる関数
    public void onclick()
    {
        //お金の支払い
        wallet.nowCoin -= price;

        //プレイヤーの召喚
        PlayerSpawn();

        //ボタンを押せないようにする
        isclicked = true;

        //ゲージを出す
        StartCoroutine(SliderUpdate());
    }

    void PlayerSpawn()
    {
        float y = Random.Range(-0.69f, -1.69f);
        SpriteRenderer pl = Instantiate(player, new Vector3(6.23f, y, 0), transform.rotation);
        pl.sortingOrder = (int)(-y * 10);
    }

    IEnumerator SliderUpdate()
    {
        //ゲージを表示
        gaugeBar.value = 0;
        gaugeBar.gameObject.SetActive(true);

        //時間経過でゲージを進める
        while (gaugeBar.value < gaugeBar.maxValue)
        {
            gaugeBar.value++;
            yield return new WaitForSeconds(0.1f);
        }

        //ゲージを非表示
        gaugeBar.gameObject.SetActive(false);

        //またボタンを押せるようにする
        isclicked = false;

    }
}
