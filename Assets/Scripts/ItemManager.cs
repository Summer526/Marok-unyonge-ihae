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
        activeResonances.Clear();
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
        // ★ 활성화된 공명서 다시 적용
        foreach (var resonance in activeResonances)
        {
            if (resonance != null)
            {
                hasResonance = true;
                break;
            }
        }

        // ★ 보유 아이템 다시 적용
        foreach (var item in ownedItems)
        {
            if (item.itemType != ItemType.AttributeResonance)  // 공명서는 제외
            {
                ApplyPassiveItem(item);
            }
        }
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

    [Header("Active Resonances")]
    public List<ItemData> activeResonances = new List<ItemData>();  // ★ 활성화된 공명서 (최대 2개)
    private const int MaxActiveResonances = 2;

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

        // ★ 액티브 아이템은 인벤토리에만 추가
        if (item.isActive)
        {
            Debug.Log($"{item.displayName} 획득 (액티브 아이템)");
            return;
        }

        // 패시브 아이템은 즉시 효과 적용
        ApplyPassiveItem(item);
    }
    void ApplyPassiveItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("null 아이템을 AddItem에 넘겼음");
            return;
        }

        switch (item.itemType)
        {
            case ItemType.AttributeResonance:
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
            case ItemType.ChaosDice:
                // 혼돈 주사위는 사용 시 효과이므로 여기서는 패스
                Debug.Log("혼돈 주사위 획득");
                break;

            case ItemType.FixPin:
                // 고정핀 아이템 획득
                PinSystem pinSystem = FindObjectOfType<PinSystem>();
                if (pinSystem != null)
                {
                    pinSystem.AddPin(3); // 3개 제공
                }
                Debug.Log("고정핀 3개 획득");
                break;

            case ItemType.Vampire:
            case ItemType.Doping:
            case ItemType.DoubleBlade:
            case ItemType.BlackContract:
            case ItemType.LizardTail:
            case ItemType.ResidueMana:
            case ItemType.DeadCoin:
                // 무한모드 아이템은 EndlessModeManager에서 처리
                if (EndlessModeManager.Instance != null)
                {
                    EndlessModeManager.Instance.AddItem(item.itemType);
                }
                break;

            case ItemType.Tent:
                // 텐트는 액티브 아이템 (사용 시 효과)
                Debug.Log("텐트 획득");
                break;

            case ItemType.MajorBook:
                // 전공 서적은 액티브 아이템 (사용 시 레벨업)
                Debug.Log("전공 서적 획득");
                break;
        }
    }
    public bool UseActiveItem(ItemData item)
    {
        if (item == null || !item.isActive)
        {
            Debug.LogWarning("액티브 아이템이 아니거나 null입니다.");
            return false;
        }

        bool success = false;

        switch (item.itemType)
        {
            case ItemType.AttributeResonance:
                success = ToggleResonance(item);  // ★ 추가
                break;
            case ItemType.HealPotion:
                success = UseHealPotion();
                break;

            case ItemType.ShieldPotion:
                success = UseShieldPotion();
                break;

            case ItemType.HowToPlay:
                // UI 열기는 UIManager에서 처리
                success = true;
                break;
            case ItemType.ChaosDice:
                success = UseChaosDice();
                break;

            case ItemType.Tent:
                success = UseTent();
                break;

            case ItemType.MajorBook:
                success = UseMajorBook(item);
                break;

        }

        // ★ 소모성 아이템이면 제거
        if (success && item.isConsumable)
        {
            ownedItems.Remove(item);
            Debug.Log($"{item.displayName} 사용 완료 (소모)");
        }

        return success;
    }
    bool UseChaosDice()
    {
        // 20% 확률로 보드 섞기
        if (Random.value < 0.2f)
        {
            GridManager gridMgr = FindObjectOfType<GridManager>();
            PinSystem pinSystem = FindObjectOfType<PinSystem>();

            if (gridMgr != null)
            {
                gridMgr.ShuffleUnpinnedTiles(pinSystem);
                Debug.Log("혼돈 주사위 발동! 보드 섞기");
            }
        }
        else
        {
            Debug.Log("혼돈 주사위: 효과 없음");
        }

        // 기본 공격력 +5 (영구)
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            player.baseATK += 5f;
            Debug.Log("혼돈 주사위: 기본 공격력 +5");
        }

        return true;
    }

    bool UseTent()
    {
        if (EndlessModeManager.Instance != null)
        {
            EndlessModeManager.Instance.UseTent();
            return true;
        }

        return false;
    }

    bool UseMajorBook(ItemData item)
    {
        if (item == null || !item.isMajorBook)
        {
            Debug.Log("전공 서적이 아닙니다!");
            return false;
        }

        MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
        if (majorSystem == null)
        {
            Debug.Log("MajorSystem을 찾을 수 없습니다!");
            return false;
        }

        bool success = false;

        if (item.isActiveMajor)
        {
            // 액티브 전공 서적
            success = majorSystem.UseMajorBook(true, item.majorType, PassiveType.None);

            if (success)
            {
                Debug.Log($"{item.majorType} 전공 서적 사용 완료!");
            }
            else
            {
                Debug.Log($"{item.majorType} 전공을 보유하고 있지 않아 사용할 수 없습니다!");
            }
        }
        else
        {
            // 페시브 전공 서적
            success = majorSystem.UseMajorBook(false, MajorType.None, item.passiveType);

            if (success)
            {
                Debug.Log($"{item.passiveType} 전공 서적 사용 완료!");
            }
            else
            {
                Debug.Log($"{item.passiveType} 전공을 보유하고 있지 않아 사용할 수 없습니다!");
            }
        }

        return success;
    }
    bool UseHealPotion()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            // 체력이 가득 차있으면 사용 불가
            if (player.currentHP >= player.maxHP)
            {
                Debug.Log("체력이 가득 차있어 회복 물약을 사용할 수 없습니다.");
                return false;
            }

            float healAmount = player.maxHP * 0.2f;
            player.Heal(healAmount);
            Debug.Log($"체력 회복 물약 사용: {healAmount:F1} 회복");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("Heal");

            return true;
        }
        return false;
    }

    bool UseShieldPotion()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            // 쉬드가 최대치면 사용 불가
            if (player.shield >= player.maxShield)
            {
                Debug.Log("쉴드가 최대치여서 쉴드 물약을 사용할 수 없습니다.");
                return false;
            }

            float shieldAmount = player.maxHP * 0.2f;
            player.AddShield(shieldAmount);
            Debug.Log($"쉴드 물약 사용: {shieldAmount:F1} 쉴드 획득");

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MakeShield");

            return true;
        }
        return false;
    }
    public bool ToggleResonance(ItemData resonanceItem)
    {
        if (resonanceItem == null || resonanceItem.itemType != ItemType.AttributeResonance)
        {
            Debug.LogWarning("공명서가 아닙니다.");
            return false;
        }

        // 이미 활성화되어 있으면 비활성화
        if (activeResonances.Contains(resonanceItem))
        {
            activeResonances.Remove(resonanceItem);
            hasResonance = activeResonances.Count > 0;

            Debug.Log($"{resonanceItem.displayName} 비활성화");
            return true;
        }

        // 최대 개수 체크
        if (activeResonances.Count >= MaxActiveResonances)
        {
            Debug.Log($"공명서는 최대 {MaxActiveResonances}개까지 활성화 가능합니다.");
            return false;
        }

        // 속성 겹침 체크
        if (IsElementOverlap(resonanceItem))
        {
            Debug.Log("이미 활성화된 속성과 겹칩니다.");
            return false;
        }

        // 활성화
        activeResonances.Add(resonanceItem);
        hasResonance = true;

        Debug.Log($"{resonanceItem.displayName} 활성화 ({activeResonances.Count}/{MaxActiveResonances})");
        return true;
    }

    // ★ 속성 겹침 체크
    bool IsElementOverlap(ItemData newResonance)
    {
        ElementType newA = newResonance.primaryElement;
        ElementType newB = newResonance.secondaryElement;

        foreach (var activeRes in activeResonances)
        {
            if (activeRes == null) continue;

            ElementType activeA = activeRes.primaryElement;
            ElementType activeB = activeRes.secondaryElement;

            // 하나라도 겹치면 true
            if (newA == activeA || newA == activeB || newB == activeA || newB == activeB)
            {
                return true;
            }
        }

        return false;
    }

    // ★ 특정 속성이 활성화된 공명서에 포함되는지 체크 (ComboManager용)
    public bool IsElementInActiveResonance(ElementType element)
    {
        foreach (var resonance in activeResonances)
        {
            if (resonance == null) continue;

            if (resonance.primaryElement == element || resonance.secondaryElement == element)
            {
                return true;
            }
        }
        return false;
    }

    // ★ 두 속성이 같은 공명서에 속하는지 체크 (ComboManager용)
    public bool AreElementsInSameResonance(ElementType elemA, ElementType elemB)
    {
        foreach (var resonance in activeResonances)
        {
            if (resonance == null) continue;

            bool hasA = (resonance.primaryElement == elemA || resonance.secondaryElement == elemA);
            bool hasB = (resonance.primaryElement == elemB || resonance.secondaryElement == elemB);

            if (hasA && hasB)
            {
                return true;
            }
        }
        return false;
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

        // ★ Major 타일 - 무한모드 + 액티브 전공 체크
        if (element == ElementType.Major)
        {
            GameManager gm = GameManager.Instance;

            // 일반모드면 0%
            if (gm == null || gm.currentGameMode != GameMode.Endless)
                return 0f;

            // 무한모드 - 액티브 전공 체크
            MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
            if (majorSystem == null || majorSystem.GetCurrentActiveMajor() == MajorType.None)
                return 0f;

            // 액티브 전공 있으면 10%
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
            int priority = 1 + orbCount;
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
    public int GetItemCount(ItemData targetItem)
    {
        if (targetItem == null) return 0;

        int count = 0;
        foreach (var item in ownedItems)
        {
            // ★ ScriptableObject 레퍼런스 비교
            if (item == targetItem)
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

        int currentCount = GetItemCount(item);
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
