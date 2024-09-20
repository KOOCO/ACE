using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CryptoTableBtnSample : MonoBehaviour
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
    }

    /// <summary>
    /// 設定加密貨幣桌按鈕訊息
    /// </summary>
    /// <param name="smallBlind">小盲</param>
    /// <param name="lobbyView">大廳</param>
    public void SetCryptoTableBtnInfo(double smallBlind, LobbyView lobbyView)
    {
        Blinds_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)} / {StringUtils.SetChipsUnit(smallBlind * 2)}";
        MinBuy_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind * DataManager.MinMagnification)}";

        Launch_Btn.onClick.AddListener(() =>
        {
            if (GameRoomManager.Instance.JudgeIsCanBeCreateRoom())
            {
                if (DataManager.UserUChips > ((smallBlind * 2) * DataManager.MinMagnification))
                {
                    if (joinRoomView == null)
                    {
                        joinRoomView = ViewManager.Instance.CreateViewInCurrCanvas<JoinRoomView>(JoinRoomViewObj);
                        joinRoomView.SetCreatRoomViewInfo(TableTypeEnum.Cash, smallBlind);
                    }
                    else
                    {
                        joinRoomView.SetCreatRoomViewInfo(TableTypeEnum.Cash, smallBlind);
                        joinRoomView.gameObject.SetActive(!joinRoomView.gameObject.activeSelf);
                    }
                }
                else
                {
                    DataManager.TipText = LanguageManager.Instance.GetText("you dont have enough Ucoins Please buy from shop");
                    DataManager.istipAppear = true;
                }
            }
            else
            {
                lobbyView.ShowMaxRoomTip();
            }
        });
    }
}
