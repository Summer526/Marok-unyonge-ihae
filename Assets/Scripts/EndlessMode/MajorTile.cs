using UnityEngine;

/// <summary>
/// 액티브 전공 타일 - 전공 타입에 따라 비주얼 변경
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

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 전공 타입에 따라 비주얼 변경
    /// </summary>
    public void SetMajorType(MajorType majorType)
    {
        if (currentType == majorType)
            return;

        currentType = majorType;

        // 해당 타입의 비주얼 찾기
        foreach (var data in visualData)
        {
            if (data.majorType == majorType)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = data.sprite;
                    spriteRenderer.color = data.color;
                }
                Debug.Log($"MajorTile 타입 변경: {majorType}");
                return;
            }
        }

        Debug.LogWarning($"MajorType {majorType}에 대한 비주얼 데이터가 없습니다!");
    }

    /// <summary>
    /// 현재 타일 타입 반환
    /// </summary>
    public MajorType GetCurrentType()
    {
        return currentType;
    }
}