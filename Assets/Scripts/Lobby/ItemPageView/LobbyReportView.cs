using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyReportView : MonoBehaviour
{
    [Header("Tittle選單")]
    [SerializeField]
    Toggle betRecord_Tog, transactionList_Tog, handHistory_Tog;
    [SerializeField]
    GameObject betRecord_Obj, transactionList_Obj, handHistory_Obj;

    [Header("日期選擇")]
    public GameObject dateSelect_Obj;
    [SerializeField]
    Button startTime_Btn, endTime_Btn;
    [SerializeField]
    TextMeshProUGUI StartTime_Txt, EndTime_Txt;
    [SerializeField]
    TMP_Dropdown Game_Drop;
    //Calendar popup
    public GameObject Calendar_Obj;
    [SerializeField]
    TextMeshProUGUI startTime_Txt, endTime_Txt;
    [SerializeField]
    Button Confirm_Btn;




    // Start is called before the first frame update
    void Start()
    {
        ListenEvent();
    }

    void ListenEvent()
    {
        /*betRecord_Btn.onClick.AddListener(() =>
        {
            betRecord_Obj.SetActive(true);
            transactionList_Obj.SetActive(false);
            handHistory_Obj.SetActive(false);
        });*/

        startTime_Btn.onClick.AddListener(() =>{
            Calendar_Obj.SetActive(true);
        });
        endTime_Btn.onClick.AddListener(() =>{
            Calendar_Obj.SetActive(true);
        });

        Confirm_Btn.onClick.AddListener(() =>
        {
            StartTime_Txt.text = startTime_Txt.text;
            EndTime_Txt.text = endTime_Txt.text;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void confirmDateRange(string first, string last)
    {
        startTime_Txt.text = first;
        endTime_Txt.text = last;
    }
}
