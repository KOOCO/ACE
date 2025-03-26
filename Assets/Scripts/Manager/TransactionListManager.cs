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

    private int nowPage, totalPage;
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
        });
    }
    public void ShowTransactionList(string data)
    {
        if(data == null) return;
        transListdata = JsonConvert.DeserializeObject<TransactionListData>(data) ?? new TransactionListData();
        totalPage = (int)((transListdata.Detail.Count + 9) / 10); //算總頁數無條件進位
        nowPage = 1;
        string totalPagetxt = "OF " + totalPage.ToString();
        TotalPage_Txt.text = totalPagetxt;
        NowPage_Txt.text = nowPage.ToString();
        if (transListdata.Detail.Count == 0)
        {
            NowPage_Txt.text = "1";
            TotalPage_Txt.text = "OF 1";
            Next_Btn.onClick.RemoveAllListeners();
            Prev_Btn.onClick.RemoveAllListeners();
        }
        UpdatePage();
        totalRecord_Txt.text = LanguageManager.Instance.GetText("Total {count} Record(S)").Replace("{count}", $" {transListdata.Detail.Count} ");

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
        for (int i = (nowPage - 1) * 10; i < nowPage * 10; i++)
        {
            if (i > transListdata.Detail.Count - 1)
            {
                for (int k = count; k <= 10; k++)
                {
                    if (k > transListdata.Detail.Count)
                    {
                        break;
                    }
                    Content.GetChild(k).gameObject.SetActive(false);
                }
                break;
            }
            TransactionDetailData detail = transListdata.Detail[i];
            TransactionListSample obj = objPool.CreateObj<TransactionListSample>(transactionListSample, Content);
            obj.ClickItem += ShowDetailView;
            UpdateObjectData(obj, detail);
            count++;
        }
    }
    private void ShowDetailView(TransactionDetailData data)
    {
        detailView.ShowDetail(data);
    }
    private void UpdateObjectData(TransactionListSample obj, TransactionDetailData detail)
    {
        obj.gameObject.SetActive(true);
        obj.setTransactionListValue(detail);
    }
}
public class TransactionListData
{
    public int TotalCount;
    public List<TransactionDetailData> Detail;
}
public class TransactionDetailData
{
    public string TransactionID;
    public string HashKey;
    public string Time;
    public string Type;
    public float Amount;
    public string State;
}
