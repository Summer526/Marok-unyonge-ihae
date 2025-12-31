using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전공 시스템 - 액티브 전공 + 페시브 전공 동시 관리
/// 최대 2개 전공 (각각 레벨 1~5)
/// </summary>
public class MajorSystem : MonoBehaviour
{
    [System.Serializable]
    public class MajorSlot
    {
        public bool isActive;           // true: MajorType, false: PassiveType
        public MajorType majorType;     // 액티브 전공 타입
        public PassiveType passiveType; // 페시브 전공 타입
        public int level;               // 1~5

        public MajorSlot()
        {
            isActive = true;
            majorType = MajorType.None;
            passiveType = PassiveType.None;
            level = 0;
        }

        public bool IsEmpty()
        {
            return level == 0 || (majorType == MajorType.None && passiveType == PassiveType.None);
        }

        public string GetName()
        {
            if (isActive)
                return majorType.ToString();
            else
                return passiveType.ToString();
        }
    }

    [Header("전공 슬롯 (최대 2개)")]
    public MajorSlot slot1 = new MajorSlot();
    public MajorSlot slot2 = new MajorSlot();

    private GridManager gridManager;
    private PlayerStats player;

    // 룬 전공 전용 - 이전 턴 체인 저장
    public int runeLastChain = 0;

    // 마도공학 전공 전용 - 현재 보드 최다 속성
    private ElementType magiTechDominantElement = ElementType.Fire;

    public void Initialize(GridManager grid, PlayerStats playerStats)
    {
        gridManager = grid;
        player = playerStats;

        slot1 = new MajorSlot();
        slot2 = new MajorSlot();

        runeLastChain = 0;
    }

    /// <summary>
    /// 전공 선택 (슬롯 지정)
    /// </summary>
    public void SelectMajor(int slotIndex, bool isActive, MajorType majorType, PassiveType passiveType)
    {
        MajorSlot targetSlot = (slotIndex == 1) ? slot1 : slot2;

        // 같은 전공 선택 시 레벨업
        if (isActive)
        {
            if (targetSlot.isActive && targetSlot.majorType == majorType)
            {
                LevelUp(slotIndex);
                return;
            }
        }
        else
        {
            if (!targetSlot.isActive && targetSlot.passiveType == passiveType)
            {
                LevelUp(slotIndex);
                return;
            }
        }

        // 다른 전공이면 교체
        targetSlot.isActive = isActive;
        targetSlot.majorType = majorType;
        targetSlot.passiveType = passiveType;
        targetSlot.level = 1;

        Debug.Log($"슬롯 {slotIndex} 전공 선택 완료!");

        UpdateVisuals();
    }
    /// <summary>
    /// 전공 레벨업 (최대 5)
    /// </summary>
    public bool LevelUp(int slotIndex)
    {
        MajorSlot targetSlot = (slotIndex == 1) ? slot1 : slot2;

        if (targetSlot.level >= 5)
        {
            Debug.Log($"이미 최대 레벨입니다! ({targetSlot.GetName()} Lv.5)");
            return false;
        }

        targetSlot.level++;
        Debug.Log($"{targetSlot.GetName()} 레벨업! Lv.{targetSlot.level}");

        // 비주얼 갱신
        UpdateVisuals();

        return true;
    }

    /// <summary>
    /// 전공 서적 사용 (해당 전공 레벨업)
    /// </summary>
    public bool UseMajorBook(bool isActive, MajorType majorType, PassiveType passiveType)
    {
        // 해당 전공이 있는지 찾기
        if (isActive)
        {
            if (slot1.isActive && slot1.majorType == majorType)
                return LevelUp(1);

            if (slot2.isActive && slot2.majorType == majorType)
                return LevelUp(2);
        }
        else
        {
            if (!slot1.isActive && slot1.passiveType == passiveType)
                return LevelUp(1);

            if (!slot2.isActive && slot2.passiveType == passiveType)
                return LevelUp(2);
        }

        Debug.Log("해당 전공을 보유하고 있지 않습니다!");
        return false;
    }

