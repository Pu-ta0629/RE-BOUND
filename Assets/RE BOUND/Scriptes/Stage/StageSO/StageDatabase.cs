using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Stage/StageDatabase")]
public class StageDatabase : ScriptableObject
{
    [SerializeField] private List<StageData> _stages;

    // ステージ数は少なく、呼ばれるのもホバー時だけなので、毎回探して構わない
    public StageData Get(int stageID)
    {
        foreach (StageData data in _stages)
        {
            if (data != null && data.StageID == stageID) return data;
        }

        return null;
    }
}
