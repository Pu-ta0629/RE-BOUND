using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearUI : MonoBehaviour
{
    [Header("Root (このスクリプトとは別のオブジェクト)")]
    [SerializeField] private GameObject _root;

    [Header("UI")]
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private StarDisplay _starDisplay;
    [SerializeField] private Button _nextButton;
    [SerializeField] private TMP_Text _nextButtonLabel;

    [Header("Timing")]
    [SerializeField] private float _startDelay = 0.3f;
    [SerializeField] private float _buttonDelay = 0.3f;

    private Action _onNext;
    private Coroutine _routine;

    public void Initialize()
    {
        _nextButton.onClick.RemoveAllListeners();
        _nextButton.onClick.AddListener(OnNextClicked);

        Hide();
    }

    public void Show(int stageID, StarResult result, bool hasNext, Action onNext)
    {
        _onNext = onNext;

        _root.SetActive(true);
        _stageText.text = $"STAGE {stageID:00} CLEAR";

        if (_nextButtonLabel != null)
        {
            _nextButtonLabel.text = hasNext ? "NEXT" : "TITLE";
        }

        _nextButton.gameObject.SetActive(false);

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(result));
    }

    public void Hide()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        _root.SetActive(false);
    }

    private IEnumerator ShowRoutine(StarResult result)
    {
        yield return new WaitForSecondsRealtime(_startDelay);

        yield return _starDisplay.PlayResult(result);

        yield return new WaitForSecondsRealtime(_buttonDelay);

        _nextButton.gameObject.SetActive(true);
        _routine = null;
    }

    private void OnNextClicked()
    {
        Action onNext = _onNext;
        _onNext = null;

        Hide();
        onNext?.Invoke();
    }
}
