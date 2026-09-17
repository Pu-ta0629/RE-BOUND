using UnityEngine;

public class EffectPoolObject : MonoBehaviour
{
    private EffectPool _pool;
    private ParticleSystem _particle;

    private bool _initialized;

    public void Initialize(EffectPool pool)
    {
        _pool = pool;

        if (_particle == null)
        {
            _particle = GetComponent<ParticleSystem>();
        }

        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;
        if (_pool == null) return;
        if (_particle == null) return;

        if (!_particle.IsAlive(true))
        {
            _pool.Return(_particle);
        }
    }
}