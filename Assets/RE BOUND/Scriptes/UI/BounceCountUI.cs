using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;

public class BounceCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Transform _stageTransform;

    private int _currentCount = -1;
    private void OnEnable()
    {
        _currentCount = -1;
    }
    private void Update()
    {
        if (PlayerManager.Instance == null) return;
        if (PlayerManager.Instance.Player == null) return;
        if (GameController.Instance == null) return;

        int count = PlayerManager.Instance.Player.BounceCount;
        if (_currentCount == count) return;
        _text.transform.position = new Vector2(_stageTransform.position.x, _text.transform.position.y);

        _currentCount = count;

        _text.text = Mathf.Min(_currentCount, 99999).ToString();
    }
}