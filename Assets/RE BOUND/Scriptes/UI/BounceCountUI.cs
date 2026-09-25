using TMPro;
using UnityEngine;

public class BounceCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private int _current;
    private void Update()
    {
        if (_current == PlayerManager.Instance.Player.BounceCount) return;

        if(GameController.Instance.IsPlaying) gameObject.SetActive(true);
        else                                  gameObject.SetActive(false);

        _current = PlayerManager.Instance.Player.BounceCount;

        if (_current > 99999) _current = 99999;
        _text.text = _current.ToString();
    }
}