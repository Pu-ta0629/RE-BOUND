using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button _gameButton;
    [SerializeField] private Button _stageSelectButton;
    [SerializeField] private Button _quitButton;

    [Header("Animation (Animatorは使わず、コードで再生)")]
    [SerializeField] private GameObject _animationTarget;

    [Tooltip("Do not play if the AnimationClip is null")]
    [SerializeField] private AnimationClip _introClip;
    [SerializeField] private AnimationClip _gameClip;           
    [SerializeField] private AnimationClip _stageSelectClip;    
    [SerializeField] private AnimationClip _quitClip;           

    private bool _isRegistered;
    private bool _isBusy;
    private Coroutine _routine;

    private GameObject Target => _animationTarget != null ? _animationTarget : gameObject;

    // TitleState に入るたびに呼ばれる（位置/Scaleの復元は GameStateBase.Enter が済ませている）
    public void Begin()
    {
        RegisterOnce();

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(IntroRoutine());
    }

    private void RegisterOnce()
    {
        if (_isRegistered) return;
        _isRegistered = true;

        _gameButton.onClick.AddListener(() => Play(_gameClip, LoadInGame));
        _stageSelectButton.onClick.AddListener(() => Play(_stageSelectClip, LoadStageSelect));
        _quitButton.onClick.AddListener(() => Play(_quitClip, LoadQuit));
    }

    private IEnumerator IntroRoutine()
    {
        _isBusy = true;

        yield return ClipPlayer.Play(Target, _introClip);

        _isBusy = false;
        _routine = null;
    }

    // 退出アニメを再生して、終わったら次の処理（旧: AnimationEvent の代わり）
    private void Play(AnimationClip clip, System.Action onFinished)
    {
        if (_isBusy) return;

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(ExitRoutine(clip, onFinished));
    }

    private IEnumerator ExitRoutine(AnimationClip clip, System.Action onFinished)
    {
        _isBusy = true;

        yield return ClipPlayer.Play(Target, clip);

        _routine = null;
        onFinished?.Invoke();   // 画面を切り替えても _isBusy は Begin() で戻る
    }

    // 外部/AnimationEventからも呼べるように名前は残す
    public void LoadInGame() => GameManager.Instance.GoInGame();
    public void LoadStageSelect() => GameManager.Instance.GoStageSelect();
    public void LoadQuit() => GameManager.Instance.Quit();
}