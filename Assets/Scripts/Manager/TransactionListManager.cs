using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransactionListManager : MonoBehaviour
{
    [SerializeField]
    private GameObject transactionListSample;
    [SerializeField]
    private TextMeshProUGUI totalRecord_Txt;
    [SerializeField]
    private Button Next_Btn, Prev_Btn;
    [SerializeField]
    private TextMeshProUGUI NowPage_Txt, TotalPage_Txt;
    [SerializeField]
    private Transform Content;
    [SerializeField]
    private ScrollRect ScrollView;
    [SerializeField]
    private TransactionDetail detailView;

    private int nowPage, totalPage, pageSize;
    private ObjPool objPool;
    private TransactionListData transListdata;

    public void Init()
    {
        objPool = new ObjPool(transform, 100);
        Next_Btn.onClick.AddListener(() =>
        {
            if (totalPage == 1) return;
            nowPage++;
            if (nowPage > totalPage)
            {
                nowPage = 1;
            }
            AppApi.GetTransactionList(nowPage, ShowTransactionList);
            UpdatePage();
        });
        Prev_Btn.onClick.AddListener(() =>
        {
            if (totalPage == 1) return;
            nowPage--;
            if (nowPage <= 0)
            {
                nowPage = totalPage;
            }
            UpdatePage();
            AppApi.GetTransactionList(nowPage, ShowTransactionList);
        });
    }
    public void ShowTransactionList(string data)
    {
        if(data == null) return;
        transListdata = JsonConvert.DeserializeObject<TransactionListData>(data) ?? new TransactionListData();
        totalPage = transListdata.meta.totalPages;
        nowPage = transListdata.meta.currentPage;
        pageSize = transListdata.meta.pageSize;
        string totalPagetxt = "OF " + totalPage.ToString();
        TotalPage_Txt.text = totalPagetxt;
        if (transListdata.meta.totalRecords == 0)
        {
            NowPage_Txt.text = "1";
            TotalPage_Txt.text = "OF 1";
            Next_Btn.onClick.RemoveAllListeners();
            Prev_Btn.onClick.RemoveAllListeners();
        }
        UpdatePage();
        totalRecord_Txt.text = LanguageManager.Instance.GetText("Total {count} Record(S)").Replace("{count}", $" {transListdata.meta.totalRecords} ");

    }
    private void UpdatePage()
    {
        NowPage_Txt.text = nowPage.ToString();
        ScrollView.verticalNormalizedPosition = 1;
        int count = 1;
        for (int i = 1; i < Content.childCount; i++)
        {
            Content.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < transListdata.data.Count; i++)
        {
            if (i > transListdata.meta.totalRecords - 1)
            {
                for (int k = count; k <= pageSize; k++)
                {
                    if (k > transListdata.meta.totalRecords)
                    {
                        break;
                    }
                    Content.GetChild(k).gameObject.SetActive(false);
                }
                break;
            }
            TransactionData detail = transListdata.data[i];
            TransactionListSample obj = objPool.CreateObj<TransactionListSample>(transactionListSample, Content);
            obj.ClickItem += ShowDetailView;
            UpdateObjectData(obj, detail);
            count++;
        }
    }
    private void ShowDetailView(TransactionData data)
    {
        detailView.ShowDetail(data);
    }
    private void UpdateObjectData(TransactionListSample obj, TransactionData detail)
    {
        obj.gameObject.SetActive(true);
        obj.setTransactionListValue(detail);
    }
}
public class TransactionListData
{
    public Meta meta;
    public List<TransactionData> data;
}
public class Meta
{
    public int currentPage;
    public int pageSize;
    public int totalPages;
    public int totalRecords;
}
public class TransactionData
{
    public string hashKey;
    public int currencyCode;
    public int transactionType;
    public double amount;
    public int transactionStatus;
    public string creationTime;
    public string id;
}
