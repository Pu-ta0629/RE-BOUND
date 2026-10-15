using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance{get; private set;}

    [Header("FPS")]
    [Range(-1, 240), SerializeField] private int _targetFPS = 120;

    [Header("Debug")]
    [SerializeField] private bool _isPaused;
    private float _fpsTimer;

    public bool IsPaused => _isPaused;
    public float CurrentFPS { get; private set; }

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        ApplySettings();
    }

    private void Update()
    {
        _fpsTimer += (Time.unscaledDeltaTime - _fpsTimer) * 0.1f;

        CurrentFPS = 1f / _fpsTimer;
    }

    public void ApplySettings()
    {
        Application.targetFrameRate = _targetFPS;
    }

    #region Pause
    public void SetPause(bool pause)
    {
        _isPaused = pause;

        Time.timeScale = pause ? 0f : 1f;
    }
    public void TogglePause()
    {
        SetPause(!_isPaused);
    }
    #endregion

    #region FPS
    public void SetFPS(int fps)
    {
        _targetFPS = fps;

        Application.targetFrameRate = fps;
    }
    #endregion
} 