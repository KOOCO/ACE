using UnityEngine;
using UnityEngine.UI;

public class ButtonState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool isNeedAccount;

    public void SetButton(bool interactable)
    {
        this.GetComponent<Button>().interactable = interactable;
    }
}