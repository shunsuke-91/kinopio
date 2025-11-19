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
            animator.SetBool("Attack", false);
            return;
        }

        // ★ 敵のコンポーネントが消えていたら攻撃停止
        var enemy = target.GetComponent<EnemyController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (enemy == null && baseCtrl == null)
        {
            animator.SetBool("Attack", false);
            target = null;
            return;
        }

        // ★ 一定間隔で攻撃する処理
        animator.SetBool("Attack", true);

    }

    public void OnAttackHit()
    {
        if (target == null) return;

        var enemy = target.GetComponent<EnemyController>();
        var baseCtrl = target.GetComponent<BaseController>();

        // ★ Enemy or Base どちらかにヒットさせる
        if (enemy != null)
        {
            // ヒットエフェクト
            if (hitEffect != null)
                Instantiate(hitEffect, target.position, Quaternion.identity);

            enemy.TakeDamage(attackPower);
            return;
        }

        if (baseCtrl != null)
        {
            // ヒットエフェクト
            if (hitEffect != null)
                Instantiate(hitEffect, target.position, Quaternion.identity);

            baseCtrl.TakeDamage(attackPower);
            return;
        }

        // ★ どちらにも該当しない場合は何もしない
    }

    // ============================
    // 衝突判定
    // ============================
    private void OnTriggerStay2D(Collider2D collision)
    {
        // ================================
        // ① EnemyBase（拠点）に触れた場合
        // ================================
        if (collision.CompareTag("EnemyBase"))
        {
            var baseCtrl = collision.GetComponent<BaseController>();
            if (baseCtrl != null)
            {
                target = collision.transform;
            }
            return;
        }

        // ================================
        // ② Enemy（通常の敵）に触れた場合 ← 元の処理
        // ================================
        if (collision.CompareTag("Enemy"))
        {
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

            // 距離 1.2以内、かつ正面なら攻撃対象にする
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
        animator.SetBool("Attack", false);
    }

    private void Die()
    {
        // 周囲の敵の target を解除
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