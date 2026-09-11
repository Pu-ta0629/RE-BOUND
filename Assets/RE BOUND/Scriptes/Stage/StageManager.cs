using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    [System.Serializable]
    public class StageInfo
    {
        public StageData StageData;
        public GameObject StageObj;
    }

    [SerializeField]
    private List<StageInfo> _stageInfo;

    private StageInfo _currentStage;

    public StageData CurrentStageData
    {
        get
        {
            if (_currentStage == null)
            {
                Debug.LogError("CurrentStage is NULL");

                return null;
            }

            return _currentStage.StageData;
        }
    }
    public void Initialize(int currentStageID)
    {
        if (Instance == null)
        {
            Instance = this;
        }

        LoadStage(currentStageID);
        Debug.Log(GameManager.Instance.CurrentStageID);
    }

    public void LoadStage(int stageID)
    {
        DisableAllStage();

        foreach (StageInfo info in _stageInfo)
        {
            if (info.StageData.StageID != stageID)
                continue;

            info.StageObj.SetActive(true);

            _currentStage = info;

            Debug.Log($"Load Stage : {info.StageData.StageName}");

            return;
        }

        Debug.LogError($"Stage ID : {stageID} Not Found");
    }
    public bool IsExistStage(int stageID)
    {
        foreach (StageInfo info in _stageInfo)
        {
            if (info.StageData.StageID == stageID)
            {
                return true;
            }
        }

        return false;
    }
    private void DisableAllStage()
    {
        foreach (StageInfo info in _stageInfo)
        {
            if (info.StageObj == null)
                continue;

            info.StageObj.SetActive(false);
        }
    }
}