    /// <summary>
    /// 특정 전공 보유 여부
    /// </summary>
    public bool HasMajor(MajorType majorType)
    {
        return (slot1.isActive && slot1.majorType == majorType) ||
               (slot2.isActive && slot2.majorType == majorType);
    }

    public bool HasPassive(PassiveType passiveType)
    {
        return (!slot1.isActive && slot1.passiveType == passiveType) ||
               (!slot2.isActive && slot2.passiveType == passiveType);
    }

    /// <summary>
    /// 전공 레벨 가져오기
    /// </summary>
    public int GetMajorLevel(MajorType majorType)
    {
        if (slot1.isActive && slot1.majorType == majorType)
            return slot1.level;

        if (slot2.isActive && slot2.majorType == majorType)
            return slot2.level;

        return 0;
    }

    public int GetPassiveLevel(PassiveType passiveType)
    {
        if (!slot1.isActive && slot1.passiveType == passiveType)
            return slot1.level;

        if (!slot2.isActive && slot2.passiveType == passiveType)
            return slot2.level;

        return 0;
    }

    /// <summary>
    /// 비주얼 갱신 (MajorTile, PlayerMagicCircle)
    /// </summary>
    void UpdateVisuals()
    {
        // MajorTile 갱신 - 액티브 전공이 있으면 타일 변경
        MajorType activeMajor = GetCurrentActiveMajor();
        if (activeMajor != MajorType.None)
        {
            // TODO: MajorTile 프리팹들에게 타입 전달
            Debug.Log($"MajorTile 타입 변경: {activeMajor}");
        }

        // PlayerMagicCircle 갱신 - 페시브 전공이 있으면 색상 변경
        PassiveType passiveMajor = GetCurrentPassiveMajor();
        if (passiveMajor != PassiveType.None)
        {
            PlayerMagicCircle circle = FindObjectOfType<PlayerMagicCircle>();
            if (circle != null)
            {
                circle.SetPassiveType(passiveMajor);
            }
        }
    }

    /// <summary>
    /// 현재 액티브 전공 가져오기 (첫 번째 액티브 슬롯)
    /// </summary>
    public MajorType GetCurrentActiveMajor()
    {
        if (slot1.isActive && slot1.majorType != MajorType.None)
            return slot1.majorType;

        if (slot2.isActive && slot2.majorType != MajorType.None)
            return slot2.majorType;

        return MajorType.None;
    }

    /// <summary>
    /// 현재 페시브 전공 가져오기 (첫 번째 페시브 슬롯)
    /// </summary>
    public PassiveType GetCurrentPassiveMajor()
    {
        if (!slot1.isActive && slot1.passiveType != PassiveType.None)
            return slot1.passiveType;

        if (!slot2.isActive && slot2.passiveType != PassiveType.None)
            return slot2.passiveType;

        return PassiveType.None;
    }

    /// <summary>
    /// 전공 효과 적용 - 데미지 계산
    /// </summary>
    public float ApplyDamageModifier(float baseDamage, int chainCount)
    {
        float finalDamage = baseDamage;

        // 마도공학: 추가 데미지
        if (HasMajor(MajorType.MagiTech))
        {
            int dominantCount = GetDominantElementCount();
            finalDamage += dominantCount;
        }

        // 검은 계약서: 층당 +1% (EndlessModeManager에서 처리)

        return finalDamage;
    }

    /// <summary>
    /// 혼돈 전공 - 다음 턴 보드 섞기 여부
    /// </summary>
    public bool ShouldShuffleBoard()
    {
        return HasMajor(MajorType.Chaos);
    }

    /// <summary>
    /// 결계 전공 - 몹 공격력 감소
    /// </summary>
    public float ApplyBarrierReduction(float enemyAtk, int chainCount)
    {
        if (!HasMajor(MajorType.Barrier))
            return enemyAtk;

        int level = GetMajorLevel(MajorType.Barrier);
        float bonusDamage = 0.03f * level;  // 레벨당 +3%

        float reduction = chainCount;
        float maxReduction = enemyAtk * (0.7f + bonusDamage);

        reduction = Mathf.Min(reduction, maxReduction);

        return Mathf.Max(0f, enemyAtk - reduction);
    }

