using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene遷移の代わりに画面(State)を切り替える。Awake / Start は持たず、GameManager から呼ばれる。
/// ChangeState は「要求」だけ受け付け、実際の切り替えは同フレームの LateUpdate で行う。
/// (Animation Event やボタンのコールバック中に、自分自身を非アクティブにしないため)
/// </summary>
public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    [SerializeField] private Enum_GameState _startState = Enum_GameState.Title;
    [SerializeField] private List<GameStateBase> _states;

    private readonly Dictionary<Enum_GameState, GameStateBase> _stateDict = new();

    private GameStateBase _current;
    private Enum_GameState _pendingState = Enum_GameState.None;
    private bool _pendingForce;
    private bool _isBegun;

    public Enum_GameState CurrentState =>
        _current != null ? _current.StateType : Enum_GameState.None;

    public event Action<Enum_GameState> OnStateChanged;

    #region INITIALIZE

    // GameManager.Awake から1回だけ呼ばれる
    public void Initialize()
    {
        Instance = this;

        _stateDict.Clear();

        foreach (GameStateBase state in _states)
        {
            if (state == null) continue;

            if (!_stateDict.TryAdd(state.StateType, state))
            {
                Debug.LogWarning($"State duplicated : {state.StateType}");
                continue;
            }

            state.Initialize();   // 全部非表示
        }
    }

    // 全ての初期化が終わった最後に GameManager から呼ばれる
    public void Begin()
    {
        _isBegun = true;
        ChangeState(_startState);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    #endregion

    #region UNITY EVENT

    private void Update()
    {
        if (_current != null) _current.OnUpdate();
    }

    private void LateUpdate()
    {
        if (!_isBegun) return;
        if (_pendingState == Enum_GameState.None) return;

        Enum_GameState next = _pendingState;
        bool force = _pendingForce;

        _pendingState = Enum_GameState.None;
        _pendingForce = false;

        Apply(next, force);
    }

    #endregion

    #region STATE

    /// <param name="force">同じStateでも入り直す</param>
    public void ChangeState(Enum_GameState next, bool force = false)
    {
        _pendingState = next;
        _pendingForce = force;
    }

    private void Apply(Enum_GameState next, bool force)
    {
        if (!_stateDict.TryGetValue(next, out GameStateBase nextState))
        {
            Debug.LogError($"State Not Found : {next}");
            return;
        }

        if (_current == nextState && !force) return;

        EffectManager.Instance?.StopAllEffects();

        if (_current != null) _current.Exit();

        _current = nextState;
        _current.Enter();

        Debug.Log($"State : {next}");
        OnStateChanged?.Invoke(next);
    }

    #endregion
}
