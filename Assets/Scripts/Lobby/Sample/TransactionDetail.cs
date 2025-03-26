using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransactionDetail : MonoBehaviour
{
    [SerializeField]
    private Button close_Btn, back_Btn, copyID_Btn, copyHash_Btn;
    [SerializeField]
    private TextMeshProUGUI transID_Txt, hashKey_Txt, time_Txt, type_Txt, amount_Txt, state_Txt;

    private string transIDFull, hashKeyFull;
    private void Start()
    {
        EventListener();
    }
    private void EventListener()
    {
        close_Btn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        back_Btn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
        copyID_Btn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            TextEditor editor = new TextEditor
            {
                text = transIDFull
            };
            editor.SelectAll();
            editor.Copy();
#endif

            JSBridgeManager.Instance.CopyString(transIDFull);
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Copy Success!"));
        });
        copyHash_Btn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            TextEditor editor = new TextEditor
            {
                text = hashKeyFull
            };
            editor.SelectAll();
            editor.Copy();
#endif

            JSBridgeManager.Instance.CopyString(hashKeyFull);
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Copy Success!"));
        });
    }    
    public void ShowDetail(TransactionDetailData data)
    {
        gameObject.SetActive(true);
        transIDFull = data.TransactionID;
        hashKeyFull = data.HashKey;
        string transID = data.TransactionID;
        string head = transID.Substring(0, 6);
        string tail = transID.Substring(transID.Length - 6);
        transID_Txt.text = head + "..." + tail;
        string hashKey = data.HashKey;
        head = hashKey.Substring(0, 6);
        tail = hashKey.Substring(hashKey.Length - 6);
        hashKey_Txt.text = head + "..." + tail;
        time_Txt.text = data.Time;
        type_Txt.text = data.Type;
        amount_Txt.text = data.Amount.ToString();
        state_Txt.text = data.State;

    }
}
