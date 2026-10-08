using System.Collections;
using UnityEngine;

/// <summary>
/// ★3個の表示。Menu・StageSelectの各ステージ・クリア画面で同じものを使い回す。
/// _stars の並び: 0 = クリア★ / 1 = バウンド★ / 2 = スワイプ★
/// </summary>
public class StarDisplay : MonoBehaviour
{
    [SerializeField] private StarUI[] _stars = new StarUI[StageStars.StarCount];
    [SerializeField] private float _interval = 0.25f;

    [Header("Tooltip")]
    [Tooltip("★にマウスを合わせたとき、取得条件を表示するか")]
    [SerializeField] private bool _enableTooltip = true;

    private int _stageID;   // 0 = 現在のステージ（Menu・クリア画面）
    private CanvasGroup _canvasGroup;

    #region UNITY EVENT

    private void OnEnable()
    {
        ApplyTooltipSetting();
    }

    #endregion

    #region VIEW

    // 獲得状況を即時反映。stageID を省略すると、ホバー時に「現在のステージ」の条件を表示する
    public void Show(StageStars stars, int stageID = 0)
    {
        _stageID = stageID;

        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] == null) continue;

            _stars[i].Bind(this, i);
            _stars[i].SetLit(stars.Has(i));
        }
    }

    // クリア演出用
    public IEnumerator PlayResult(StarResult result, int stageID = 0)
    {
        Show(result.Before, stageID);

        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] == null) continue;
            if (!result.IsNew(i)) continue;

            yield return new WaitForSecondsRealtime(_interval);
            yield return _stars[i].PlayUnlock();
        }
    }

    #endregion

    #region TOOLTIP

    public void SetTooltipEnabled(bool enable)
    {
        _enableTooltip = enable;

        ApplyTooltipSetting();

        if (!enable) OnStarExit();
    }

    // ツールチップを使わない★は、マウスを受け取らないようにする（負荷を抑え、下のボタンを邪魔しない）
    private void ApplyTooltipSetting()
    {
        if (_canvasGroup == null && !TryGetComponent(out _canvasGroup))
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        _canvasGroup.blocksRaycasts = _enableTooltip;
    }

    // StarUI から呼ばれる
    public void OnStarEnter(int index)
    {
        if (!_enableTooltip) return;
        if (StarTooltip.Instance == null) return;

        int stageID = _stageID > 0 ? _stageID : GameManager.Instance.CurrentStageID;

        StarTooltip.Instance.Show((Enum_StarType)index, stageID);
    }

    public void OnStarExit()
    {
        if (StarTooltip.Instance == null) return;

        StarTooltip.Instance.Hide();
    }

    #endregion
}