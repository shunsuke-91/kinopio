using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("パラメータ（Inspector調整用）")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackPower = 10f;

    [Header("エフェクト")]
    [SerializeField] private GameObject hitEffect;    // 攻撃ヒット時
    [SerializeField] private GameObject deathEffect;  // 死亡時

    private float currentHP;
    private Animator animator;
    private CharacterInstance currentInstance;

    private StageRuleType ruleType;
    private Vector2 moveDirection = Vector2.right; // ★ OneWayでは右方向（左→右）

    private Transform target; // 攻撃対象（Enemy）

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHP = maxHP;
        SetupInitialDirection();
    }

    public void Initialize(StageRuleType stageRule)
    {
        ruleType = stageRule;
        SetupInitialDirection();
    }

    /// <summary>
    /// インスタンスのステータスを適用する
    /// </summary>
    public void ApplyInstance(CharacterInstance instance)
    {
        if (instance == null) return;

        currentInstance = instance;
        maxHP = instance.GetMaxHP();
        attackPower = instance.GetAttack();

        float attackSpeed = instance.GetAttackSpeed();
        if (animator != null)
        {
            animator.speed = attackSpeed;
        }

        currentHP = maxHP;
    }

    private void Update()
    {
        if (target == null)
        {
            MoveBehavior();
        }
        else
        {
            AttackBehavior();
        }
    }

    // ============================
    // 移動処理
    // ============================
    private void MoveBehavior()
    {
        switch (ruleType)
        {
            case StageRuleType.OneWay:
                // ★ Player は右方向へ進む
                transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
                break;

            case StageRuleType.BothSides:
                transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
                break;

            case StageRuleType.FreeField:
                // TODO: 必要なら Player 用 FreeField 行動を追加
                break;
        }
    }

    private void SetupInitialDirection()
    {
        if (ruleType == StageRuleType.BothSides)
        {
            if (transform.position.x < 0)
                moveDirection = Vector2.right;
            else
                moveDirection = Vector2.left;
        }
    }

    // ============================
    // 攻撃処理
    // ============================
    private void AttackBehavior()
    {
        if (target == null)
        {
            if (animator != null) animator.SetBool("Attack", false);
            return;
        }

        var enemy = target.GetComponent<EnemyController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (enemy == null && baseCtrl == null)
        {
            if (animator != null) animator.SetBool("Attack", false);
            target = null;
            return;
        }

        if (animator != null) animator.SetBool("Attack", true);
    }

    public void OnAttackHit()
    {
        if (target == null) return;

        var enemy = target.GetComponent<EnemyController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (enemy != null)
        {
            if (hitEffect != null)
                Instantiate(hitEffect, target.position, Quaternion.identity);

            enemy.TakeDamage(attackPower);
            return;
        }

        if (baseCtrl != null)
        {
            if (hitEffect != null)
                Instantiate(hitEffect, target.position, Quaternion.identity);

            baseCtrl.TakeDamage(attackPower);
            return;
        }
    }

    // ============================
    // 衝突判定
    // ============================
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBase"))
        {
            var baseCtrl = collision.GetComponent<BaseController>();
            if (baseCtrl != null)
            {
                target = collision.transform;
            }
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy == null) return;

            Vector2 toEnemy = (collision.transform.position - transform.position).normalized;
            Vector2 forward = -transform.right;

            float dot = Vector2.Dot(forward, toEnemy);
            float distance = Vector2.Distance(transform.position, collision.transform.position);

            if (dot > 0 && distance < 1.2f)
            {
                target = collision.transform;
            }
        }
    }

    // ============================
    // HP処理
    // ============================
    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
            Die();
    }

    public Transform CurrentTarget => target;

    public void ClearTarget()
    {
        target = null;
        if (animator != null) animator.SetBool("Attack", false);
    }

    private void Die()
    {
        var myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    var ec = h.GetComponent<EnemyController>();
                    if (ec != null && ec.CurrentTarget == transform)
                    {
                        ec.ClearTarget();
                    }
                }
            }
        }

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
