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
    ShieldPotion
}
public enum GameMode
{
    Normal,    // 1~100 스테이지
    Endless    // 100+ 무한모드
}