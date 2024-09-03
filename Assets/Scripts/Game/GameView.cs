using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using System.Threading.Tasks;

using RequestBuf;

public class GameView : MonoBehaviour
{
    [SerializeField]
    Request_GameView baseRequest;

    [Header("遊戲控制腳本")]
    [SerializeField]
    GameControl gameControl;

    [Header("遮罩按鈕")]
    [SerializeField]
    Button Mask_Btn;

    [Header("座位上玩家訊息")]
    [SerializeField]
    List<GamePlayerInfo> SeatGamePlayerInfoList;
    [SerializeField]
    List<Button> SeatButtonList;

    [Header("操作按鈕")]
    [SerializeField]
    Button Raise_Btn, Call_Btn, Fold_Btn;
    [SerializeField]
    RectTransform AutoActionFrame_Tr;
    [SerializeField]
    Button[] ShowPokerBtnList;
    [SerializeField]
    Button BackToSit_Btn;
    [SerializeField]
    TextMeshProUGUI RaiseBtn_Txt, CallBtn, FoldBtn_Txt, BackToSitBtn_Txt;

    [Header("加注操作")]
    [SerializeField]
    List<Button> PotPercentRaiseBtnList;
    [SerializeField]
    Slider Raise_Sli;
    [SerializeField]
    RectTransform Raise_Tr;
    [SerializeField]
    SliderClickDetection SliderClickDetection;
    [SerializeField]
    Button AllIn_Btn, MinRaise_Btn;
    [SerializeField]
    List<TextMeshProUGUI> PotPercentRaiseTxtList;
    [SerializeField]
    TextMeshProUGUI RaiseSliHandle_Txt, CurrRaise_Txt, MinRaiseBtn_Txt;

    [Header("底池")]
    [SerializeField]
    Image Pot_Img;
    [SerializeField]
    TextMeshProUGUI TotalPot_Txt, WaitingTip_Txt, WinType_Txt;

    [Header("公共牌")]
    [SerializeField]
    List<Poker> CommunityPokerList = new();

    [Header("離開按鈕")]
    [SerializeField]
    Button LogOut_Btn;
    [SerializeField]
    TextMeshProUGUI LogOutBtn_Txt;

    [Header("選單")]
    [SerializeField]
    RectTransform MenuPage_Tr;
    [SerializeField]
    Button Menu_Btn, MenuClose_Btn, SitOut_Btn, BuyChips_Btn, GameRules_Btn , HandHistory_Btn;
    [SerializeField]
    Image MenuAvatar_Img;
    [SerializeField]
    TextMeshProUGUI MenuCloseBtn_Txt, SitOutBtn_Txt, BuyChipsBtn_Txt, HandHistoryBtn_Txt,
                    GameSettingsBtn_Txt,
                    MenuNickname_Txt, MenuWalletAddr_Txt,GameRules_Txt;

    [Header("聊天")]
    [SerializeField]
    RectTransform ChatPage_Tr, ChatContent_Tr, NotReadChat_Tr;
    [SerializeField]
    Button Chat_Btn, ChatClose_Btn, ChatSend_Btn, NewMessage_Btn;
    [SerializeField]
    GameObject OtherChatSample, LocalChatSample;
    [SerializeField]
    ScrollRect ChatArea_Sr;
    [SerializeField]
    TMP_InputField Chat_If;
    [SerializeField]
    TextMeshProUGUI NotReadChat_Txt, NewMessageBtn_Txt, 
                    ChatSendBtn_Txt, ChatIf_Placeholder;

    [Header("手牌紀錄")]
    [SerializeField]
    RectTransform HandHistoryPage_Tr;
    [SerializeField]
    Button HandHistoryClose_Btn;
    [SerializeField]
    TextMeshProUGUI HandHistoryTitle_Txt, HandHistoryTip_Txt;

    [Header("購買籌碼")]
    [SerializeField]
    BuyChipsView buyChipsView;

    [Header("遊戲規則")]
    [SerializeField]
    GameObject RuleView,Ch_text,En_text, GameRules_ScrollView;
    [SerializeField]
    TextMeshProUGUI GameRules_Top, GameRules_mid;
    [SerializeField]
    Button closeRule_Btn,got_it_btn;
 

    [Header("遊戲結果")]
    [SerializeField]
    RectTransform BattleResultView;
    [SerializeField]
    GameObject WinChipsObj;

    [Header("遊戲暫停")]
    [SerializeField]
    GameObject GamePause_Obj;
    [SerializeField]
    Button GameContinue_Btn;

    [Header("遊戲測試用")]
    [SerializeField]
    GameObject GameTest_Obj;
    [SerializeField]
    List<GameObject> PlayerTestObjList;
    [SerializeField]
    Button GameTestStart_Btn;
    [SerializeField]
    public List<TMP_Dropdown> CP_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> CP_NumTogList;
    [SerializeField]
    public List<TMP_Dropdown> PH0_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> PH1_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> PN0_NumTogList;
    [SerializeField]
    public List<TMP_Dropdown> PN1_NumTogList;

    public bool IsStartGameTest;                                //是否開始遊戲測試

    const float PageMoveTime = 0.25f;                           //滑動頁面移動時間

    //底池倍率
    readonly float[] PotPercentRate = new float[]
    {
        33, 50, 80, 100,
    };

    //加註大盲倍率
    readonly float[] PotBbRate = new float[]
    {
        2f, 3f, 4.0f, 4.0f,
    };

    const int MaxChatCount = 50;                                //保留聊天最大訊息數

    GameRoomData gameRoomData;                                  //房間資料

    AudioPool audioPool;
    ObjPool objPool;
    List<GamePlayerInfo> gamePlayerInfoList = new();            //玩家資料

    Vector2 InitPotPointPos;                                    //初始底池位置
    int notReadMsgCount;                                        //未讀取數
    bool isNextRountSitOut;                                     //是否下局保留作位離開

    #region 遊戲過程紀錄

    List<int> exitPlayerSeatList;                               //玩家離開座位
    GameInitHistoryData gameInitHistoryData;                    //遊戲初始資料紀錄
    ProcessHistoryData processHistoryData;                      //遊戲過程資料紀錄
    ResultHistoryData saveResultData;                           //遊戲結果資料紀錄

    #endregion

    private TableTypeEnum roomType;
    /// <summary>
    /// 房間類型
    /// </summary>
    public TableTypeEnum RoomType
    {
        get
        {
            return roomType;
        }
        set
        {
            roomType = value;

            //積分房
            if (roomType == TableTypeEnum.IntegralTable)
            {
                BuyChips_Btn.interactable = false;
                SitOut_Btn.interactable = false;

                for (int i = 0; i < SeatButtonList.Count; i++)
                {
                    if (i != 0 && i != 3)
                    {
                        SeatButtonList[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private ThisData thisData;
    public class ThisData
    {
        public GamePlayerInfo LocalGamePlayerInfo;         //本地玩家
        public int LocalPlayerSeat;                        //本地玩家座位
        public double LocalPlayerChips;                    //本地玩家籌碼
        public double LocalPlayerCurrBetValue;             //本地玩家當前下注值
        public double TotalPot;                            //當前底池
        public double CallDifference;                      //當前跟注差額
        public double CurrCallValue;                       //當前跟注值
        public double CurrRaiseValue;                      //當前加注值
        public double MinRaiseValue;                       //最小加注值
        public double SmallBlindValue;                     //小盲值
        public bool IsFirstRaisePlayer;                    //首位加注玩家
        public bool IsUnableRaise;                         //無法加注
        public bool IsPlaying;                             //有參與遊戲
        public bool IsSitOut;                              //是否離開座位
        public bool isLocalPlayerTurn;                     //本地玩家回合
        public bool isFold;                                //是否已棄牌
        public bool isCanCall;                             //是否可以跟注
        public List<int> CurrCommunityPoker;               //當前公共牌
        public List<string> PotWinnerList;                 //主池贏家
        public double PowWinChips;                         //主池贏得籌碼
        public List<string> SideWinnerList;                //邊池贏家
        public double SideWinChips;                        //邊池贏得籌碼
        public Dictionary<int, double> BackChipsDic;       //退回籌碼(座位,退回籌碼值)
    }

    private StrData strData;
    public class StrData
    {
        public string FoldStr { get; set; }
        public string CallStr { get; set; }
        public string CallValueStr { get; set; }
        public string RaiseStr { get; set; }
        public string RaiseValueStr { get; set; }
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 操作按鈕

        if (strData != null)
        {
            FoldBtn_Txt.text = LanguageManager.Instance.GetText(strData.FoldStr);
            CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
            RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr) + strData.RaiseValueStr;
        }
        BackToSitBtn_Txt.text = LanguageManager.Instance.GetText("Back To Sit");

        #endregion

        #region 選單

        MenuCloseBtn_Txt.text = LanguageManager.Instance.GetText("MENU");
        BuyChipsBtn_Txt.text = LanguageManager.Instance.GetText("Buy Chips");
        HandHistoryBtn_Txt.text = LanguageManager.Instance.GetText("Hand History");
        LogOutBtn_Txt.text = LanguageManager.Instance.GetText("Log Out");
        GameSettingsBtn_Txt.text = LanguageManager.Instance.GetText("Game Settings");
        GameRules_Txt.text = LanguageManager.Instance.GetText("Game Rules");
        #endregion

        #region 規則
        GameRules_Txt.text = LanguageManager.Instance.GetText("Game Rules");
        GameRules_Top.text= LanguageManager.Instance.GetText("Game Rules");
        GameRules_mid.text= LanguageManager.Instance.GetText("Welcome to AISA POKER! Learn the rules and start playing right away!");

        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            GameRules_ScrollView.GetComponent<ScrollRect>().content = En_text.GetComponent<RectTransform>(); 
            En_text.SetActive(true);
            Ch_text.SetActive(false);
        }
        else if (LanguageManager.Instance.GetCurrLanguageIndex() == 1)
        {
            GameRules_ScrollView.GetComponent<ScrollRect>().content = Ch_text.GetComponent<RectTransform>();
            Ch_text.SetActive(true);
            En_text.SetActive(false);
        }

        #endregion

        #region 聊天

        NewMessageBtn_Txt.text = LanguageManager.Instance.GetText("New Message");
        ChatSendBtn_Txt.text = LanguageManager.Instance.GetText("Send");
        ChatIf_Placeholder.text = LanguageManager.Instance.GetText("Enter text...");

        #endregion

        #region 手牌紀錄

        HandHistoryTitle_Txt.text = LanguageManager.Instance.GetText("HAND HISTORY");
        HandHistoryTip_Txt.text = LanguageManager.Instance.GetText("Show last 20 hands");

        #endregion

        SetSitOutDisplay();
    }

    public void Awake()
    {
        objPool = new ObjPool(transform, MaxChatCount);
        audioPool = new AudioPool(transform);

        ListenerEvent();

        //初始底池位置
        InitPotPointPos = Pot_Img.rectTransform.anchoredPosition;
    }

    private void OnDisable()
    {
        OnRemoveData();
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
        OnRemoveData();
    }

    /// <summary>
    /// 聆聽事件
    /// </summary>
    private void ListenerEvent()
    {
        //遮罩按鈕
        Mask_Btn.onClick.AddListener(() =>
        {
            if (MenuPage_Tr.gameObject.activeSelf)
            {
                Mask_Btn.gameObject.SetActive(false);
                StartCoroutine(UnityUtils.Instance.IViewSlide(false,
                                                              MenuPage_Tr,
                                                              DirectionEnum.Left,
                                                              PageMoveTime,
                                                              () =>
                                                              {
                                                                  GameRoomManager.Instance.IsCanMoveSwitch = true;
                                                              }));
            }
            else if (ChatPage_Tr.gameObject.activeSelf)
            {
                CloseChatPage();
            }
        });

        //遊戲繼續按鈕
        GameContinue_Btn.onClick.AddListener(() =>
        {
     
        });

        #region 選單

        //開啟選單
        Menu_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            Mask_Btn.gameObject.SetActive(true);
            StartCoroutine(UnityUtils.Instance.IViewSlide(true,
                                                          MenuPage_Tr,
                                                          DirectionEnum.Left,
                                                          PageMoveTime));
        });

        //離開房間
        LogOut_Btn.onClick.AddListener(() =>
        {
            ConfirmView confirmView = ViewManager.Instance.OpenConfirmView();
            confirmView.SetContent(LanguageManager.Instance.GetText("return to the lobby?"),
                                   LanguageManager.Instance.GetText("If you leave now, you will not be able to get back your staked chips."));
            confirmView.SetBnt(() =>
            {
                gameControl.ExitGame();
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
            CloseMenu();
        });

        //購買籌碼
        BuyChips_Btn.onClick.AddListener(() =>
        {
            buyChipsView.gameObject.SetActive(true);
            buyChipsView.SetBuyChipsViewInfo(gameControl,
                                             true,
                                             thisData.SmallBlindValue,
                                             transform.name,
                                             RoomType,
                                             BuyChips);
        });

        //開始規則
        GameRules_Btn.onClick.AddListener(() => {
            RuleView.SetActive(true);
            CloseMenu();


        });

        closeRule_Btn.onClick.AddListener(() => {
            RuleView.SetActive(false);
           

        });
        got_it_btn.onClick.AddListener(() => {
            RuleView.SetActive(false);


        });
        //離開/回到座位
        SitOut_Btn.onClick.AddListener(() =>
        {
            thisData.IsSitOut = !thisData.IsSitOut;
            SetSitOutDisplay();

            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.IS_SIT_OUT, thisData.IsSitOut},         //是否保留座位離開
            };
            gameControl.UpdataPlayerData(DataManager.UserId,
                                         data);

            CloseMenu();
        });

        #endregion

        #region 操作按鈕

        //回到座位
        BackToSit_Btn.onClick.AddListener(() =>
        {
            thisData.IsSitOut = false;
            SetSitOutDisplay();
            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.IS_SIT_OUT, thisData.IsSitOut},         //是否保留座位離開
            };
            gameControl.UpdataPlayerData(DataManager.UserId,
                                         data);
        });

        //棄牌顯示手牌按鈕
        for (int i = 0; i < ShowPokerBtnList.Count(); i++)
        {
            int index = i;
            ShowPokerBtnList[i].onClick.AddListener(delegate { SetShowFoldPoker(index); });
        }

        //加注滑條
        Raise_Sli.onValueChanged.AddListener((value) =>
        {
            double newRaiseValue = TexasHoldemUtil.SliderValueChange(Raise_Sli,
                                                                    value,
                                                                    thisData.SmallBlindValue * 2,
                                                                    thisData.MinRaiseValue,
                                                                    thisData.LocalPlayerChips,
                                                                    SliderClickDetection);

            strData.RaiseStr = Math.Floor(newRaiseValue) >= thisData.LocalPlayerChips ?
                               "AllIn" :
                               "RaiseTo";

            if (strData.RaiseStr == "RaiseTo" && 
                gameRoomData.actionPlayerCount == 0)
            {
                strData.RaiseStr = "BetTo";
            }

            strData.RaiseValueStr = newRaiseValue >= thisData.LocalPlayerChips ?
                                    $"\n{StringUtils.SetChipsUnit(thisData.LocalPlayerChips)}" :
                                    $"\n{StringUtils.SetChipsUnit(newRaiseValue)}";
            RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr) + strData.RaiseValueStr;

            SetRaiseToText = newRaiseValue;
            thisData.CurrRaiseValue = Math.Floor(newRaiseValue);
        });

