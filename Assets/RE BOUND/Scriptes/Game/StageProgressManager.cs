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

    // クリア画面や状態を残さないように
    public void ResetState()
    {
        _isCleared = false;
        _stageClearUI.Hide();
    }

    #region CLEAR
    public void StageClear()
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

        // Startの獲得演出 → 演出後にボタン表示 → 押したら NextStageへ
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
            GameManager.Instance.GoTitle();
            return;
        }

        GameManager.Instance.SetCurrentStage(nextStage);
        GameController.Instance.LoadStage(nextStage);

        RetryManager.Instance.Retry();
    }

    #endregion
}
