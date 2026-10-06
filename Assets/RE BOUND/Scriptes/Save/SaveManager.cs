using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string _savePath;

    private Dictionary<int, StageRecord> _stageRecordDict;

    public void Initialize()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");

        BuildCache();
    }

    #region CACHE

    private void BuildCache()
    {
        SaveData data = LoadRaw();
        _stageRecordDict = new();
        foreach (StageRecord record in data.StageRecords)
        {
            _stageRecordDict[record.StageID] = record;
        }
    }

    #endregion

    #region SAVE

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        BuildCache();

        Debug.Log($"Save : {_savePath}");
    }

    public SaveData Load()
    {
        SaveData data = LoadRaw();
        if (_stageRecordDict == null)
        {
            BuildCache();
        }

        return data;
    }

    private SaveData LoadRaw()
    {
        if (!File.Exists(_savePath))
        {
            SaveData newData = CreateDefaultData();

            string _json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(_savePath, _json);

            return newData;
        }

        string json = File.ReadAllText(_savePath);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
            data = CreateDefaultData();
        }

        if (data.StageRecords == null)
        {
            data.StageRecords = new();
        }

        return data;
    }

    #endregion

    #region UPDATE

    public SaveData UpdateSaveData(int currentStageID, int maxUnlockedStage)
    {
        SaveData data = Load();

        data.CurrentStageID = currentStageID;
        data.MaxUnlockedStage = maxUnlockedStage;

        Save(data);

        return data;
    }

    /// ステージクリア時の記録。ベストバウンドの更新と★の保存をまとめて行う。
    /// 一度獲得した★は消えない（再挑戦で条件を満たせなくても保持される）。
    public StarResult RecordClear(int stageID, int bounceCount, bool bounceStar, bool swipeStar)
    {
        SaveData data = Load();

        // _stageRecordDict の中身は data とは別のインスタンスなので、
        // 更新する場合は必ず data 側のリストから探す
        StageRecord record = data.StageRecords.Find(r => r.StageID == stageID);

        StageStars before = ToStars(record);

        if (record == null)
        {
            record = new StageRecord
            {
                StageID = stageID,
                BestBounceCount = bounceCount
            };

            data.StageRecords.Add(record);
        }
        else if (bounceCount < record.BestBounceCount)
        {
            record.BestBounceCount = bounceCount;
        }

        record.BounceStar |= bounceStar;
        record.SwipeStar |= swipeStar;

        Save(data);

        return new StarResult(before, ToStars(record));
    }

    #endregion

    #region GET

    public int GetBestBounce(int stageID)
    {
        if (_stageRecordDict.TryGetValue(stageID, out StageRecord stageRecord))
        {
            return stageRecord.BestBounceCount;
        }

        return -1;
    }

    public bool IsStageCleared(int stageID)
    {
        return _stageRecordDict.ContainsKey(stageID);
    }

    public StageRecord GetStageRecord(int stageID)
    {
        _stageRecordDict.TryGetValue(stageID, out StageRecord stageRecord);

        return stageRecord;
    }

    // ステージごとの星の獲得状況
    public StageStars GetStars(int stageID)
    {
        if (_stageRecordDict == null) BuildCache();

        _stageRecordDict.TryGetValue(stageID, out StageRecord record);

        return ToStars(record);
    }

    //クリアしたら★獲得。記録がなければ全て未獲得
    private static StageStars ToStars(StageRecord record)
    {
        if (record == null) return StageStars.None;

        return new StageStars(true, record.BounceStar, record.SwipeStar);
    }

    #endregion

    #region RESET

    public void ClearSave()
    {
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);
        }

        Save(CreateDefaultData());

        Debug.Log("Save Data Reset");
    }

    #endregion

    #region CREATE

    private SaveData CreateDefaultData()
    {
        return new SaveData
        {
            CurrentStageID = 1,
            MaxUnlockedStage = 1,
            StageRecords = new()
        };
    }

    #endregion
}
