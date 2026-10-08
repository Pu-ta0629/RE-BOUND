public enum Enum_PlayerType
{
    Circle,
    Square,
    Triangle,
    Hexagon, 
}

public enum Enum_RotationMode
{
    Normal, 
    Constant, 
    MaxClamp
}

public enum Enum_MoveType
{
    Horizontal,
    Vertical,
    Free
}

public enum Enum_RotateType
{
    Constant,
    Physics
}

public enum Enum_EffectType
{
    PlayerIdle,
    PlayerBounce,
    WallBounce,
    Goal,
    Retry,
    WarpEnter,
    WarpExit
}

public enum Enum_MovePhysicsMode
{
    Constant, 
    Physics
}
public enum Enum_SEType
{
    PlayerBounce,
    PlayerLaunch,
    WallBounce,
    StageClear,
    Retry,
    StarUnlock,
    ButtonClick
}

public enum Enum_SEPlayMode
{
    Random,      
    All,         
    Sequential   
}

public enum Enum_StarType
{
    Clear = 0,
    Bounce = 1,
    Swipe = 2
}
