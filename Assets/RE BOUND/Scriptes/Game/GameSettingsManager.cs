using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    public static GameSettingsManager Instance{get; private set;}

    [Header("FPS")]
    [Range(-1, 240), SerializeField] private int _targetFPS = 120;

    [Header("VSync")]
    [SerializeField] private bool _useVSync = false;

    [Header("Audio")]
    [Range(0f, 1f), SerializeField] private float _bgmVolume = 1f;

    [Range(0f, 1f),SerializeField] private float _seVolume = 1f;
    [SerializeField] private bool _bgmMute;
    [SerializeField] private bool _seMute;

    [Header("Debug")]
    [SerializeField] private bool _isPaused;
    private float _fpsTimer;

    public bool IsPaused => _isPaused;
    public float BgmVolume => _bgmVolume;
    public float SeVolume => _seVolume;
    public bool BgmMute => _bgmMute;
    public bool SeMute => _seMute;
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
        QualitySettings.vSyncCount = _useVSync ? 1 : 0;

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
    public void SetVSync(bool enable)
    {
        _useVSync = enable;

        QualitySettings.vSyncCount = enable ? 1 : 0;
    }
    #endregion

    #region Audio
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = volume;
    }
    public void SetSEVolume(float volume)
    {
        _seVolume = volume;
    }
    public void SetBGMMute(bool mute)
    {
        _bgmMute = mute;
    }
    public void SetSEMute(bool mute)
    {
        _seMute = mute;
    }
    #endregion
} 