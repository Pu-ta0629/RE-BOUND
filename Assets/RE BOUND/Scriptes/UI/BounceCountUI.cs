using TMPro;
using UnityEngine;

public class BounceCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private GameObject _stageClear;
    private int _current;
    private void Update()
    {
        if (PlayerManager.Instance == null) return;
        if (PlayerManager.Instance.Player == null) return;
        if (_current == PlayerManager.Instance.Player.BounceCount) return;

        if(GameController.Instance.IsPlaying && _stageClear.activeSelf == false) gameObject.SetActive(true);
        else                                  gameObject.SetActive(false);

        _current = PlayerManager.Instance.Player.BounceCount;

        if (_current > 99999) _current = 99999;
        _text.text = _current.ToString();
    }
}