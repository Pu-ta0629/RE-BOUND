using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面（旧Scene）1つ分のStateの基底クラス。
/// このコンポーネントは「常にアクティブなオブジェクト」(StateManagerの子など)に置き、
/// 表示/非表示にする対象は _root で指定する。
///
/// 全Stateで共通: Initialize 時（=まだ何も動いていない状態）に _root / _extraRoots 以下の
/// 位置・大きさ・回転・色・alpha・ON/OFF を記録し、Enter のたびに元へ戻す。
/// さらに、Animator が動いていればパラメータ(Bool/Trigger)と状態も初期化する。
/// </summary>
public abstract class GameStateBase : MonoBehaviour
{
    [Header("View")]
    [SerializeField] protected GameObject _root;

    [Tooltip("同時に ON/OFF したい別のオブジェクト（例: 共通Canvas内のこの画面用UI）。常にSetActiveで切り替える")]
    [SerializeField] private GameObject[] _extraRoots;
    [SerializeField] private Enum_ViewMode _viewMode = Enum_ViewMode.SetActive;

    [Header("Reset")]
    [Tooltip("入るたびに、Animation等で動いた位置/Scale/色/ON-OFFを初期状態へ戻す")]
    [SerializeField] private bool _resetOnEnter = true;

    private readonly ViewSnapshot _snapshot = new();
    private CanvasGroup _canvasGroup;

    public abstract Enum_GameState StateType { get; }

    // StateManager.Initialize から1回だけ。まだ何も動いていないので、ここで初期状態を記録する
    public virtual void Initialize()
    {
        TakeSnapshot();
        SetVisible(false);
    }

    public void Enter()
    {
        SetVisible(true);   // _root が ON になる

        if (_resetOnEnter)
        {
            _snapshot.Restore();
            ResetAnimators();
        }

        OnEnter();          // Start は走らないので、初期化はここで明示的に行う
    }

    public void Exit()
    {
        OnExit();
        SetVisible(false);
    }

    public virtual void OnUpdate() { }

    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }

    #region RESET

    private void TakeSnapshot()
    {
        _snapshot.Clear();

        if (!_resetOnEnter) return;

        if (_root != null) _snapshot.Add(_root.transform, true);

        if (_extraRoots == null) return;

        foreach (GameObject extra in _extraRoots)
        {
            if (extra != null) _snapshot.Add(extra.transform, true);
        }
    }

    // 動作中の Animator を最初の状態に戻す（無効な Animator は触らない）
    private void ResetAnimators()
    {
        if (_root != null) ResetAnimatorsIn(_root);

        if (_extraRoots == null) return;

        foreach (GameObject extra in _extraRoots)
        {
            if (extra != null) ResetAnimatorsIn(extra);
        }
    }

    private static void ResetAnimatorsIn(GameObject target)
    {
        foreach (Animator animator in target.GetComponentsInChildren<Animator>(false))
        {
            if (!animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) continue;

            animator.Rebind();

            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.type == AnimatorControllerParameterType.Bool) animator.SetBool(p.nameHash, p.defaultBool);
                else if (p.type == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(p.nameHash);
            }

            // ここで animator.Update(0f) を呼ばない:
            // 時刻0の AnimationEvent が、各システムの初期化前に発火してしまう（NullReference の原因）
        }
    }

    #endregion

    private void SetVisible(bool visible)
    {
        if (_extraRoots != null)
        {
            foreach (GameObject extra in _extraRoots)
            {
                if (extra != null) extra.SetActive(visible);
            }
        }

        if (_root == null)
        {
            Debug.LogWarning($"{name} : Root not found");
            return;
        }

        switch (_viewMode)
        {
            case Enum_ViewMode.SetActive:
                _root.SetActive(visible);
                break;

            case Enum_ViewMode.CanvasGroup:
                if (_canvasGroup == null && !_root.TryGetComponent(out _canvasGroup))
                {
                    _canvasGroup = _root.AddComponent<CanvasGroup>();
                }

                if (!_root.activeSelf) _root.SetActive(true);

                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
                break;
        }
    }
}