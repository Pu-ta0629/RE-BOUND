using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class StageButton
    {
        public StageData StageData;

        public Button Button;

        public StarDisplay Stars;

        [Tooltip("このステージを押したときに再生するアニメ（任意）。空なら Default Select Clip")]
        public AnimationClip SelectClip;

        [Tooltip("SelectClip のパスの基準。空ならボタン自身")]
        public GameObject SelectTarget;
    }

    [SerializeField] private List<StageButton> _stageButtons;
    [SerializeField] private bool _showStarTooltip = false;

    [Header("Animation (Animatorは使わず、コードで再生)")]
    [Tooltip("入場アニメのパスの基準（元の Animator が付いていた StageSelectRoot など）。空ならこのオブジェクト")]
    [SerializeField] private GameObject _introTarget;
    [SerializeField] private AnimationClip _introClip;

    [Tooltip("個別の Select Clip が無いボタンで再生するアニメ（任意）")]
    [SerializeField] private AnimationClip _defaultSelectClip;

    [Tooltip("残っている旧 Animator（あれば自動で無効にする。競合を防ぐ）")]
    [SerializeField] private Animator _legacyAnimator;

    [SerializeField] private ButtonSE _buttonSE;

    [Header("Page (Stage Buttons の並び順で、Page Size 個ずつ表示)")]
    [SerializeField] private Button _nextPageButton;
    [SerializeField] private int _pageSize = 10;
    [Tooltip("ページ切り替えで、ボタンが動く距離(px)")]
    [SerializeField] private float _pageSlideDistance = 800f;
    [SerializeField] private float _pageSlideTime = 0.25f;
    [Tooltip("ボタンごとの、動き出しのずらし(秒)")]
    [SerializeField] private float _pageStagger = 0.03f;

    private int _page;
    private Coroutine _pageRoutine;
    private RectTransform[] _rects;
    private CanvasGroup[] _groups;
    private Vector2[] _basePositions;

    private int PageCount => Mathf.Max(1, Mathf.CeilToInt(_stageButtons.Count / (float)_pageSize));

    private bool _isBusy;
    private Coroutine _routine;

    public bool IsBusy => _isBusy;

    private GameObject IntroTarget => _introTarget != null ? _introTarget : gameObject;

    // StageSelectState に入るたびに呼ばれる（位置/Scaleの復元は GameStateBase.Enter が済ませている）
    public void Begin()
    {
        if (_legacyAnimator != null) _legacyAnimator.enabled = false;

        CacheButtons();

        int maxUnlock = GameManager.Instance.MaxUnlockStage;

        foreach (StageButton stage in _stageButtons)
        {
            bool canPlay = stage.StageData.StageID < maxUnlock + 1;

            stage.Button.interactable = canPlay;

            int id = stage.StageData.StageID;

            if (stage.Stars != null)
            {
                stage.Stars.SetTooltipEnabled(_showStarTooltip);
                stage.Stars.Show(GameManager.Instance.SaveManager.GetStars(id), id);
            }

            StageButton captured = stage;

            stage.Button.onClick.RemoveAllListeners();
            stage.Button.onClick.AddListener(_buttonSE.ButtonClick);
            stage.Button.onClick.AddListener(() => OnStageClicked(captured));
        }

        // 次のページのボタン（ボタンの SE → ページ送り）
        _nextPageButton.onClick.RemoveAllListeners();
        _nextPageButton.onClick.AddListener(_buttonSE.ButtonClick);
        _nextPageButton.onClick.AddListener(NextPage);
        _nextPageButton.gameObject.SetActive(PageCount > 1);

        // 最後に選んだステージのページから開く
        _page = Mathf.Clamp(GetIndex(GameManager.Instance.CurrentStageID) / _pageSize, 0, PageCount - 1);
        ShowPage(_page);

        if (_pageRoutine != null) StopCoroutine(_pageRoutine);
        _pageRoutine = null;

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(IntroRoutine());
    }

    #region PAGE

    // ボタンの元の位置を覚えて、透明度を操作する CanvasGroup を用意する（Begin の最初。入場アニメの前）
    private void CacheButtons()
    {
        int count = _stageButtons.Count;

        _rects = new RectTransform[count];
        _groups = new CanvasGroup[count];
        _basePositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Button button = _stageButtons[i].Button;

            _rects[i] = button.GetComponent<RectTransform>();
            _basePositions[i] = _rects[i].anchoredPosition;

            if (!button.TryGetComponent(out CanvasGroup group)) group = button.gameObject.AddComponent<CanvasGroup>();

            group.alpha = 1f;
            _groups[i] = group;
        }
    }

    private int GetIndex(int stageID)
    {
        for (int i = 0; i < _stageButtons.Count; i++)
        {
            if (_stageButtons[i].StageData.StageID == stageID) return i;
        }

        return 0;
    }

    // 指定ページのボタンだけ表示する
    private void ShowPage(int page)
    {
        for (int i = 0; i < _stageButtons.Count; i++)
        {
            _stageButtons[i].Button.gameObject.SetActive(i / _pageSize == page);
        }
    }

    // 右矢印: 次のページへ（最後のページの次は、最初のページに戻る）
    private void NextPage()
    {
        if (_isBusy || PageCount <= 1) return;

        _pageRoutine = StartCoroutine(PageRoutine((_page + 1) % PageCount));
    }

    // 今のページのボタンが左へ流れて消える -> 次のページのボタンが右から流れてくる
    private IEnumerator PageRoutine(int nextPage)
    {
        _isBusy = true;

        yield return SlidePage(_page, 0f, -_pageSlideDistance, 1f, 0f);

        // 元の位置と透明度に戻してから隠す
        ResetPage(_page);
        _page = nextPage;
        ShowPage(_page);

        yield return SlidePage(_page, _pageSlideDistance, 0f, 0f, 1f);

        ResetPage(_page);

        _isBusy = false;
        _pageRoutine = null;
    }

    // ボタンを、ずらしながら横に動かして透明度を変える（コードのみ。unscaled）
    private IEnumerator SlidePage(int page, float fromX, float toX, float fromAlpha, float toAlpha)
    {
        int first = page * _pageSize;
        int last = Mathf.Min(first + _pageSize, _stageButtons.Count);
        int count = last - first;

        float totalTime = _pageSlideTime + _pageStagger * Mathf.Max(0, count - 1);

        // 動き出す前の姿勢
        ApplySlide(first, last, fromX, fromAlpha);

        for (float time = 0f; time < totalTime; time += Time.unscaledDeltaTime)
        {
            for (int i = first; i < last; i++)
            {
                float t = Mathf.Clamp01((time - _pageStagger * (i - first)) / _pageSlideTime);

                // 最初は速く、最後はゆっくり止まる
                float eased = 1f - (1f - t) * (1f - t) * (1f - t);

                SetSlide(i, Mathf.Lerp(fromX, toX, eased), Mathf.Lerp(fromAlpha, toAlpha, eased));
            }

            yield return null;
        }

        ApplySlide(first, last, toX, toAlpha);
    }

    private void ApplySlide(int first, int last, float x, float alpha)
    {
        for (int i = first; i < last; i++) SetSlide(i, x, alpha);
    }

    private void SetSlide(int index, float offsetX, float alpha)
    {
        _rects[index].anchoredPosition = _basePositions[index] + new Vector2(offsetX, 0f);
        _groups[index].alpha = alpha;
    }

    private void ResetPage(int page)
    {
        int first = page * _pageSize;
        int last = Mathf.Min(first + _pageSize, _stageButtons.Count);

        ApplySlide(first, last, 0f, 1f);
    }

    #endregion

    private IEnumerator IntroRoutine()
    {
        _isBusy = true;

        yield return ClipPlayer.Play(IntroTarget, _introClip);

        _isBusy = false;
        _routine = null;
    }

    private void OnStageClicked(StageButton stage)
    {
        if (_isBusy) return;

        AnimationClip clip = stage.SelectClip != null ? stage.SelectClip : _defaultSelectClip;

        if (clip == null)
        {
            SelectStage(stage.StageData.StageID);
            return;
        }

        if (_routine != null) StopCoroutine(_routine);

        GameObject target = stage.SelectTarget != null ? stage.SelectTarget : IntroTarget;

        _routine = StartCoroutine(SelectRoutine(target, clip, stage.StageData.StageID));
    }

    // 押したアニメを再生して、終わったら InGame へ（旧: AnimationEvent から SelectStage を呼んでいた）
    private IEnumerator SelectRoutine(GameObject target, AnimationClip clip, int stageID)
    {
        _isBusy = true;

        yield return ClipPlayer.Play(target, clip);

        _routine = null;
        SelectStage(stageID);   // 画面が切り替わっても _isBusy は Begin() で戻る
    }

    // 外部/AnimationEvent からも呼べるように名前と引数は変えない
    public void SelectStage(int stageID)
    {
        GameManager.Instance.SetCurrentStage(stageID);

        GameManager.Instance.GoInGame();
    }
}