using UnityEngine;

public class HitPoint : MonoBehaviour
{
    public int hp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Damage(int damage)
    {
        hp -= damage;

        if(hp <= 0)
        {
            Destroy(this.gameObject);
        }

    }
}
