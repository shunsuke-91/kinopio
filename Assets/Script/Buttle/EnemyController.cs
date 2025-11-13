using UnityEngine;

/// <summary>
/// 敵キャラの基本行動制御クラス
/// - HP管理（Inspectorで調整可能）
/// - 死亡処理
/// - ステージルールに応じた移動AI
/// - 攻撃処理（TODO）
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("パラメータ（Inspector調整可能）")]
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackPower = 10f;

    private float currentHP;

    // ステージのルール（OneWay / BothSides / FreeField）
    private StageRuleType ruleType;

    // OneWay・BothSides の進行方向
    private Vector2 moveDirection = Vector2.left;

    // FreeField 用ターゲット（後で設定,例：プレイヤー拠点）
    [SerializeField] private Transform freeFieldTarget;


    // =============================
    //   初期化
    // =============================
    private void Start()
    {
        currentHP = maxHP;
        SetupInitialDirection(); // BothSides向け
    }

    /// <summary>
    /// BattleManager から呼び出される初期化メソッド
    /// </summary>
    public void Initialize(StageRuleType stageRule)
    {
        ruleType = stageRule;
    }


    // =============================
    //   移動処理
    // =============================
    private void Update()
    {
        MoveBehavior();
    }

    private void MoveBehavior()
    {
        switch (ruleType)
        {
            case StageRuleType.OneWay:
                MoveOneWay();
                break;

            case StageRuleType.BothSides:
                MoveBothSides();
                break;

            case StageRuleType.FreeField:
                MoveFreeField();
                break;
        }
    }


    // ▼ OneWay（横スクロール）
    private void MoveOneWay()
    {
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    // ▼ BothSides（左右から）
    private void MoveBothSides()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
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

    // ▼ FreeField（自由移動）
    private void MoveFreeField()
    {
        if (freeFieldTarget == null)
            return;

        Vector2 dir = (freeFieldTarget.position - transform.position).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }


    // =============================
    //   HP管理
    // =============================
    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        // TODO: 死亡演出
        Destroy(gameObject);
    }
}