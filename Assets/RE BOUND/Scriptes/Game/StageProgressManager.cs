using UnityEngine;

public class StageProgressManager : MonoBehaviour
{
    public static StageProgressManager Instance
    {
        get;
        private set;
    }

    [SerializeField] private StageClearUI _stageClearUI;

    private bool _isCleared;
    public bool IsCleared => _isCleared;

    public void Initialize()
    {
        Instance = this;
        _isCleared = false;

        _stageClearUI.Initialize();
    }

    #region CLEAR
    public void StageClear() // next stage transition
    {
        if (_isCleared) return;
        _isCleared = true;

        GameManager gameManager = GameManager.Instance;
        StageData stageData = StageManager.Instance.CurrentStageData;
        PlayerMovement player = PlayerManager.Instance.Player;

        int stageID = gameManager.CurrentStageID;

        player.BeginResult();

        int bounceCount = player.BounceCount;
        int swipeCount = player.SwipeCount;

        bool bounceStar = IsWithinLimit(bounceCount, stageData.BounceLimit, "BounceLimit", stageData);
        bool swipeStar = IsWithinLimit(swipeCount, stageData.SwipeLimit, "SwipeLimit", stageData);

        StarResult result = gameManager.SaveManager.RecordClear(stageID, bounceCount, swipeCount, bounceStar, swipeStar);

        // 次のステージを解放
        int nextStage = stageID + 1;
        bool hasNext = StageManager.Instance.IsExistStage(nextStage);

        if (hasNext)
        {
            gameManager.UnlockStage(nextStage);
        }

        // ★の獲得演出 → 演出後にボタン表示 → 押したら GoNext
        _stageClearUI.Show(stageID, result, hasNext, () => GoNext(nextStage, hasNext));
    }

    private bool IsWithinLimit(int count, int limit, string limitName, StageData stageData)
    {
        if (limit <= 0)
        {
            Debug.LogWarning($"{stageData.StageName} : {limitName} が未設定のため★を獲得できません");
            return false;
        }

        return count <= limit;
    }

    #endregion

    #region NEXT
    private void GoNext(int nextStage, bool hasNext)
    {
        _isCleared = false;

        if (!hasNext)
        {
            GameManager.Instance.LoadStart();
            return;
        }

        GameManager.Instance.SetCurrentStage(nextStage);
        StageManager.Instance.LoadStage(nextStage);

        FindFirstObjectByType<GimmickManager>().Initialize();
        RetryManager.Instance.Retry();
    }

    #endregion
}
