using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BetHistorySample : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI dateTimeValue_Txt, blindsValue_Txt, roomIDValue_Txt, roomRateValue_Txt, betsValue_Txt, validBetValue_Txt, winsValue_Txt, profitValue_Txt;

    [Header("文本翻譯")]
    public TextMeshProUGUI dateTime_Txt, Ace_Txt, Blinds_Txt, roomID_Txt, roomRate_Txt, Bets_Txt, validBet_Txt, Wins_Txt, Profit_Txt;

    // Start is called before the first frame update
    void Start()
    {
        UpdateLanguage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        dateTime_Txt.text = LanguageManager.Instance.GetText("Game Date");
        Ace_Txt.text = LanguageManager.Instance.GetText("ACE");
        Blinds_Txt.text = LanguageManager.Instance.GetText("Blinds");
        roomID_Txt.text = LanguageManager.Instance.GetText("Room ID");
        roomRate_Txt.text = LanguageManager.Instance.GetText("Room rate");
        Bets_Txt.text = LanguageManager.Instance.GetText("Bets");
        validBet_Txt.text = LanguageManager.Instance.GetText("Valid Bet");
        Wins_Txt.text = LanguageManager.Instance.GetText("Wins");
        Profit_Txt.text = LanguageManager.Instance.GetText("Profit");
    }

    public void setBetRecordValue(string dateTime, string blinds, string roomID, float roomRate, float bets, float validBet, float wins, float profit)
    {
        dateTimeValue_Txt.text = dateTime;
        blindsValue_Txt.text = blinds;
        roomIDValue_Txt.text = roomID;
        roomRateValue_Txt.text = toString(roomRate);
        betsValue_Txt.text = toString(bets);
        validBetValue_Txt.text = toString(validBet);
        string winStr = "";
        string profitStr = "";
        if (wins > 0)
        {
            winStr = $"<color=#36D982>{toString(wins)}</color>";
        }
        else if (wins < 0)
        {
            winStr = $"<color=#EC6273>{toString(wins)}</color>";
        }
        else
        {
            winStr = $"<color=#FFFFFF>{toString(wins)}</color>";
        }
        winsValue_Txt.text = winStr;
        if (profit > 0)
        {
            profitStr = $"<color=#36D982>{toString(profit)}</color>";
        }
        else if (profit < 0)
        {
            profitStr = $"<color=#EC6273>{toString(profit)}</color>";
        }
        else
        {
            profitStr = $"<color=#FFFFFF>{toString(profit)}</color>";
        }
        profitValue_Txt.text = profitStr;
    }

    private string toString(float number)
    {
        string numStr = "";
        if (number < 0)
        {
            numStr = number.ToString().Insert(1, "$");
        }
        else
        {
            numStr = $"${number}";
        }
        return numStr;
    }
}
