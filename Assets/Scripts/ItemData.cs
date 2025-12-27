using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public Sprite icon;          // 아이콘 스프라이트
    public ItemType itemType;
    public string displayName;
    [TextArea(3, 5)]
    public string description;
    public int basePrice;

    [Header("Optional: Element Info (오브/공명 전용)")]
    public ElementType primaryElement;   // 오브나 공명 A
    public ElementType secondaryElement; // 공명 B (오브일 때는 무시)

    [Header("Optional: Major Info (전공 서적 전용)")]
    public bool isMajorBook = false;     // 전공 서적인지 여부
    public bool isActiveMajor = true;    // true: 액티브 전공, false: 페시브 전공
    public MajorType majorType;          // 액티브 전공 타입
    public PassiveType passiveType;      // 페시브 전공 타입

    [Header("Stack / 중복 수 제한")]
    [Tooltip("0 이하면 무제한, 1이면 1개까지만, 3이면 3개까지 소유 가능")]
    public int maxStack = 1;
    [Header("Active Item")]
    public bool isActive = false;        // ★ 추가
    public bool isConsumable = false;    // ★ 추가 - 사용 시 소모되는지
}