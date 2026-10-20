using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// AnimationClip を Animator なしで、コードから再生する。
/// 毎フレーム「その時刻の値」を書き込むだけなので、Animatorのような状態の持ち越しが起きない。
/// (StageClear の星の演出と同じ、コルーチンで動かす方式)
/// </summary>
public static class ClipPlayer
{
    /// <param name="target">クリップのパスの基準になるオブジェクト（元の Animator が付いていたもの）</param>
    /// <param name="speed">再生速度</param>
    /// <param name="unscaledTime">true: ポーズ中も進む(UI向け) / false: ポーズで止まる(ゲーム内向け)</param>
    /// <param name="fireEvents">true: クリップの AnimationEvent を target に SendMessage で再現する</param>
    public static IEnumerator Play(GameObject target, AnimationClip clip, float speed = 1f,
                                   bool unscaledTime = true, bool fireEvents = false)
    {
        if (target == null || clip == null) yield break;

        float length = clip.length;
        AnimationEvent[] events = fireEvents ? clip.events.OrderBy(e => e.time).ToArray() : null;
        int next = 0;

        // 開始の見た目はこの場で即反映する（1フレームも元の姿勢を見せない）
        clip.SampleAnimation(target, 0f);

        // イベントは次フレーム以降に発火する（呼び出し元の初期化が終わった後になる）
        yield return null;

        float time = 0f;

        while (target != null)
        {
            time += (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * speed;

            bool finished = time >= length;
            float t = finished ? length : time;

            clip.SampleAnimation(target, t);

            if (events != null) next = FireEvents(target, events, next, t, finished);

            if (finished) yield break;

            yield return null;
        }
    }

    private static int FireEvents(GameObject target, AnimationEvent[] events, int next, float time, bool all)
    {
        while (next < events.Length && (all || events[next].time <= time))
        {
            Dispatch(target, events[next]);
            next++;
        }

        return next;
    }

    // 引数の型は Animator と同じく、設定されている値から選ぶ（int/float が 0 のときは引数なし）
    private static void Dispatch(GameObject target, AnimationEvent e)
    {
        if (string.IsNullOrEmpty(e.functionName)) return;

        const SendMessageOptions option = SendMessageOptions.DontRequireReceiver;

        if (!string.IsNullOrEmpty(e.stringParameter)) target.SendMessage(e.functionName, e.stringParameter, option);
        else if (e.objectReferenceParameter != null) target.SendMessage(e.functionName, e.objectReferenceParameter, option);
        else if (e.floatParameter != 0f) target.SendMessage(e.functionName, e.floatParameter, option);
        else if (e.intParameter != 0) target.SendMessage(e.functionName, e.intParameter, option);
        else target.SendMessage(e.functionName, option);
    }
}