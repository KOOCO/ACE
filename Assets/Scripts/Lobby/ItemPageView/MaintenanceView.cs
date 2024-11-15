using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaintenanceView : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI startTime_Value, endTime_Value;
    [SerializeField]
    string startTime, endTime;
    public Button Confirm_Btn;

    [Header("文本翻譯")]
    [SerializeField]
    TextMeshProUGUI Tittle_Txt, startTime_Txt, endTime_Txt, Content_Txt, Confirm_Txt;


    // Start is called before the first frame update
    void Start()
    {
        UpdateLanguage();

        startTime_Value.text = startTime;
        endTime_Value.text = endTime;

        Confirm_Btn.onClick.AddListener(() =>
        {
            JSBridgeManager.Instance.WindowClose();
        });
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        Tittle_Txt.text = LanguageManager.Instance.GetText("MAINTENANCE");
        startTime_Txt.text = "◆ " + LanguageManager.Instance.GetText("Start Time");
        endTime_Txt.text = "◆ " + LanguageManager.Instance.GetText("End Time");
        Content_Txt.text = LanguageManager.Instance.GetText("Maintenance content");
    }
}
