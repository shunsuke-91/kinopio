using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("パラメータ（Inspector調整用）")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float attackInterval = 1.0f;

    private float currentHP;

    private StageRuleType ruleType;
    private Vector2 moveDirection = Vector2.right; // ★ OneWayでは右方向（左→右）

    private Transform target; // 攻撃対象（Enemy）
    private float lastAttackTime = 0f;


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
            // Player は基本味方なので左側に出る前提として右に動く
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
        // ★ 敵が消えていたら攻撃停止
        if (target == null)
        {
            return;
        }

        // ★ 敵のコンポーネントが消えていたら攻撃停止
        var enemy = target.GetComponent<EnemyController>();
        if (enemy == null)
        {
            target = null;
            return;
        }

        // ★ 一定間隔で攻撃する処理
        if (Time.time - lastAttackTime >= attackInterval)
        {
            lastAttackTime = Time.time;
            enemy.TakeDamage(attackPower);
        }
    }


    // ============================
    // 衝突判定
    // ============================
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy == null) return;

        // 自分 → 相手
        Vector2 toEnemy = (collision.transform.position - transform.position).normalized;

        // Player は左向き（前方向 = -transform.right）
        Vector2 forward = -transform.right;

        // 方向判定
        float dot = Vector2.Dot(forward, toEnemy);

        // 距離判定
        float distance = Vector2.Distance(transform.position, collision.transform.position);

        // 距離1.2以内、かつ前にいればターゲット
        if (dot > 0 && distance < 1.2f)
        {
            target = collision.transform;
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

    private void Die()
    {
        Debug.Log($"{name} died.");
        Destroy(gameObject);
    }
}