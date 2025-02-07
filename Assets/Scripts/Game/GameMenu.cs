using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class GameMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    RectTransform MenuPage_Tr;
    [SerializeField]
    Button MenuClose_Btn, SitOut_Btn, BuyChips_Btn, GameRules_Btn, HandHistory_Btn, Mask_Btn, LogOut_Btn;
    [SerializeField]
    Image MenuAvatar_Img;
    [SerializeField]
    TextMeshProUGUI MenuNickname_Txt, MenuWalletAddr_Txt, MenuWalletCoin_Txt;

    [Header("購買籌碼")]
    [SerializeField]
    BuyChipsView buyChipsView;
    public event Action OpenBuyView;

    [Header("遊戲規則")]
    [SerializeField]
    GameObject RuleView, GameRules_ScrollView;
    [SerializeField]
    List<GameObject> RuleObjList;
    [SerializeField]
    Button closeRule_Btn, got_it_btn;

    [Header("手牌紀錄")]
    [SerializeField]
    RectTransform HandHistoryPage_Tr;
    [SerializeField]
    Button HandHistoryClose_Btn;

    public event Action LeaveRoom;
    public event Action SitOut;
    

    public void Init()
    {
        MenuPage_Tr.gameObject.SetActive(false);
        ListenerEvent();
        //選單玩家訊息
        StringUtils.StrExceedSize(DataManager.UserWalletAddress, MenuWalletAddr_Txt);
        MenuNickname_Txt.text = $"@{DataManager.UserNickname}";
        MenuWalletCoin_Txt.text = DataManager.UserChips.ToString("F2");
        MenuAvatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];
    }
    private void ListenerEvent()
    {
        Mask_Btn.onClick.AddListener(() =>
        {
            Mask_Btn.gameObject.SetActive(false);
            StartCoroutine(UnityUtils.Instance.IViewSlide(false, MenuPage_Tr, DirectionEnum.Left, 0.25f, () =>
            {
                GameRoomManager.Instance.IsCanMoveSwitch = true;
            }));
        });
        //離開房間
        LogOut_Btn.onClick.AddListener(() =>
        {
            ConfirmView confirmView = ViewManager.Instance.OpenConfirmView();
            confirmView.SetContent(LanguageManager.Instance.GetText("return to the lobby?"),
                                   LanguageManager.Instance.GetText("If you leave now, you will not be able to get back your staked chips."));
            confirmView.SetBnt(() =>
            {
                LeaveRoom?.Invoke();
                //LoadSceneManager.Instance.LoadScene(SceneEnum.Lobby);
            },
            true,
            () =>
            {
                GameRoomManager.Instance.IsCanMoveSwitch = true;
            });
        });

        //關閉選單
        MenuClose_Btn.onClick.AddListener(() =>
        {
            ShowMenu(false);
        });
        //購買籌碼
        BuyChips_Btn.onClick.AddListener(() =>
        {
            OpenBuyView.Invoke();
        });

        //開始規則
        GameRules_Btn.onClick.AddListener(() =>
        {
            ShowRule(true);
        });
        closeRule_Btn.onClick.AddListener(() =>
        {
            RuleView.SetActive(false);
        });
        got_it_btn.onClick.AddListener(() =>
        {
            RuleView.SetActive(false);
        });

        //離開/回到座位
        SitOut_Btn.onClick.AddListener(() =>
        {
            SitOut.Invoke();
            ShowMenu(false);
        });
        #region 手牌紀錄

        //開啟手牌紀錄
        HandHistory_Btn.onClick.AddListener(() =>
        {
            Mask_Btn.gameObject.SetActive(false);
            StartCoroutine(UnityUtils.Instance.IViewSlide(false, MenuPage_Tr, DirectionEnum.Left, 0.25f));
            HandHistoryPage_Tr.gameObject.SetActive(true);
            StartCoroutine(UnityUtils.Instance.IViewSlide(true, HandHistoryPage_Tr.GetChild(0).GetComponent<RectTransform>(), DirectionEnum.Up, 0.25f));
        });

        //關閉手牌紀錄
        HandHistoryClose_Btn.onClick.AddListener(() =>
        {
            StartCoroutine(UnityUtils.Instance.IViewSlide(false, HandHistoryPage_Tr.GetChild(0).GetComponent<RectTransform>(), DirectionEnum.Up, 0.25f, () =>
            {
                GameRoomManager.Instance.IsCanMoveSwitch = true;
                HandHistoryPage_Tr.gameObject.SetActive(false);
            }));
        });
        #endregion
    }
    /// <summary>
    /// 開啟選單
    /// </summary>
    public void ShowMenu(bool isShow)
    {
        if (isShow)
        {
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            Mask_Btn.gameObject.SetActive(true);
            StartCoroutine(UnityUtils.Instance.IViewSlide(true, MenuPage_Tr, DirectionEnum.Left, 0.25f));
        }
        else
        {
            Mask_Btn.gameObject.SetActive(false);
            StartCoroutine(UnityUtils.Instance.IViewSlide(false, MenuPage_Tr, DirectionEnum.Left, 0.25f, () =>
            {
                GameRoomManager.Instance.IsCanMoveSwitch = true;
            }));
        }
    }

    public void OpenBuyChipView(GameControl gameControl, bool isJustBuyChips, double smallBlind, string roomName,
        TableTypeEnum tableTypeEnum, UnityAction<double> sendBuyCallback)
    {
        buyChipsView.gameObject.SetActive(true);
        buyChipsView.SetBuyChipsViewInfo(gameControl, isJustBuyChips, smallBlind, roomName, tableTypeEnum, sendBuyCallback);
    }
    public void CloseBuyChipView()
    {
        buyChipsView.gameObject.SetActive(false);
    }

    public void ShowRule(bool isShow)
    {
        if (isShow)
        {
            RuleView.SetActive(true);
            int languageIndex = LanguageManager.Instance.GetCurrLanguageIndex();
            int OtherIndex = (languageIndex - 1 < 0) ? 1 : languageIndex - 1;
            GameRules_ScrollView.GetComponent<ScrollRect>().content = RuleObjList[languageIndex].GetComponent<RectTransform>();
            RuleObjList[languageIndex].SetActive(true);
            RuleObjList[OtherIndex].SetActive(false);
            ShowMenu(false);
        }
        else
        {
            RuleView.SetActive(false);
        }
    }
}
