using UnityEngine;
using UnityEngine.Events;

public class HitPoint : MonoBehaviour
{
    public int hp;
    private GameManager gm;

    [SerializeField] UnityEvent OnDamageEvent;
    [SerializeField] UnityEvent OnDestroyEvent;

    void Awake()
    {
        gm = GameManager.instance;  
    }

    public void Damage(int damage)
    {
        if (gm.isGame)
        {
            hp -= damage;

            if (OnDamageEvent != null)
            {
                OnDamageEvent.Invoke();
            }

            if (hp <= 0)
            {
                if (OnDestroyEvent != null)
                {
                    OnDestroyEvent.Invoke();
                }
            }
        }

    }
}
