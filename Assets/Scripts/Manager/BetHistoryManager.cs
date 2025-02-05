//using Dynamitey.DynamicObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetHistoryManager : UnitySingleton<BetHistoryManager>
{
    public GameObject bethistorySample;
    public TextMeshProUGUI allWin_Txt, allValidBet_Txt, allBet_Txt, totalRecord_Txt;
    public Button Next_Btn, Prev_Btn;
    public TextMeshProUGUI NowPage_Txt, TotalPage_Txt;
    public Transform Content;
    public ScrollRect ScrollView;

    private float allWin, allValidBet, allBet;
    private int nowPage, totalPage;

    private BettingDetail detailData;

    private ObjPool objPool;


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
    public void showBetHistory(string data)
    {
        detailData = JsonConvert.DeserializeObject<BettingDetail>(data) ?? new BettingDetail();

        totalPage = (int)((detailData.items.Count + 9) / 10); //算總頁數無條件進位
        nowPage = 1;
        string totalPagetxt = "OF " + totalPage.ToString();
        TotalPage_Txt.text = totalPagetxt;
        NowPage_Txt.text = nowPage.ToString();
        if (detailData.items.Count == 0)
        {
            NowPage_Txt.text = "1";
            TotalPage_Txt.text = "OF 1";
            Next_Btn.onClick.RemoveAllListeners();
            Prev_Btn.onClick.RemoveAllListeners();
        }

        foreach (Item item in detailData.items)
        {
            allWin += item.profit;
            allValidBet += item.validBet;
            allBet += item.bets;
        }
        UpdatePage();
        allWin_Txt.text = allWin_Txt.text + $"${allWin}";
        allValidBet_Txt.text = allValidBet_Txt.text + $"${allValidBet}";
        allBet_Txt.text = allBet_Txt.text + $"${allBet}";
        totalRecord_Txt.text = totalRecord_Txt.text.Replace("{count}", $" {detailData.items.Count} ");

    }
    private void UpdatePage()
    {
        NowPage_Txt.text = nowPage.ToString();
        ScrollView.verticalNormalizedPosition = 1;
        int count = 1;
        for (int i = 1; i < bethistorySample.GetComponent<Transform>().parent.childCount; i++)
        {
            bethistorySample.GetComponent<Transform>().parent.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = (nowPage - 1) * 10; i < nowPage * 10; i++)
        {
            if (i > detailData.items.Count - 1)
            {
                for (int k = count; k <= 10; k++)
                {
                    if (k > detailData.items.Count) return;
                    Content.GetChild(k).gameObject.SetActive(false);
                }
                return;
            }
            Item item = detailData.items[i];
            BetHistorySample obj = objPool.CreateObj<BetHistorySample>(bethistorySample, bethistorySample.GetComponent<Transform>().parent);
            UpdateObjectData(obj, item);
            count++;
        }
    }
    private void UpdateObjectData(BetHistorySample obj,Item item)
    {
        obj.gameObject.SetActive(true);
        string time = item.bettingTime.Replace("T", " ");
        obj.setBetRecordValue(time, "", item.roomID.ToString(), item.roomFee, item.bets, item.validBet, item.wins, item.profit);
    }
}

public class BettingDetail
{
    public int totalCount;
    public List<Item> items;
}
public class Item
{
    public string bettingNumber;
    public string tenantName;
    public string memberAccount;
    public string memberID;
    public string currency;
    public float bets;
    public float validBet;
    public float wins;
    public float profit;
    public float roomFee;
    public float insurance;
    public string tableID;
    public long roomID;
    public int roundID;
    public string bettingTime;
    public string calculationTime;
}