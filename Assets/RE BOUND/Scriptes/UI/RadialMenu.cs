using UnityEngine;
using UnityEngine.UI;

public enum Enum_RadialItem
{
    None,
    Retry,
    Menu,
    Cancel
}
public class RadialMenu : MonoBehaviour
{
    [SerializeField] private Image _retryItem;
    [SerializeField] private Image _menuItem;
    [Tooltip("何も選ばない（×）")]
    [SerializeField] private Image _cancelItem;

    private const float SelectedAlpha = 0.7f;
    private const float SelectedScale = 1.1f;

    private Image[] _items;
    private Vector3[] _baseScales;
    private float[] _baseAlphas;
    private Camera _camera;

    public bool IsOpen { get; private set; }

    // InputManager.Initialize から呼ばれる
    public void Initialize()
    {
        _items = new[] { _retryItem, _menuItem, _cancelItem };
        _baseScales = new Vector3[_items.Length];
        _baseAlphas = new float[_items.Length];

        // 項目の元の透明度と大きさを覚えておく（選択を外すときに戻す）
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null) continue;

            _baseScales[i] = _items[i].rectTransform.localScale;
            _baseAlphas[i] = _items[i].color.a;
        }

        Close();
    }

    // 押した位置に表示する
    public void Open(Vector2 mousePosition)
    {
        IsOpen = true;

        RectTransform parent = transform.parent as RectTransform;
        Canvas canvas = GetComponentInParent<Canvas>();

        _camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, mousePosition, _camera, out Vector3 world))
        {
            transform.position = world;
        }

        UpdateItems(Enum_RadialItem.None);

        gameObject.SetActive(true);
    }

    // マウスが重なっている項目を選んで強調する。どれにも重なっていなければ None
    public Enum_RadialItem Select(Vector2 mousePosition)
    {
        Enum_RadialItem selected = Enum_RadialItem.None;

        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(_items[i].rectTransform, mousePosition, _camera))
            {
                selected = (Enum_RadialItem)(i + 1);
                break;
            }
        }

        UpdateItems(selected);

        return selected;
    }

    public void Close()
    {
        IsOpen = false;

        // 次に開いたとき、元の見た目に戻っているように
        if (_items != null) UpdateItems(Enum_RadialItem.None);

        gameObject.SetActive(false);
    }

    // 選んでいる項目は、少し透けて、1.1倍になる
    private void UpdateItems(Enum_RadialItem selected)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] == null) continue;

            bool isSelected = (int)selected == i + 1;

            Color color = _items[i].color;
            color.a = isSelected ? _baseAlphas[i] * SelectedAlpha : _baseAlphas[i];
            _items[i].color = color;

            _items[i].rectTransform.localScale = _baseScales[i] * (isSelected ? SelectedScale : 1f);
        }
    }
}