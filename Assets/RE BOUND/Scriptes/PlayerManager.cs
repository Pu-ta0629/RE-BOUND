using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public Enum_PlayerType PlayerType;
        public Sprite Sprite;
    }

    [SerializeField] private List<PlayerData> _playerDataList;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private PlayerMovement _playerMovement;

    public PlayerMovement SpawnPlayer(StageData stageData)
    {
        ChangeSprite(stageData.PlayerType);

        _playerMovement.Initialize(
            stageData.MoveSpeed,
            stageData.RotateSpeed,
            stageData.RotationMode,
            stageData.StartPos);

        return _playerMovement;
    }

    public void Initialzie(StageData stageData)
    {
        _playerMovement.Initialize(
            stageData.MoveSpeed,
            stageData.RotateSpeed,
            stageData.RotationMode,
            stageData.StartPos);
    }

    private void ChangeSprite(Enum_PlayerType playerType)
    {
        foreach (var data in _playerDataList)
        {
            if (data.PlayerType != playerType) continue;

            _spriteRenderer.sprite = data.Sprite;

            return;
        }

        Debug.LogWarning($"PlayerType : {playerType} Not Found");
    }
}