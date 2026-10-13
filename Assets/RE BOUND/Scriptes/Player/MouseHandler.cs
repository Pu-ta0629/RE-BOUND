using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(TrailRenderer))]

public class MouseHandler : MonoBehaviour
{
    private TrailRenderer _trailRenderer;

    private void NULLCHECK()
    {
        if (_trailRenderer == null)
            Debug.LogError($"{name} : TrailRenderer not found");
    }
    #region UNITY EVENT
    private void OnEnable()
    {
        Cache();
        NULLCHECK();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ClearTrail();
    private void Cache()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }
    private void Update()
    {
        transform.position = GetMouseWorldPosition();
    }
    #endregion
    public void ClearTrail()
    {
        _trailRenderer.Clear();
    }
    private Vector2 GetMouseWorldPosition()
    {
        return GameManager.Instance.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
