using System;
using System.IO;
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