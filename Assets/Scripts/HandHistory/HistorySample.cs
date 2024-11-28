using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HistorySample : MonoBehaviour
{
    [SerializeField]
    Image Avatar_Img,
          WinACoin_Img, WinUCoin_Img,
          BlindACoin_Img, BlindUCoin_Img;
    [SerializeField]
    Poker[] HandPokers;
    [SerializeField]
    Poker[] CommunityPokers;
    [SerializeField]
    Button Play_Btn, CopyID_Btn;
    [SerializeField]
    TextMeshProUGUI Index_Txt, Blind_Txt, Nicaname_Txt, WinChips_Txt,
                    CoinType_Txt, TableName_Txt, RoomID_Txt;

    ResultHistoryData tempResultHistory;                                                //紀錄資料
    int tempIndex;                                                                      //紀錄顯示的筆數

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        SetData(tempResultHistory, tempIndex);
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
    /// 設置手牌紀錄資料
    /// </summary>
    /// <param name="resultHistory"></param>
    /// <param name="index"></param>
    public void SetData(ResultHistoryData resultHistory, int index)
    {
        if (resultHistory == null)
        {
            return;
        }

        WinACoin_Img.gameObject.SetActive(resultHistory.roomType == "Classic Battle");
        WinUCoin_Img.gameObject.SetActive(resultHistory.roomType == "High Roller Battleground");
        CoinType_Txt.text = resultHistory.roomType == "Classic Battle" ?
                            "A COIN" :
                            "U COIN";

        BlindACoin_Img.gameObject.SetActive(resultHistory.roomType == "Classic Battle");
        BlindUCoin_Img.gameObject.SetActive(resultHistory.roomType == "High Roller Battleground");

        tempResultHistory = resultHistory;
        tempIndex = index;

        TableName_Txt.text = LanguageManager.Instance.GetText(resultHistory.roomType);
        RoomID_Txt.text = $"ID：{resultHistory.roomId.ToString()}";
        var winner = resultHistory.playerDetails.FirstOrDefault(x => x.playerHandData.isWinner);
        Index_Txt.text = $"{LanguageManager.Instance.GetText("NO.")}{index + 1}";
        Blind_Txt.text = $"{StringUtils.SetChipsUnit(resultHistory.smallBlind)}/{StringUtils.SetChipsUnit(resultHistory.smallBlind * 2)}";
        Avatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[resultHistory.avatar];
        Nicaname_Txt.text = winner.playerName;
        WinChips_Txt.text = StringUtils.SetChipsUnit(winner.playerHandData.potWinChips);
        HandPokers[0].PokerNum = winner.playerHandData.playerHand[0];
        HandPokers[1].PokerNum = winner.playerHandData.playerHand[1];

        foreach (var common in CommunityPokers)
        {
            common.PokerNum = -1;
        }
        if (resultHistory.communityPoker != null)
        {
            for (int i = 0; i < resultHistory.communityPoker.Count; i++)
            {
                CommunityPokers[i].PokerNum = resultHistory.communityPoker[i];
            }
        }

        Play_Btn.onClick.AddListener(() =>
        {
            HandHistoryManager.Instance.PlayVideo(index);
        });
        CopyID_Btn.onClick.AddListener(() =>
        {
#if UNITY_EDITOR
            TextEditor editor = new TextEditor
            {
                text = RoomID_Txt.text.Substring(3)
            };
            editor.SelectAll();
            editor.Copy();
            return;
#endif

            JSBridgeManager.Instance.CopyString(RoomID_Txt.text.Substring(3));
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Copy Success!"));
        });
    }
}
