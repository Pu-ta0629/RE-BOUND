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

    public void UpdateBestBounce(int stageID, int bounceCount)
    {
        SaveData data = Load();

        if (!_stageRecordDict.TryGetValue(stageID, out StageRecord stageRecord))
        {
            stageRecord = new StageRecord
            {
                StageID = stageID,
                BestBounceCount = bounceCount
            };

            data.StageRecords.Add(stageRecord);
            _stageRecordDict.Add(stageID, stageRecord);
        }
        else
        {
            if (bounceCount < stageRecord.BestBounceCount)
            {
                stageRecord.BestBounceCount = bounceCount;
            }
        }

        Save(data);
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