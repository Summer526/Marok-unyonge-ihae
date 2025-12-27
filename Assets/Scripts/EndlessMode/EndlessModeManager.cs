using UnityEngine;
using System.Collections;

/// <summary>
/// 무한모드 전용 매니저
/// </summary>
public class EndlessModeManager : MonoBehaviour
{
    public static EndlessModeManager Instance;

    [Header("References")]
    public GameManager gameManager;
    public MajorSystem majorSystem;
    public ItemManager itemManager;
    public PlayerStats player;

    [Header("무한모드 상태")]
    public int floorsCleared = 0;           // 클리어한 층수
    public float residueMana = 0f;          // 잔류 마나 구체
    public float vampireHealedThisFloor = 0f; // 흡혈 - 이번 층 회복량
    public int blackContractStacks = 0;     // 검은 계약서 - 층수 스택

    [Header("아이템 보유 여부")]
    public bool hasVampire = false;
    public bool hasDoping = false;
    public bool hasDoubleBlade = false;
    public bool hasBlackContract = false;
    public int lizardTailCount = 0;
    public bool hasResidueMana = false;
    public int deadCoinCount = 0;

    [Header("도핑약 HP 증가")]
    private float dopingHPBonus = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize()
    {
        floorsCleared = 0;
        residueMana = 0f;
        vampireHealedThisFloor = 0f;
        blackContractStacks = 0;
        dopingHPBonus = 0f;

        hasVampire = false;
        hasDoping = false;
        hasDoubleBlade = false;
        hasBlackContract = false;
        lizardTailCount = 0;
        hasResidueMana = false;
        deadCoinCount = 0;
    }

    /// <summary>
    /// 아이템 효과 적용
    /// </summary>
    public void AddItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Vampire:
                hasVampire = true;
                Debug.Log("흡혈의 인장 획득");
                break;

            case ItemType.Doping:
                hasDoping = true;
                ApplyDopingBonus();
                Debug.Log("도핑약 획득 - 최대 HP +10%");
                break;

            case ItemType.DoubleBlade:
                hasDoubleBlade = true;
                Debug.Log("양날단검 획득");
                break;

            case ItemType.BlackContract:
                hasBlackContract = true;
                // 최대 HP -10%
                if (player != null)
                {
                    player.maxHP *= 0.9f;
                    player.currentHP = Mathf.Min(player.currentHP, player.maxHP);
                }
                Debug.Log("검은 계약서 획득 - 최대 HP -10%");
                break;

            case ItemType.LizardTail:
                lizardTailCount++;
                Debug.Log($"도마뱀 꼬리 획득 ({lizardTailCount}개)");
                break;

            case ItemType.ResidueMana:
                hasResidueMana = true;
                Debug.Log("잔류 마나 구체 획득");
                break;

