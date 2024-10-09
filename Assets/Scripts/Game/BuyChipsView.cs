using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

public class BuyChipsView : MonoBehaviour
{
    [SerializeField]
    Image BlindACoin_Img, BlindUCoin_Img,
          MinBuyACoin_Img, MinBuyUCoin_Img,
          MaxBuyACoin_Img, MaxBuyUCoin_Img;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Close_Btn, Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, BuyChipsTip_Txt, BlindsTitle_Txt,
                    Blind_Txt, PreBuyChips_Txt,
                    MinBuyChips_Txt, MaxBuyChips_Txt,
                    CancelBtn_Txt, BuyBtn_Txt,
                    CountDownTip_Txt;

    [SerializeField]
    SliderClickDetection sliderClickDetection;

    double newValue;                     //更新後的購買籌碼

    private ThisData thisData;
    public class ThisData
    {
        public GameControl gameControl;
        public bool IsJustBuyChips;                           //一般購買籌碼/籌碼不足須購買
        public string RoomName;                               //掛載的房間名
        public double SmallBlind;                             //小盲值
        public UnityAction<double> SendBuyChipsCallback;      //發送購買籌碼回傳
    }

    int cdTime;                             //倒數時間

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        BlindsTitle_Txt.text = LanguageManager.Instance.GetText("Blind Bet");
        CancelBtn_Txt.text = LanguageManager.Instance.GetText("CANCEL");
        BuyBtn_Txt.text = LanguageManager.Instance.GetText("CONFIRM");
        BuyChipsTip_Txt.text = LanguageManager.Instance.GetText("Start replenishing chips for the next hand");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    public void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
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
        //取消
        Close_Btn.onClick.AddListener(() =>
        {
            if (thisData.IsJustBuyChips)
            {
                //一般購買籌碼
                gameObject.SetActive(false);
            }
            else
            {
                //籌碼不足須購買
                thisData.gameControl.ExitGame();
            }
        });

        //返回大廳
        Cancel_Btn.onClick.AddListener(() =>
        {
            if (thisData.IsJustBuyChips)
            {
                //一般購買籌碼
                gameObject.SetActive(false);
            }
            else
            {
                //籌碼不足須購買
                thisData.gameControl.ExitGame();
            }
        });

        //購買Slider單位設定
        BuyChips_Sli.onValueChanged.AddListener((value) =>
        {
            newValue = TexasHoldemUtil.SliderValueChange(BuyChips_Sli,
                                                        value,
                                                        thisData.SmallBlind * 2,
                                                        BuyChips_Sli.minValue,
                                                        BuyChips_Sli.maxValue,
                                                        sliderClickDetection);
            PreBuyChips_Txt.text = StringUtils.SetChipsUnit(newValue);
        });

        //購買按鈕
        Buy_Btn.onClick.AddListener(() =>
        {
            CancelInvoke(nameof(SetCountDownTip));
            thisData.SendBuyChipsCallback(newValue);
            DataManager.UserUChips -= newValue;
            DataManager.DataUpdated = true;
        });

        //+按鈕
        BuyPlus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newValue + thisData.SmallBlind * 2);
        });

        //-按鈕
        BuyMinus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newValue - thisData.SmallBlind * 2);
        });
    }

    /// <summary>
    /// 設定倒數提示
    /// </summary>
    private void SetCountDownTip()
    {
        cdTime--;
        CountDownTip_Txt.text = $"<color=#F43535>{cdTime}</color>\n" +
                                $"{LanguageManager.Instance.GetText("Exit the game after the countdown ends.")}";

        if (cdTime <= 0)
        {
            CancelInvoke(nameof(SetCountDownTip));
            thisData.gameControl.ExitGame();
        }
    }

    /// <summary>
    /// 設定購買介面
    /// </summary>
    /// <param name="isJustBuyChips">一般購買籌碼/籌碼不足須購買</param>
    /// <param name="smallBlind">小盲值</param>
    /// <param name="roomName">房間名</param>
    /// <param name="tableTypeEnum">遊戲房間類型</param>
    /// <param name="sendBuyCallback">購買結果回傳</param>
    public void SetBuyChipsViewInfo(GameControl gameControl, bool isJustBuyChips, double smallBlind, string roomName,
        TableTypeEnum tableTypeEnum, UnityAction<double> sendBuyCallback)
    {
        thisData.gameControl = gameControl;
        thisData.IsJustBuyChips = isJustBuyChips;
        thisData.RoomName = roomName;
        thisData.SmallBlind = smallBlind;
        thisData.SendBuyChipsCallback = sendBuyCallback;

        BuyChipsTip_Txt.gameObject.SetActive(isJustBuyChips);

        CountDownTip_Txt.text = "";
        if (isJustBuyChips == false)
        {
            cdTime = DataManager.BuyChipsCountDown;
            InvokeRepeating(nameof(SetCountDownTip), 1, 1);
        }

        string titleStr = "";
        string maxBuyChipsStr = "";
        switch (tableTypeEnum)
        {
            //現金桌
            case TableTypeEnum.Cash:
                titleStr = "High Roller Battleground";
                maxBuyChipsStr = $"{(smallBlind * 2) * DataManager.MaxMagnification}";
                BlindACoin_Img.gameObject.SetActive(true);
                BlindUCoin_Img.gameObject.SetActive(false);
                MinBuyACoin_Img.gameObject.SetActive(true);
                MinBuyUCoin_Img.gameObject.SetActive(false);
                MaxBuyACoin_Img.gameObject.SetActive(true);
                MaxBuyUCoin_Img.gameObject.SetActive(false);
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "Classic Battle";
                maxBuyChipsStr = $"{(smallBlind * 2) * DataManager.MaxMagnification}";
                BlindACoin_Img.gameObject.SetActive(false);
                BlindUCoin_Img.gameObject.SetActive(true);
                MinBuyACoin_Img.gameObject.SetActive(false);
                MinBuyUCoin_Img.gameObject.SetActive(true);
                MaxBuyACoin_Img.gameObject.SetActive(false);
                MaxBuyUCoin_Img.gameObject.SetActive(true);
                break;
        }
        Title_Txt.text = LanguageManager.Instance.GetText(titleStr);

        Blind_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)} / " +
                         $"{StringUtils.SetChipsUnit(smallBlind * 2)}";

        thisData.SmallBlind = smallBlind;

        double maxBuyValue = tableTypeEnum == TableTypeEnum.Cash ?
                             (smallBlind * 2) * DataManager.MaxMagnification :
                             (smallBlind * 2) * DataManager.MaxMagnification;

        TexasHoldemUtil.SetBuySlider(smallBlind * 2, (smallBlind * 2) * DataManager.MaxMagnification, BuyChips_Sli, tableTypeEnum, gameControl.PreBuyChipsValue);
        MinBuyChips_Txt.text = $"{StringUtils.SetChipsUnit(((thisData.SmallBlind * 2) * DataManager.MinMagnification) + gameControl.PreBuyChipsValue)}";

        MaxBuyChips_Txt.text = maxBuyChipsStr;
    }
}
