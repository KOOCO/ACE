using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CryptoTableBtn : MonoBehaviour
{
    [SerializeField]
    Button Launch_Btn;
    [SerializeField]
    Text Blinds_Txt, MinBuy_Txt;

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
            AudioManager.Instance.PlayConfirmClick();
            if (GameRoomManager.Instance.JudgeIsCanBeCreateRoom())
            {
                JoinRoomView joinRoomView = ViewManager.Instance.OpenPartsView(PartsViewEnum.JoinRoomView).GetComponent<JoinRoomView>();
                joinRoomView.SetCreatRoomViewInfo(TableTypeEnum.CryptoTable, smallBlind);
            }
            else
            {
                lobbyView.ShowMaxRoomTip();
            }            
        });
    }
}