            case ItemType.DeadCoin:
                deadCoinCount++;
                Debug.Log($"망자의 동전 획득 ({deadCoinCount}개)");
                break;
        }
    }

    /// <summary>
    /// 층 클리어 처리
    /// </summary>
    public void OnFloorCleared()
    {
        floorsCleared++;

        // 검은 계약서 스택 증가
        if (hasBlackContract && blackContractStacks < 50)
        {
            blackContractStacks++;
        }

        // 도핑약 - 20층마다 HP +5%
        if (hasDoping && floorsCleared % 20 == 0)
        {
            ApplyDopingBonus();
        }

        // 흡혈 초기화
        vampireHealedThisFloor = 0f;

        // 50층마다 전공 선택
        if (floorsCleared % 50 == 0)
        {
            TriggerMajorSelection();
        }
    }

    /// <summary>
    /// 도핑약 HP 증가 적용
    /// </summary>
    void ApplyDopingBonus()
    {
        if (!hasDoping || player == null) return;

        float bonusPercent = 0.05f; // 5%
        if (floorsCleared == 0)
            bonusPercent = 0.1f; // 첫 획득 시 10%

        dopingHPBonus += bonusPercent;

        float oldMaxHP = player.maxHP;
        player.maxHP = player.maxHP * (1f + bonusPercent);

        Debug.Log($"도핑약 효과: 최대 HP {oldMaxHP:F0} → {player.maxHP:F0}");
    }

    /// <summary>
    /// 흡혈 처리 - 데미지의 5% 회복 (층당 최대 15%)
    /// </summary>
    public void ApplyVampireHeal(float damage)
    {
        if (!hasVampire || player == null) return;

        float maxHealThisFloor = player.maxHP * 0.15f;

        if (vampireHealedThisFloor >= maxHealThisFloor)
        {
            Debug.Log("이번 층 흡혈 한계 도달");
            return;
        }

        float healAmount = damage * 0.05f;
        float remainingHeal = maxHealThisFloor - vampireHealedThisFloor;
        healAmount = Mathf.Min(healAmount, remainingHeal);

        player.Heal(healAmount);
        vampireHealedThisFloor += healAmount;

        Debug.Log($"흡혈 회복: {healAmount:F1} (이번 층 누적: {vampireHealedThisFloor:F1}/{maxHealThisFloor:F1})");
    }

    /// <summary>
    /// 양날단검 - 첫 피해 30% 감소
    /// </summary>
    public float ApplyDoubleBladeReduction(float damage, ref bool isFirstHit)
    {
        if (!hasDoubleBlade || !isFirstHit) return damage;

        isFirstHit = false;
        float reducedDamage = damage * 0.7f;
        Debug.Log($"양날단검 발동: {damage:F1} → {reducedDamage:F1}");
        return reducedDamage;
    }

    /// <summary>
    /// 검은 계약서 - 데미지 보정 (층당 +1%, 최대 50%)
    /// </summary>
    public float ApplyBlackContractBonus(float baseDamage)
    {
        if (!hasBlackContract) return baseDamage;

        float bonus = Mathf.Min(blackContractStacks * 0.01f, 0.5f);
        return baseDamage * (1f + bonus);
    }

    /// <summary>
    /// 도마뱀 꼬리 - 부활 (최대 HP 20% 감소)
    /// </summary>
    public bool TryLizardTailRevive()
    {
        if (lizardTailCount <= 0 || player == null) return false;

        lizardTailCount--;

        // 최대 HP 20% 감소
        player.maxHP *= 0.8f;
        player.currentHP = player.maxHP * 0.2f; // 20% HP로 부활

        Debug.Log($"도마뱀 꼬리 발동! 최대 HP 감소, {player.currentHP:F1} HP로 부활 (남은 꼬리: {lizardTailCount}개)");

        return true;
    }

    /// <summary>
    /// 잔류 마나 구체 - 오버킬 저장
    /// </summary>
    public void StoreResidueMana(float overkillDamage)
    {
        if (!hasResidueMana) return;

        float stored = overkillDamage * 0.5f;
        residueMana += stored;

        Debug.Log($"잔류 마나 저장: +{stored:F1} (총 {residueMana:F1})");
    }

    /// <summary>
    /// 잔류 마나 구체 - 다음 공격에 추가
    /// </summary>
    public float ConsumeResidueMana()
    {
        if (!hasResidueMana || residueMana <= 0f) return 0f;

        float bonus = residueMana;
        residueMana = 0f;

        Debug.Log($"잔류 마나 소모: +{bonus:F1} 데미지");
        return bonus;
    }

    /// <summary>
    /// 망자의 동전 - 골드 보너스
    /// </summary>
    public int ApplyDeadCoinBonus(int baseGold)
    {
        if (deadCoinCount <= 0) return baseGold;

        float bonus = deadCoinCount * 0.1f; // 개당 10%
        return Mathf.RoundToInt(baseGold * (1f + bonus));
    }

    /// <summary>
    /// 망자의 동전 - 받는 피해 증가
    /// </summary>
    public float ApplyDeadCoinPenalty(float damage)
    {
        if (deadCoinCount <= 0) return damage;

        float penalty = deadCoinCount * 0.1f; // 개당 10%
        return damage * (1f + penalty);
    }

    /// <summary>
    /// 텐트 사용 - 전투 스킵
    /// </summary>
    public void UseTent()
    {
        if (player == null) return;

        // HP 30% 회복
        float healAmount = player.maxHP * 0.3f;
        player.Heal(healAmount);

        Debug.Log($"텐트 사용: {healAmount:F1} 회복, 전투 스킵");

        // 다음 층으로 (골드/드랍 없음)
        if (gameManager != null)
        {
            gameManager.stage++;
            floorsCleared++;

            // 전공 선택 체크
            if (floorsCleared % 50 == 0)
            {
                TriggerMajorSelection();
            }

            gameManager.SpawnEnemy();
        }
    }

    /// <summary>
    /// 50층마다 전공 선택 트리거
    /// </summary>
    void TriggerMajorSelection()
    {
        Debug.Log($"{floorsCleared}층 도달! 전공 선택");

        // TODO: UIManager에서 EndlessChosePanel 열기
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            // uiManager.ShowEndlessChosePanel();
        }
    }
}