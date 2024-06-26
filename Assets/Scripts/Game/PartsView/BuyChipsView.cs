using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using RequestBuf;

public class BuyChipsView : MonoBehaviour
{
    [SerializeField]
    Text Title_Txt, SB_Txt, BB_Txt, PreBuyChips_Txt, MinBuyChips_Txt, MaxBuyChips_Txt;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;

    private ThisData thisData;
    public class ThisData
    {
        public string RoomName;                               //掛載的房間名
        public double SmallBlind;                             //小盲值
        public UnityAction<double> SendBuyChipsCallback;      //發送購買籌碼回傳
    }

    public void Awake()
    {
        ListenerEvent();
    }

    private void OnEnable()
    {
        thisData = new ThisData();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //返回大廳
        Cancel_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.RemoveGameRoom(thisData.RoomName);
        });

        //購買Slider單位設定
        BuyChips_Sli.onValueChanged.AddListener((value) =>
        {
            float newRaiseValue = TexasHoldemUtil.SliderValueChange(BuyChips_Sli,
                                                                    value,
                                                                    (float)thisData.SmallBlind * 2,
                                                                    BuyChips_Sli.minValue,
                                                                    BuyChips_Sli.maxValue);
            PreBuyChips_Txt.text = StringUtils.SetChipsUnit(newRaiseValue);
        });

        //購買按鈕
        Buy_Btn.onClick.AddListener(() =>
        {
            double buyChipsValue = BuyChips_Sli.value;
            thisData.SendBuyChipsCallback(buyChipsValue);
        });

        //+按鈕
        BuyPlus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value += (float)thisData.SmallBlind * 2;
        });

        //-按鈕
        BuyMinus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value -= (float)thisData.SmallBlind * 2;
        });
    }

    /// <summary>
    /// 設定購買介面
    /// </summary>
    /// <param name="smallBlind"></param>
    /// <param name="roomName"></param>
    /// <param name="tableTypeEnum"></param>
    /// <param name="sendBuyCallback"></param>
    public void SetBuyChipsViewInfo(double smallBlind, string roomName, TableTypeEnum tableTypeEnum, UnityAction<double> sendBuyCallback)
    {
        thisData.RoomName = roomName;
        thisData.SmallBlind = smallBlind;
        thisData.SendBuyChipsCallback = sendBuyCallback;

        string titleStr = "";
        string maxBuyChipsStr = "";
        switch (tableTypeEnum)
        {
            //加密貨幣桌
            case TableTypeEnum.CryptoTable:
                titleStr = "CRYPTO TABLE";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserCryptoChips)}";
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "VIRTUAL CURRENCY TABLE";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserVCChips)}";
                break;
        }
        Title_Txt.text = titleStr;

        SB_Txt.text = $"{smallBlind} /";
        BB_Txt.text = $"{smallBlind * 2}";

        thisData.SmallBlind = smallBlind;

        TexasHoldemUtil.SetBuySlider(thisData.SmallBlind, BuyChips_Sli, tableTypeEnum);
        MinBuyChips_Txt.text = $"{StringUtils.SetChipsUnit(thisData.SmallBlind * DataManager.MinMagnification)}";
        MaxBuyChips_Txt.text = maxBuyChipsStr;
    }
}
