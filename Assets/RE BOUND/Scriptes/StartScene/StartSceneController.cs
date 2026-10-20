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
    [Tooltip("クリップのパスの基準（元の Animator が付いていた TitleRoot）。空ならこのオブジェクト")]
    [SerializeField] private GameObject _animationTarget;

    [Tooltip("入場アニメ（旧 StartScene のクリップ）。空なら再生しない")]
    [SerializeField] private AnimationClip _introClip;
    [SerializeField] private AnimationClip _gameClip;        
    [SerializeField] private AnimationClip _stageSelectClip; 
    [SerializeField] private AnimationClip _quitClip;        

    [Tooltip("残っている旧 Animator（あれば自動で無効にする。競合を防ぐ）")]
    [SerializeField] private Animator _legacyAnimator;

    private bool _isRegistered;
    private bool _isBusy;
    private Coroutine _routine;

    public bool IsBusy => _isBusy;

    private GameObject Target => _animationTarget != null ? _animationTarget : gameObject;

    // TitleState に入るたびに呼ばれる（位置/Scaleの復元は GameStateBase.Enter が済ませている）
    public void Begin()
    {
        RegisterOnce();

        if (_legacyAnimator != null) _legacyAnimator.enabled = false;

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(IntroRoutine());
    }

    private void RegisterOnce()
    {
        if (_isRegistered) return;
        _isRegistered = true;

        ButtonSE se = GameManager.Instance.ButtonSE;

        _gameButton.onClick.AddListener(se.ButtonClick);
        _stageSelectButton.onClick.AddListener(se.ButtonClick);
        _quitButton.onClick.AddListener(se.ButtonClick);

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
        onFinished?.Invoke();   
    }

    public void LoadInGame() => GameManager.Instance.GoInGame();
    public void LoadStageSelect() => GameManager.Instance.GoStageSelect();
    public void LoadQuit() => GameManager.Instance.Quit();
}