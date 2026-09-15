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

    [Header("========== Player ==========")]
    public Enum_PlayerType PlayerType;

    [Header("Movement")] [Min(1)]
    public float MoveSpeed = 10f;

    [Header("Rotation")]
    public Enum_RotationMode RotationMode;
    public float RotateSpeed = 360f;
    [Space(20)]

    [Header("========== Goal ==========")]
    [Header("Move")]
    public Enum_MoveType GoalMoveType;
    public float GoalMoveSpeed = 0f;
    public float GoalMoveRange = 0f;
    public Vector2 GoalFreeDirection;

    [Header("Rotate")]
    public Enum_RotateType GoalRotateType = Enum_RotateType.Constant;
    public float GoalRotateSpeed = 0f;
}