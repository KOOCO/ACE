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
    public void ShowDetail(TransactionData data)
    {
        gameObject.SetActive(true);
        transIDFull = data.id;
        hashKeyFull = data.hashKey;
        string head = transIDFull.Substring(0, 6);
        string tail = transIDFull.Substring(transIDFull.Length - 6);
        transID_Txt.text = head + "..." + tail;
        if (hashKeyFull.Length > 6)
        {
            head = hashKeyFull.Substring(0, 6);
            tail = hashKeyFull.Substring(hashKeyFull.Length - 6);
            hashKey_Txt.text = head + "..." + tail;
        }
        else
        {
            hashKey_Txt.text = hashKeyFull;
        }
        string time = data.creationTime.Replace("T", " <color=#698CD6>");
        int dotIndex = time.IndexOf('.');
        if (dotIndex != -1)
        {
            time = time.Substring(0, dotIndex);
        }
        time_Txt.text = time;
        switch (data.transactionType)
        {
            case 1:
                type_Txt.text = LanguageManager.Instance.GetText("Transfer In");
                break;
            case 2:
                type_Txt.text = LanguageManager.Instance.GetText("Transfer Out");
                break;
        }
        amount_Txt.text = data.amount.ToString();
        switch (data.transactionStatus)
        {
            case 0:
                state_Txt.text = LanguageManager.Instance.GetText("Created");
                break;
            case 1:
                state_Txt.text = LanguageManager.Instance.GetText("Pending");
                break;
            case 2:
                state_Txt.text = LanguageManager.Instance.GetText("Completed");
                break;
            case 3:
                state_Txt.text = LanguageManager.Instance.GetText("Failed");
                break;
            case 4:
                state_Txt.text = LanguageManager.Instance.GetText("SystemFailed");
                break;
        }

    }
}
