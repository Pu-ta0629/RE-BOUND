using System.Collections;
using UnityEngine;

/// <summary>
/// ステージ1つ分。開かれるたびに導入アニメ（AnimationClip）をコードで再生する。
/// クリップ内の AnimationEvent（BeginSceneTransition / EndSceneTransition）もそのまま使える。
/// </summary>
public class Stage : MonoBehaviour
{
    [Header("Intro (旧: Animator)")]
    [Tooltip("ステージが開いた直後に流れるアニメ。クリップのパスは、このオブジェクトが基準")]
    [SerializeField] private AnimationClip _introClip;
    [SerializeField] private float _introSpeed = 1f;

    [Tooltip("残っている旧 Animator（あれば自動で無効にする。競合を防ぐ）")]
    [SerializeField] private Animator _legacyAnimator;

    private Coroutine _routine;

    // StageManager.LoadStage から、ステージを開くたびに呼ばれる
    public void PlayIntro()
    {
        if (_legacyAnimator != null) _legacyAnimator.enabled = false;

        if (!isActiveAndEnabled) return;

        if (_routine != null) StopCoroutine(_routine);

        _routine = StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        // クリップが無い場合も、Playerを動けるようにするため必ず最後に EndSceneTransition を呼ぶ
        if (_introClip != null)
        {
            // ポーズ(timeScale=0)中は止める。イベントも再現する
            yield return ClipPlayer.Play(gameObject, _introClip, _introSpeed, false, true);
        }
        else
        {
            yield return null;
        }

        EndSceneTransition();
        _routine = null;
    }

    // AnimationEvent から呼ばれる。名前は変えない
    public void BeginSceneTransition()
    {
        PlayerMovement player = GetPlayer();
        if (player != null) player.BeginSceneTransition();
    }

    // 導入アニメが終わったら Player を出す（出現アニメ → 操作可能）
    public void EndSceneTransition()
    {
        if (PlayerManager.Instance != null) PlayerManager.Instance.Appear();
    }

    private static PlayerMovement GetPlayer()
    {
        return PlayerManager.Instance != null ? PlayerManager.Instance.Player : null;
    }
}