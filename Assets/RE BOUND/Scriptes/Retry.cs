using System;
using UnityEngine;
using UnityEngine.UI;

public class Retry : MonoBehaviour
{
    [SerializeField] private Button _button;
    public event Action OnRetry;

    private void Awake()
    {
        _button.onClick.AddListener(InvokeRetry);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(InvokeRetry);
    }

    private void InvokeRetry()
    {
        OnRetry?.Invoke();
    }
}
