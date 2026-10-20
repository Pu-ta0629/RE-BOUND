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

    private bool _isBusy;
    private Coroutine _routine;

    public bool IsBusy => _isBusy;

    private GameObject IntroTarget => _introTarget != null ? _introTarget : gameObject;

    // StageSelectState に入るたびに呼ばれる（位置/Scaleの復元は GameStateBase.Enter が済ませている）
    public void Begin()
    {
        if (_legacyAnimator != null) _legacyAnimator.enabled = false;

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

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(IntroRoutine());
    }

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

    public void SelectStage(int stageID)
    {
        GameManager.Instance.SetCurrentStage(stageID);

        GameManager.Instance.GoInGame();
    }
}