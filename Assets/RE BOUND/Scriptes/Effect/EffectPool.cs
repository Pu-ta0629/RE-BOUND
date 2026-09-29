using System.Collections.Generic;
using UnityEngine;

public class EffectPool
{
    private readonly Queue<ParticleSystem> _pool = new();
    private readonly List<ParticleSystem> _allParticles = new();
    private readonly ParticleSystem _prefab;
    private readonly Transform _parent;

    public EffectPool(ParticleSystem prefab, int count, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < count; i++)
        {
            ParticleSystem effect = Object.Instantiate(_prefab, parent);
            _allParticles.Add(effect);

            EffectPoolObject poolObject = effect.gameObject.GetComponent<EffectPoolObject>();

            if (poolObject == null)
            {
                poolObject = effect.gameObject.AddComponent<EffectPoolObject>();
            }

            poolObject.Initialize(effect, this);
            effect.gameObject.SetActive(false);
            _pool.Enqueue(effect);
        }
    }

    public ParticleSystem Get()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }

        ParticleSystem effect = Object.Instantiate(_prefab, _parent);

        EffectPoolObject poolObject = effect.gameObject.GetComponent<EffectPoolObject>();
        if (poolObject == null)
        {
            poolObject = effect.gameObject.AddComponent<EffectPoolObject>();
        }

        poolObject.Initialize(effect, this);
        effect.gameObject.SetActive(false);

        return effect;
    }

    public void Return(ParticleSystem effect)
    {
        effect.gameObject.SetActive(false);
        _pool.Enqueue(effect);
    }

    public void StopAll()
    {
        foreach(ParticleSystem particle in _allParticles)
        {
            if (particle == null) continue;
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Clear();
            particle.gameObject.SetActive(false);
            if(!_pool.Contains(particle))
            {
                _pool.Enqueue(particle);
            }
        }
    }
}