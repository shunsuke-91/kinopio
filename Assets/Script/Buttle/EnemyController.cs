using UnityEngine;

public class EnemyController : MonoBehaviour
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

    // ステージルール（BattleManager から渡される）
    private StageRuleType ruleType;
    private Vector2 moveDirection = Vector2.left;

    // 攻撃対象（Playerなど）
    private Transform target;

    // FreeField 用ターゲット（必要なら Inspector で設定）
    [SerializeField] private Transform freeFieldTarget;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    private void Start()
    {
        currentHP = maxHP;
        SetupInitialDirection();
    }

    /// <summary>
    /// BattleManager から呼び出される初期化メソッド
    /// </summary>
    public void Initialize(StageRuleType stageRule, DifficultySettings diff)
    {
        ruleType = stageRule;
        // 必要ならここで向きを決め直してもOK
        SetupInitialDirection();

        // ★ HPを倍率適用
        maxHP = Mathf.Ceil(maxHP * diff.hpMultiplier);
        currentHP = maxHP;

        // ★ 攻撃力を倍率適用
        attackPower = Mathf.Ceil(attackPower * diff.attackPowerMultiplier);

        // ★ 攻撃スピードを倍率適用（Animator.speed で反映）
        animator.speed = diff.attackSpeedMultiplier;
    }


    private void Update()
    {
        if (target == null)
        {
            // 攻撃対象がいない → 移動する
            MoveBehavior();
        }
        else
        {
            // 攻撃対象がいる → 攻撃する
            AttackBehavior();
        }
    }

    // ============================
    // 移動処理
    // ============================
    private void MoveBehavior()
    {
        // 移動中は Attack アニメを切る
        animator.SetBool("Attack", false);

        switch (ruleType)
        {
            case StageRuleType.OneWay:
                transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
                break;

            case StageRuleType.BothSides:
                transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
                break;

            case StageRuleType.FreeField:
                if (freeFieldTarget == null) return;
                Vector2 dir = (freeFieldTarget.position - transform.position).normalized;
                transform.Translate(dir * moveSpeed * Time.deltaTime);
                break;
        }
    }

    private void SetupInitialDirection()
    {
        if (ruleType == StageRuleType.BothSides)
        {
            if (transform.position.x > 0)
                moveDirection = Vector2.left;
            else
                moveDirection = Vector2.right;
        }
    }


    // ============================
    // 攻撃処理（今はフローだけ）
    // ============================
    private void AttackBehavior()
    {
        // ★ target 自体がないなら何もしない
        if (target == null)
        {
            animator.SetBool("Attack", false);
            return;
        }

        //  PlayerController か BaseController のどちらかを取得
        var player = target.GetComponent<PlayerController>();
        var baseCtrl = target.GetComponent<BaseController>();

        // どちらもなければ攻撃停止
        if (player == null && baseCtrl == null)
        {
            animator.SetBool("Attack", false);
            target = null;
            return;
        }

        // アニメを攻撃に切り替える
        animator.SetBool("Attack", true);

        // ダメージ処理はアニメーションイベントで行うため、ここでは何もしない
    }

    //AnimationEventで呼ばれる処理を追加
    public void OnAttackHit()
    {
        if (target == null) return;

        var player = target.GetComponent<PlayerController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (player != null)
            player.TakeDamage(attackPower);
        else if (baseCtrl != null)
            baseCtrl.TakeDamage(attackPower); // ★ Base にダメージ

        if (hitEffect != null)
            Instantiate(hitEffect, target.position, Quaternion.identity);
    }


    // ============================
    // 衝突判定（ぶつかったら攻撃開始）
    // ============================
    private void OnTriggerStay2D(Collider2D collision)
    {
        // ========== ① PlayerBase（拠点） ==========
        if (collision.CompareTag("PlayerBase"))
        {
            // Base は動かないので方向判定なしでOK
            var baseCtrl = collision.GetComponent<BaseController>();
            if (baseCtrl != null)
            {
                target = collision.transform;
            }
            return;
        }

        // ========== ② Player（従来の処理） ==========
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player == null) return;

            // 自分 → 相手の方向ベクトル
            Vector2 toPlayer = (collision.transform.position - transform.position).normalized;

            // Enemy = 右向き固定
            Vector2 forward = transform.right;

            // 方向判定（0以上なら正面）
            float dot = Vector2.Dot(forward, toPlayer);

            // 距離判定
            float distance = Vector2.Distance(transform.position, collision.transform.position);

            if (dot > 0 && distance < 1.2f)
            {
                target = collision.transform;
            }
        }
    }


    // ============================
    // HP管理
    // ============================
    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public Transform CurrentTarget => target;

    public void ClearTarget()
    {
        target = null;
        animator.SetBool("Attack", false);
    }

    private void Die()
        {
                // 攻撃されていた Player の target を解除する
        var myCollider = GetComponent<Collider2D>();
        if (myCollider != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
            foreach (var h in hits)
            {
                if (h.CompareTag("Player"))
                {
                    var pc = h.GetComponent<PlayerController>();
                    if (pc != null && pc.CurrentTarget == transform)
                    {
                        pc.ClearTarget();
                    }
                }
            }
        }

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}