using UnityEngine;

/// <summary>
/// 액티브 전공 타일 - MajorSystem의 현재 전공을 자동 추적
/// </summary>
public class MajorTile : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [System.Serializable]
    public class MajorVisualData
    {
        public MajorType majorType;
        public Sprite sprite;
        public Color color = Color.white;
    }

    [Header("전공별 비주얼 데이터")]
    public MajorVisualData[] visualData;

    private MajorType currentType = MajorType.None;
    private MajorSystem majorSystem;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        majorSystem = FindObjectOfType<MajorSystem>();

        // 초기 비주얼 설정
        UpdateVisual();
    }

    void Update()
    {
        // ★ 매 프레임 전공 변경 체크
        if (majorSystem != null)
        {
            MajorType activeMajor = majorSystem.GetCurrentActiveMajor();

            if (activeMajor != currentType)
            {
                currentType = activeMajor;
                UpdateVisual();
            }
        }
    }

    /// <summary>
    /// 현재 전공에 맞춰 비주얼 업데이트
    /// </summary>
    void UpdateVisual()
    {
        if (currentType == MajorType.None)
        {
            // 전공 없으면 기본 회색
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
            }
            return;
        }

        // 해당 타입의 비주얼 찾기
        foreach (var data in visualData)
        {
            if (data.majorType == currentType)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = data.sprite;
                    spriteRenderer.color = data.color;
                }
                return;
            }
        }

        Debug.LogWarning($"MajorType {currentType}에 대한 비주얼 데이터가 없습니다!");
    }
}