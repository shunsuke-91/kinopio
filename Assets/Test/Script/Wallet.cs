using UnityEngine;
using UnityEngine.UI;


public class Wallet : MonoBehaviour
{
    public static Wallet instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }


    [SerializeField] Text coinText;

    public int coinlevel;
    [SerializeField] int[] maxCoin;
    public float nowCoin = 0;
    public int coinSpeed = 6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coinlevel = 0;
        coinText.text = nowCoin.ToString() + "/" + maxCoin[coinlevel].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.isGame && nowCoin <= maxCoin[coinlevel])
        {
            nowCoin += Time.deltaTime * coinSpeed;
            coinText.text = nowCoin.ToString("F0") + "/" + maxCoin[coinlevel].ToString();
        }

    }
}