        //最小加注
        MinRaise_Btn.onClick.AddListener(() =>
        {
            Raise_Sli.value = (float)thisData.MinRaiseValue;
        });

        //All In
        AllIn_Btn.onClick.AddListener(() =>
        {
            Raise_Sli.value = (float)thisData.LocalPlayerChips;
        });

        //底池百分比加註
        for (int i = 0; i < PotPercentRaiseBtnList.Count; i++)
        {
            int index = i;
            PotPercentRaiseBtnList[i].onClick.AddListener(delegate { PotRaisePercent(index); });
        }

        //棄牌
        Fold_Btn.onClick.AddListener(() =>
        {
            if (thisData.isLocalPlayerTurn)
            {
                OnFold();
            }
            else
            {
                AutoActionState = AutoActionState == AutoActingEnum.CheckAndFold ?
                                  AutoActingEnum.None :
                                  AutoActingEnum.CheckAndFold;
            }
            Raise_Tr.gameObject.SetActive(false);
            SetActionButton = false;
        });

        //跟注/過牌
        Call_Btn.onClick.AddListener(() =>
        {
            if (thisData.isLocalPlayerTurn)
            {
                OnCallAndCheck();
            }
            else
            {
                AutoActionState = AutoActionState == AutoActingEnum.Check ?
                             AutoActingEnum.None :
                             AutoActingEnum.Check;
            }
            Raise_Tr.gameObject.SetActive(false);
            SetActionButton = false;
        });

        //加注/All In
        Raise_Btn.onClick.AddListener(() =>
        {
            if (thisData.isLocalPlayerTurn)
            {
                bool isAllIn = thisData.LocalPlayerChips < thisData.MinRaiseValue ||
                               thisData.CurrRaiseValue == thisData.LocalPlayerChips;

                BetActingEnum acting = isAllIn == true ?
                                    BetActingEnum.AllIn :
                                    BetActingEnum.Raise;

                //加注
                if (acting == BetActingEnum.Raise && 
                    gameRoomData.actionPlayerCount == 0)
                {
                    acting = BetActingEnum.Bet;
                }

                if (Raise_Tr.gameObject.activeSelf || isAllIn == true)
                {
                    double betValue = isAllIn == true ?
                                  thisData.LocalPlayerChips :
                                  thisData.CurrRaiseValue;

                    gameControl.UpdateBetAction(DataManager.UserId,
                                                acting,
                                                betValue);

                    Raise_Tr.gameObject.SetActive(false);
                    SetActionButton = false;
                }
                else
                {
                    Raise_Tr.gameObject.SetActive(true);
                    strData.RaiseStr = acting == BetActingEnum.Bet ?
                                       "BetTo":
                                       "RaiseTo";
                    strData.RaiseValueStr = $"\n{StringUtils.SetChipsUnit(thisData.CurrRaiseValue)}";
                    //RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr) + strData.RaiseValueStr;
                }
            }
            else
            {
                AutoActionState = AutoActionState == AutoActingEnum.CallAny ?
                                  AutoActingEnum.None :
                                  AutoActingEnum.CallAny;
            }
        });

        #endregion

        #region 聊天

        //開啟聊天
        Chat_Btn.onClick.AddListener(() =>
        {
            SetNotReadChatCount = 0;
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            Mask_Btn.gameObject.SetActive(true);
            StartCoroutine(UnityUtils.Instance.IViewSlide(true,
                                                          ChatPage_Tr,
                                                          DirectionEnum.Left,
                                                          PageMoveTime));
            StartCoroutine(IYieldSetNewMessageActive());
        });

        //發送聊天訊息
        ChatSend_Btn.onClick.AddListener(() =>
        {
            SendChat();
        });

        //關閉聊天
        ChatClose_Btn.onClick.AddListener(() =>
        {
            CloseChatPage();
        });

        //聊天移動至最新訊息位置
        NewMessage_Btn.onClick.AddListener(() =>
        {
            StartCoroutine(IGoNewChatMessage());
        });

        //聊天區域
        ChatArea_Sr.onValueChanged.AddListener((value) =>
        {
            if (NewMessage_Btn.gameObject.activeSelf &&
                IsChatOnBottom())
            {
                NewMessage_Btn.gameObject.SetActive(false);
            }
        });

        #endregion

        #region 手牌紀錄

        //開啟手牌紀錄
        HandHistory_Btn.onClick.AddListener(() =>
        {
            Mask_Btn.gameObject.SetActive(false);
            StartCoroutine(UnityUtils.Instance.IViewSlide(false,
                                                          MenuPage_Tr,
                                                          DirectionEnum.Left,
                                                          PageMoveTime));
            StartCoroutine(UnityUtils.Instance.IViewSlide(true,
                                                          HandHistoryPage_Tr,
                                                          DirectionEnum.Up,
                                                          PageMoveTime));
        });

        //關閉手牌紀錄
        HandHistoryClose_Btn.onClick.AddListener(() =>
        {            
            StartCoroutine(UnityUtils.Instance.IViewSlide(false,
                                                          HandHistoryPage_Tr,
                                                          DirectionEnum.Up,
                                                          PageMoveTime,
                                                          () =>
                                                          {
                                                              GameRoomManager.Instance.IsCanMoveSwitch = true;
                                                          }));

        });

        #endregion

        #region 遊戲測試

        //測試開始
        GameTestStart_Btn.onClick.AddListener(() =>
        {
            IsStartGameTest = true;
            gameControl.preUpdateGameFlow = GameFlowEnum.None;
            gameControl.preLocalGameFlow = GameFlowEnum.None;
            StartCoroutine(gameControl.IStartGameFlow(GameFlowEnum.Licensing));
            IsOpenGameTestObj = false;
        });

