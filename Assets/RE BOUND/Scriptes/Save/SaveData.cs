using System;
using System.Collections.Generic;
[Serializable]
public class SaveData
{
    public int CurrentStageID;
    public int MaxUnlockedStage;

    public List<StageRecord> StageRecords = new();
}