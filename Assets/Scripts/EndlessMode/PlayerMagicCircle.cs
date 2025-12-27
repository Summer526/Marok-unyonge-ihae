using UnityEngine;

/// <summary>
/// 플레이어 발 밑 마법진 - 페시브 전공 색상 표현
/// </summary>
public class PlayerMagicCircle : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer spriteRenderer;

    [Header("페시브 전공별 색상")]
    public Color explosionColor = new Color(1f, 0.3f, 0f);    // 불-폭발 (주황)
    public Color snowColor = new Color(0.5f, 0.8f, 1f);       // 물-눈 (하늘색)
    public Color lightningColor = new Color(1f, 1f, 0f);      // 전기-번개 (노랑)
    public Color stormColor = new Color(0.6f, 1f, 0.6f);      // 바람-질풍 (연두)
    public Color crystalColor = new Color(0.6f, 0.4f, 0.2f);  // 땅-크리스탈 (갈색)
    public Color darkColor = new Color(0.3f, 0f, 0.3f);       // 어둠-흑 (보라)
    public Color holyColor = new Color(1f, 1f, 0.8f);         // 빛-성 (금색)
    public Color defaultColor = new Color(0.5f, 0.5f, 0.5f);  // 페시브 없음 (회색)

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // 초기 색상
        if (spriteRenderer != null)
            spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// 페시브 타입에 따라 색상 변경
    /// </summary>
    public void SetPassiveType(PassiveType passiveType)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer가 없습니다!");
            return;
        }

        Color targetColor = defaultColor;

        switch (passiveType)
        {
            case PassiveType.Fire_Explosion:
                targetColor = explosionColor;
                break;
            case PassiveType.Water_Snow:
                targetColor = snowColor;
                break;
            case PassiveType.Lightning_Bolt:
                targetColor = lightningColor;
                break;
            case PassiveType.Wind_Gale:
                targetColor = stormColor;
                break;
            case PassiveType.Earth_Crystal:
                targetColor = crystalColor;
                break;
            case PassiveType.Dark_Death:
                targetColor = darkColor;
                break;
            case PassiveType.Light_Holy:
                targetColor = holyColor;
                break;
            default:
                targetColor = defaultColor;
                break;
        }

        spriteRenderer.color = targetColor;
        Debug.Log($"마법진 색상 변경: {passiveType}");
    }
}