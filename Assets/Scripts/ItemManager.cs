using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아이템 보유/효과 관리
/// </summary>
public class ItemManager : MonoBehaviour
{
    [Header("Owned Items")]
    public List<ItemData> ownedItems = new List<ItemData>();

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    public void Initialize(GridManager grid)
    {
        gridManager = grid;
        ownedItems.Clear();
        ResetAllFlags();
    }
    void ResetAllFlags()
    {
        hasResonance = false;
        hasOrb = false;
        hasComboKeeper = false;
        hasChainBooster = false;
        hasHealBoost = false;
        hasBarrier = false;
        hasShopDiscount = false;
        hasLastStand = false;
    }
    // 공명
    [Header("Resonance")]
    public bool hasResonance = false;
    public ElementType resonanceA;
    public ElementType resonanceB;

    // 콤보 유지 / 연속 마력
    [Header("Combo / Chain")]
    public bool hasComboKeeper = false;
    public int chainBoosterCount = 0;   // 0~3

    // 힐/방어막
    [Header("Heal / Barrier")]
    public bool hasHealBoost = false;
    public bool hasBarrier = false;

    // 상점 할인
    [Header("Shop / Economy")]
    public int shopDiscountCount = 0;   // 0~3
    public bool hasLastStand = false;

    // 공격력 / 골드
    [Header("Attack & Gold")]
    public float bonusAttackFlat = 0f;  // 마나 강화 세트 누적
    public int madisHandCount = 0;      // 마디스의 손 개수
    public bool hasShopDiscount = false;
    public bool hasChainBooster = false;

    // 오브: 속성별 스택 (각 0~5)
    [Header("Orbs")]
    public bool hasOrb = false;
    private readonly Dictionary<ElementType, int> orbStacks = new Dictionary<ElementType, int>();

    // 상수
    private const int MaxOrbStackPerElement = 5;
    private const int MaxChainBoosterStack = 3;
    private const int MaxShopDiscountStack = 3;

    void Awake()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        ownedItems.Clear();

        hasResonance = false;
        resonanceA = ElementType.Fire;
        resonanceB = ElementType.Light;

        hasComboKeeper = false;
        chainBoosterCount = 0;

        hasHealBoost = false;
        hasBarrier = false;

        shopDiscountCount = 0;
        hasLastStand = false;

        bonusAttackFlat = 0f;
        madisHandCount = 0;

