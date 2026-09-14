using TMPro;
using UnityEngine;

public class BounceCountUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private TMP_Text _text;

    private void Update()
    {
       _text.text = $"{_player.BounceCount}";
    }
}