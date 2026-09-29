using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RetryManager : MonoBehaviour
{
    public static RetryManager Instance { get; private set; }

    private bool _isRetrying;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsRetrying => _isRetrying;

    public void Retry()
    {
        if (_isRetrying) return;

        StartCoroutine(RetryRoutine());
    }

    private IEnumerator RetryRoutine()
    {
        _isRetrying = true;

        PlayerMovement player = PlayerManager.Instance.Player;
        EffectManager.Instance.StopAllEffects();
        player.gameObject.SetActive(false);
        EffectManager.Instance.Play(Enum_EffectType.Retry, player.transform.position, Vector2.up);
        yield return new WaitForSecondsRealtime(0.15f);
        PlayerManager.Instance.Initialize(StageManager.Instance.CurrentStageData); 
        _isRetrying = false;
    }
}