    /// <summary>
    /// 마도공학 - 현재 보드에서 가장 많은 속성 개수
    /// </summary>
    int GetDominantElementCount()
    {
        if (gridManager == null) return 0;

        Dictionary<ElementType, int> counts = new Dictionary<ElementType, int>();

        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.tiles[x, y] != null)
                {
                    ElementType elem = gridManager.tiles[x, y].elementType;

                    if (!counts.ContainsKey(elem))
                        counts[elem] = 0;

                    counts[elem]++;
                }
            }
        }

        int maxCount = 0;
        foreach (var kvp in counts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                magiTechDominantElement = kvp.Key;
            }
        }

        return maxCount;
    }
    // ==================== 액티브 전공 효과 ====================

    /// <summary>
    /// Chaos - 받는 피해 배수 (25% 증가, 레벨당 -3%)
    /// </summary>
    public float ApplyChaosDamageTaken(float damage)
    {
        if (!HasMajor(MajorType.Chaos))
            return damage;

        int level = GetMajorLevel(MajorType.Chaos);
        float penalty = 0.25f - (level - 1) * 0.03f; // Lv1: 25%, Lv2: 22%, ... Lv5: 13%

        return damage * (1f + penalty);
    }

    /// <summary>
    /// Chaos - 해당 속성이 빛 또는 어둠인지 체크
    /// </summary>
    public bool IsChaosElement(ElementType element)
    {
        if (!HasMajor(MajorType.Chaos))
            return false;

        return element == ElementType.Light || element == ElementType.Dark;
    }

    /// <summary>
    /// Pure - 체인 계수 배수 (0.8배, 레벨당 +0.03)
    /// </summary>
    public float ApplyPureChainMultiplier(float chainMult)
    {
        if (!HasMajor(MajorType.Pure))
            return chainMult;

        int level = GetMajorLevel(MajorType.Pure);
        float mult = 0.8f + (level - 1) * 0.03f; // Lv1: 0.8, Lv2: 0.83, ... Lv5: 0.92

        return chainMult * mult;
    }

    /// <summary>
    /// Rune - 이전 턴 체인 저장
    /// </summary>
    public void SaveRuneChain(int chainCount)
    {
        if (!HasMajor(MajorType.Rune))
            return;

        runeLastChain = chainCount;
        Debug.Log($"룬 체인 저장: {chainCount}");
    }

    /// <summary>
    /// Rune - 다음 공격에 룬 체인 추가
    /// </summary>
    public int ApplyRuneChainBonus(int currentChain)
    {
        if (!HasMajor(MajorType.Rune) || runeLastChain <= 0)
            return currentChain;

        int bonusChain = runeLastChain;
        Debug.Log($"룬 체인 추가: +{bonusChain}");

        return currentChain + bonusChain;
    }

    /// <summary>
    /// Rune - 페널티 체크 (다음 턴 체인이 룬 체인보다 짧으면 HP 감소)
    /// </summary>
    public void CheckRunePenalty(int currentChain, PlayerStats player)
    {
        if (!HasMajor(MajorType.Rune) || runeLastChain <= 0)
            return;

        if (currentChain < runeLastChain)
        {
            int level = GetMajorLevel(MajorType.Rune);
            float penaltyMult = 3f - 0.2f * (level - 1); // Lv1: 3, Lv2: 2.8, ... Lv5: 2.2

            float damage = runeLastChain * penaltyMult;
            player.currentHP -= damage;

            Debug.Log($"룬 페널티! 체인 {currentChain} < {runeLastChain}, HP -{damage:F1}");
        }
    }

    /// <summary>
    /// Dragon - 상성 배수 증가 (1.5배)
    /// </summary>
    public float ApplyDragonAffinityBonus(float affinityMult)
    {
        if (!HasMajor(MajorType.Dragon))
            return affinityMult;

        // 1.3배 → 1.95배 (1.3 * 1.5)
        return affinityMult * 1.5f;
    }

    /// <summary>
    /// Dragon - 상점 가격 배수 (15% 증가, 레벨당 -3%)
    /// </summary>
    public float GetDragonShopPriceMultiplier()
    {
        if (!HasMajor(MajorType.Dragon))
            return 1f;

        int level = GetMajorLevel(MajorType.Dragon);
        float mult = 1.15f - (level - 1) * 0.03f; // Lv1: 1.15, Lv2: 1.12, ... Lv5: 1.03

        return mult;
    }

    /// <summary>
    /// MagiTech - 받는 피해 증가 (15%, 레벨당 -3%)
    /// </summary>
    public float ApplyMagiTechDamageTaken(float damage)
    {
        if (!HasMajor(MajorType.MagiTech))
            return damage;

        int level = GetMajorLevel(MajorType.MagiTech);
        float penalty = 0.15f - (level - 1) * 0.03f; // Lv1: 15%, Lv2: 12%, ... Lv5: 3%

        return damage * (1f + penalty);
    }

    /// <summary>
    /// Barrier - 내 데미지 감소 (-20%, 레벨당 +3%)
    /// </summary>
    public float ApplyBarrierDamagePenalty(float damage)
    {
        if (!HasMajor(MajorType.Barrier))
            return damage;

        int level = GetMajorLevel(MajorType.Barrier);
        float penalty = 0.2f - (level - 1) * 0.03f; // Lv1: 20%, Lv2: 17%, ... Lv5: 8%

        return damage * (1f - penalty);
    }

    // ==================== 페시브 전공 효과 ====================

    /// <summary>
    /// Fire_Explosion - 폭발 확률 및 데미지
    /// </summary>
    public bool TryFireExplosion(out float explosionDamage)
    {
        explosionDamage = 0f;

        if (!HasPassive(PassiveType.Fire_Explosion))
            return false;

        int level = GetPassiveLevel(PassiveType.Fire_Explosion);
        float chance = 0.2f + (level - 1) * 0.03f; // Lv1: 20%, Lv2: 23%, ... Lv5: 32%

        if (Random.value < chance)
        {
            explosionDamage = 10f + (level - 1) * 1f; // Lv1: 10, Lv2: 11, ... Lv5: 14
            return true;
        }

        return false;
    }

    /// <summary>
    /// Water_Snow - 원킬 시 공격력 증가
    /// </summary>
    public void ApplyWaterSnowKill(PlayerStats player)
    {
        if (!HasPassive(PassiveType.Water_Snow) || player == null)
            return;

        int level = GetPassiveLevel(PassiveType.Water_Snow);
        float maxBonus = 10f + (level - 1) * 1f; // Lv1: 10, Lv2: 11, ... Lv5: 14

        // PlayerStats에 누적 공격력 필드 필요
        if (player.waterSnowBonus < maxBonus)
        {
            player.waterSnowBonus += 0.5f;
            Debug.Log($"Water_Snow: 공격력 +0.5 (현재 +{player.waterSnowBonus}/{maxBonus})");
        }
    }

    /// <summary>
    /// Lightning_Bolt - 마비 확률
    /// </summary>
    public bool TryLightningStun(EnemyStats enemy)
    {
        if (!HasPassive(PassiveType.Lightning_Bolt) || enemy == null)
            return false;

        int level = GetPassiveLevel(PassiveType.Lightning_Bolt);
        float chance = 0.2f + (level - 1) * 0.03f; // Lv1: 20%, Lv2: 23%, ... Lv5: 32%

        if (Random.value < chance)
        {
            enemy.ApplyStun(1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Wind_Gale - 회피율 증가
    /// </summary>
    public float GetWindGaleEvasionBonus()
    {
        if (!HasPassive(PassiveType.Wind_Gale))
            return 0f;

        int level = GetPassiveLevel(PassiveType.Wind_Gale);
        return 0.1f + (level - 1) * 0.01f; // Lv1: 10%, Lv2: 11%, ... Lv5: 14%
    }

    /// <summary>
    /// Earth_Crystal - 골드 증가
    /// </summary>
    public int ApplyEarthCrystalGold(int baseGold, int chainCount)
    {
        if (!HasPassive(PassiveType.Earth_Crystal))
            return baseGold;

        int level = GetPassiveLevel(PassiveType.Earth_Crystal);
        int bonus = (chainCount - 3) + (level - 1); // Lv1: chain-3, Lv2: chain-2, ... Lv5: chain+1

        if (bonus > 0)
        {
            Debug.Log($"Earth_Crystal: 골드 +{bonus}");
            return baseGold + bonus;
        }

        return baseGold;
    }

    /// <summary>
    /// Dark_Death - 몹/플레이어 즉사 확률
    /// </summary>
    public bool TryDarkDeathInstantKill(bool isEnemy)
    {
        if (!HasPassive(PassiveType.Dark_Death))
            return false;

        int level = GetPassiveLevel(PassiveType.Dark_Death);

        if (isEnemy)
        {
            // 몹 즉사
            float chance = 0.01f + (level - 1) * 0.005f; // Lv1: 1%, Lv2: 1.5%, ... Lv5: 3%
            return Random.value < chance;
        }
        else
        {
            // 플레이어 즉사
            float chance = 0.001f + (level - 1) * 0.0005f; // Lv1: 0.1%, Lv2: 0.15%, ... Lv5: 0.3%
            return Random.value < chance;
        }
    }

    /// <summary>
    /// Light_Holy - 공격 시 회복 확률
    /// </summary>
    public bool TryLightHolyHeal(float damageDealt, PlayerStats player)
    {
        if (!HasPassive(PassiveType.Light_Holy) || player == null)
            return false;

        int level = GetPassiveLevel(PassiveType.Light_Holy);
        float chance = 0.1f + (level - 1) * 0.03f; // Lv1: 10%, Lv2: 13%, ... Lv5: 22%

        if (Random.value < chance)
        {
            float healAmount = damageDealt * 0.5f;
            player.Heal(healAmount);
            Debug.Log($"Light_Holy: {healAmount:F1} 회복");
            return true;
        }

        return false;
    }
    /// <summary>
    /// 전공 데이터 구조체 (UI 전달용)
    /// </summary>
    [System.Serializable]
    public class MajorData
    {
        public bool isActive;           // true: 액티브, false: 페시브
        public MajorType majorType;     // 액티브 전공 타입
        public PassiveType passiveType; // 페시브 전공 타입

        public MajorData(bool active, MajorType major, PassiveType passive)
        {
            isActive = active;
            majorType = major;
            passiveType = passive;
        }
    }

    /// <summary>
    /// 랜덤 전공 N개 선택 (중복 없음)
    /// </summary>
    public List<MajorData> GetRandomMajors(int count)
    {
        List<MajorData> allMajors = new List<MajorData>();

        // 액티브 전공 6개
        allMajors.Add(new MajorData(true, MajorType.Chaos, PassiveType.None));
        allMajors.Add(new MajorData(true, MajorType.Pure, PassiveType.None));
        allMajors.Add(new MajorData(true, MajorType.Rune, PassiveType.None));
        allMajors.Add(new MajorData(true, MajorType.Dragon, PassiveType.None));
        allMajors.Add(new MajorData(true, MajorType.MagiTech, PassiveType.None));
        allMajors.Add(new MajorData(true, MajorType.Barrier, PassiveType.None));

        // 페시브 전공 7개
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Fire_Explosion));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Water_Snow));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Lightning_Bolt));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Wind_Gale));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Earth_Crystal));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Dark_Death));
        allMajors.Add(new MajorData(false, MajorType.None, PassiveType.Light_Holy));

        // 섞기
        for (int i = 0; i < allMajors.Count; i++)
        {
            int randomIndex = Random.Range(i, allMajors.Count);
            var temp = allMajors[i];
            allMajors[i] = allMajors[randomIndex];
            allMajors[randomIndex] = temp;
        }

        // count개만 반환
        List<MajorData> result = new List<MajorData>();
        for (int i = 0; i < Mathf.Min(count, allMajors.Count); i++)
        {
            result.Add(allMajors[i]);
        }

        return result;
    }
}