        #endregion
    }

    private void OnEnable()
    {
        thisData = new ThisData();
        thisData.IsPlaying = false;
        SetSitOutDisplay();

        strData = new StrData();

        if (gamePlayerInfoList != null)
        {
            foreach (var player in gamePlayerInfoList)
            {
                Destroy(player.gameObject);
            }
        }

        gamePlayerInfoList = new List<GamePlayerInfo>();
        buyChipsView.gameObject.SetActive(false);
        BattleResultView.gameObject.SetActive(false);
        BackToSit_Btn.gameObject.SetActive(false);
        RuleView.SetActive(false);
        TotalPot_Txt.text = StringUtils.SetChipsUnit(0);

        //選單玩家訊息
        StringUtils.StrExceedSize(DataManager.UserWalletAddress, MenuWalletAddr_Txt);
        MenuNickname_Txt.text = $"@{DataManager.UserNickname}";
        MenuAvatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];

        SetNotReadChatCount = 0;

        Init();
        GameInit();
    }

    private void Start()
    {
        OtherChatSample.SetActive(false);
        LocalChatSample.SetActive(false);
        NewMessage_Btn.gameObject.SetActive(false);
        Mask_Btn.gameObject.SetActive(false);
        MenuPage_Tr.gameObject.SetActive(false);
        ChatPage_Tr.gameObject.SetActive(false);
        HandHistoryPage_Tr.gameObject.SetActive(false);

        //清除座位上玩家
        for (int i = 1; i < SeatGamePlayerInfoList.Count; i++)
        {
            SeatGamePlayerInfoList[i].gameObject.SetActive(false);
        }

        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);


        #region 遊戲測試

        IsOpenGameTestObj = false;
        CP_SuitTogList.AddRange(PH0_SuitTogList);
        CP_SuitTogList.AddRange(PH1_SuitTogList);
        List<string> suitName = new()
        {
            "C",//梅花
            "D",//方塊
            "H",//紅心
            "S",//黑桃
        };
        for (int i = 0; i < CP_SuitTogList.Count; i++)
        {
            Utils.SetOptionsToDropdown(CP_SuitTogList[i], suitName);
        }

        List<string> numName = new()
        {
            "A","2","3","4","5","6","7","8","9","10","J","Q","K",
        };
        CP_NumTogList.AddRange(PN0_NumTogList);
        CP_NumTogList.AddRange(PN1_NumTogList);
        for (int i = 0; i < CP_NumTogList.Count; i++)
        {
            Utils.SetOptionsToDropdown(CP_NumTogList[i], numName);
        }

        #endregion
    }

    private void Update()
    {
        //發送聊天訊息
        if ((Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter)) &&
            ChatPage_Tr.gameObject.activeSelf &&
            !string.IsNullOrEmpty(Chat_If.text))
        {
            SendChat();
            Chat_If.ActivateInputField();
            Chat_If.Select();
        }
    }

    /// <summary>
    /// 是否開啟遊戲測試操作
    /// </summary>
    public bool IsOpenGameTestObj
    {
        set
        {
            GameTest_Obj.SetActive(value);

            foreach (var item in PlayerTestObjList)
            {
                item.SetActive(false);
            }

            if (gameRoomData != null &&
                gameRoomData.playerDataDic != null)
            {
                foreach (var item in gameRoomData.playerDataDic)
                {
                    PlayerTestObjList[item.Value.gameSeat].SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName"></param>
    public void PlaySound(string soundName)
    {
        audioPool.PlaySound(soundName);
    }

    /// <summary>
    /// 遊戲暫停
    /// </summary>
    public bool GamePause
    {
        set
        {
            GamePause_Obj.SetActive(value);
        }
    }

    /// <summary>
    /// 關閉選單
    /// </summary>
    private void CloseMenu()
    {
        Mask_Btn.gameObject.SetActive(false);
        StartCoroutine(UnityUtils.Instance.IViewSlide(false,
                                                      MenuPage_Tr,
                                                      DirectionEnum.Left,
                                                      PageMoveTime,
                                                      () =>
                                                      {
                                                          GameRoomManager.Instance.IsCanMoveSwitch = true;
                                                      }));
    }

    /// <summary>
    /// 設置離/回座顯示
    /// </summary>
    private void SetSitOutDisplay()
    {
        SitOutBtn_Txt.text = thisData.IsSitOut ?
                             $"{LanguageManager.Instance.GetText("Back To Sit")}" :
                             $"{LanguageManager.Instance.GetText("Sit out next hand")}";

        if (!thisData.IsPlaying)
        {
            BackToSit_Btn.gameObject.SetActive(thisData.IsSitOut);
            WaitingTip_Txt.text = thisData.IsSitOut == false ?
                           "" :
                           $"{LanguageManager.Instance.GetText("Waiting for the next round...")}";
        }
    }

    /// <summary>
    /// 設定加註至文字
    /// </summary>
    public double SetRaiseToText
    {
        set
        {
            if (value >= thisData.LocalPlayerChips)
            {
                //All In
                CurrRaise_Txt.text = LanguageManager.Instance.GetText("AllIn");
                RaiseSliHandle_Txt.text = LanguageManager.Instance.GetText("AllIn");
            }
            else
            {
                CurrRaise_Txt.text = StringUtils.SetChipsUnit(value);
                RaiseSliHandle_Txt.text = StringUtils.SetChipsUnit(value);
            }
        }
    }

    /// <summary>
    /// 設置行動按鈕文字(是否為玩家回合)
    /// </summary>
    public bool SetActionButton
    {
        set
        {
            thisData.isLocalPlayerTurn = value;
            if (SetActingButtonEnable == true)
            {
                if (value == false)
                {
                    Raise_Tr.gameObject.SetActive(false);

                    if (gameControl.GetLocalPlayer() != null &&
                        gameRoomData != null &&
                        (gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind))
                    {
                        strData.FoldStr = "Fold";
                    }
                    else
                    {
                        strData.FoldStr = "CheckOrFold";
                    }

                    FoldBtn_Txt.text = LanguageManager.Instance.GetText(strData.FoldStr);
                    if (gameControl.GetLocalPlayer() != null &&
                        gameControl.GetLocalPlayer().currAllBetChips < gameRoomData.smallBlind * 2 &&
                        gameRoomData != null &&
                        gameRoomData.currCallValue <= gameRoomData.smallBlind * 2 &&
                        (gameRoomData.currGameFlow == (int)GameFlowEnum.Licensing || gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind))
                    {
                        GameRoomPlayerData local = gameControl.GetLocalPlayer();
                        if (local != null)
                        {
                            switch ((SeatCharacterEnum)local.seatCharacter)
                            {
                                case SeatCharacterEnum.SB:
                                    strData.CallStr = "Call";
                                    strData.CallValueStr = $"\n{gameRoomData.smallBlind.ToString()}";
                                    CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
                                    break;
                                case SeatCharacterEnum.BB:
                                    strData.CallStr = "Check";
                                    strData.CallValueStr = "";
                                    CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
                                    break;
                                default:
                                    strData.CallStr = "Call";
                                    strData.CallValueStr = $"\n{(gameRoomData.smallBlind * 2).ToString()}" ;
                                    CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        strData.CallStr = "Check";
                        strData.CallValueStr = "";
                        CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
                    }
                    strData.RaiseStr = "CallAny";
                    strData.RaiseValueStr = "";
                    RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr) + strData.RaiseValueStr;
                }
                else
                {
                    strData.FoldStr = "Fold";
                    FoldBtn_Txt.text = LanguageManager.Instance.GetText(strData.FoldStr);
                }
            }
        }
    }

    /// <summary>
    /// 行動按鈕激活
    /// </summary>
    public bool SetActingButtonEnable
    {
        get
        {
            return Raise_Btn.interactable;
        }
        set
        {
            Raise_Btn.interactable = value;
            Call_Btn.interactable = value;
            Fold_Btn.interactable = value;
        }
    }

    private AutoActingEnum autoActingEnum;
    /// <summary>
    /// 自動操作
    /// </summary>
    public enum AutoActingEnum
    {
        None,
        CallAny,
        Check,
        CheckAndFold,
    }

    /// <summary>
    /// 自動操作狀態
    /// </summary>
    public AutoActingEnum AutoActionState
    {
        get
        {
            return autoActingEnum;
        }
        set
        {
            autoActingEnum = value;
            switch (value)
            {
                case AutoActingEnum.None:
                    SetAutoAction(false);
                    break;
                case AutoActingEnum.CallAny:
                    SetAutoAction(true, Raise_Btn.transform);
                    break;
                case AutoActingEnum.Check:
                    SetAutoAction(true, Call_Btn.transform);
                    break;
                case AutoActingEnum.CheckAndFold:
                    SetAutoAction(true, Fold_Btn.transform);
                    break;
            }
        }
    }

    /// <summary>
    /// 自動操作設定
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="parent"></param>
    private void SetAutoAction(bool isActive, Transform parent = null)
    {
        AutoActionFrame_Tr.gameObject.SetActive(isActive);
        if (parent != null)
        {
            AutoActionFrame_Tr.SetParent(parent);
            AutoActionFrame_Tr.anchoredPosition = Vector2.zero;
            AutoActionFrame_Tr.offsetMax = Vector2.zero;
            AutoActionFrame_Tr.offsetMin = Vector2.zero;
        }
    }

    /// <summary>
    /// 設置底池顯示
    /// </summary>
    public bool SetPotActive
    {
        set
        {
            Pot_Img.enabled = value;
            Pot_Img.rectTransform.anchoredPosition = InitPotPointPos;
        }
    }

    /// <summary>
    /// 設置底池籌碼
    /// </summary>
    public double SetTotalPot
    {
        set
        {
            if (TotalPot_Txt.text != StringUtils.SetChipsUnit(value))
            {
                StringUtils.ChipsChangeEffect(TotalPot_Txt, Math.Floor(value));
            }
            thisData.TotalPot = value;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        WaitingTip_Txt.text = $"{LanguageManager.Instance.GetText("Waiting for the next round...")}";
        strData.FoldStr = "Fold";
        FoldBtn_Txt.text = LanguageManager.Instance.GetText(strData.FoldStr);
        strData.CallStr = "Check";
        strData.CallValueStr = "";
        CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;
        strData.RaiseStr = "CallAny";
        strData.RaiseValueStr = "";
        RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr) + strData.RaiseValueStr;
        SetTotalPot = 0;

        Raise_Tr.gameObject.SetActive(false);
    }

    /// <summary>
    /// 遊戲初始化
    /// </summary>
    public void GameInit()
    {
        foreach (var poker in CommunityPokerList)
        {
            poker.gameObject.SetActive(false);
            poker.SetColor = 1;
        }
        foreach (var player in gamePlayerInfoList)
        {
            player.SetPokerShapeTxtStr = "";
            player.IsWinnerActive = false;
            player.SetBackChips = 0;
            player.GetHandPoker[0].gameObject.SetActive(false);
            player.GetHandPoker[1].gameObject.SetActive(false);
            player.IsOpenInfoMask = true;
            player.IsPlaying = false;
            player.SetSeatCharacter(SeatCharacterEnum.None);
        }
        foreach (var show in ShowPokerBtnList)
        {
            show.gameObject.SetActive(false);
        }

        SetTotalPot = 0;
        WinType_Txt.text = "";
        SetPotActive = false;
        SetActionButton = false;
        AutoActionState = AutoActingEnum.None;
        Fold_Btn.gameObject.SetActive(true);
        Call_Btn.gameObject.SetActive(true);
        Raise_Btn.gameObject.SetActive(true);

        thisData.IsPlaying = false;
        thisData.isFold = false;
        thisData.CurrCommunityPoker = new List<int>();
    }

    /// <summary>
    /// 發送更新房間
    /// </summary>
    public void SendRequest_UpdateRoomInfo()
    {
        baseRequest.SendRequest_UpdateRoomInfo();
    }

    /// <summary>
    /// 棄牌
    /// </summary>
    private void OnFold()
    {
        gameControl.UpdateBetAction(DataManager.UserId,
                                    BetActingEnum.Fold,
                                    0);
    }

    /// <summary>
    /// 跟注/過牌
    /// </summary>
    private void OnCallAndCheck()
    {
        double betValue = 0;
        BetActingEnum acting = BetActingEnum.Call;

        if (thisData.IsFirstRaisePlayer)
        {
            if (thisData.CurrCallValue == thisData.SmallBlindValue * 2)
            {
                acting = BetActingEnum.Check;
            }
            else
            {
                betValue = thisData.CurrCallValue;
            }
        }
        else
        {
            if (thisData.LocalPlayerCurrBetValue == thisData.CurrCallValue)
            {
                acting = BetActingEnum.Check;
            }
            else if (thisData.LocalPlayerChips <= thisData.CurrCallValue)
            {
                acting = BetActingEnum.AllIn;
                betValue = thisData.LocalPlayerChips;
            }
            else
            {
                betValue = thisData.CurrCallValue;
            }
        }

        AutoActionState = AutoActingEnum.None;

        gameControl.UpdateBetAction(DataManager.UserId,
                                    acting,
                                    betValue);
    }

    /// <summary>
    /// 設定顯示棄牌手牌
    /// </summary>
    /// <param name="index"></param>
    private void SetShowFoldPoker(int index)
    {
        ShowPokerBtnList[index].gameObject.SetActive(false);

        GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                                  .FirstOrDefault()
                                                                  .Value;

        GamePlayerInfo playerInfo = GetPlayer(playerData.userId);
        playerInfo.OpenLocalShowHandPoker(index, playerData.handPoker[index]);

        List<int> showHandPoker = playerData.showHandPoker;
        showHandPoker[index] = playerData.handPoker[index];

        //更新玩家資料
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.SHOW_HAND_POKER, showHandPoker},         //棄牌後顯示手牌
        };
        gameControl.UpdataPlayerData(DataManager.UserId,
                                     data);

    }

    /// <summary>
    /// 顯示棄牌手牌
    /// </summary>
    public void ShowFoldPoker()
    {
        foreach (var player in gameRoomData.playerDataDic.Values)
        {
            GamePlayerInfo gamePlayerInfo = GetPlayer(player.userId);

            if ((PlayerStateEnum)player.gameState == PlayerStateEnum.Fold)
            {
                if (player.userId != DataManager.UserId)
                {
                    List<int> showPoker = player.showHandPoker;                    
                    gamePlayerInfo.SetShowHandPoker(true, showPoker);
                }
            }
        }
    }

    /// <summary>
    /// 底池百分比加注
    /// </summary>
    /// <param name="btnIndex"></param>
    private void PotRaisePercent(int btnIndex)
    {
        bool isBlindFirst = false;
        if ((GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.SetBlind &&
            gameRoomData.playingPlayersIdList.Count > 3 &&
            gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.SB &&
            gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.BB)
        {
            isBlindFirst = true;
        }

        float raiseValue = isBlindFirst ?
                          ((float)(gameRoomData.smallBlind * 2) * PotBbRate[btnIndex]) :
                          ((float)gameRoomData.potChips * (PotPercentRate[btnIndex] / 100));
        if (btnIndex == 3)
        {
            raiseValue = (float)gameRoomData.potChips;
        }
        else
        {
            raiseValue = isBlindFirst ?
                          ((float)(gameRoomData.smallBlind * 2) * PotBbRate[btnIndex]) :
                          ((float)gameRoomData.potChips * (PotPercentRate[btnIndex] / 100));
        }

        Raise_Sli.value = (int)raiseValue;
    }

    /// <summary>
    /// 輪到本地玩家檢查下注區域狀態
    /// </summary>
    public void CheckActionArea(GameRoomData gameRoomData)
    {
        //顯示異常
        if (FoldBtn_Txt.text == "CheckOrFold")
        {
            LocalPlayerRound(gameRoomData);
        }
    }

    /// <summary>
    /// 本地玩家回合
    /// </summary>
    /// <param name="gameRoomData"></param>
    public void LocalPlayerRound(GameRoomData gameRoomData)
    {
        GameRoomPlayerData gameRoomPlayerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                                          .FirstOrDefault()
                                                                          .Value;

        //首位加注玩家
        bool isFirst = gameRoomData.actionPlayerCount == 0;

        //是否無法加注
        double allInMin = gameControl.GetAllInPlayer().Count > 0 ?
                          gameControl.GetAllInPlayer().Min(x => x.currAllBetChips) :
                          0;
        bool IsUnableRaise =(gameControl.GetAllInPlayer().Count() > 0 && gameRoomData.playingPlayersIdList != null &&
                             gameControl.GetAllInPlayer().Count() == gameRoomData.playingPlayersIdList.Count - 1) ||
                            (gameRoomData.currCallValue == allInMin &&
                             gameRoomData.betActionDataDic != null &&
                            (BetActingEnum)gameRoomData.betActionDataDic.betAction == BetActingEnum.Call);

        //是否無法跟注
        bool isCanCall = true;
        if (gameRoomData.currCallValue == gameRoomData.smallBlind &&
            gameRoomData.currGameFlow > (int)GameFlowEnum.SetBlind)
        {
            isCanCall = false;
        }

        SetActionButton = true;

        //當前小盲值
        thisData.SmallBlindValue = gameRoomData.smallBlind;
        //當前底池
        thisData.TotalPot = gameRoomData.potChips;
        //玩家籌碼
        thisData.LocalPlayerChips = gameRoomPlayerData.carryChips;
        //首位加注玩家
        thisData.IsFirstRaisePlayer = isFirst;
        //當前跟注值
        thisData.CurrCallValue = gameRoomData.currCallValue;
        //跟注差額
        thisData.CallDifference = gameRoomData.currCallValue - gameRoomPlayerData.currAllBetChips;
        //玩家當前下注值
        thisData.LocalPlayerCurrBetValue = gameRoomPlayerData.currAllBetChips;
        //無法加注
        thisData.IsUnableRaise = IsUnableRaise;
        //是否無法跟注
        thisData.isCanCall = isCanCall;
        //最小加注
        thisData.MinRaiseValue = isFirst ?
                                 thisData.CurrCallValue :
                                 thisData.CurrCallValue * 2;
        thisData.CurrRaiseValue = thisData.MinRaiseValue;


        if (AutoActionState != AutoActingEnum.None)
        {
            StartCoroutine(JudgeAutoAction());
            return;
        }

        ShowBetArea();
    }

    /// <summary>
    /// 自動操作判斷
    /// </summary>
    private IEnumerator JudgeAutoAction()
    {
        yield return new WaitForSeconds(0.2f);

        switch (AutoActionState)
        {
            //任何跟注
            case AutoActingEnum.CallAny:
                OnCallAndCheck();
                break;

            //過牌
            case AutoActingEnum.Check:
                if (thisData.IsFirstRaisePlayer == true)
                {
                    OnCallAndCheck();
                }
                else
                {
                    if (thisData.LocalPlayerCurrBetValue == thisData.CurrCallValue)
                    {
                        OnCallAndCheck();
                    }
                    else if (thisData.LocalPlayerCurrBetValue < thisData.SmallBlindValue * 2 &&
                             gameRoomData.currCallValue <= thisData.SmallBlindValue * 2 &&
                             gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind)
                    {
                        OnCallAndCheck();
                    }
                    else
                    {
                        ShowBetArea();
                    }
                }
                break;

            //過牌或棄牌
            case AutoActingEnum.CheckAndFold:
                if (thisData.IsFirstRaisePlayer == true)
                {
                    if (thisData.CurrCallValue <= thisData.SmallBlindValue * 2)
                    {
                        OnCallAndCheck();
                    }
                    else
                    {
                        OnFold();
                    }
                }
                else
                {
                    if (gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind)
                    {
                        OnFold();
                    }
                    else if (thisData.LocalPlayerCurrBetValue == thisData.CurrCallValue)
                    {
                        OnCallAndCheck();
                    }
                    else if (thisData.CurrCallValue <= thisData.SmallBlindValue * 2)
                    {
                        OnCallAndCheck();
                    }
                    else
                    {
                        OnFold();
                    }
                }
                break;
        }


        yield return null;

        AutoActionState = AutoActingEnum.None;
    }

    /// <summary>
    /// 顯示下注區塊
    /// </summary>
    private void ShowBetArea()
    {
        //是否無法在加注
        bool IsUnableRaise = thisData.IsUnableRaise;
        bool isJustAllIn = thisData.LocalPlayerChips <= thisData.CurrCallValue;
        bool isCanCall = thisData.isCanCall;

        //棄牌
        Fold_Btn.gameObject.SetActive(true);

        //加注&All In
        strData.RaiseStr = isJustAllIn == true ?
                           "AllIn" :
                           "Raise";
        strData.RaiseValueStr = isJustAllIn == true ?
                                $"\n${StringUtils.SetChipsUnit(thisData.LocalPlayerChips)}" :
                                "";
        RaiseBtn_Txt.text = LanguageManager.Instance.GetText(strData.RaiseStr);

        if (IsUnableRaise == true && isJustAllIn == false)
        {
            Raise_Btn.gameObject.SetActive(false);
        }
        else
        {
            Raise_Btn.gameObject.SetActive(true);
        }

        //跟注&過牌
        strData.CallStr = "Call";
        strData.CallValueStr = $"\n{StringUtils.SetChipsUnit(thisData.CurrCallValue - thisData.CallDifference)}";
        if (thisData.IsFirstRaisePlayer == true)
        {
            strData.CallStr = "Check";
            strData.CallValueStr = "";
        }
        else
        {
            if (thisData.LocalPlayerCurrBetValue == thisData.CurrCallValue)
            {
                strData.CallStr = "Check";
                strData.CallValueStr = "";
            }
            else
            {
                strData.CallStr = "Call";
                strData.CallValueStr = $"\n{StringUtils.SetChipsUnit(thisData.CallDifference)}";
            }
        }
        CallBtn.text = LanguageManager.Instance.GetText(strData.CallStr) + strData.CallValueStr;

        if (IsUnableRaise == true && isJustAllIn == false && isCanCall == true)
        {
            Call_Btn.gameObject.SetActive(true);
        }
        else
        {
            Call_Btn.gameObject.SetActive(isJustAllIn == false);
        }

        //加注區域物件
        Raise_Tr.gameObject.SetActive(false);
        if (isJustAllIn == false)
        {
            //倍數
            float multiple = (int)Mathf.Ceil((float)thisData.LocalPlayerChips / (float)(thisData.SmallBlindValue * 2));
            Raise_Sli.maxValue = (float)(thisData.SmallBlindValue * 2) * multiple;
            Raise_Sli.minValue = 0;
            Raise_Sli.value = 0;
            Raise_Sli.value = (float)thisData.MinRaiseValue;

            //加注值
            SetRaiseToText = thisData.MinRaiseValue;

            //最小加注值
            MinRaiseBtn_Txt.text = thisData.MinRaiseValue.ToString(); ;

            //底池倍率
            for (int i = 0; i < 4; i++)
            {
                if (gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.SB &&
                    gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.BB &&
                    gameRoomData.playingPlayersIdList.Count > 3)
                {
                    PotPercentRaiseTxtList[i].text = $"{PotBbRate[i]}BB";
                }
                else
                {
                    PotPercentRaiseTxtList[i].text = $"{PotPercentRate[i]}%";
                }
            }
            if (gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.SB &&
                gameControl.GetLocalPlayer().seatCharacter != (int)SeatCharacterEnum.BB &&
                gameRoomData.playingPlayersIdList.Count > 3)
            {
                PotPercentRaiseTxtList[3].text = LanguageManager.Instance.GetText("Pot");
            }
            else
            {
                PotPercentRaiseTxtList[3].text = $"{PotPercentRate[3]}%";
            }
        }
    }

    /// <summary>
    /// 籌碼不足
    /// </summary>
    /// <param name="pack"></param>
    public void OnNotEnoughChips(MainPack pack)
    {
        string id = pack.PlayerInOutRoomPack.PlayerInfoPack.UserID;
        if (id == Entry.TestInfoData.LocalUserId)
        {

        }
        else
        {
            //PlayerExitRoom(id);
        }
    }

    /// <summary>
    /// 更新遊戲房間訊息
    /// </summary>
    /// <param name="gameRoomData">遊戲房間資料</param>
    public void UpdateGameRoomInfo(GameRoomData gameRoomData)
    {
        //清除座位上玩家
        for (int i = 1; i < SeatGamePlayerInfoList.Count; i++)
        {
            SeatGamePlayerInfoList[i].gameObject.SetActive(false);
        }
        gamePlayerInfoList = new List<GamePlayerInfo>();

        GameRoomPlayerData localData = gameControl.GetLocalPlayer();

        if (localData == null)
        {
            return;
        }

        //本地玩家座位
        thisData.LocalPlayerSeat = localData.gameSeat;

        //更新玩家訊息
        foreach (var player in gameRoomData.playerDataDic.Values)
        {
            GamePlayerInfo gamePlayerInfo = AddPlayer(player, gameRoomData);

            gamePlayerInfo.CloseChatInfo();
            if (player.userId != DataManager.UserId &&
                gameRoomData.playingPlayersIdList != null &&
                gameRoomData.playingPlayersIdList.Count() >= 2 &&
                gameRoomData.playingPlayersIdList.Contains(player.userId))
            {
                gamePlayerInfo.SetPokerShapeTxtStr = "";
                gamePlayerInfo.SetHandPoker(-1, -1);
            }
            else
            {
                //本地玩家

                //沒有離座/非等待
                if (player.isSitOut == false &&
                   (PlayerStateEnum)player.gameState != PlayerStateEnum.Waiting &&
                   (PlayerStateEnum)player.gameState != PlayerStateEnum.Fold)
                {
                    thisData.IsPlaying = true;

                    WaitingTip_Txt.text = "";
                    gamePlayerInfo.IsOpenInfoMask = false;

                    //判斷牌行
                    if (gameRoomData.playingPlayersIdList.Contains(DataManager.UserId))
                    {
                        gamePlayerInfo.SetHandPoker(player.handPoker[0],
                                                    player.handPoker[1]);
                        JudgePokerShape(gamePlayerInfo,
                                        true);
                    }
                }

                WaitingTip_Txt.text = (PlayerStateEnum)player.gameState != PlayerStateEnum.Waiting ?
                                      "" :
                                      $"{LanguageManager.Instance.GetText("Waiting for the next round...")}";



                if ((PlayerStateEnum)player.gameState == PlayerStateEnum.Waiting)
                {
                    gamePlayerInfo.SetPokerShapeTxtStr = "";
                    gamePlayerInfo.GetHandPoker[0].gameObject.SetActive(false);
                    gamePlayerInfo.GetHandPoker[1].gameObject.SetActive(false);
                }
            }

            if (player.userId == gameRoomData.currActionerId &&
                gameRoomData.currGameFlow > (int)GameFlowEnum.Licensing &&
                gameRoomData.actionCD > 0)
            {
                gamePlayerInfo.ActionFrame = true;
                gamePlayerInfo.CountDown(DataManager.StartCountDownTime,
                                         gameRoomData.actionCD);
            }
        }

        //底池
        if (gameRoomData.currGameFlow != (int)GameFlowEnum.PotResult &&
            gameRoomData.currGameFlow != (int)GameFlowEnum.SideResult)
        {
            TotalPot_Txt.text = StringUtils.SetChipsUnit(Math.Floor(gameRoomData.potChips));
            thisData.TotalPot = gameRoomData.potChips;
        }

        //公共牌
        List<int> currCommunityPoker = gameRoomData.currCommunityPoker;
        if (currCommunityPoker != null)
        {
            for (int i = 0; i < currCommunityPoker.Count; i++)
            {
                CommunityPokerList[i].gameObject.SetActive(true);
                CommunityPokerList[i].PokerNum = currCommunityPoker[i];
            }
        }
    }

    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="playerData"></param>
    /// <param name="gameRoomData"></param>
    /// <returns></returns>
    public GamePlayerInfo AddPlayer(GameRoomPlayerData playerData, GameRoomData gameRoomData)
    {       
        GamePlayerInfo gamePlayerInfo = null;
        int seatIndex = 0;//座位(本地玩家 = 0)
        if (playerData.userId != DataManager.UserId)
        {
            if (RoomType == TableTypeEnum.IntegralTable)
            {
                seatIndex = 3;
            }
            else
            {
                seatIndex = playerData.gameSeat > thisData.LocalPlayerSeat ?
                            playerData.gameSeat - thisData.LocalPlayerSeat :
                            SeatButtonList.Count - (thisData.LocalPlayerSeat - playerData.gameSeat);

            }

            gamePlayerInfo = SeatGamePlayerInfoList[seatIndex];
            SeatButtonList[seatIndex].image.enabled = false;
        }
        else
        {
            //本地玩家
            gamePlayerInfo = SeatGamePlayerInfoList[0];
            thisData.LocalGamePlayerInfo = gamePlayerInfo;
        }

        if (playerData.gameState == (int)PlayerStateEnum.Waiting ||
            playerData.gameState == (int)PlayerStateEnum.Fold)
        {
            gamePlayerInfo.IsOpenInfoMask = true;
        }

        gamePlayerInfo.gameObject.SetActive(true);
        gamePlayerInfo.ActionFrame = false;
 
        gamePlayerInfo.GetHandPoker[0].gameObject.SetActive(playerData.gameState == (int)PlayerStateEnum.Playing ||
                                                            playerData.gameState == (int)PlayerStateEnum.AllIn);
        gamePlayerInfo.GetHandPoker[1].gameObject.SetActive(playerData.gameState == (int)PlayerStateEnum.Playing ||
                                                            playerData.gameState == (int)PlayerStateEnum.AllIn);


        gamePlayerInfo.SwitchBetChipsActive = playerData.currAllBetChips > 0;
        if (playerData.gameState == (int)PlayerStateEnum.Waiting)
        {
            gamePlayerInfo.DisplayBetAction(false);
        }

        if (playerData.gameSeat == gameRoomData.buttonSeat)
        {
            gamePlayerInfo.SetSeatCharacter(SeatCharacterEnum.Button);
        }
        gamePlayerInfo.SetSeatCharacter((SeatCharacterEnum)playerData.seatCharacter);
        gamePlayerInfo.SetInitPlayerInfo(seatIndex,
                                         playerData.userId,
                                         playerData.nickname,
                                         playerData.carryChips,
                                         playerData.avatarIndex);

        gamePlayerInfoList.Add(gamePlayerInfo);
        return gamePlayerInfo;
    }

    /// <summary>
    /// 有玩家退出房間
    /// </summary>
    /// <param name="id">退出玩家ID</param>
    /// <returns></returns>
    public GamePlayerInfo PlayerExitRoom(string id)
    {
        GamePlayerInfo exitPlayer = GetPlayer(id);

        gamePlayerInfoList.Remove(exitPlayer);

        exitPlayerSeatList.Add(exitPlayer.SeatIndex);

        exitPlayer.gameObject.SetActive(false);

        if (RoomType == TableTypeEnum.IntegralTable)
        {
            SetBattleResult(true);
        }

        return exitPlayer;
    }

    /// <summary>
    /// 獲取玩家
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GamePlayerInfo GetPlayer(string id)
    {
        return gamePlayerInfoList.Where(x => x.UserId == id).FirstOrDefault();
    }

    /// <summary>
    /// 接收玩家行動
    /// </summary>
    /// <param name="gameRoomData"></param>
    public void GetPlayerAction(GameRoomData gameRoomData)
    {
        string id = gameRoomData.betActionDataDic.betActionerId;
        BetActingEnum actionEnum = (BetActingEnum)gameRoomData.betActionDataDic.betAction;
        double betValue = gameRoomData.betActionDataDic.betActionValue;
        double chips = gameRoomData.betActionDataDic.updateCarryChips;
        bool isLocalPlayer = id == DataManager.UserId;

        //音效播放
        switch (actionEnum)
        {
            case BetActingEnum.Blind:
                PlaySound("SoundBet");
                break;
            case BetActingEnum.Fold:
                PlaySound("SoundFold");
                break;
            case BetActingEnum.Check:
                PlaySound("SoundCheck");
                break;
            case BetActingEnum.Raise:
                PlaySound("SoundRaise");
                break;
            case BetActingEnum.Bet:
                PlaySound("SoundBet");
                break;
            case BetActingEnum.Call:
                PlaySound("SoundCall");
                break;
            case BetActingEnum.AllIn:
                PlaySound("SoundGatherChips");
                break;
        }

        //本地玩家
       // SetActionButton = isLocalPlayer;
        if (isLocalPlayer)
        {
            switch (actionEnum)
            {
                //棄牌
                case BetActingEnum.Fold:
                    SetAutoAction(false);
                    SetActingButtonEnable = false;
                    Raise_Tr.gameObject.SetActive(false);
                    thisData.isFold = true;
                    thisData.IsPlaying = false;
                    break;

                //All In
                case BetActingEnum.AllIn:
                    SetActingButtonEnable = false;
                    break;
            }
        }

        GamePlayerInfo playerInfo = GetPlayer(id);
        if (playerInfo != null &&
            playerInfo.gameObject.activeSelf)
        {
            playerInfo.PlayerAction(actionEnum,
                                    betValue,
                                    chips);

            playerInfo.InitCountDown();
        }

        //本地玩家有參與
        if (thisData.LocalGamePlayerInfo.IsPlaying &&
            playerInfo != null)
        {
            //紀錄存檔
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = playerInfo.SeatIndex;
            processStepHistoryData.ActionIndex = (int)actionEnum;

            processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }
    }

    /// <summary>
    /// 下注籌碼集中
    /// </summary>
    /// <returns></returns>
    private IEnumerator IConcentrateBetChips()
    {
        for (int i = 0; i < gamePlayerInfoList.Count; i++)
        {
            if (gamePlayerInfoList[i].GetBetChipsActive == true)
            {
                yield return new WaitForSeconds(1);

                foreach (var player in gamePlayerInfoList)
                {
                    player.ConcentrateBetChips(Pot_Img.transform.position);
                }

                yield return new WaitForSeconds(0.5f);

                break;
            }
        }

        //顯示底池籌碼
        SetPotActive = true;
    }

    /// <summary>
    /// 設置Button座位物件
    /// </summary>
    /// <param name="id"></param>
    public void SetButtonSeat(string id)
    {
        GamePlayerInfo buttonPlayer = gamePlayerInfoList.Where(x => x.UserId == id).FirstOrDefault();
        buttonPlayer.SetSeatCharacter(SeatCharacterEnum.Button);

        if (gamePlayerInfoList.Count >= 3)
        {
            int buttonSeat = gamePlayerInfoList.Select((v, i) => (v, i))
                                               .Where(x => x.v.UserId == id)
                                               .FirstOrDefault().i;

            int sb = buttonSeat + 1 < gamePlayerInfoList.Count ?
                     buttonSeat + 1 :
                     0;

            int bb = buttonSeat + 2 < gamePlayerInfoList.Count ?
                     buttonSeat + 2 :
                     0;

            gamePlayerInfoList[sb].SetSeatCharacter(SeatCharacterEnum.SB);
            gamePlayerInfoList[bb].SetSeatCharacter(SeatCharacterEnum.BB);
        }
    }

    /// <summary>
    /// 設定大小盲
    /// </summary>
    /// <param name="blindStagePack"></param>
    private void SetBlind(BlindStagePack blindStagePack)
    {
        //小盲
        GamePlayerInfo sbPlayer = GetPlayer(blindStagePack.SBPlayerId);
        sbPlayer.PlayerAction(BetActingEnum.Blind,
                              thisData.SmallBlindValue,
                              blindStagePack.SBPlayerChips);

        //大盲
        GamePlayerInfo bbPlayer = GetPlayer(blindStagePack.BBPlayerId);
        bbPlayer.PlayerAction(BetActingEnum.Blind,
                              thisData.SmallBlindValue * 2,
                              blindStagePack.BBPlayerChips);
    }

    /// <summary>
    /// 手牌發牌
    /// </summary>
    /// <param name="handPokerDic"></param>
    private void HandPokerLicensing(Dictionary<string, (int, int)> handPokerDic)
    {
        foreach (var dic in handPokerDic)
        {
            GamePlayerInfo player = gamePlayerInfoList.First(x => x.UserId == dic.Key);
            if (player.UserId == Entry.TestInfoData.LocalUserId)
            {
                thisData.IsPlaying = true;
                player.SetHandPoker(dic.Value.Item1,
                                    dic.Value.Item2);
                JudgePokerShape(player, true);
            }
            else
            {
                player.SetHandPoker(-1, -1);
            }
        }
    }

    /// <summary>
    /// 每輪回合開始初始
    /// </summary>
    private void RountInit()
    {
        foreach (var player in gamePlayerInfoList)
        {
            player.RountInit();
        }
    }

    /// <summary>
    /// 翻開公共牌
    /// </summary>
    /// <param name="currCommunityPoker"></param>
    /// <returns></returns>
    public IEnumerator IFlopCommunityPoker(List<int> currCommunityPoker)
    {
        foreach (var player in gamePlayerInfoList)
        {
            if (!player.IsFold && !player.IsAllIn)
            {
                player.CurrBetAction = BetActionEnum.None;
            }
        }

        if (currCommunityPoker != null)
        {
            thisData.CurrCommunityPoker = currCommunityPoker;
        }        
        else
        {
            currCommunityPoker = new List<int>();
        }

        //本地玩家
        GameRoomPlayerData localPlayer = gameControl.GetLocalPlayer();

        //本地玩家有參與
        if (((PlayerStateEnum)localPlayer.gameState == PlayerStateEnum.Playing || 
            (PlayerStateEnum)localPlayer.gameState == PlayerStateEnum.AllIn) &&
            currCommunityPoker.Count > 0)
        {
            //紀錄存檔
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = -1;
            processStepHistoryData.ActionIndex = -1;

            processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }

        yield return IConcentrateBetChips();

        if (currCommunityPoker != null)
        {
            for (int i = 0; i < currCommunityPoker.Count; i++)
            {
                if (CommunityPokerList[i].gameObject.activeSelf == false)
                {
                    PlaySound("SoundShowCard");
                    CommunityPokerList[i].gameObject.SetActive(true);
                    CommunityPokerList[i].PokerNum = currCommunityPoker[i];
                    StartCoroutine(CommunityPokerList[i].IHorizontalFlopEffect(currCommunityPoker[i]));
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        yield return new WaitForSeconds(0.6f);

        //判斷牌行
        if (thisData.IsPlaying == true)
        {
            GamePlayerInfo localPlayerInfo = gamePlayerInfoList.Where(x => x.UserId == DataManager.UserId)
                                                               .FirstOrDefault();
            JudgePokerShape(localPlayerInfo, 
                            true);
        }
    }

    /// <summary>
    /// 判斷牌型
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isOpenMatchPokerFrame">是否開啟符合的撲克框</param>
    /// <param name="isWinEffect">贏家效果</param>
    private void JudgePokerShape(GamePlayerInfo player, bool isOpenMatchPokerFrame, bool isWinEffect = false)
    {
        //手牌
        Poker[] handPoker = player.GetHandPoker;
        List<int> judgePoker = new List<int>();
        foreach (var poker in handPoker)
        {
            judgePoker.Add(poker.PokerNum);
        }

        if (judgePoker != null &&
            thisData.CurrCommunityPoker != null &&
            handPoker != null)
        {
            //公共牌
            judgePoker = judgePoker.Concat(thisData.CurrCommunityPoker).ToList();

            List<Poker> pokers = CommunityPokerList.Concat(handPoker.ToList()).ToList();

            //關閉公共牌撲克效果
            foreach (var poker in pokers)
            {
                poker.PokerEffectEnable = false;
            }

            //判斷牌型
            PokerShape.JudgePokerShape(judgePoker, (resultIndex, matchPokerList) =>
            {
                if (player.GetHandPoker[0].gameObject.activeSelf)
                {
                    player.SetPokerShapeStr(resultIndex);

                    if (isOpenMatchPokerFrame && resultIndex < 10)
                    {
                        PokerShape.OpenMatchPokerFrame(pokers,
                                                       matchPokerList,
                                                       isWinEffect);
                    }
                }
            });
        }
    }

    /// <summary>
    /// 主池結果
    /// </summary>
    /// <param name="gameRoomData"></param>
    /// <returns></returns>
    public IEnumerator IPotResult(GameRoomData gameRoomData)
    {
        thisData.IsPlaying = false;
        SetActingButtonEnable = false;
        thisData.CurrCommunityPoker = new List<int>();

        yield return IConcentrateBetChips();
        yield return IFlopCommunityPoker(gameRoomData.currCommunityPoker);

        //是否所有人都棄牌
        bool isOnePlayerLeft = gameRoomData.playingPlayersIdList.Count() - gameControl.GetFoldPlayer().Count() == 1;

        if (isOnePlayerLeft == false)
        {
            //顯示手牌牌型
            foreach (var playerId in gameRoomData.playingPlayersIdList)
            {
                GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == playerId)
                                                                          .FirstOrDefault()
                                                                          .Value;
                if ((PlayerStateEnum)playerData.gameState != PlayerStateEnum.Waiting &&
                    (PlayerStateEnum)playerData.gameState != PlayerStateEnum.Fold)
                {
                    GamePlayerInfo player = GetPlayer(playerId);
                    player.SetHandPoker(playerData.handPoker[0],
                                        playerData.handPoker[1]);

                    JudgePokerShape(player, false);
                }
            }
        }

        yield return new WaitForSeconds(1);

        thisData.PowWinChips = gameRoomData.potWinData.potWinChips;

        //開啟遮罩
        foreach (var player in gamePlayerInfoList)
        {
            player.IsOpenInfoMask = true;
        }

        //贏得類型顯示
        WinType_Txt.text = LanguageManager.Instance.GetText("Pot");
        SetTotalPot = gameRoomData.potWinData.potWinChips;

        //贏家效果
        foreach (var potWinnerId in gameRoomData.potWinData.potWinnersId)
        {
            //本地玩家
            if (potWinnerId == DataManager.UserId)
            {
                //更新用戶籌碼資料
                double changeValue = gameRoomData.potWinData.potWinChips / gameRoomData.potWinData.potWinnersId.Count();
                gameControl.UpdateLocalChips(changeValue);
            }

            CloseAllPokerEffect();

            GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == potWinnerId)
                                                                      .FirstOrDefault()
                                                                      .Value;
            GamePlayerInfo player = GetPlayer(potWinnerId);
            player.IsOpenInfoMask = false;

            JudgePokerShape(player, true, true);

            player.IsWinnerActive = true;

            Vector2 winnerSeatPos = player.gameObject.transform.position;
            
            //產生贏得籌碼物件            
            RectTransform rt = Instantiate(WinChipsObj, Pot_Img.transform).GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            SetPotActive = false;

            yield return new WaitForSeconds(0.5f);

            ObjMoveUtils.ObjMoveToTarget(rt, winnerSeatPos, 0.5f,
                                        () =>
                                        {
                                            PlaySound("SoundWinPot");
                                            player.PlayerRoomChips = playerData.carryChips;
                                            Destroy(rt.gameObject);
                                        });
            
            yield return new WaitForSeconds(2);
        }        

        //記錄存檔
        int winIndex = 0;
        foreach (var potWinnerId in gameRoomData.potWinData.potWinnersId)
        {
            winIndex++;
            //GamePlayerInfo player = GetPlayer(potWinnerId);
            GameRoomPlayerData playerData = gameControl.GetPlayerData(potWinnerId);

            if (playerData == null)
            {
                yield break;
            }

            //本地玩家有參與
            if (thisData.LocalGamePlayerInfo.IsPlaying)
            {
                //獲勝牌局紀錄存檔
                if (winIndex == 1)
                {
                    string roomName = "";
                    switch (roomType)
                    {
                        case TableTypeEnum.IntegralTable:
                            roomName = "Integral";
                            break;
                        case TableTypeEnum.Cash:
                            roomName = "High Roller Battleground";
                            break;
                        case TableTypeEnum.VCTable:
                            roomName = "Classic Battle";
                            break;
                    }

                    saveResultData = new ResultHistoryData();
                    saveResultData.RoomType = $"{roomName}";
                    saveResultData.SmallBlind = gameRoomData.smallBlind;
                    saveResultData.NickName = playerData.nickname;
                    saveResultData.Avatar = playerData.avatarIndex;
                    saveResultData.HandPokers = new int[] { playerData.handPoker[0],
                                                            playerData.handPoker[1] };
                    saveResultData.CommunityPoker = gameRoomData.currCommunityPoker == null ?
                                                    new List<int>() :
                                                    gameRoomData.currCommunityPoker;
                    saveResultData.WinChips = gameRoomData.potWinData.potWinChips;
                }
            }
        }

        //主池紀錄存檔
        if (thisData.LocalGamePlayerInfo.IsPlaying)
        {
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = -1;
            processStepHistoryData.ActionIndex = -1;

            processStepHistoryData.PotWinnerSeatList = new List<int>();
            foreach (var id in gameRoomData.potWinData.potWinnersId)
            {
                int potWinSeat = GetPlayer(id).SeatIndex;
                processStepHistoryData.PotWinnerSeatList.Add(potWinSeat);
            }
            processStepHistoryData.PotWinChips = thisData.PowWinChips;

            processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }

    }

    /// <summary>
    /// 邊池結果
    /// </summary>
    /// <param name="gameRoomData"></param>
    public IEnumerator SideResult(GameRoomData gameRoomData)
    {
        thisData.SideWinnerList = new List<string>();
        thisData.SideWinChips = gameRoomData.sideWinData.sideWinChips / gameRoomData.sideWinData.sideWinnersId.Count();

        //開啟遮罩
        foreach (var player in gamePlayerInfoList)
        {
            player.IsOpenInfoMask = true;
            player.IsWinnerActive = false;
        }

        //邊池贏家效果
        thisData.SideWinnerList = new List<string>();

        if (gameRoomData.sideWinData.sideWinChips > 0)
        {
            WinType_Txt.text = LanguageManager.Instance.GetText("Side");
            SetTotalPot = gameRoomData.sideWinData.sideWinChips;

            foreach (var sideWinnerId in gameRoomData.sideWinData.sideWinnersId)
            {
                //本地玩家
                if (sideWinnerId == DataManager.UserId)
                {
                    //更新用戶籌碼資料
                    double changeValue = gameRoomData.sideWinData.sideWinChips / gameRoomData.sideWinData.sideWinnersId.Count();
                    gameControl.UpdateLocalChips(changeValue);
                }

                CloseAllPokerEffect();

                GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == sideWinnerId)
                                                                          .FirstOrDefault()
                                                                          .Value;

                thisData.SideWinnerList.Add(sideWinnerId);

                GamePlayerInfo player = GetPlayer(sideWinnerId);

                player.IsOpenInfoMask = false;
                player.IsWinnerActive = true;

                Vector2 winnerSeatPos = player.gameObject.transform.position;
                JudgePokerShape(player, true, true);

                if (player.PlayerRoomChips != playerData.carryChips)
                {
                    //獲勝籌碼物件
                    RectTransform rt = Instantiate(WinChipsObj, Pot_Img.transform).GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;
                    yield return new WaitForSeconds(0.5f);
                    ObjMoveUtils.ObjMoveToTarget(rt, winnerSeatPos, 0.5f,
                                                () =>
                                                {
                                                    PlaySound("SoundWinPot");
                                                    player.PlayerRoomChips = playerData.carryChips;
                                                    Destroy(rt.gameObject);
                                                });
                }

                yield return new WaitForSeconds(2);

                //關閉撲克外框
                Poker[] handPoker = player.GetHandPoker;
                if (CommunityPokerList != null &&
                    handPoker != null)
                {
                    List<Poker> pokerList = CommunityPokerList.Concat(handPoker.ToList()).ToList();
                    foreach (var poker in pokerList)
                    {
                        poker.PokerEffectEnable = false;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        //顯示退回籌碼
        thisData.BackChipsDic = new Dictionary<int, double>();
        if (gameRoomData.sideWinData.backChipsData != null)
        {
            foreach (var backChipsData in gameRoomData.sideWinData.backChipsData.Values)
            {
                if (backChipsData.backChipsValue > 0)
                {
                    //本地玩家
                    if (backChipsData.backUserId == DataManager.UserId)
                    {
                        //更新用戶籌碼資料
                        double changeValue = backChipsData.backChipsValue;
                        gameControl.UpdateLocalChips(changeValue);
                    }

                    GamePlayerInfo player = GetPlayer(backChipsData.backUserId);
                    player.PlayerRoomChips = player.PlayerRoomChips + backChipsData.backChipsValue;
                    player.SetBackChips = backChipsData.backChipsValue;
                    thisData.BackChipsDic.Add(player.SeatIndex, backChipsData.backChipsValue);

                    GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == backChipsData.backUserId)
                                                                              .FirstOrDefault()
                                                                              .Value;

                    if (playerData != null)
                    {
                        player.PlayerRoomChips = playerData.carryChips;
                    }
                }
            }
        }

        //邊池紀錄存檔
        if (thisData.LocalGamePlayerInfo.IsPlaying &&
            thisData.SideWinnerList.Count > 0)
        {
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = -1;
            processStepHistoryData.ActionIndex = -1;

            processStepHistoryData.SildWinnerSeatList = new List<int>();
            foreach (var id in thisData.SideWinnerList)
            {
                int sideWinSeat = GetPlayer(id).SeatIndex;
                processStepHistoryData.SildWinnerSeatList.Add(sideWinSeat);
            }
            processStepHistoryData.SildWinChips = thisData.SideWinChips;
            processStepHistoryData.BackChipsDic = thisData.BackChipsDic;

            processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }
    }

    /// <summary>
    /// 關閉所有撲克效果
    /// </summary>
    private void CloseAllPokerEffect()
    {
        List<Poker> playersPoker = new List<Poker>();
        foreach (var p in gamePlayerInfoList)
        {
            foreach (var poker in p.GetHandPoker)
            {
                playersPoker.Add(poker);
            }
        }
        List<Poker> allPokerList = CommunityPokerList.Concat(playersPoker.ToList()).ToList();
        foreach (var poker in allPokerList)
        {
            poker.PokerEffectEnable = false;
        }
    }

    /// <summary>
    /// 判斷勝率
    /// </summary>
    private void JudgeWinRate()
    {
        DateTime startTime = DateTime.Now;
        List<int> judgeHand = thisData.LocalGamePlayerInfo.GetHandPoker.Select(x => x.PokerNum).ToList();
        PokerWinRateCalculator pokerWinRateCalculator = new PokerWinRateCalculator(judgeHand, thisData.CurrCommunityPoker);
        pokerWinRateCalculator.CalculateWinRate((winRate) =>
        {
            //Debug.Log($"Judge Win Rate Time : {(DateTime.Now - startTime).TotalSeconds}");
            //Debug.Log($"Win Rate : {winRate}");
        });
    }

    /// <summary>
    /// 遊戲階段
    /// </summary>
    /// <param name="gameRoomData">遊戲房間資料</param>
    /// <param name="smallBlind">小盲值</param>
    /// <returns></returns>
    public IEnumerator IGameStage(GameRoomData gameRoomData, double smallBlind)
    {
        AutoActionState = AutoActingEnum.None;
        thisData.SmallBlindValue = smallBlind;
        thisData.CurrRaiseValue = thisData.SmallBlindValue * 2;

        //重製玩家行動文字顯示
        if ((GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.PotResult)
        {
            //階段=遊戲結果
            foreach (var player in gamePlayerInfoList)
            {
                player.DisplayBetAction(false);
            }
        }
        else
        {
            //翻牌階段
            foreach (var player in gamePlayerInfoList)
            {
                if (!player.IsFold && !player.IsAllIn)
                {
                    player.DisplayBetAction(false);
                }
            }
        }

        //判斷當前遊戲進程
        switch ((GameFlowEnum)gameRoomData.currGameFlow)
        {
            //發牌
            case GameFlowEnum.Licensing:
                SavePreGame();
                //GameInit();

                //HandPokerLicensing(pack.LicensingStagePack.HandPokerDic);
                //SetButtonSeat(pack.LicensingStagePack.ButtonSeatId);
                SetSitOutDisplay();
                JudgeWinRate();
                break;

            //大小盲
            case GameFlowEnum.SetBlind:
                SetActingButtonEnable = thisData.IsPlaying;

               /* SetTotalPot = pack.GameRoomInfoPack.TotalPot;
                SetBlind(pack.BlindStagePack);*/

                
                break;

            //翻牌
            case GameFlowEnum.Flop:
               /* yield return IFlopCommunityPoker(pack.CommunityPokerPack.CurrCommunityPoker);
                RountInit();
                JudgeWinRate();*/
                break;

            //轉牌
            case GameFlowEnum.Turn:
               /* yield return IFlopCommunityPoker(pack.CommunityPokerPack.CurrCommunityPoker);
                RountInit();
                JudgeWinRate();*/
                break;

            //河牌
            case GameFlowEnum.River:
               /* yield return IFlopCommunityPoker(pack.CommunityPokerPack.CurrCommunityPoker);
                RountInit();
                JudgeWinRate();*/
                break;

            //主池結果
            case GameFlowEnum.PotResult:
                //棄牌顯示手牌按鈕
                if (thisData.isFold == true)
                {
                    foreach (var show in ShowPokerBtnList)
                    {
                        show.gameObject.SetActive(true);
                    }
                }
                //yield return IPotResult(pack);
                break;
        }

        yield return null;
    }

    /// <summary>
    /// 籌碼不足
    /// </summary>
    /// <param name="pack"></param>
    public void OnInsufficientChips()
    {
        thisData.IsPlaying = false;

        //正在觀看紀錄影片
        LobbyHandHistoryView handHistoryMainView = GameObject.FindAnyObjectByType<LobbyHandHistoryView>();
        if (handHistoryMainView != null)
        {
            Destroy(handHistoryMainView.gameObject);
        }
        HistoryVideoView historyVideoView = GameObject.FindAnyObjectByType<HistoryVideoView>();
        if (historyVideoView != null)
        {
            Destroy(historyVideoView.gameObject);
        }

        //顯示購買金幣/積分結果
        if (RoomType == TableTypeEnum.Cash ||
            RoomType == TableTypeEnum.VCTable)
        {
            if (buyChipsView.gameObject.activeSelf == false)
            {
                buyChipsView.gameObject.SetActive(true);
                buyChipsView.SetBuyChipsViewInfo(gameControl,
                                                 false,
                                                 gameRoomData.smallBlind,
                                                 transform.name,
                                                 RoomType,
                                                 InsufficientChipsBuyChipsCallback);

                thisData.LocalGamePlayerInfo.Init();
                thisData.LocalGamePlayerInfo.IsOpenInfoMask = true;
                WaitingTip_Txt.text = $"{LanguageManager.Instance.GetText("Waiting for the next round...")}";
            }
        }
        else if (RoomType == TableTypeEnum.IntegralTable)
        {
            SetBattleResult(false);
        }
    }

    /// <summary>
    /// 籌碼不足購買籌碼回傳
    /// </summary>
    /// <param name="buyValue"></param>
    private void InsufficientChipsBuyChipsCallback(double buyValue)
    {
        buyChipsView.gameObject.SetActive(false);
        gameControl.PreBuyChipsValue = Math.Floor(buyValue);
        gameControl.UpdateCarryChips();
    }

    /// <summary>
    /// 購買籌碼
    /// </summary>
    /// <param name="buyValue"></param>
    public void BuyChips(double buyValue)
    {
        buyChipsView.gameObject.SetActive(false);
        ViewManager.Instance.OpenTipMsgView(transform,
                                            LanguageManager.Instance.GetText("Start replenishing chips for the next hand"));
        gameControl.PreBuyChipsValue = Math.Floor(buyValue);
    }

    /// <summary>
    /// 購買籌碼回到遊戲
    /// </summary>
    /// <param name="pack"></param>
    public void BuyChipsGoBack()
    {
        ViewManager.Instance.CloseWaitingView(transform);
        CloseMenu();
        buyChipsView.gameObject.SetActive(false);

        double newChips = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                    .FirstOrDefault()
                                                    .Value
                                                    .carryChips;
        thisData.LocalGamePlayerInfo.PlayerRoomChips = newChips;
    }

    /// <summary>
    /// 設置積分結果
    /// </summary>
    /// <param name="isWin"></param>
    public void SetBattleResult(bool isWin)
    {
        BattleResultView.gameObject.SetActive(true);
        BattleResultView battleResult = BattleResultView.GetComponent<BattleResultView>();
        battleResult.OnSetBattleResult(isWin, transform.name, gameControl);
    }

    #region 聊天

    /// <summary>
    /// 關閉聊天
    /// </summary>
    private void CloseChatPage()
    {
        Mask_Btn.gameObject.SetActive(false);
        StartCoroutine(UnityUtils.Instance.IViewSlide(false,
                                                      ChatPage_Tr,
                                                      DirectionEnum.Left,
                                                      PageMoveTime,
                                                      () =>
                                                      {
                                                          //判斷保留訊息數量
                                                          if (ChatContent_Tr.childCount > MaxChatCount)
                                                          {
                                                              int closeCount = ChatContent_Tr.childCount - MaxChatCount;
                                                              for (int i = 0; i < closeCount; i++)
                                                              {
                                                                  if (ChatContent_Tr.GetChild(2 + i).gameObject.activeSelf)
                                                                  {
                                                                      ChatContent_Tr.GetChild(2 + i).gameObject.SetActive(false);
                                                                      float chatHeight = ChatContent_Tr.GetChild(2 + i).GetComponent<RectTransform>().rect.height;
                                                                      float reduce = Mathf.Max(0, ChatContent_Tr.anchoredPosition.y - chatHeight);
                                                                      ChatContent_Tr.anchoredPosition = new Vector2(ChatContent_Tr.anchoredPosition.x,
                                                                                                                    reduce);
                                                                  }
                                                              }
                                                          }

                                                          StartCoroutine(IGoNewChatMessage());
                                                          GameRoomManager.Instance.IsCanMoveSwitch = true;
                                                      }));
    }

    /// <summary>
    /// 設置未讀聊天訊息數
    /// </summary>
    private int SetNotReadChatCount
    {
        get
        {
            return notReadMsgCount;
        }
        set
        {
            notReadMsgCount = value;
            string countStr = value > 99 ?
                              "99+" :
                              $"{value}";
            NotReadChat_Txt.text = countStr;
            NotReadChat_Tr.gameObject.SetActive(value > 0);

            NotReadChat_Tr.sizeDelta = value > 99 ?
                                       new Vector2(20, 15) :
                                       new Vector2(15, 15);
        }
    }

    /// <summary>
    /// 延遲設置新訊息按鈕是否顯示
    /// </summary>
    /// <returns></returns>
    private IEnumerator IYieldSetNewMessageActive()
    {
        yield return null;
        NewMessage_Btn.gameObject.SetActive(!IsChatOnBottom());
    }

    /// <summary>
    /// 聊天移動至最新訊息位置
    /// </summary>
    /// <returns></returns>
    private IEnumerator IGoNewChatMessage()
    {
        yield return null;

        NewMessage_Btn.gameObject.SetActive(false);

        //顯示在最新訊息
        float chatAreaHeight = ChatArea_Sr.GetComponent<RectTransform>().rect.height;
        float currChatContentHeight = ChatContent_Tr.rect.height;
        float goPosY = Mathf.Max(0, currChatContentHeight - chatAreaHeight);
        ChatContent_Tr.anchoredPosition = new Vector2(ChatContent_Tr.anchoredPosition.x,
                                                      goPosY);
    }

    /// <summary>
    /// 判斷聊天位置是否在最底部
    /// </summary>
    /// <returns></returns>
    private bool IsChatOnBottom()
    {
        float chatAreaHeight = ChatArea_Sr.GetComponent<RectTransform>().rect.height;
        float currChatContentHeight = ChatContent_Tr.rect.height;
        float bottomPosY = Mathf.Max(0, currChatContentHeight - chatAreaHeight);
        return ChatContent_Tr.anchoredPosition.y >= Mathf.Max(0, bottomPosY - 20);
    }

    /// <summary>
    /// 發送聊天訊息
    /// </summary>
    private void SendChat()
    {
        if (string.IsNullOrEmpty(Chat_If.text))
        {
            return;
        }

        gameControl.UpdateChatMsg(Chat_If.text);
       /* baseRequest.SendRequestRequest_Chat(Chat_If.text);
        CreateChatContent(DataManager.UserAvatarIndex,
                          DataManager.UserNickname,
                          Chat_If.text,
                          true);*/

        Chat_If.text = "";

        if (!DataManager.IsMobilePlatform)
        {
            Chat_If.Select();
        }

        StartCoroutine(IGoNewChatMessage());
    }

    /// <summary>
    /// 接收聊天訊息
    /// </summary>
    /// <param name="chatData"></param>
    public void ReciveChat(ChatData chatData)
    {
        string id = chatData.userId;
        string nickname = chatData.nickname;
        string content = chatData.chatMsg;
        int avatar = chatData.avatarIndex;
        bool isLocal = id == DataManager.UserId;

        //判斷是否在最新訊息位置
        bool isBottom = IsChatOnBottom();
        if (ChatPage_Tr.gameObject.activeSelf)
        {
            NewMessage_Btn.gameObject.SetActive(!isBottom);
        }
        else
        {
            NewMessage_Btn.gameObject.SetActive(true);
        }

        CreateChatContent(avatar,
                          nickname,
                          content,
                          isLocal);

        if (isBottom)
        {
            StartCoroutine(IGoNewChatMessage());
        }

        //未開啟聊天頁面顯示新訊息提示
        if (!ChatPage_Tr.gameObject.activeSelf &&
            id != DataManager.UserId)
        {
            GamePlayerInfo player = GetPlayer(id);
            player.ShowChatInfo(content);

            SetNotReadChatCount = ++SetNotReadChatCount;
        }
    }

    /// <summary>
    /// 產生聊天內容
    /// </summary>
    /// <param name="avatar"></param>
    /// <param name="nickname"></param>
    /// <param name="content">聊天內容</param>
    /// <param name="isLocal">是否為本地玩家</param>
    private void CreateChatContent(int avatar, string nickname, string content, bool isLocal)
    {
        GameObject sample = isLocal ?
                            LocalChatSample :
                            OtherChatSample;

        ChatInfoSample chatInfo = objPool.CreateObj<ChatInfoSample>(sample, ChatContent_Tr);
        chatInfo.gameObject.SetActive(true);
        chatInfo.GetComponent<RectTransform>().SetSiblingIndex(ChatContent_Tr.childCount + 1);
        chatInfo.SetChatInfo(avatar,
                             nickname,
                             content);
    }

    #endregion

    #region 記錄存檔

    /// <summary>
    /// 上一局遊戲紀錄存檔
    /// </summary>
    private void SavePreGame()
    {
        //本地玩家有參與
        if (thisData != null &&
            thisData.LocalGamePlayerInfo != null &&
            thisData.LocalGamePlayerInfo.IsPlaying &&
            saveResultData != null &&
            gameInitHistoryData != null &&
            processHistoryData != null)
        {
            HandHistoryManager.Instance.SaveResult(saveResultData);
            HandHistoryManager.Instance.SaveGameInit(gameInitHistoryData);
            HandHistoryManager.Instance.SaveProcess(processHistoryData);
        }

        exitPlayerSeatList = new List<int>();
        processHistoryData = new ProcessHistoryData();
        processHistoryData.processStepHistoryDataList = new List<ProcessStepHistoryData>();

        //更新存檔資料
        HandHistoryView handHistoryView = GameObject.FindAnyObjectByType<HandHistoryView>();
        handHistoryView?.UpdateHitoryDate();
    }

    /// <summary>
    /// 添加行動存檔新紀錄
    /// </summary>
    /// <returns></returns>
    private ProcessStepHistoryData AddNewStepHistory()
    {
        //紀錄存檔
        ProcessStepHistoryData processStepHistoryData = new ProcessStepHistoryData();

        processStepHistoryData.SeatList = new List<int>();
        processStepHistoryData.ChipsList = new List<double>();
        processStepHistoryData.BetChipsList = new List<double>();
        processStepHistoryData.HandPoker1 = new List<int>();
        processStepHistoryData.HandPoker2 = new List<int>();
        processStepHistoryData.BetActionEnumIndex = new List<int>();
        foreach (var player in gamePlayerInfoList)
        {
            processStepHistoryData.SeatList.Add(player.SeatIndex);
            processStepHistoryData.ChipsList.Add(player.CurrRoomChips);
            processStepHistoryData.BetChipsList.Add(player.CurrBetValue);
            processStepHistoryData.HandPoker1.Add(player.GetHandPoker[0].PokerNum);
            processStepHistoryData.HandPoker2.Add(player.GetHandPoker[1].PokerNum);
            processStepHistoryData.BetActionEnumIndex.Add(Convert.ToInt32(player.CurrBetAction));
        }
        processStepHistoryData.CommunityPoker = thisData.CurrCommunityPoker;
        processStepHistoryData.TotalPot = gameRoomData.potChips;
        processStepHistoryData.ExitPlayerSeatList = exitPlayerSeatList;
        return processStepHistoryData;
    }

    #endregion

    #region 流程控制

    /// <summary>
    /// 關閉所有玩家倒數訊息
    /// </summary>
    /// <param name="id">排除的ID</param>
    public void CloseCDInfo(string id)
    {
        if (gameRoomData == null ||
            gameRoomData.playerDataDic == null)
        {
            return;
        }

        foreach (var player in gameRoomData.playerDataDic.Values)
        {
            GamePlayerInfo playerInfo = gamePlayerInfoList.Where(x => x.UserId == player.userId)
                                                          .FirstOrDefault();
                                                          
            if (player.userId != id &&
                playerInfo != null)
            {
                if ((PlayerStateEnum)player.gameState == PlayerStateEnum.Playing ||
                    (PlayerStateEnum)player.gameState == PlayerStateEnum.AllIn)
                {
                    playerInfo.ActionFrame = false;
                    playerInfo.InitCountDown();
                }
            }
        }
    }

    /// <summary>
    /// 更新房間資料
    /// </summary>
    /// <param name="gameRoomData"></param>
    public void UpdateGameRoomData(GameRoomData gameRoomData)
    {
        this.gameRoomData = gameRoomData;

        //底池
        if (gameRoomData.currGameFlow != (int)GameFlowEnum.PotResult &&
            gameRoomData.currGameFlow != (int)GameFlowEnum.SideResult)
        {
            if (TotalPot_Txt.text != StringUtils.SetChipsUnit(Math.Floor(gameRoomData.potChips)))
            {
                StringUtils.ChipsChangeEffect(TotalPot_Txt, Math.Floor(gameRoomData.potChips));
            }
        }

        if (gameRoomData.currGameFlow < (int)GameFlowEnum.Flop)
        {
            //公共牌
            for (int i = 0; i < CommunityPokerList.Count(); i++)
            {
                CommunityPokerList[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 遊戲開始初始化
    /// </summary>
    public void GameStartInit()
    {
        Init();
        GameInit();
    }

    /// <summary>
    /// 發牌流程
    /// </summary>
    /// <param name="gameRoomData"></param>
    public void OnLicensingFlow(GameRoomData gameRoomData)
    {
        foreach (var userId in gameRoomData.playingPlayersIdList)
        {
            GamePlayerInfo gamePlayerInfo = GetPlayer(userId);

            gamePlayerInfo.SwitchShoHandPoker(new List<int>() { -1, -1 });
            gamePlayerInfo.SetShowHandPoker(false, new List<int>() { -1, -1 });

            gamePlayerInfo.Init();
            //重製座位角色
            gamePlayerInfo.SetSeatCharacter(SeatCharacterEnum.None);

            //設置手牌
            if (userId == DataManager.UserId)
            {
                //本地玩家
                GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                                          .FirstOrDefault()
                                                                          .Value;

                //沒有離座
                if (playerData.isSitOut == false &&
                    (PlayerStateEnum)playerData.gameState != PlayerStateEnum.Waiting)
                {
                    thisData.IsPlaying = true;
                    gamePlayerInfo.SetHandPoker(playerData.handPoker[0],
                                                playerData.handPoker[1]);

                    WaitingTip_Txt.text = "";

                    //判斷牌行
                    if (gameRoomData.playingPlayersIdList.Contains(DataManager.UserId))
                    {
                        JudgePokerShape(gamePlayerInfo,
                                        true);
                    }
                }
            }
            else
            {
                //其他玩家
                gamePlayerInfo.SetHandPoker(-1, -1);
                gamePlayerInfo.SetPokerShapeTxtStr = "";
            }
        }

        //房主執行
        if (gameRoomData.hostId == DataManager.UserId)
        {
            //設置Button座位
            GameRoomPlayerData buttonPlayerData = gameRoomData.playerDataDic.Where(x => x.Value.gameSeat == gameRoomData.buttonSeat)
                                                                            .FirstOrDefault()
                                                                            .Value;
            var dataDic = new Dictionary<string, object>()
            {
                { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.Button},
            };
            gameControl.UpdataPlayerData(buttonPlayerData.userId,
                                         dataDic);

            GameRoomPlayerData sbPlayerData;
            GameRoomPlayerData bbPlayerData;
            if (gameRoomData.playingPlayersIdList.Count == 1)
            {
                sbPlayerData = buttonPlayerData;
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.SB},
                };
                gameControl.UpdataPlayerData(sbPlayerData.userId,
                                             dataDic);

                //設置BB座位
                bbPlayerData = buttonPlayerData;
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.BB},
                };
                gameControl.UpdataPlayerData(bbPlayerData.userId,
                                             dataDic);
            }
            else if (gameRoomData.playingPlayersIdList.Count == 2)
            {
                //只有2人

                //設置SB座位
                sbPlayerData = buttonPlayerData;
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.SB},
                };
                gameControl.UpdataPlayerData(sbPlayerData.userId,
                                             dataDic);
                //設置BB座位
                bbPlayerData = gameControl.GetNextPlayer(gameRoomData.buttonSeat);
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.BB},
                };
                gameControl.UpdataPlayerData(bbPlayerData.userId,
                                             dataDic);
            }
            else
            {
                //3人以上玩家

                //設置SB座位
                sbPlayerData = gameControl.GetNextPlayer(gameRoomData.buttonSeat);
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.SB},
                };
                gameControl.UpdataPlayerData(sbPlayerData.userId,
                                             dataDic);

                //設置BB座位
                bbPlayerData = gameControl.GetNextPlayer(sbPlayerData.gameSeat);
                dataDic = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, (int)SeatCharacterEnum.BB},
                };
                gameControl.UpdataPlayerData(bbPlayerData.userId,
                                             dataDic);
            }

            gameRoomData.playerDataDic[sbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind;
            gameRoomData.playerDataDic[bbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind * 2;
        }
    }

    /// <summary>
    /// 盲注流程
    /// </summary>
    /// <param name="gameRoomData"></param>
    public void OnBlindFlow(GameRoomData gameRoomData)
    {
        //Button座位
        GameRoomPlayerData buttonPlayerData = gameRoomData.playerDataDic.Where(x => x.Value.gameSeat == gameRoomData.buttonSeat)
                                                                        .FirstOrDefault()
                                                                        .Value;
        GamePlayerInfo buttonPlayer = GetPlayer(buttonPlayerData.userId);
        buttonPlayer.SetSeatCharacter(SeatCharacterEnum.Button);

        //SB下注
        GameRoomPlayerData sbPlayerData = gameRoomData.playerDataDic.Where(x => (SeatCharacterEnum)x.Value.seatCharacter == SeatCharacterEnum.SB)
                                                                    .FirstOrDefault()
                                                                    .Value;

        if (sbPlayerData == null)
        {
            sbPlayerData = buttonPlayerData;
        }

        GamePlayerInfo sbPlayer = GetPlayer(sbPlayerData.userId);
        sbPlayer.SetSeatCharacter(SeatCharacterEnum.SB);
        sbPlayer.PlayerAction(BetActingEnum.Blind,
                              gameRoomData.smallBlind,
                              sbPlayerData.carryChips - gameRoomData.smallBlind);
        if (DataManager.UserId == sbPlayerData.userId)
        {
            gameControl.UpdateLocalChips(-gameRoomData.smallBlind);
        }

        //BB下注
        GameRoomPlayerData bbPlayerData = gameRoomData.playerDataDic.Where(x => (SeatCharacterEnum)x.Value.seatCharacter == SeatCharacterEnum.BB)
                                                                    .FirstOrDefault()
                                                                    .Value;
        if (bbPlayerData == null)
        {
            bbPlayerData = buttonPlayerData;
        }
        GamePlayerInfo bbPlayer = GetPlayer(bbPlayerData.userId);
        bbPlayer.SetSeatCharacter(SeatCharacterEnum.BB);
        bbPlayer.PlayerAction(BetActingEnum.Blind,
                              gameRoomData.smallBlind * 2,
                              bbPlayerData.carryChips - (gameRoomData.smallBlind * 2));
        if (DataManager.UserId == sbPlayerData.userId)
        {
            gameControl.UpdateLocalChips(-gameRoomData.smallBlind * 2);
        }

        gameRoomData.playerDataDic[sbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind;
        gameRoomData.playerDataDic[bbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind * 2;
        SetActionButton = false;

        thisData.TotalPot = gameRoomData.smallBlind + (gameRoomData.smallBlind * 2);

        //遊戲紀錄
        gameInitHistoryData = HandHistoryManager.Instance.SetGameInitData(gamePlayerInfoList,
                                                                          thisData.TotalPot);

        //房主執行
        if (gameRoomData.hostId == DataManager.UserId)
        {
            var data = new Dictionary<string, object>();

            //SB攜帶籌碼更新
            double sbNewCarryChips = sbPlayerData.carryChips - gameRoomData.smallBlind;
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.CARRY_CHIPS, sbNewCarryChips },                           //攜帶籌碼
                { FirebaseManager.CURR_ALL_BET_CHIPS, gameRoomData.smallBlind},             //當前流程總下注籌碼
                { FirebaseManager.ALL_BET_CHIPS, gameRoomData.smallBlind},                  //該局總下注籌碼
            };
            gameControl.UpdataPlayerData(sbPlayerData.userId,
                                         data);

            //BB攜帶籌碼更新
            double bbNewCarryChips = bbPlayerData.carryChips - (gameRoomData.smallBlind * 2);
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.CARRY_CHIPS, bbNewCarryChips },                           //攜帶籌碼
                { FirebaseManager.CURR_ALL_BET_CHIPS, gameRoomData.smallBlind * 2},         //當前流程總下注籌碼
                { FirebaseManager.ALL_BET_CHIPS, gameRoomData.smallBlind * 2},              //該局總下注籌碼
            };
            gameControl.UpdataPlayerData(bbPlayerData.userId,
                                         data);

            //更新底池
            double totalPot = gameRoomData.smallBlind + (gameRoomData.smallBlind * 2);
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.POT_CHIPS, totalPot },                               //底池
                { FirebaseManager.CURR_CALL_VALUE, gameRoomData.smallBlind * 2},       //當前跟注值
                { FirebaseManager.CURR_ACTIONER_ID, bbPlayerData.userId},              //當前行動玩家Id
                { FirebaseManager.CURR_ACTIONER_SEAT, bbPlayerData.gameSeat},          //當前行動玩家座位
                { FirebaseManager.ACTIONP_PLAYER_COUNT, 1},                            //當前流程行動玩家次數
            };
            gameControl.UpdateGameRoomData(data);
        }
    }

    #endregion

    /// <summary>
    /// 移除資料
    /// </summary>
    private void OnRemoveData()
    {
        thisData = null;
        StopAllCoroutines();
    }
}
