using UnityEngine;

public class CharactorMove : MonoBehaviour
{
    public enum TYPE
    {
        PLAYER,
        ENEMY,
    }

    public TYPE type = TYPE.PLAYER;

    float direction;
    Vector3 pos;
    bool isMove = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (type)
        {
            case TYPE.PLAYER:
                //PLAYERの時の処理
                direction = -1;
                break;

            case TYPE.ENEMY:
                //ENEMYの時の処理
                direction = 1;
                break;
        }
        pos = new Vector3(direction,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMove)
        {
            transform.position += pos * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        //敵にぶつかったら、移動を止める
        if(collision.gameObject.tag == "Enemy" && type == TYPE.PLAYER
            || collision.gameObject.tag == "Player" && type == TYPE.ENEMY)
        {
            isMove = false;
        }
        //攻撃を始める
    }

    //Enterの条件終了後に呼ばれる関数
    private void OnTriggerExit2D(Collider2D collision) 
    {
        //敵にぶつかったら、移動を止める
        if(collision.gameObject.tag == "Enemy" && type == TYPE.PLAYER
            || collision.gameObject.tag == "Player" && type == TYPE.ENEMY)
        {
            isMove = true;
        }
        //攻撃を始める
    }
}