        hasOrb = false;
        orbStacks.Clear();
    }

    // ===== 아이템 획득 =====

    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("null 아이템을 AddItem에 넘겼음");
            return;
        }

        ownedItems.Add(item);

        switch (item.itemType)
        {
            case ItemType.AttributeResonance:
                AddResonance(item);
                break;

            case ItemType.AttributeOrb:
                AddOrb(item.primaryElement);
                break;

            case ItemType.ComboKeeper:
                if (!hasComboKeeper)
                {
                    hasComboKeeper = true;
                    Debug.Log("콤보 유지 획득");
                }
                break;

            case ItemType.ChainBooster:
                AddChainBooster();
                break;

            case ItemType.HealBoost:
                if (!hasHealBoost)
                {
                    hasHealBoost = true;
                    Debug.Log("치유 강화 획득");
                }
                break;

            case ItemType.Barrier:
                if (!hasBarrier)
                {
                    hasBarrier = true;
                    Debug.Log("방어막 형성 획득");
                }
                break;

            case ItemType.ShopDiscount:
                AddShopDiscount();
                break;

            case ItemType.LastStand:
                if (!hasLastStand)
                {
                    hasLastStand = true;
                    Debug.Log("응급 대처 획득");
                }
                break;

            case ItemType.ManaBracelet:
                bonusAttackFlat += 3f;
                Debug.Log($"마나 강화 팔찌 획득: 기본 공격력 +3 (누적 +{bonusAttackFlat})");
                break;

            case ItemType.ManaNecklace:
                bonusAttackFlat += 5f;
                Debug.Log($"마나 강화 목걸이 획득: 기본 공격력 +5 (누적 +{bonusAttackFlat})");
                break;

            case ItemType.ManaRing:
                bonusAttackFlat += 7f;
                Debug.Log($"마나 강화 반지 획득: 기본 공격력 +7 (누적 +{bonusAttackFlat})");
                break;

            case ItemType.MadisHand:
                madisHandCount++;
                Debug.Log($"마디스의 손 획득: 몹 처치 골드 +{madisHandCount * 20}%");
                break;
        }
    }

    void AddResonance(ItemData item)
    {
        if (hasResonance)
        {
            Debug.Log("이미 속성 공명을 보유 중이므로 무시");
            return;
        }

        hasResonance = true;

        // ScriptableObject에서 지정한 두 속성 사용
        resonanceA = item.primaryElement;
        resonanceB = item.secondaryElement;

        // 혹시 둘 다 같은 값이면 기본값으로
        if (resonanceA == resonanceB)
        {
            resonanceA = ElementType.Fire;
            resonanceB = ElementType.Light;
        }

        Debug.Log($"속성 공명 획득: {resonanceA} <-> {resonanceB}");
    }

    void AddOrb(ElementType element)
    {
        hasOrb = true;

        int current = 0;
        if (orbStacks.TryGetValue(element, out current))
        {
            if (current >= MaxOrbStackPerElement)
            {
                Debug.Log($"{element} 오브는 이미 최대 {MaxOrbStackPerElement}개");
                return;
            }

            orbStacks[element] = current + 1;
        }
        else
        {
            orbStacks[element] = 1;
        }

        Debug.Log($"{element} 오브 획득 (현재 {orbStacks[element]}개)");

        // 실제 출현 확률 적용은 GridManager 쪽에서
        // ItemManager.GetElementSpawnProbability를 사용해서 구현
    }

    void AddChainBooster()
    {
        if (chainBoosterCount >= MaxChainBoosterStack)
        {
            Debug.Log($"연속 마력은 최대 {MaxChainBoosterStack}개까지");
            return;
        }

        chainBoosterCount++;
        Debug.Log($"연속 마력 획득 (현재 {chainBoosterCount}개)");
    }

    void AddShopDiscount()
    {
        if (shopDiscountCount >= MaxShopDiscountStack)
        {
            Debug.Log($"상점 단골은 최대 {MaxShopDiscountStack}개까지");
            return;
        }

        shopDiscountCount++;
        Debug.Log(
            $"상점 단골 획득 (현재 {shopDiscountCount}개, 할인율 {GetShopDiscountRate() * 100f:F0}%)"
        );
    }

    // ===== 전투 보정 =====

    /// <summary>
    /// 기본 공격력에 마나 강화 세트 보정 적용
    /// GameManager.PlayerAttack에서 player.GetCurrentATK() 뒤에 이거 한 번 태우면 됨.
    /// </summary>
    public float ApplyAttackBonus(float baseAtk)
    {
        return baseAtk + bonusAttackFlat;
    }

    /// <summary>
    /// 체인 길이를 연속 마력 개수만큼 증가시킨 값 반환
    /// GameManager.GetChainMultiplier 호출 전에 이걸로 chainCount 보정.
    /// </summary>
    public int GetEffectiveChainCount(int rawChainCount)
    {
        if (chainBoosterCount <= 0) return rawChainCount;

        int result = rawChainCount + chainBoosterCount;
        return Mathf.Max(result, 1);
    }

    /// <summary>
    /// 방어막 형성: 실제 입힌 피해의 10%를 쉴드로, 최대 HP의 40%까지
    /// PlayerAttack에서 데미지 계산 후 한 번 호출.
    /// </summary>
    public void ApplyBarrierOnDamage(PlayerStats player, float damageDealt)
    {
        if (hasBarrier)
        {
            float shieldAmount = damageDealt * 0.2f;
            player.AddShield(shieldAmount);
            Debug.Log($"방어막 생성: {shieldAmount}");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MakeShield");
        }
    }


    /// <summary>
    /// 몹 처치 시 골드 보정 (마디스의 손)
    /// OnEnemyKilled에서 gold += GetGoldOnKill(baseGold) 식으로 사용.
    /// </summary>
    public int GetGoldOnKill(int baseGold)
    {
        if (madisHandCount <= 0) return baseGold;

        float bonusRate = 0.2f * madisHandCount; // 1개당 20%
        int result = Mathf.RoundToInt(baseGold * (1f + bonusRate));
        return Mathf.Max(result, 0);
    }

    // ===== 상점 =====

    /// <summary>
    /// 상점 전체 할인율 (0~0.3)
    /// </summary>
    public float GetShopDiscountRate()
    {
        float rate = 0.1f * shopDiscountCount;
        return Mathf.Clamp01(rate);
    }

    /// <summary>
    /// 상점 가격에 할인 적용
    /// ShopManager.GetItemPrice 에서 사용.
    /// </summary>
    public int GetDiscountedPrice(int basePrice)
    {
        float rate = GetShopDiscountRate();
        int result = Mathf.RoundToInt(basePrice * (1f - rate));
        return Mathf.Max(result, 0);
    }

    // ===== 오브 확률 계산 보조 =====

    /// <summary>
    /// 속성별 출현 확률 계산
    /// Shield, Heal: 각 10% 고정
    /// 나머지 7개 속성: 80%를 분배 (오브 1개당 4% 보너스)
    /// </summary>
    public float GetElementSpawnProbability(ElementType element)
    {
        // Shield, Heal은 10% 고정
        if (element == ElementType.Shield || element == ElementType.Heal)
        {
            return 0.1f;
        }

        // 세븐 오브 컬렉션 달성 시 균등 분배
        if (HasAllSevenOrbs())
        {
            return 0.8f / 7f; // 11.43%씩
        }

        // 나머지 7개 속성의 우선도 계산
        ElementType[] combatElements = new ElementType[]
        {
        ElementType.Wind,
        ElementType.Fire,
        ElementType.Lightning,
        ElementType.Water,
        ElementType.Earth,
        ElementType.Light,
        ElementType.Dark
        };

        // 총 우선도 합계 계산
        int totalPriority = 0;
        foreach (var elem in combatElements)
        {
            int orbCount = 0;
            orbStacks.TryGetValue(elem, out orbCount);
            int priority = 1 + orbCount; // 기본 우선도 1 + 오브 개수
            totalPriority += priority;
        }

        // 해당 속성의 우선도 계산
        int myOrbCount = 0;
        orbStacks.TryGetValue(element, out myOrbCount);
        int myPriority = 1 + myOrbCount;

        // 80%를 우선도 비율로 분배
        float probability = 0.8f * ((float)myPriority / (float)totalPriority);

        return probability;
    }
    /// <summary>
    /// 특정 아이템을 몇 개 소유하고 있는지 반환
    /// </summary>
    public int GetItemCount(ItemType itemType)
    {
        int count = 0;
        foreach (var item in ownedItems)
        {
            if (item != null && item.itemType == itemType)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 해당 아이템을 더 구매할 수 있는지 체크
    /// </summary>
    public bool CanBuyMore(ItemData item)
    {
        if (item == null) return false;
        if (item.maxStack <= 0) return true; // 무제한

        int currentCount = GetItemCount(item.itemType);
        return currentCount < item.maxStack;
    }
    public bool HasAllSevenOrbs()
    {
        ElementType[] combatElements = new ElementType[]
        {
        ElementType.Wind,
        ElementType.Fire,
        ElementType.Lightning,
        ElementType.Water,
        ElementType.Earth,
        ElementType.Light,
        ElementType.Dark
        };

        foreach (var elem in combatElements)
        {
            int count = 0;
            orbStacks.TryGetValue(elem, out count);

            if (count < 3)  // 각 속성당 3개 필요
            {
                return false;
            }
        }

        return true;
    }
}
