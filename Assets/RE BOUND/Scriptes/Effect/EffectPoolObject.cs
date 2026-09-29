using UnityEngine;

public class EffectPoolObject : MonoBehaviour
{
    private ParticleSystem _particle;
    private EffectPool _pool;

    public void Initialize(ParticleSystem particle, EffectPool pool)
    {
        _particle = particle;
        _pool = pool;

        ParticleSystem.MainModule main = _particle.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    private void OnParticleSystemStopped()
    {
        _pool.Return(_particle);
    }
}