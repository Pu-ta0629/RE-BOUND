using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class StageButton
    {
        public StageData StageData;

        public Button Button;
    }

    [SerializeField] private List<StageButton> _stageButtons;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Initialize();
    }
    public void Initialize()
    {
        int maxUnlock = GameManager.Instance.MaxUnlockStage;

        foreach (StageButton stage in _stageButtons)
        {
            bool canPlay = stage.StageData.StageID < maxUnlock + 1;

            stage.Button.interactable = canPlay;

            int id = stage.StageData.StageID;

            stage.Button.onClick.RemoveAllListeners();

            stage.Button.onClick.AddListener(() =>{SelectStage(id);});
        }
    }

    private void SelectStage(int stageID)
    {
        GameManager.Instance.SetCurrentStage(stageID);

        GameManager.Instance.LoadInGame();
    }
}