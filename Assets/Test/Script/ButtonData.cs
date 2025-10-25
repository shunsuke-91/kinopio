using UnityEngine;

public class ButtonData : MonoBehaviour
{
    [SerializeField] SpriteRenderer player;
    // 押した時に呼ばれる関数
    public void onclick()
    {
        //プレイヤーの召喚
        PlayerSpawn();
        //ボタンを押せないようにする
        //ゲージを出す
    }

    void PlayerSpawn()
    {
        float y = Random.Range(-0.69f, -1.69f);
        SpriteRenderer pl = Instantiate(player, new Vector3(6.23f, y, 0), transform.rotation);
        pl.sortingOrder = (int)(-y * 10);
    }
}
