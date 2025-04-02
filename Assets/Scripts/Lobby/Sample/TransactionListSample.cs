using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TransactionListSample : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI T_Time_Txt, typesValue_Txt, Amount_Txt, State_Txt;
    [SerializeField]
    public Button item_Btn;
    //[SerializeField]
    //public TransactionDetail transactionDetail;

    [Header("文本翻譯")]
    public TextMeshProUGUI Types_Txt;

    private TransactionData detailData = null;
    public Action<TransactionData> ClickItem;

    // Start is called before the first frame update
    void Start()
    {
        item_Btn.onClick.AddListener(() =>
        {
            //transactionDetail.ShowDetail(detailData);
            ClickItem.Invoke(detailData);
        });
    }

    /// <summary>
    /// 設置額度紀錄值
    /// </summary>
    /// <param name="type">use "In" or "Out"</param>
    public void setTransactionListValue(TransactionData data)
    {
        detailData = data;
        string time = data.creationTime.Replace("T", "\n<color=#698CD6>");
        int dotIndex = time.IndexOf('.');
        if (dotIndex != -1)
        {
            time = time.Substring(0, dotIndex);
        }
        T_Time_Txt.text = time;
        switch (data.transactionType)
        {
            case 1:
                Types_Txt.text = LanguageManager.Instance.GetText("Transfer In");
                break;
            case 2:
                Types_Txt.text = LanguageManager.Instance.GetText("Transfer Out");
                break;
        }
        Amount_Txt.text = data.amount.ToString();
        switch(data.transactionStatus)
        {
            case 0:
                State_Txt.text= LanguageManager.Instance.GetText("Created");
                break;
            case 1:
                State_Txt.text = LanguageManager.Instance.GetText("Pending");
                break;
            case 2:
                State_Txt.text = LanguageManager.Instance.GetText("Completed");
                break;
            case 3:
                State_Txt.text = LanguageManager.Instance.GetText("Failed");
                break;
            case 4:
                State_Txt.text = LanguageManager.Instance.GetText("SystemFailed");
                break;
        }
    }
}
