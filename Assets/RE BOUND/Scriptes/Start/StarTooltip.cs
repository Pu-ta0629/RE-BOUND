using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StarTooltip : MonoBehaviour
{
    public static StarTooltip Instance { get; private set; }

    [Header("View")]
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _text;

    [Header("Message ({0} = 記録, {1} = 条件の回数)")]
    [SerializeField] private string _clearFormat = "条件：クリア";
    [SerializeField] private string _bounceFormat = "条件：バウンド {0}/{1}回以内";
    [SerializeField] private string _swipeFormat = "条件：スワイプ {0}/{1}回以内";
    [SerializeField] private string _noRecord = "-";

    [Header("Position")]
    [SerializeField] private Vector2 _offset = new Vector2(16f, 16f);

    private RectTransform _rect;
    private RectTransform _parentRect;
    private Canvas _canvas;
    private bool _isVisible;

    #region INITIALIZE

    // GameManager.Awake から1回だけ呼ばれる
    public void Initialize()
    {
        Instance = this;

        if (_root == null || _text == null)
        {
            Debug.LogError($"{name} : Root / Text が未設定です", this);
            return;
        }

        _rect = _root.GetComponent<RectTransform>();
        _parentRect = _rect.parent as RectTransform;

        Canvas canvas = _root.GetComponentInParent<Canvas>(true);
        if (canvas != null) _canvas = canvas.rootCanvas;

        // ツールチップ自身がマウスを受け取ると、ホバーが外れて点滅するので受け取らない
        if (!_root.TryGetComponent(out CanvasGroup group))
        {
            group = _root.AddComponent<CanvasGroup>();
        }

        group.blocksRaycasts = false;
        group.interactable = false;

        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    #region SHOW / HIDE

    public void Show(Enum_StarType type, int stageID)
    {
        if (_root == null) return;

        _text.text = BuildMessage(type, stageID);

        _root.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);   // 文字数に合わせて大きさを確定させる

        _isVisible = true;
        UpdatePosition();
    }

    public void Hide()
    {
        _isVisible = false;

        if (_root != null) _root.SetActive(false);
    }

    private void Update()
    {
        if (_isVisible) UpdatePosition();
    }

    #endregion

    #region MESSAGE

    // ★の種類に応じて文言を切り替える
    private string BuildMessage(Enum_StarType type, int stageID)
    {
        GameManager gameManager = GameManager.Instance;

        StageData stageData = gameManager.GetStageData(stageID);
        StageRecord record = gameManager.SaveManager.GetStageRecord(stageID);

        switch (type)
        {
            case Enum_StarType.Bounce:
            {
                string best = record != null ? record.BestBounceCount.ToString() : _noRecord;
                string limit = stageData != null ? stageData.BounceLimit.ToString() : "?";

                return string.Format(_bounceFormat, best, limit);
            }

            case Enum_StarType.Swipe:
            {
                // スワイプ記録は旧セーブデータには無いので、「記録あり」のときだけ数字を出す
                string best = (record != null && record.HasSwipeRecord) ? record.BestSwipeCount.ToString() : _noRecord;
                string limit = stageData != null ? stageData.SwipeLimit.ToString() : "?";

                return string.Format(_swipeFormat, best, limit);
            }

            default:
                return _clearFormat;
        }
    }

    #endregion

    #region POSITION

    // カーソルの右下に表示し、画面の右端・下端にはみ出す場合は反対側に出す
    private void UpdatePosition()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || _parentRect == null || _canvas == null) return;

        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect, mouse.position.ReadValue(), cam, out Vector2 localPos))
        {
            return;
        }

        Rect area = _parentRect.rect;
        Vector2 size = _rect.rect.size;

        bool flipX = localPos.x + _offset.x + size.x > area.xMax;
        bool flipY = localPos.y - _offset.y - size.y < area.yMin;

        _rect.pivot = new Vector2(flipX ? 1f : 0f, flipY ? 0f : 1f);

        float x = flipX ? -_offset.x : _offset.x;
        float y = flipY ? _offset.y : -_offset.y;

        _rect.localPosition = new Vector3(localPos.x + x, localPos.y + y, 0f);
    }

    #endregion
}
