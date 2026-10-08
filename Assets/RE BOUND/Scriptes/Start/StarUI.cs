using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class StarUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly int UnlockHash = Animator.StringToHash("Unlock");

    [Header("View")]
    [SerializeField] private GameObject _onObj;
    [SerializeField] private GameObject _offObj;

    [Header("Unlock Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _unlockDuration = 0.5f;
    [SerializeField] private AnimationCurve _popCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.6f, 1.3f), new Keyframe(1f, 1f));

    private StarDisplay _owner;
    private int _index;
    private bool _isHovering;

    #region UNITY EVENT

    // ホバー中に非表示になった（メニューを閉じた等）ときは OnPointerExit が呼ばれないので、ここで消す
    private void OnDisable()
    {
        if (!_isHovering) return;

        _isHovering = false;

        if (_owner != null) _owner.OnStarExit();
    }

    #endregion

    #region TOOLTIP

    // StarDisplay.Show から呼ばれる。どの★かを覚えておく
    public void Bind(StarDisplay owner, int index)
    {
        _owner = owner;
        _index = index;
    }

    // 子の On / Off 画像（Raycast Target ON）にマウスが乗ると、親であるこちらにも通知が届く
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_owner == null) return;

        _isHovering = true;
        _owner.OnStarEnter(_index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_owner == null) return;

        _isHovering = false;
        _owner.OnStarExit();
    }

    #endregion

    #region VIEW

    public void SetLit(bool lit)
    {
        _onObj.SetActive(lit);
        _offObj.SetActive(!lit);
        _onObj.transform.localScale = Vector3.one;
    }

    public IEnumerator PlayUnlock()
    {
        _offObj.SetActive(false);
        _onObj.SetActive(true);

        AudioManager.Instance.Play(Enum_SEType.StarUnlock);
        if (_animator != null)
        {
            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            _animator.SetTrigger(UnlockHash);
            _animator.SetBool("_IsStar", true);

            yield return new WaitForSecondsRealtime(_unlockDuration);
            yield break;
        }

        Transform target = _onObj.transform;
        target.localScale = Vector3.zero;

        float time = 0f;
        while (time < _unlockDuration)
        {
            time += Time.unscaledDeltaTime;
            target.localScale = Vector3.one * _popCurve.Evaluate(time / _unlockDuration);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    #endregion
}