using Dynamitey.DynamicObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BetHistoryManager : UnitySingleton<BetHistoryManager>
{
    public GameObject bethistorySample;
    public TextMeshProUGUI allWin_Txt, allValidBet_Txt, allBet_Txt, totalRecord_Txt;

    private float allWin, allValidBet, allBet;

    private BettingDetail detailData;
    public void showBetHistory(string data)
    {
        for (int i = 1; i < bethistorySample.GetComponent<Transform>().parent.childCount; i++)
        {
            Destroy(bethistorySample.GetComponent<Transform>().parent.GetChild(i).gameObject);
        }

        detailData = JsonConvert.DeserializeObject<BettingDetail>(data) ?? new BettingDetail();
        
        foreach (Item item in detailData.items)
        {
            BetHistorySample obj = Instantiate(bethistorySample, bethistorySample.GetComponent<Transform>().parent).GetComponent<BetHistorySample>();
            obj.gameObject.SetActive(true);
            obj.setBetRecordValue(item.bettingTime, "", item.tableID.ToString(), item.roomFee, item.bets, item.validBet, item.wins, item.profit);
            allWin += item.wins;
            allValidBet += item.validBet;
            allBet += item.bets;
        }
        allWin_Txt.text = $"${allWin}";
        allValidBet_Txt.text = $"${allValidBet}";
        allBet_Txt.text = $"${allBet}";
        totalRecord_Txt.text = $"{detailData.items.Count}";

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
    public int bets;
    public int validBet;
    public int wins;
    public float profit;
    public float roomFee;
    public int insurance;
    public string tableID;
    public long roomID;
    public int roundID;
    public string bettingTime;
    public string calculationTime;
}