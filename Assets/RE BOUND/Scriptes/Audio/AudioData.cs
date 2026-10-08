using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Audio/AudioData")]
public class AudioData : ScriptableObject
{
    [Header("ID")]
    public Enum_SEType SEType;

    [Header("Clip")]
    public AudioClip[] Clips;

    public Enum_SEPlayMode PlayMode = Enum_SEPlayMode.Random;

    [Header("Volume")]
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(0f, 0.5f)] public float RandomVolumeRange = 0f;

    [Header("Pitch")]
    [Range(0.1f, 3f)] public float Pitch = 1f;
    [Range(0f, 0.5f)] public float RandomPitchRange = 0f;

    [Header("Limit")]
    [Min(0f)] public float Cooldown = 0f;
}
