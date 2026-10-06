using System.Collections;
using UnityEngine;

public class StarDisplay : MonoBehaviour
{
    [SerializeField] private StarUI[] _stars = new StarUI[StageStars.StarCount];
    [SerializeField] private float _interval = 0.25f;

    public void Show(StageStars stars)
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] == null) continue;

            _stars[i].SetLit(stars.Has(i));
        }
    }

    // クリア演出用
    public IEnumerator PlayResult(StarResult result)
    {
        Show(result.Before);

        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] == null) continue;
            if (!result.IsNew(i)) continue;

            yield return new WaitForSecondsRealtime(_interval);
            yield return _stars[i].PlayUnlock();
        }
    }
}
