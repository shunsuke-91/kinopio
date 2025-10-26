using System.Collections;
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

    Animator anim;

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
        pos = new Vector3(direction, 0, 0);
        anim = GetComponent<Animator>();
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
        if (collision.gameObject.tag == "Enemy" && type == TYPE.PLAYER
            || collision.gameObject.tag == "Player" && type == TYPE.ENEMY)
        {
            isMove = false;
            //攻撃を始める
            //攻撃アニメーションの再生
            anim.SetBool("Attack", true);
            //相手のHPを削る
            HitPoint hitPoint = collision.gameObject.GetComponent<HitPoint>();
            StartCoroutine(AttackAction(hitPoint));
            //倒したらまた前に進む
        }

    }

    //Enterの条件終了後に呼ばれる関数
    private void OnTriggerExit2D(Collider2D collision)
    {
        //敵にぶつかったら、移動を止める
        if (collision.gameObject.tag == "Enemy" && type == TYPE.PLAYER
            || collision.gameObject.tag == "Player" && type == TYPE.ENEMY)
        {
            isMove = true;
            anim.SetBool("Attack", false);
        }
    }

    IEnumerator AttackAction(HitPoint hitPoint)
    {
        while (hitPoint != null && hitPoint.hp > 0)
        {
            yield return new WaitForSeconds(0.5f);
            if (hitPoint != null)//destroyされた後を防ぐ、独自追加
            {
                hitPoint.Damage(1);
            }

        }
    }
    
    public void DestroyEvent()
    {
        Destroy(gameObject);
    }
}

