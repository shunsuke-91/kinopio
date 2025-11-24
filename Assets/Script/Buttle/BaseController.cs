using UnityEngine;
using System;

public class BaseController : MonoBehaviour
{
    [Header("Base HP")]
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    [Header("死亡エフェクト")]
    [SerializeField] private GameObject destroyEffect;

    public event Action<float, float> OnHpChanged; // ★追加：HP変更イベント
    public event Action OnBaseDestroyed;

    private void Start()
    {
        currentHP = maxHP;
        OnHpChanged?.Invoke(currentHP, maxHP); // 初期値通知
    }

    public void Initialize(float hp)
    {
        maxHP = hp;
        currentHP = hp;
        OnHpChanged?.Invoke(currentHP, maxHP); // 初期値通知
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        // HP変化イベント通知
        OnHpChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        OnBaseDestroyed?.Invoke();
        Destroy(gameObject);
    }
}