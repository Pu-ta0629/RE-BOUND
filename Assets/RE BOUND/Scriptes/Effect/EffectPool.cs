using System.Collections.Generic;
using UnityEngine;

public class EffectPool
{
    private readonly Queue<ParticleSystem> _pool = new();
    private readonly ParticleSystem _prefab;
    private readonly Transform _parent;

    public EffectPool(ParticleSystem prefab, int count, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < count; i++)
        {
            ParticleSystem effect = Object.Instantiate(_prefab, parent);

            EffectPoolObject poolObject = effect.gameObject.GetComponent<EffectPoolObject>();

            if (poolObject == null)
            {
                poolObject = effect.gameObject.AddComponent<EffectPoolObject>();
            }

            poolObject.Initialize(this);
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

        poolObject.Initialize(this);
        effect.gameObject.SetActive(false);

        return effect;
    }

    public void Return(ParticleSystem effect)
    {
        effect.gameObject.SetActive(false);
        _pool.Enqueue(effect);
    }
}