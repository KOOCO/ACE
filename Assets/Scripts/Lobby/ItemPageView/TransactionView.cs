using QRCodeShareMain;
using UnityEngine;
using UnityEngine.UI;

public class TransactionView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Toggle create_Tog, confirm_Tog;
    [SerializeField]
    private GameObject create_Obj, ccmfirm_Obj;
    [SerializeField]
    private Button close_Btn, back_Btn;

    private void Start()
    {
        create_Obj.GetComponent<CreateTransactionManager>().SetQRcode();
        EventListener();
    }
    private void EventListener()
    {
        create_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                create_Obj.SetActive(true);
                ccmfirm_Obj.SetActive(false);
            }
        });
        confirm_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                create_Obj.SetActive(false);
                ccmfirm_Obj.SetActive(true);
            }
        });
        close_Btn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        back_Btn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

    }
}
