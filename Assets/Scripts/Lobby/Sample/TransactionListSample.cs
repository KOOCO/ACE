using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransactionListSample : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI T_Number_Txt, T_Date_Txt, T_Time_Txt, typesValue_Txt, Amount_Txt;

    [Header("文本翻譯")]
    public TextMeshProUGUI Types_Txt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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
    public void setTransactionListValue(string T_Number, string date, string time, string type, float amount)
    {
        T_Number_Txt.text = T_Number;
        T_Date_Txt.text = date;
        T_Time_Txt.text = time;
        UpdateLanguage(type);
        Amount_Txt.text = amount.ToString();
    }
}
