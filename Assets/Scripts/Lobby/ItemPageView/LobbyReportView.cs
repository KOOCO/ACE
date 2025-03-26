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
                selectObj(betRecord_Obj);
            dateSelect_Obj.SetActive(true);
            bethistory.gameObject.SetActive(false);
            StartTime_Txt.text = System.DateTime.Today.ToShortDateString();
            EndTime_Txt.text = System.DateTime.Today.ToShortDateString();
            Game_Drop.gameObject.SetActive(true);
        });
        handHistory_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectObj(handHistory_Obj);
            dateSelect_Obj.SetActive(false);
        });
        transList_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                selectObj(transList_Obj);
            dateSelect_Obj.SetActive(false);
            string data = "{\"TotalCount\":12,\"Detail\":[{\"TransactionID\":\"S4gzKkqhtksWk3OfsNRUSWJTew\",\"HashKey\":\"3Z89C98xre9FUIo6GPIYH4QblTphvY52rAACaVwl7GuIQFOGGVE2JHiBSZBt2t7m\",\"Time\":\"2023-06-06T07:02:23\",\"Type\":\"Transfer In\",\"Amount\":495.73,\"State\":\"Failed\"},{\"TransactionID\":\"2xSa0NlNzm5VJv6yAX30aQXmf1\",\"HashKey\":\"f2plIx6DlN8xUaAZMpyanCYNz1Pc7ROSdvYNrscz72vUqq8LNt7tVePNbDFOZ2Av\",\"Time\":\"2022-11-21T22:36:46\",\"Type\":\"Transfer Out\",\"Amount\":909.13,\"State\":\"Complete\"},{\"TransactionID\":\"DFodL7MDf7VI1tODjdKLIJicdV\",\"HashKey\":\"i612xUY2hBnXhvvf4psb8ZJtzzr1Av0DKtYSbbbq2WxZKE5cOWbep5XXuZUdLqlQ\",\"Time\":\"2023-04-04T22:22:05\",\"Type\":\"Transfer Out\",\"Amount\":310.37,\"State\":\"Failed\"},{\"TransactionID\":\"w38WxeJ1I1gKScQhvgdR8j8wov\",\"HashKey\":\"6xeV22slo8xzF9XuP3d0thiwdb6a50hCQxAirrJxsy4Q3DNFkHcF1DAK6kDOcJK1\",\"Time\":\"2023-10-21T10:14:45\",\"Type\":\"Transfer Out\",\"Amount\":525.87,\"State\":\"Failed\"},{\"TransactionID\":\"v254vNJyXw5gCmnNEK4kPcDmMt\",\"HashKey\":\"i612xUY2hBnXhvvf4psb8ZJtzzr1Av0DKtYSbbbq2WxZKE5cOWbep5XXuZUdLqlQ\",\"Time\":\"2023-04-04T22:22:05\",\"Type\":\"Transfer Out\",\"Amount\":310.37,\"State\":\"Failed\"},{\"TransactionID\":\"w38WxeJ1I1gKScQhvgdR8j8wov\",\"HashKey\":\"6xeV22slo8xzF9XuP3d0thiwdb6a50hCQxAirrJxsy4Q3DNFkHcF1DAK6kDOcJK1\",\"Time\":\"2023-10-21T10:14:45\",\"Type\":\"Transfer Out\",\"Amount\":525.87,\"State\":\"Failed\"},{\"TransactionID\":\"v254vNJyXw5gCmnNEK4kPcDmMt\",\"HashKey\":\"i612xUY2hBnXhvvf4psb8ZJtzzr1Av0DKtYSbbbq2WxZKE5cOWbep5XXuZUdLqlQ\",\"Time\":\"2023-04-04T22:22:05\",\"Type\":\"Transfer Out\",\"Amount\":310.37,\"State\":\"Failed\"},{\"TransactionID\":\"w38WxeJ1I1gKScQhvgdR8j8wov\",\"HashKey\":\"6xeV22slo8xzF9XuP3d0thiwdb6a50hCQxAirrJxsy4Q3DNFkHcF1DAK6kDOcJK1\",\"Time\":\"2023-10-21T10:14:45\",\"Type\":\"Transfer Out\",\"Amount\":525.87,\"State\":\"Failed\"},{\"TransactionID\":\"v254vNJyXw5gCmnNEK4kPcDmMt\",\"HashKey\":\"i612xUY2hBnXhvvf4psb8ZJtzzr1Av0DKtYSbbbq2WxZKE5cOWbep5XXuZUdLqlQ\",\"Time\":\"2023-04-04T22:22:05\",\"Type\":\"Transfer Out\",\"Amount\":310.37,\"State\":\"Failed\"},{\"TransactionID\":\"w38WxeJ1I1gKScQhvgdR8j8wov\",\"HashKey\":\"6xeV22slo8xzF9XuP3d0thiwdb6a50hCQxAirrJxsy4Q3DNFkHcF1DAK6kDOcJK1\",\"Time\":\"2023-10-21T10:14:45\",\"Type\":\"Transfer Out\",\"Amount\":525.87,\"State\":\"Failed\"},{\"TransactionID\":\"v254vNJyXw5gCmnNEK4kPcDmMt\",\"HashKey\":\"i612xUY2hBnXhvvf4psb8ZJtzzr1Av0DKtYSbbbq2WxZKE5cOWbep5XXuZUdLqlQ\",\"Time\":\"2023-04-04T22:22:05\",\"Type\":\"Transfer Out\",\"Amount\":310.37,\"State\":\"Failed\"},{\"TransactionID\":\"w38WxeJ1I1gKScQhvgdR8j8wov\",\"HashKey\":\"6xeV22slo8xzF9XuP3d0thiwdb6a50hCQxAirrJxsy4Q3DNFkHcF1DAK6kDOcJK1\",\"Time\":\"2023-10-21T10:14:45\",\"Type\":\"Transfer Out\",\"Amount\":525.87,\"State\":\"Failed\"}]}";
            transactionList.ShowTransactionList(data);
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
