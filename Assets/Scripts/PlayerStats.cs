using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int stage;
    public float maxHP;
    public float currentHP;
    public float baseATK = 10f;
    public float evadeChance = 0.2f;
    public float shield = 0f;
    public float maxShield;

    public bool hasLastStandUsed = false;
    public bool lastStandTriggeredThisHit = false;

    public void UpdateStatsForStage(int newStage)
    {
        stage = newStage;
        maxHP = 100f * Mathf.Pow(1.03f, stage - 1);
        baseATK = 10f * Mathf.Pow(1.04f, stage - 1);
        maxShield = maxHP * 0.4f;

        if (stage == 1)
        {
            currentHP = maxHP;
        }
    }

    public float GetCurrentATK()
    {
        return baseATK;
    }

    public void TakeDamage(float damage, bool hasLastStand)
    {
        if (damage <= 0f)
            return;
        lastStandTriggeredThisHit = false;

        // 1) 회피 판정
        if (Random.value < evadeChance)
        {
            Debug.Log("플레이어 회피!");
            return;
        }

        float remaining = damage;

        // 2) 쉴드로 먼저 흡수
        if (shield > 0f)
        {
            float absorbed = Mathf.Min(shield, remaining);
            shield -= absorbed;
            remaining -= absorbed;
            Debug.Log($"실드로 {absorbed} 피해 흡수, 남은 쉴드: {shield:F1}");
            if (shield <= 0f)
            {
                shield = 0f;
                Debug.Log("실드 파괴!");

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySE("ShieldBroke");
            }
        }

        // 3) 남은 데미지를 HP에 적용
        if (remaining > 0f)
        {
            currentHP -= remaining;
            Debug.Log($"플레이어가 {remaining} 피해를 받음. HP: {currentHP:F1}/{maxHP:F1}");
        }

        // 4) 라스트 스탠드
        if (currentHP <= 0f && hasLastStand && !hasLastStandUsed)
        {
            hasLastStandUsed = true;
            currentHP = maxHP * 0.3f;
            lastStandTriggeredThisHit = true;
            Debug.Log($"라스트 스탠드 발동! HP {currentHP:F1}/{maxHP:F1}로 부활");
        }

        if (currentHP < 0f)
            currentHP = 0f;
    }


    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public void OnEnemyKilled()
    {
        float healAmount = maxHP * 0.1f;
        Heal(healAmount);
        Debug.Log($"몹 처치 자동 회복: {healAmount}");
    }

    public void AddShield(float amount)
    {
        if (amount <= 0f) return;

        shield += amount;

        Debug.Log($"쉴드 +{amount:F1} (현재 {shield:F1})");
    }

    public void AddShieldPercent(float percent)
    {
        if (percent <= 0f) return;

        float amount = maxHP * percent;
        shield += amount;

        Debug.Log($"쉴드 +{amount:F1} (현재 {shield:F1})");
    }

    public bool IsDead()
    {
        return currentHP <= 0;
    }
}