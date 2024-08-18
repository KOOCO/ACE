using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class BuyChipsView : MonoBehaviour
{
    [SerializeField]
    Image SB_Img, BB_Img;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, BlindsTitle_Txt,
                    SB_Txt, BB_Txt, PreBuyChips_Txt, 
                    MinBuyChips_Txt, MaxBuyChips_Txt,
                    CancelBtn_Txt, BuyBtn_Txt,
                    CountDownTip_Txt;

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
        BlindsTitle_Txt.text = LanguageManager.Instance.GetText("Blinds");
        CancelBtn_Txt.text = LanguageManager.Instance.GetText("Cancel");
        BuyBtn_Txt.text = LanguageManager.Instance.GetText("Buy");
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
            CancelInvoke(nameof(SetCountDownTip));
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
    public void SetBuyChipsViewInfo(GameControl gameControl, bool isJustBuyChips, double smallBlind, string roomName, TableTypeEnum tableTypeEnum, UnityAction<double> sendBuyCallback)
    {
        thisData.gameControl = gameControl;
        thisData.IsJustBuyChips = isJustBuyChips;
        thisData.RoomName = roomName;
        thisData.SmallBlind = smallBlind;
        thisData.SendBuyChipsCallback = sendBuyCallback;

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
                titleStr = "CRYPTO TABLE";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserUChips)}";
                SB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[0];
                BB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[0];
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "VIRTUAL CURRENCY TABLE";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserAChips)}";
                SB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[1];
                BB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[1];
                break;
        }
        Title_Txt.text = LanguageManager.Instance.GetText(titleStr);

        SB_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)}";
        BB_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind * 2)}";

        thisData.SmallBlind = smallBlind;

        TexasHoldemUtil.SetBuySlider(thisData.SmallBlind, BuyChips_Sli, tableTypeEnum);
        MinBuyChips_Txt.text = $"{StringUtils.SetChipsUnit(thisData.SmallBlind * DataManager.MinMagnification)}";
        MaxBuyChips_Txt.text = maxBuyChipsStr;
    }
}
