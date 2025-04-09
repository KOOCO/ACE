using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyReportView : MonoBehaviour
{
    public ZCalendar calendar;
    [Header("Tittle選單")]
    [SerializeField]
    Toggle betRecord_Tog, handHistory_Tog, transList_Tog;
    [SerializeField]
    GameObject betRecord_Obj, handHistory_Obj, transList_Obj;
    [SerializeField]
    List<GameObject> reportObjs;

    [Header("日期選擇")]
    public GameObject dateSelect_Obj;
    [SerializeField]
    Button startTime_Btn, endTime_Btn, startIcon_Btn, endIcon_Btn;
    [SerializeField]
    TextMeshProUGUI StartTime_Txt, EndTime_Txt;
    [SerializeField]
    TMP_Dropdown Game_Drop;
    //Calendar popup
    public GameObject Calendar_Obj;
    public Transform dayBox_Trans;
    [SerializeField]
    TextMeshProUGUI startTime_Txt, endTime_Txt;
    [SerializeField]
    Button Confirm_Btn, Submit_Btn;

    [Header("注單查詢")]
    public BetHistoryManager bethistory;
    [Header("交易紀錄")]
    public TransactionListManager transactionList;


    // Start is called before the first frame update
    void Start()
    {
        ListenEvent();
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        StartTime_Txt.text = System.DateTime.Today.ToShortDateString();
        EndTime_Txt.text = System.DateTime.Today.ToShortDateString();
        startTime_Txt.text = System.DateTime.Today.ToShortDateString();
        endTime_Txt.text = System.DateTime.Today.ToShortDateString();
        bethistory.Init();
        transactionList.Init();
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        Game_Drop.options[0].text = LanguageManager.Instance.GetText("All");
        Game_Drop.options[1].text = LanguageManager.Instance.GetText("Texas hold'em");
    }

    void ListenEvent()
    {
        betRecord_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectObj(betRecord_Obj);
                dateSelect_Obj.SetActive(true);
                bethistory.gameObject.SetActive(false);
                StartTime_Txt.text = System.DateTime.Today.ToShortDateString();
                EndTime_Txt.text = System.DateTime.Today.ToShortDateString();
                Game_Drop.gameObject.SetActive(true);
            }
        });
        handHistory_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectObj(handHistory_Obj);
                dateSelect_Obj.SetActive(false);
            }
        });
        transList_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                selectObj(transList_Obj);
                AppApi.GetTransactionList(1, GetTransactionData);
            }
        });

        startTime_Btn.onClick.AddListener(() =>{
            Calendar_Obj.SetActive(true);
            StartCoroutine(selectToday(System.DateTime.Today.Day));
        });
        endTime_Btn.onClick.AddListener(() =>{
            Calendar_Obj.SetActive(true);
            StartCoroutine(selectToday(System.DateTime.Today.Day));
        });
        startIcon_Btn.onClick.AddListener(() =>
        {
            startTime_Btn.onClick.Invoke();
        });
        endIcon_Btn.onClick.AddListener(() =>
        {
            endTime_Btn.onClick.Invoke();
        });

        Confirm_Btn.onClick.AddListener(() =>
        {
            StartTime_Txt.text = startTime_Txt.text;
            EndTime_Txt.text = endTime_Txt.text;
            Calendar_Obj.SetActive(false);
        });
        Submit_Btn.onClick.AddListener(() =>
        {
            AppApi.GetBettingDetail(startTime_Txt.text, endTime_Txt.text, GetBettingData);
        });
    }

    private void GetBettingData(string data)
    {
        bethistory.gameObject.SetActive(true);
        dateSelect_Obj.SetActive(false);
        bethistory.ShowBetHistory(data);
    }
    private void GetTransactionData(string data)
    {
        dateSelect_Obj.SetActive(false);
        transactionList.ShowTransactionList(data);
    }
    public void confirmDateRange(string first, string last)
    {
        startTime_Txt.text = first;
        endTime_Txt.text = last;
    }

    void selectObj(GameObject obj)
    {
        foreach(var target in reportObjs)
        {
            if (obj == target)
            {
                target.SetActive(true);
            }
            else
                target.SetActive(false);
        }
    }

    IEnumerator selectToday(int day)
    {
        yield return new WaitUntil(() => dayBox_Trans.childCount > 0);

        foreach (var dTrans in dayBox_Trans.GetComponentsInChildren<ZCalendarDayItem>())
        {
            //print(dTrans.transform.GetChild(2).GetComponent<Text>().text);
            if (day.ToString() == dTrans.transform.GetChild(2).GetComponent<Text>().text)
            {
                if (dTrans.GetComponent<Button>().interactable == false)
                    continue;
                else
                {
                    print(day);
                    dTrans.GetComponent<Button>().onClick.Invoke();
                    break;
                }
            }
        }
    }
}
