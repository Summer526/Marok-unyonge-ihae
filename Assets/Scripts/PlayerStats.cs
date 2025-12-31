using System.Collections;
using System.Collections.Generic;
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

    [Header("Visual Effects")]
    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();  // ★ 추가
    public float hitFlashDuration = 0.2f;   // 피격 깜빡임 시간
    public float evadeFadeDuration = 0.3f;  // 회피 페이드 시간

    [Header("무한모드 전용")]
    public bool isFirstHitThisTurn = true; // 양날단검용

    [Header("Passive Major Bonuses")]
    public float waterSnowBonus = 0f; // Water_Snow 누적 공격력

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
        return baseATK + waterSnowBonus;
    }

    public void TakeDamage(float damage, bool hasLastStand)
    {
        if (damage <= 0f)
            return;

        lastStandTriggeredThisHit = false;

        // 무한모드 - 망자의 동전 피해 증가
        if (EndlessModeManager.Instance != null)
        {
            damage = EndlessModeManager.Instance.ApplyDeadCoinPenalty(damage);
        }

        // ★ 무한모드 - 전공 받는 피해 증가
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.currentGameMode == GameMode.Endless)
        {
            MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
            if (majorSystem != null)
            {
                damage = majorSystem.ApplyChaosDamageTaken(damage);
                damage = majorSystem.ApplyMagiTechDamageTaken(damage);
            }
        }

        // 무한모드 - 양날단검 첫 피해 감소
        if (EndlessModeManager.Instance != null)
        {
            damage = EndlessModeManager.Instance.ApplyDoubleBladeReduction(damage, ref isFirstHitThisTurn);
        }

        // ★ 무한모드 - Wind_Gale 회피율 보너스
        float evasionBonus = 0f;
        if (gm != null && gm.currentGameMode == GameMode.Endless)
        {
            MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
            if (majorSystem != null)
            {
                evasionBonus = majorSystem.GetWindGaleEvasionBonus();
            }
        }

        // 1) 회피 판정
        if (Random.value < evadeChance + evasionBonus)
        {
            Debug.Log("플레이어 회피!");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySE("MobAttackMiss");

            StartCoroutine(EvadeEffect());
            return;
        }

        // ★ 무한모드 - Dark_Death 플레이어 즉사 확률
        if (gm != null && gm.currentGameMode == GameMode.Endless)
        {
            MajorSystem majorSystem = FindObjectOfType<MajorSystem>();
            if (majorSystem != null && majorSystem.TryDarkDeathInstantKill(false))
            {
                currentHP = 0f;
                Debug.Log("Dark_Death: 플레이어 즉사!");

                WorldUnitHUD hud = GetComponentInChildren<WorldUnitHUD>();
                if (hud != null)
                {
                    hud.UpdateUI();
                }
                return;
            }
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
            else
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySE("ShieldDamage");
            }
        }

        // 3) 남은 데미지를 HP에 적용
        if (remaining > 0f)
        {
            currentHP -= remaining;
            Debug.Log($"플레이어가 {remaining} 피해를 받음. HP: {currentHP:F1}/{maxHP:F1}");
            StartCoroutine(HitEffect());
        }

        // 4) 라스트 스탠드
        if (currentHP <= 0f && hasLastStand && !hasLastStandUsed)
        {
            hasLastStandUsed = true;
            currentHP = maxHP * 0.3f;
            lastStandTriggeredThisHit = true;
            Debug.Log($"라스트 스탠드 발동! HP {currentHP:F1}/{maxHP:F1}로 부활");

            if (gm != null)
            {
                gm.PlayReviveEffect();
            }
        }

        if (currentHP < 0f)
            currentHP = 0f;

        // UI 즉시 업데이트
        WorldUnitHUD playerHUD = GetComponentInChildren<WorldUnitHUD>();
        if (playerHUD != null)
        {
            playerHUD.UpdateUI();
        }
    }
    IEnumerator HitEffect()
    {
        if (spriteRenderers.Count == 0) yield break;

        Color hitColor = new Color(1f, 0.3f, 0.3f, 1f);  // 빨간색
        List<Color> originalColors = new List<Color>();

        // 원래 색상 저장
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                originalColors.Add(sr.color);
                sr.color = hitColor;
            }
        }

        yield return new WaitForSeconds(hitFlashDuration);

        // 원래 색상으로 복구
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null && i < originalColors.Count)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    // ★ 회피 이펙트 - 투명도 낮추기
    IEnumerator EvadeEffect()
    {
        if (spriteRenderers.Count == 0) yield break;

        List<Color> originalColors = new List<Color>();

        // 원래 색상 저장 & 투명도 낮추기
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                originalColors.Add(sr.color);
                Color fadeColor = sr.color;
                fadeColor.a = 0.3f;  // 투명도 30%
                sr.color = fadeColor;
            }
        }

        yield return new WaitForSeconds(evadeFadeDuration);

        // 원래 색상으로 복구
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteRenderers[i] != null && i < originalColors.Count)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
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
        isFirstHitThisTurn = true;
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