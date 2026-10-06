using System.Collections;
using UnityEngine;

public class StarUI : MonoBehaviour
{
    private static readonly int UnlockHash = Animator.StringToHash("Unlock");

    [Header("View")]
    [SerializeField] private GameObject _onObj;
    [SerializeField] private GameObject _offObj;

    [Header("Unlock Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _unlockDuration = 0.5f;
    [SerializeField] private AnimationCurve _popCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.6f, 1.3f), new Keyframe(1f, 1f));

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
}
