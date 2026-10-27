using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Stage/StageData")]
public class StageData : ScriptableObject
{
    [Header("========== Stage ==========")]
    public string StageName;
    public int StageID;

    [Header("Position")]
    public Vector2 StartPos;
    public Vector2 GoalPos;

    [Header("========== Star Condition ==========")]
    public int SwipeLimit = 10;
    public int BounceLimit = 10;

    [Header("========== Player ==========")]
    public Enum_PlayerType PlayerType;

    [Header("Movement")]
    [Min(1)]
    public float MoveSpeed = 25f;

    [Header("Rotation")]
    public Enum_RotationMode RotationMode;
    public float RotateSpeed = 360f;
    [Space(20)]

    [Header("========== Goal ==========")]
    [Header("Move")]
    public Enum_MoveType GoalMoveType;
    public float GoalMoveSpeed = 0f;
    [Tooltip("Horizontal / Vertical : GoalPos からの往復の幅 / Free : GoalPos を中心に動き回る円の半径")]
    public float GoalMoveRange = 0f;
    [Tooltip("Free のときの曲がる速さ（度/秒）。小さいほど大きく弧を描く")]
    public float FreeTurnRate = 180f;
    [Tooltip("Free のとき、目的地に着いたとみなす距離")]
    public float FreeArriveDistance = 0.3f;
    [Tooltip("Free のとき、目的地に着けなくても次の目的地に変える秒数")]
    public float FreeRetargetTime = 4f;

    [Header("Rotate")]
    public Enum_RotateType GoalRotateType = Enum_RotateType.Constant;
    public float GoalRotateSpeed = 0f;

}
