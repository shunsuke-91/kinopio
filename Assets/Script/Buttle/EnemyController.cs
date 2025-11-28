using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("パラメータ（Inspector調整用）")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackPower = 10f;

    [Header("エフェクト")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject deathEffect;

    private float currentHP;
    private Animator animator;

    private StageRuleType ruleType;
    private Vector2 moveDirection = Vector2.left;

    private Transform target;
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

    public void Initialize(StageRuleType stageRule, DifficultySettings diff)
    {
        ruleType = stageRule;
        SetupInitialDirection();

        maxHP = Mathf.Ceil(maxHP * diff.hpMultiplier);
        currentHP = maxHP;
        attackPower = Mathf.Ceil(attackPower * diff.attackPowerMultiplier);

        animator.speed = diff.attackSpeedMultiplier;
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

    private void MoveBehavior()
    {
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

    private void AttackBehavior()
    {
        if (target == null)
        {
            animator.SetBool("Attack", false);
            return;
        }

        var player = target.GetComponent<PlayerController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (player == null && baseCtrl == null)
        {
            animator.SetBool("Attack", false);
            target = null;
            return;
        }

        animator.SetBool("Attack", true);
    }

    public void OnAttackHit()
    {
        if (target == null) return;
        if (target.gameObject == null) return;

        var player = target.GetComponent<PlayerController>();
        var baseCtrl = target.GetComponent<BaseController>();

        if (player != null)
            player.TakeDamage(attackPower);
        else if (baseCtrl != null)
            baseCtrl.TakeDamage(attackPower);
        else
            return;

        if (hitEffect != null && target != null && target.gameObject != null)
            Instantiate(hitEffect, target.position, Quaternion.identity);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBase"))
        {
            var baseCtrl = collision.GetComponent<BaseController>();
            if (baseCtrl != null)
            {
                target = collision.transform;
            }
            return;
        }

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player == null) return;

            Vector2 toPlayer = (collision.transform.position - transform.position).normalized;
            Vector2 forward = transform.right;

            float dot = Vector2.Dot(forward, toPlayer);
            float distance = Vector2.Distance(transform.position, collision.transform.position);

            if (dot > 0 && distance < 1.2f)
            {
                target = collision.transform;
            }
        }
    }

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