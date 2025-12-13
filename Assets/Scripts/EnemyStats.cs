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

        // 랜덤 속성 (프리팹에 설정 안 했으면)
        if (elementType == ElementType.Wind) // 기본값이면 랜덤
        {
            if (availableElements != null && availableElements.Length > 0)
            {
                elementType = availableElements[Random.Range(0, availableElements.Length)];
            }
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
    }


    public void AttackPlayer(PlayerStats player, bool hasLastStand)
    {
        Debug.Log($"몹 공격: {atk} 데미지");
        if (animator != null)
            animator.SetTrigger(ANIM_ATTACK);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySE("MobAttack");

        player.TakeDamage(atk, hasLastStand);
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}