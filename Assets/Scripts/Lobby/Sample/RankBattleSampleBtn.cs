using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankBattleSampleBtn : MonoBehaviour
{
    [SerializeField]
    Button Launch_Btn;
    [SerializeField]
    GameObject JoinRoomViewObj;
    [SerializeField]
    TextMeshProUGUI BlindsStr_Txt, Blinds_Txt,
                    MinBuyStr_Txt, MinBuy_Txt,
                    LaunchBtn_Txt;
    public static JoinRoomView joinRoomView;

    CurrencyType currencyType;

    string tableId;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        BlindsStr_Txt.text = LanguageManager.Instance.GetText("Blinds");
        MinBuyStr_Txt.text = LanguageManager.Instance.GetText("Min Buy-In");
        LaunchBtn_Txt.text = LanguageManager.Instance.GetText("LAUNCH");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        currencyType = CurrencyType.ACoin;
    }

    /// <summary>
    /// 設定虛擬貨幣桌按鈕訊息
    /// </summary>
    /// <param name="smallBlind">小盲</param>
    /// <param name="lobbyView">大廳</param>
    public void SetRankBattleBtnInfo(double smallBlind, LobbyView lobbyView, string _tableId)
    {
        tableId = _tableId;
        Blinds_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)} / {StringUtils.SetChipsUnit(smallBlind * 2)}";
        MinBuy_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind * DataManager.MinMagnification)}";

        Launch_Btn.onClick.AddListener(() =>
        {
            DataManager.TableId = tableId;
            DataManager.CurrencyType = currencyType;
            if (GameRoomManager.Instance.JudgeIsCanBeCreateRoom())
            {
                if (DataManager.UserAChips > ((smallBlind * 2) * DataManager.MinMagnification))
                {
                    if (joinRoomView == null)
                    {
                        joinRoomView = ViewManager.Instance.CreateViewInCurrCanvas<JoinRoomView>(JoinRoomViewObj);
                        joinRoomView.SetCreatRoomViewInfo(TableTypeEnum.VCTable, smallBlind);
                    }
                    else
                    {
                        joinRoomView.SetCreatRoomViewInfo(TableTypeEnum.VCTable, smallBlind);
                        joinRoomView.gameObject.SetActive(!joinRoomView.gameObject.activeSelf);
                    }
                }
                else
                {
                    ViewManager.Instance.OpenTipMsgView(transform, LanguageManager.Instance.GetText("you dont have enough chips Please buy from shop"));
                }
            }
            else
            {
                lobbyView.ShowMaxRoomTip();
            }
        });
    }
}