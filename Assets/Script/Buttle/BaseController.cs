using UnityEngine;

public class BaseController : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    public System.Action OnBaseDestroyed; // 破壊時にBattleManagerへ通知

    private void Start()
    {
        currentHP = maxHP;
    }

    public void Initialize(float hp)
    {
        maxHP = hp;
        currentHP = hp;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnBaseDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}