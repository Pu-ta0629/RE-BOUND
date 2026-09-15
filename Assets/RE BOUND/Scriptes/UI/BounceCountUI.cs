using TMPro;
using UnityEngine;

public class BounceCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private int _current;
    private void Update()
    {
        if (_current == PlayerMovement.Instance.BounceCount)
            return;

        _current = PlayerMovement.Instance.BounceCount;

        _text.text = _current.ToString();
    }
}