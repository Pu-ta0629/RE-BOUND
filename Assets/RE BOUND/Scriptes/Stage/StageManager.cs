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

    // Scene再読込をしなくなったので、ステージの初期配置を覚えておいて入り直すたびに戻す
    private class TransformSnapshot
    {
        public Transform Target;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    [SerializeField] private List<StageInfo> _stageInfo;

    private readonly Dictionary<GameObject, List<TransformSnapshot>> _snapshots = new();

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

    // GameController.Initialize から1回だけ
    public void Initialize()
    {
        Instance = this;

        _snapshots.Clear();

        foreach (StageInfo info in _stageInfo)
        {
            if (info.StageObj == null) continue;

            _snapshots[info.StageObj] = TakeSnapshot(info.StageObj);
        }
    }

    public void LoadStage(int stageID)
    {
        DisableAllStage();

        foreach (StageInfo info in _stageInfo)
        {
            if (info.StageData.StageID != stageID)
                continue;

            // 先にONにしてから位置を戻す（Animatorの初期ポーズが上書きされないように）
            info.StageObj.SetActive(true);
            RestoreSnapshot(info.StageObj);

            _currentStage = info;

            // 導入アニメ（旧: ステージがONになった瞬間に Animator が流していたもの）
            Stage stage = info.StageObj.GetComponentInChildren<Stage>();
            if (stage != null) stage.PlayIntro();

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

    #region SNAPSHOT

    private List<TransformSnapshot> TakeSnapshot(GameObject stageObj)
    {
        List<TransformSnapshot> list = new();

        foreach (Transform t in stageObj.GetComponentsInChildren<Transform>(true))
        {
            list.Add(new TransformSnapshot
            {
                Target = t,
                LocalPosition = t.localPosition,
                LocalRotation = t.localRotation,
                LocalScale = t.localScale
            });
        }

        return list;
    }

    private void RestoreSnapshot(GameObject stageObj)
    {
        if (!_snapshots.TryGetValue(stageObj, out List<TransformSnapshot> list)) return;

        foreach (TransformSnapshot s in list)
        {
            if (s.Target == null) continue;

            s.Target.SetLocalPositionAndRotation(s.LocalPosition, s.LocalRotation);
            s.Target.localScale = s.LocalScale;
        }

        foreach (Rigidbody2D rb in stageObj.GetComponentsInChildren<Rigidbody2D>(true))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    #endregion
}