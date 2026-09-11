using UnityEngine;

[CreateAssetMenu(
    fileName = "StageData",
    menuName = "Stage/StageData")]
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

    [Header("Movement")]
    [Min(1)]
    public float MoveSpeed = 10f;

    [Header("Rotation")]
    public Enum_RotationMode RotationMode;

    public float RotateSpeed = 360f;
}
