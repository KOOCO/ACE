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
    Toggle betRecord_Tog, transactionList_Tog, handHistory_Tog;
    [SerializeField]
    GameObject betRecord_Obj, transactionList_Obj, handHistory_Obj;
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

    [Header("文本")]
    public TextMeshProUGUI betRecord_Txt, transactionList_Txt, handHistory_Txt,
        allWins_Txt, allValidBet_Txt, allBets_Txt, totalRecords_Txt, totalRecords_Txt1,
        StartTime_Text, startTime_Text, EndTime_Text, endTime_Text, Game_Txt,
        All_Txt, asiaPoker_Txt,
        Date_Txt, Cancel_Txt, Confirm_Txt, Submit_Txt,
        T_Number_Txt, Time_Txt, Types_Txt, Amount_Txt,
        hadH_Tip_Txt;


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
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 選單
        betRecord_Txt.text = LanguageManager.Instance.GetText("BETS RECORD");
        transactionList_Txt.text = LanguageManager.Instance.GetText("TRANSACTION LIST");
        handHistory_Txt.text = LanguageManager.Instance.GetText("HAND HISTORY");
        #endregion

        #region 總資料
        allWins_Txt.text = LanguageManager.Instance.GetText("All Wins：");
        allValidBet_Txt.text = LanguageManager.Instance.GetText("All Valid Bet：");
        allBets_Txt.text = LanguageManager.Instance.GetText("All Bets：");
        totalRecords_Txt.text = LanguageManager.Instance.GetText("Total         Record(S)");
        totalRecords_Txt1.text = LanguageManager.Instance.GetText("Total         Record(S)");
        #endregion

        #region 日期選擇
        StartTime_Text.text = LanguageManager.Instance.GetText("Start Time");
        startTime_Text.text = LanguageManager.Instance.GetText("Start");
        EndTime_Text.text = LanguageManager.Instance.GetText("End Time");
        endTime_Text.text = LanguageManager.Instance.GetText("End");
        Game_Txt.text = LanguageManager.Instance.GetText("Game");
        Date_Txt.text = LanguageManager.Instance.GetText("Date");
        Cancel_Txt.text = LanguageManager.Instance.GetText("Cancel");
        Confirm_Txt.text = LanguageManager.Instance.GetText("Confirm");
        Submit_Txt.text = LanguageManager.Instance.GetText("SUBMIT");
        Game_Drop.options[0].text = LanguageManager.Instance.GetText("All");
        Game_Drop.options[1].text = LanguageManager.Instance.GetText("Texas hold'em");
        #endregion

        #region 額度紀錄
        T_Number_Txt.text = LanguageManager.Instance.GetText("Transaction Number");
        Time_Txt.text = LanguageManager.Instance.GetText("Time");
        Types_Txt.text = LanguageManager.Instance.GetText("Types");
        Amount_Txt.text = LanguageManager.Instance.GetText("Amount");
        #endregion

        hadH_Tip_Txt.text = LanguageManager.Instance.GetText("Show last 20 hands");
    }

    void ListenEvent()
    {
        betRecord_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectObj(betRecord_Obj);
            dateSelect_Obj.SetActive(true);
            bethistory.gameObject.SetActive(false);
            StartTime_Txt.text = System.DateTime.Today.ToShortDateString();
            EndTime_Txt.text = System.DateTime.Today.ToShortDateString();
            Game_Drop.gameObject.SetActive(true);
        });
        transactionList_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectObj(transactionList_Obj);
            dateSelect_Obj.SetActive(true);
            StartTime_Txt.text = System.DateTime.Today.ToShortDateString();
            EndTime_Txt.text = System.DateTime.Today.ToShortDateString();
            Game_Drop.gameObject.SetActive(false);
        });
        handHistory_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectObj(handHistory_Obj);
            dateSelect_Obj.SetActive(false);
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
        bethistory.showBetHistory(data);
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
