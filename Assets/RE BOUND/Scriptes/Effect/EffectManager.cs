using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance {get; private set;}

    [System.Serializable]
    public class EffectData
    {
        public Enum_EffectType EffectType;
        public ParticleSystem Prefab;
        public Transform Parent;
        public int PoolSize = 10;
    }

    [SerializeField] private List<EffectData> _effectList;

    private Dictionary<Enum_EffectType, EffectPool> _effectPools;

    public void Initialize()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _effectPools = new();

        foreach (var data in _effectList)
        {
            EffectPool pool = new EffectPool(data.Prefab, data.PoolSize, data.Parent);
            _effectPools.Add(data.EffectType,pool);
        }
    }

    public void Play(Enum_EffectType effectType, Vector3 position, Vector2 direction)
    {
        if (!_effectPools.TryGetValue(effectType, out EffectPool pool))
        {
            Debug.LogWarning($"Effect Not Found : {effectType}");
            return;
        }

        ParticleSystem effect = pool.Get();

        effect.transform.position = position;
        effect.transform.up = direction;
        effect.gameObject.SetActive(true);
        effect.Clear();
        effect.Play();
    }
    
    public void PlayWallBounce(Enum_EffectType effectType, Vector3 position, Vector2 direction)
    {
        if (!_effectPools.TryGetValue(effectType, out EffectPool pool))
        {
            Debug.LogWarning($"Effect Not Found : {effectType}");
            return;
        }

        ParticleSystem effect = pool.Get();

        effect.transform.position = position;
        ParticleSystemRenderer renderer = effect.GetComponent<ParticleSystemRenderer>();
        
        renderer.material.SetVector("_HitPosition", position);
        effect.transform.up = direction;
        effect.gameObject.SetActive(true);
        effect.Clear();
        effect.Play();
    }
}