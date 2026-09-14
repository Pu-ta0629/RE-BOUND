using System;
using System.IO;
using System.Xml.Serialization;
using UnityEditor.Overlays;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string _savePath;

    public void Initialize()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(_savePath, json);

        Debug.Log($"Save : {_savePath}");
    }

    public SaveData Load()
    {
        if (!File.Exists(_savePath))
        {
            SaveData newData = CreateDefaultData();

            Save(newData);

            return newData;
        }

        string json = File.ReadAllText(_savePath);

        return JsonUtility.FromJson<SaveData>(json);
    }

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

        StageRecord stageRecord = data.StageRecords.Find(x => x.StageID == stageID);

        if (stageRecord == null)
        {
            stageRecord = new StageRecord()
            {
                StageID = stageID,
                BestBounceCount = bounceCount
            };

            data.StageRecords.Add(stageRecord);
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

    private SaveData CreateDefaultData()
    {
        return new SaveData()
        {
            CurrentStageID = 1,
            MaxUnlockedStage = 1
        };
    }

    //internal void Save(int currentStageID, object maxUnlockedStage)
    //{
    //    throw new NotImplementedException();
    //}
}