using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private class Voice
    {
        public AudioSource Source;
        public Enum_SEType Type;
        public float BaseVolume;   
        public float BasePitch;    
        public float StartTime;
    }

    [Header("SE")]
    [SerializeField] private List<AudioData> _audioList;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float _masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float _seVolume = 1f;
    [SerializeField] private bool _mute;

    [Min(1)] [SerializeField] private int _maxVolume = 16;
    [Range(0.1f, 3f)] [SerializeField] private float _globalPitch = 1f;
    [SerializeField] private bool _stealOldestVoice = true;

    private readonly Dictionary<Enum_SEType, AudioData> _dataDict = new();
    private readonly Dictionary<Enum_SEType, float> _lastPlayTime = new();
    private readonly Dictionary<Enum_SEType, int> _sequenceIndex = new();
    private readonly List<Voice> _voices = new();

    public float MasterVolume => _masterVolume;
    public float SEVolume => _seVolume;
    public bool IsMuted => _mute;
    public float GlobalPitch => _globalPitch;
    public int MaxVoices => _maxVolume;

    private float OutputVolume => _mute ? 0f : _masterVolume * _seVolume;

    #region INITIALIZE
    public void Initialize()
    {
        Instance = this;

        BuildDictionary();
        ResizeVoices(_maxVolume);
    }

    private void BuildDictionary()
    {
        _dataDict.Clear();

        foreach (AudioData data in _audioList)
        {
            if (data == null) continue;

            if (!_dataDict.TryAdd(data.SEType, data))
            {
                Debug.LogWarning($"AudioData duplicated : {data.SEType} ({data.name})");
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnValidate()
    {
        _maxVolume = Mathf.Max(1, _maxVolume);

        if (Application.isPlaying && _voices.Count > 0)
        {
            RefreshVoices();
        }
    }

    #endregion

    #region PLAY

    public void Play(Enum_SEType type, float volumeScale = 1f, float pitchScale = 1f)
    {
        if (!_dataDict.TryGetValue(type, out AudioData data))
        {
            Debug.LogWarning($"Audio Not Found : {type}");
            return;
        }

        Play(data, volumeScale, pitchScale);
    }

    public void Play(AudioData data, float volumeScale = 1f, float pitchScale = 1f)
    {
        if (data == null) return;
        if (data.Clips == null || data.Clips.Length == 0) return;
        if (OutputVolume <= 0f) return;

        // クールダウン（連続再生の防止）
        float now = Time.unscaledTime;

        if (data.Cooldown > 0f
            && _lastPlayTime.TryGetValue(data.SEType, out float lastTime)
            && now - lastTime < data.Cooldown)
        {
            return;
        }

        _lastPlayTime[data.SEType] = now;

        switch (data.PlayMode)
        {
            case Enum_SEPlayMode.All:
                foreach (AudioClip clip in data.Clips)
                {
                    PlayClip(data, clip, volumeScale, pitchScale);
                }
                break;

            case Enum_SEPlayMode.Sequential:
                PlayClip(data, GetNextClip(data), volumeScale, pitchScale);
                break;

            default:
                PlayClip(data, data.Clips[Random.Range(0, data.Clips.Length)], volumeScale, pitchScale);
                break;
        }
    }

    private AudioClip GetNextClip(AudioData data)
    {
        _sequenceIndex.TryGetValue(data.SEType, out int index);

        AudioClip clip = data.Clips[index % data.Clips.Length];
        _sequenceIndex[data.SEType] = (index + 1) % data.Clips.Length;

        return clip;
    }

    private void PlayClip(AudioData data, AudioClip clip, float volumeScale, float pitchScale)
    {
        if (clip == null) return;

        Voice voice = GetVoice();
        if (voice == null) return;

        float volume = data.Volume * volumeScale;
        if (data.RandomVolumeRange > 0f)
        {
            volume *= 1f + Random.Range(-data.RandomVolumeRange, data.RandomVolumeRange);
        }

        float pitch = data.Pitch * pitchScale;
        if (data.RandomPitchRange > 0f)
        {
            pitch *= 1f + Random.Range(-data.RandomPitchRange, data.RandomPitchRange);
        }

        voice.Type = data.SEType;
        voice.BaseVolume = Mathf.Clamp01(volume);
        voice.BasePitch = pitch;
        voice.StartTime = Time.unscaledTime;

        AudioSource source = voice.Source;
        source.clip = clip;
        source.volume = voice.BaseVolume * OutputVolume;
        source.pitch = Mathf.Clamp(voice.BasePitch * _globalPitch, 0.1f, 3f);
        source.Play();
    }

    // 空いているボイスを返す。なければ設定に応じて一番古いものを止めて再利用する
    private Voice GetVoice()
    {
        foreach (Voice voice in _voices)
        {
            if (!voice.Source.isPlaying) return voice;
        }

        if (!_stealOldestVoice || _voices.Count == 0) return null;

        Voice oldest = _voices[0];

        foreach (Voice voice in _voices)
        {
            if (voice.StartTime < oldest.StartTime) oldest = voice;
        }

        oldest.Source.Stop();

        return oldest;
    }

    #endregion

    #region STOP

    public void Stop(Enum_SEType type)
    {
        foreach (Voice voice in _voices)
        {
            if (voice.Type == type && voice.Source.isPlaying)
            {
                voice.Source.Stop();
            }
        }
    }

    public void StopAll()
    {
        foreach (Voice voice in _voices)
        {
            voice.Source.Stop();
        }
    }

    #endregion

    #region SETTINGS

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        RefreshVoices();
    }

    public void SetSEVolume(float volume)
    {
        _seVolume = Mathf.Clamp01(volume);
        RefreshVoices();
    }

    public void SetMute(bool mute)
    {
        _mute = mute;
        RefreshVoices();
    }

    public void SetGlobalPitch(float pitch)
    {
        _globalPitch = Mathf.Clamp(pitch, 0.1f, 3f);
        RefreshVoices();
    }

    // 同時に鳴らせる数を変更する
    public void SetMaxVoices(int count)
    {
        _maxVolume = Mathf.Max(1, count);
        ResizeVoices(_maxVolume);
    }

    public void SetStealOldestVoice(bool value)
    {
        _stealOldestVoice = value;
    }

    // 再生中の音にも、音量・ピッチの変更をすぐ反映する
    private void RefreshVoices()
    {
        foreach (Voice voice in _voices)
        {
            if (!voice.Source.isPlaying) continue;

            voice.Source.volume = voice.BaseVolume * OutputVolume;
            voice.Source.pitch = Mathf.Clamp(voice.BasePitch * _globalPitch, 0.1f, 3f);
        }
    }

    #endregion

    #region VOICE POOL

    private void ResizeVoices(int count)
    {
        while (_voices.Count < count)
        {
            _voices.Add(CreateVoice(_voices.Count));
        }

        while (_voices.Count > count)
        {
            Voice last = _voices[_voices.Count - 1];

            last.Source.Stop();
            Destroy(last.Source.gameObject);

            _voices.RemoveAt(_voices.Count - 1);
        }
    }

    private Voice CreateVoice(int index)
    {
        GameObject obj = new GameObject($"Voice_{index:00}");
        obj.transform.SetParent(transform, false);

        AudioSource source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;   //常に2D再生

        return new Voice { Source = source };
    }

    #endregion
}
