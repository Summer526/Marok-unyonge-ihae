public enum ElementType
{
    Wind,
    Fire,
    Lightning,
    Water,
    Earth,
    Light,
    Dark,
    Heal,
    Shield
}

public enum TileState
{
    Normal
}

public enum PlayerActionType
{
    Shield,
    Attack,
    Heal
}

public enum ItemType
{
    AttributeResonance,
    AttributeOrb,
    ComboKeeper,
    ChainBooster,
    HealBoost,
    Barrier,
    ShopDiscount,
    LastStand,
    
    ManaBracelet,   // 마나 강화 팔찌
    ManaNecklace,   // 마나 강화 목걸이
    ManaRing,       // 마나 강화 반지
    MadisHand,       // 마디스의 손(장갑)

    HowToPlay,
    HealPotion,    
    ShieldPotion,

    ChaosDice,      // 혼돈 주사위
    FixPin,         // 고정핀

    // 무한모드 아이템
    Vampire,        // 흡혈의 인장
    Doping,         // 도핑약
    DoubleBlade,    // 양날단검
    BlackContract,  // 검은 계약서
    LizardTail,     // 도마뱀 꼬리
    ResidueMana,    // 잔류 마나 구체
    Tent,           // 텐트
    DeadCoin,       // 망자의 동전
    MajorBook       // 전공 서적
}
public enum GameMode
{
    Normal,    // 1~100 스테이지
    Endless    // 100+ 무한모드
}
public enum EnemyState
{
    Normal,     // 정상
    Stunned     // 마비 (번개 전공)
}

// 전공 타입 (액티브 속성)
public enum MajorType
{
    None,       // 전공 없음
    Chaos,      // 혼돈
    Pure,       // 무속성(순수)
    Rune,       // 룬
    Dragon,     // 용
    MagiTech,   // 마도공학
    Barrier     // 결계
}

// 페시브 속성 타입
public enum PassiveType
{
    None,           // 페시브 없음
    Fire_Explosion, // 불-폭발
    Water_Snow,     // 물-눈
    Lightning_Bolt, // 전기-번개
    Wind_Gale,      // 바람-질풍
    Earth_Crystal,  // 땅-크리스탈
    Dark_Death,     // 어둠-흑
    Light_Holy      // 빛-성
}