using UnityEngine;

public class StageProgressManager : MonoBehaviour
{
    public static StageProgressManager Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void NextStage()
    {
        int bounceCount = PlayerManager.Instance.Player.BounceCount;

        GameManager.Instance.SaveManager.UpdateBestBounce(GameManager.Instance.CurrentStageID, bounceCount);

        int nextStage = GameManager.Instance.CurrentStageID + 1;

        if (!StageManager.Instance.IsExistStage(nextStage))
        {
            GameManager.Instance.LoadStart();
            return;
        }

        GameManager.Instance.UnlockStage(nextStage);
        GameManager.Instance.SetCurrentStage(nextStage);
        StageManager.Instance.LoadStage(nextStage);

        FindFirstObjectByType<GimmickManager>().Initialize();
        RetryManager.Instance.Retry();
    }
}