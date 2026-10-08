using UnityEngine;
using UnityEngine.UI;

public class ButtonSE : MonoBehaviour
{
    public void ButtonClick()
    {
        AudioManager.Instance.Play(Enum_SEType.ButtonClick);
    }
}
