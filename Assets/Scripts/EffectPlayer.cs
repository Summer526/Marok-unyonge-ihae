using UnityEngine;

/// <summary>
/// 애니메이션 이펙트 재생 후 자동 삭제
/// </summary>
public class EffectPlayer : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;

    [Header("Manual Duration (0 = Auto)")]
    public float manualDuration = 0f; // 0이면 자동, 값 있으면 수동 설정

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        float duration = manualDuration;

        // manualDuration이 0이면 자동으로 애니메이션 길이 감지
        if (duration <= 0f && animator != null)
        {
            duration = GetAnimationLength();
        }

        // 기본값 (애니메이션 없으면 1초)
        if (duration <= 0f)
            duration = 1f;

        Destroy(gameObject, duration);
        Debug.Log($"EffectPlayer: {duration}초 후 삭제");
    }

    /// <summary>
    /// 현재 애니메이션 클립 길이 가져오기
    /// </summary>
    float GetAnimationLength()
    {
        if (animator == null) return 0f;

        // Animator가 초기화될 때까지 대기
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        if (ac == null) return 0f;

        // 첫 번째 애니메이션 클립 길이 가져오기
        AnimationClip[] clips = ac.animationClips;
        if (clips != null && clips.Length > 0)
        {
            float length = clips[0].length;
            Debug.Log($"애니메이션 길이 감지: {length}초");
            return length;
        }

        return 0f;
    }
}