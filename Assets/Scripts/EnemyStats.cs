using UnityEngine;
using static GameManager;

public class EnemyStats : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    // 애니메이션 파라미터 이름
    private readonly string ANIM_ATTACK = "Attack";
    private readonly string ANIM_DIE = "Die";

    [Header("Enemy Type Settings")]
    public string typeName = "일반 몹";

    [Tooltip("이 몹이 등장하는 최소 스테이지")]
    public int minStage = 1;

    [Tooltip("이 몹이 등장하는 최대 스테이지 (0 = 제한 없음)")]
    public int maxStage = 0;

    [Tooltip("이 몹의 기본 HP")]
    public float baseHPOverride = 20f;

    [Tooltip("이 몹의 기본 ATK")]
    public float baseATKOverride = 8f;

    [Tooltip("처치 시 추가 골드")]
    public int goldBonus = 0;

    [Header("Runtime")]
    public int stage;
    public float maxHP;
    public float currentHP;
    public float atk;
    public float BaseAtk = 8;
    public float BaseHP = 20;

    public ElementType elementType;
    public SpriteRenderer spriteRenderer;

    public static Sprite[] availableSprites;
    public static ElementType[] availableElements;

    [Header("Enemy State")]
    public EnemyState currentState = EnemyState.Normal;
    public int stunnedTurnsLeft = 0; 

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    public void InitializeRandom(int stageNum)
    {
        stage = stageNum;

        // ★ 프리팹에 설정된 기본값 사용
        BaseHP = baseHPOverride;
        BaseAtk = baseATKOverride;

        if (availableElements != null && availableElements.Length > 0)
        {
            elementType = availableElements[Random.Range(0, availableElements.Length)];
        }


        UpdateStatsForStage(stage);

        Debug.Log($"{typeName} 생성 - BaseHP:{BaseHP}, BaseATK:{BaseAtk}, 골드보너스:{goldBonus}");
    }

    public void UpdateStatsForStage(int stageNum)
    {
        stage = stageNum;

        if (stage <= 30)
        {
            // 1~30 구간
            maxHP = BaseHP * Mathf.Pow(1.07f, stage - 1);
            atk = BaseAtk * Mathf.Pow(1.04f, stage - 1);
        }
        else if (stage <= 60)
        {
            // 31~60 구간
            float hp30 = BaseHP * Mathf.Pow(1.07f, 29);
            float atk30 =BaseAtk * Mathf.Pow(1.04f, 29);

            maxHP = hp30 * Mathf.Pow(1.08f, stage - 30);
            atk = atk30 * Mathf.Pow(1.05f, stage - 30);
        }
        else
        {
            // 61~100 구간
            float hp30 = BaseHP * Mathf.Pow(1.07f, 29);
            float atk30 = BaseAtk * Mathf.Pow(1.04f, 29);
            float hp60 = hp30 * Mathf.Pow(1.08f, 30);
            float atk60 = atk30 * Mathf.Pow(1.05f, 30);

            maxHP = hp60 * Mathf.Pow(1.09f, stage - 60);
            atk = atk60 * Mathf.Pow(1.06f, stage - 60);
        }

        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;

            // ★ Die 애니메이션
            if (animator != null)
                animator.SetTrigger(ANIM_DIE);

            // 몹 사망 SE
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MobDie");
        }
        else
        {
            // 몹 피격 SE
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MobAttacked");
        }
        WorldUnitHUD hud = GetComponentInChildren<WorldUnitHUD>();
        if (hud != null)
        {
            hud.UpdateUI();
        }
    }


    public void AttackPlayer(PlayerStats player, bool hasLastStand)
    {
        // 마비 상태면 공격 불가
        if (currentState == EnemyState.Stunned && stunnedTurnsLeft > 0)
        {
            stunnedTurnsLeft--;
            Debug.Log($"몹 마비 상태 ({stunnedTurnsLeft}턴 남음)");

            if (stunnedTurnsLeft <= 0)
            {
                currentState = EnemyState.Normal;
                Debug.Log("몹 마비 해제");
            }

            return;
        }

        Debug.Log($"몹 공격: {atk} 데미지");

        float finalAtk = atk;

        // 무한모드 - 양날단검 적 공격력 +10%
        if (EndlessModeManager.Instance != null && EndlessModeManager.Instance.hasDoubleBlade)
        {
            finalAtk *= 1.1f;
        }

        // 전공 - 결계 공격력 감소
        MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
        if (majorSystem != null)
        {
            // chainCount는 GameManager에서 전달 필요 (임시로 0)
            finalAtk = majorSystem.ApplyBarrierReduction(finalAtk, 0);
        }

        if (animator != null)
            animator.SetTrigger(ANIM_ATTACK);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE("MobAttack");

        player.TakeDamage(finalAtk, hasLastStand);
    }

    /// <summary>
    /// 마비 상태로 만들기 (번개 전공)
    /// </summary>
    public void ApplyStun(int turns)
    {
        currentState = EnemyState.Stunned;
        stunnedTurnsLeft = turns;
        Debug.Log($"몹 마비 {turns}턴");
    }
    public bool IsDead()
    {
        return currentHP <= 0;
    }
}