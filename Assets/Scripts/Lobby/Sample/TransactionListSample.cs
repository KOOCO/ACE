using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TransactionListSample : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI T_Number_Txt, T_Time_Txt, typesValue_Txt, Amount_Txt, State_Txt;
    [SerializeField]
    public Button item_Btn;
    //[SerializeField]
    //public TransactionDetail transactionDetail;

    [Header("文本翻譯")]
    public TextMeshProUGUI Types_Txt;

    private TransactionDetailData detailData = null;
    public Action<TransactionDetailData> ClickItem;

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
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage(string type)
    {
        if(type == "In")
            Types_Txt.text = LanguageManager.Instance.GetText("Transfer In");
        else if(type == "Out")
            Types_Txt.text = LanguageManager.Instance.GetText("Transfer Out");
    }

    /// <summary>
    /// 設置額度紀錄值
    /// </summary>
    /// <param name="type">use "In" or "Out"</param>
    public void setTransactionListValue(TransactionDetailData data)
    {
        detailData = data;
        T_Number_Txt.text = data.TransactionID;
        T_Time_Txt.text = data.Time;
        UpdateLanguage(data.Type);
        Amount_Txt.text = data.Amount.ToString();
        State_Txt.text = data.State;
    }
}
