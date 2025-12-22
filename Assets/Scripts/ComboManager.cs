using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public ElementType? lastAttackElement = null;
    public int comboStreak = 1;
    public int nonAttackInARow = 0;

    private ItemManager itemManager;

    public void Initialize(ItemManager itemMgr)
    {
        itemManager = itemMgr;
        lastAttackElement = null;
        comboStreak = 1;
        nonAttackInARow = 0;
    }

    public void OnAttack(ElementType element)
    {
        // 속성 공명 체크
        bool isSameGroup = false;

        if (lastAttackElement.HasValue)
        {
            if (lastAttackElement.Value == element)
            {
                isSameGroup = true;
            }
            else if (itemManager != null && itemManager.hasResonance)
            {
                // ★ 새로운 공명 체크 방식
                bool lastInResonance = itemManager.IsElementInActiveResonance(lastAttackElement.Value);
                bool currentInResonance = itemManager.IsElementInActiveResonance(element);

                if (lastInResonance && currentInResonance)
                {
                    // 같은 공명서에 속하는지 체크
                    isSameGroup = itemManager.AreElementsInSameResonance(lastAttackElement.Value, element);
                }
            }
        }

        if (isSameGroup)
        {
            comboStreak++;
        }
        else
        {
            comboStreak = 1;
        }

        lastAttackElement = element;
        nonAttackInARow = 0;

        Debug.Log($"콤보 스택: {comboStreak} (속성: {element})");
    }

    public void OnNonAttack()
    {
        if (itemManager != null && itemManager.hasComboKeeper)
        {
            nonAttackInARow++;

            if (nonAttackInARow >= 2)
            {
                // 비공격 행동이 2번 연속이면 콤보 끊김
                comboStreak = 1;
                lastAttackElement = null;
                Debug.Log("콤보 끊김 (비공격 2회 연속)");
            }
            else
            {
                Debug.Log("콤보 유지 (비공격 1회)");
            }
        }
        else
        {
            // 콤보 유지 아이템 없으면 바로 끊김
            comboStreak = 1;
            lastAttackElement = null;
            Debug.Log("콤보 끊김 (비공격)");
        }
    }
    public void ResetCombo()
    {
        comboStreak = 1;
        lastAttackElement = null;
        nonAttackInARow = 0;
        Debug.Log("라스트 스탠드로 콤보 리셋");
    }
    public float GetComboMultiplier()
    {
        float mult = 1f + 0.15f * (comboStreak - 1);
        return Mathf.Min(mult, 2f);
    }
}