using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;
using RequestBuf;
using Newtonsoft.Json;
using UnityEditor;

public class GameView : MonoBehaviour
{
    [SerializeField]
    Request_GameView baseRequest;

    [Header("遊戲控制腳本")]
    [SerializeField]
    public GameControl gameControl;

    [Header("籌碼動畫腳本")]
    [SerializeField]
    public ChipsTween chipsTween;

    public GameData gameData = new GameData();

    [Header("座位上玩家訊息")]
    [SerializeField]
    public List<GamePlayerInfo> SeatGamePlayerInfoList;
    [SerializeField]
    List<Button> SeatButtonList;

    [Header("操作按鈕")]
    [SerializeField]
    ActionButtons actionButtons;

    [Header("底池")]
    [SerializeField]
    GamePot gamePot;

    [Header("公共牌")]
    [SerializeField]
    CommunityPoker communityPoker;

    [Header("選單")]
    [SerializeField]
    Button Menu_Btn;
    [SerializeField]
    GameMenu gameMenu;

    [Header("聊天")]
    [SerializeField]
    GameChat gameChat;

    [Header("遊戲結果")]
    [SerializeField]
    GameObject WinChipsObj;

    [Header("遊戲暫停")]
    [SerializeField]
    GameObject GamePause_Obj;
    [SerializeField]
    Button GameContinue_Btn;

    [Header("遊戲測試用")]
    [SerializeField]
    public GameTest GameTest;
    public Button Pause_Btn;

    [Header("提示POP")]
    public GameObject Notice;
    public TextMeshProUGUI noticeText;
    public Button ConfirmBtn;

    [Header("等待下局")]
    public Image WaitNext_Obj;
    public List<Sprite> WaitNext_ImgList;

    public string roomName = "";

    AudioPool audioPool;

    public GameObject BGMask;
    public GameObject GameMask;
    public GameObject TopBar;

    public Button testApi_Btn;

    

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 等待下局
        WaitNext_Obj.sprite = WaitNext_ImgList[LanguageManager.Instance.GetCurrLanguageIndex()];
        #endregion

        actionButtons.SetSitOutDisplay();
    }

    public void Awake()
    {
        audioPool = new AudioPool(transform);

        GameMask.SetActive(false);
        ListenerEvent();
        SetTopBar(true);
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
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

        //遊戲繼續按鈕
        GameContinue_Btn.onClick.AddListener(() =>
        {

        });

        #region 選單

        //開啟選單
        Menu_Btn.onClick.AddListener(() =>
        {
            gameMenu.ShowMenu(true);
        });
        gameMenu.LeaveRoom += menuLeaveRoom;
        gameMenu.OpenBuyView += openBuyChipView;
        gameMenu.SitOut += sitOut;

        #endregion

        #region 操作按鈕

        actionButtons.UpdateActionBtn += UpdateActionBtns;
        actionButtons.SetMenuBtn += setMenuButton;
        actionButtons.UpdateBetAction += updateBetAction;
        actionButtons.UpdataPlayerData += updatePlayerData;
        actionButtons.SetShowFoldPoker += setShowFoldPoker;

        #endregion

        #region 聊天

        gameChat.ShowChat += ShowChat;
        gameChat.UpdateChatMsg += UpdateChatMsg;

        #endregion

        //Test
        Pause_Btn.onClick.AddListener(() =>
        {
            if (Time.timeScale != 0)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        });

        ConfirmBtn.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("nullData", 0);
            PlayerPrefs.SetString("PlayerIsOnline", "True");
            PlayerPrefs.Save();
            JSBridgeManager.Instance.WindowClose();
        });
    }
    public void Initialize()
    {
        gameData = GameRoomManager.Instance.GetGameData(roomName);
        gameData.thisData = new ThisData();
        gameData.thisData.IsPlaying = false;

        gameData.strData = new StrData();

        if (gameData.gamePlayerInfoList != null)
        {
            foreach (var player in gameData.gamePlayerInfoList)
            {
                Destroy(player.gameObject);
            }
        }

        gameData.gamePlayerInfoList = new List<GamePlayerInfo>();
        gameMenu.ShowRule(false);

        gameChat.SetNotReadChatCount = 0;

#if !UNITY_EDITOR
        JSBridgeManager.Instance.RegisterOnPageUnload(gameObject.name, nameof(OnAllPlayerLeft));
#endif

        Init();
        GameInit();
    }

    private void Start()
    {
        gameMenu.Init();

        //清除座位上玩家
        for (int i = 1; i < SeatGamePlayerInfoList.Count; i++)
        {
            SeatGamePlayerInfoList[i].gameObject.SetActive(false);
        }

        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);

        InvokeRepeating(nameof(checkIsIdle), 0, 2);

        #region 遊戲測試

        GameTest.testToolSetUp();

        #endregion
        //MusicSwitchBtn.IsPlayAudio(AudioSource_Obj);
        SFXSwitchBtn.IsPlaySFX();
    }

    // [DllImport("__Internal")]
    // private static extern void sendBeaconRequest();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            gameControl.CreateRobot(false);
        }

        if (Input.GetKey(KeyCode.Backspace))
        {
            PlayerPrefs.DeleteAll();
        }

        Notice.gameObject.SetActive(DataManager.istipAppear);
        noticeText.text = DataManager.TipText;

        string callSprName = actionButtons.CallBtn_Img.sprite.name;
        if (callSprName != "跟注" && callSprName != "caLL")
            actionButtons.CallBtnText = "";
    }

    #region Action接收

    private void menuLeaveRoom()
    {
        gameControl.JudgeHost();
        gameControl.ExitGame();
    }
    private void openBuyChipView()
    {
        gameMenu.OpenBuyChipView(gameControl, true, gameData.thisData.SmallBlindValue, transform.name, gameData.RoomType, BuyChips);
    }
    private void sitOut()
    {
        gameData.thisData.IsSitOut = !gameData.thisData.IsSitOut;
        actionButtons.SetSitOutDisplay();

        var data = new Dictionary<string, object>()
            {
                { FirebaseManager.IS_SIT_OUT, gameData.thisData.IsSitOut},         //是否保留座位離開
            };
        gameControl.UpdataPlayerData(DataManager.UserId,
                                     data);

        if (gameData.thisData.IsSitOut)
            ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful,
                                        LanguageManager.Instance.GetText("Sit out next hand"));
    }
    private void setMenuButton(bool isAble)
    {
        Menu_Btn.interactable = isAble;
    }
    private void updateBetAction(BetActingEnum acting, double betValue)
    {
        gameControl.UpdateBetAction(DataManager.UserId, acting, betValue);
    }
    private void updatePlayerData(Dictionary<string, object> data)
    {
        gameControl.UpdataPlayerData(DataManager.UserId, data);
    }
    private void setShowFoldPoker(int index)
    {
        GameRoomPlayerData playerData = gameData.gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId).FirstOrDefault().Value;

        GamePlayerInfo playerInfo = GetPlayer(playerData.userId);
        playerInfo.OpenLocalShowHandPoker(index, playerData.handPoker[index]);

        List<int> showHandPoker = playerData.showHandPoker;
        showHandPoker[index] = playerData.handPoker[index];

        //更新玩家資料
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.SHOW_HAND_POKER, showHandPoker},         //棄牌後顯示手牌
        };
        updatePlayerData(data);
    }
    private void ShowChat(string id, string content)
    {
        GamePlayerInfo player = GetPlayer(id);
        player.ShowChatInfo(content);

        gameChat.SetNotReadChatCount = ++gameChat.SetNotReadChatCount;
    }
    private void UpdateChatMsg(string msg)
    {
        gameControl.UpdateChatMsg(msg);
    }

    #endregion

    public void SetRoomType(TableTypeEnum roomType)
    {
        gameData.RoomType = roomType;
        if (roomType == TableTypeEnum.IntegralTable)
        {
            for (int i = 0; i < SeatButtonList.Count; i++)
            {
                if (i != 0 && i != 3)
                {
                    SeatButtonList[i].gameObject.SetActive(false);
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
        AudioManager.Instance.playSFX(soundName);
    }

    ///<summary>
    ///Chip transaction
    /// </summary>
    public void postNoodleChip(double currRaiseBet)
    {
        print("遊戲結果(籌碼異動): " + gameData.saveResultData);
        try
        {
            NoodleApi.PostTableChipsTransaction(DataManager.UserId, gameData.saveResultData.roundId.ToString(), currRaiseBet, 9, ChipTransactionType.Raise, (x) =>
            {
                Debug.Log("Raise Table ChipsTransaction Success");
            },
            (error) =>
            {
                Debug.LogError($"Raise Table ChipsTransaction Failed Error: {error}");
            });
        }
        catch
        {
            print("Result Data為空");
        }
    }

    public void UpdateActionBtns()
    {
        Debug.Log($"UpdateActionBtns called. Player's turn: {gameData.thisData.isLocalPlayerTurn}");

        var localPlayer = gameControl.GetLocalPlayer();
        if (localPlayer == null || gameData.gameRoomData == null) return;

        bool isRaised = gameData.gameRoomData.currCallValue >= gameData.gameRoomData.smallBlind * 2;  // Raised if currCallValue exceeds minimum bet
        //bool isRaised = gameData.thisData.CurrRaiseValue>= gameData.gameRoomData.smallBlind * 2;  // Raised if currRaiseValue exceeds minimum bet
        bool isBigBlind = localPlayer.seatCharacter == (int)SeatCharacterEnum.BB;
        bool isSmallBlind = localPlayer.seatCharacter == (int)SeatCharacterEnum.SB;
        bool isPreFlop = gameData.gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind;
        bool isLocalPlayerTurn = gameData.thisData.isLocalPlayerTurn;
        bool isLocalPlayer = localPlayer.userId == DataManager.UserId;

        // Local player's turn
        if (isLocalPlayerTurn)
        {
            if (isPreFlop)
            {
                SetLocalPlayerPreFlopActions(isBigBlind, isSmallBlind, isRaised, localPlayer);
            }
            else
            {
                SetLocalPlayerPostFlopActions(isRaised, localPlayer);
            }
        }
        // Other players' turn
        else
        {
            if (isPreFlop)
            {
                SetPreFlopActionsForOthers(isBigBlind, isSmallBlind, isRaised, localPlayer);
            }
            else
            {
                SetPostFlopActionsForOthers(isRaised, localPlayer);
            }
        }
    }

    private void SetLocalPlayerPreFlopActions(bool isBigBlind, bool isSmallBlind, bool isRaised, GameRoomPlayerData localPlayer)
    {
        GamePlayerInfo locPlayer = GetPlayer(DataManager.UserId);
        if (isRaised)
        {
            // If a raise has occurred, show Fold and Call with amount for the local player
            if (isBigBlind)
            {
                // Big Blind sees Check/Fold and Check pre-flop with no raise
                print("如果加注了，大盲注看到Check/Fold和Check翻牌前沒有加注");
                if (locPlayer.CurrRoomChips > 0)
                {
                    gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                    if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
                    {
                        print("等於當前跟注金額");
                        gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                        gameData.strData.CallValueStr = "";
                    }
                    else
                    {
                        if (gameData.thisData.CurrCallValue > gameData.thisData.LocalPlayerChips)
                        {
                            //print("當前加注小於玩家籌碼: " + (gameData.thisData.CurrRaiseValue < gameData.thisData.LocalPlayerChips));
                            gameData.strData.CallStr = LanguageManager.Instance.GetText("AllIn");
                            gameData.strData.CallValueStr = "";
                        }
                        else
                        {
                            gameData.strData.CallStr = LanguageManager.Instance.GetText("Call");
                            gameData.strData.CallValueStr = $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                            //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                        }
                    }
                }
                else
                {
                    print("玩家已All In");
                    gameData.strData.FoldStr = "";
                    gameData.strData.CallStr = "";
                    gameData.strData.CallValueStr = "";
                }
            }
            else
            {
                print("如果加注了，其他玩家看到Fold和Call翻牌前沒有加注");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                if (gameData.thisData.CurrCallValue > gameData.thisData.LocalPlayerChips)
                {
                    //print("當前加注小於玩家籌碼: " + (gameData.thisData.CurrRaiseValue < gameData.thisData.LocalPlayerChips));
                    gameData.strData.CallStr = LanguageManager.Instance.GetText("AllIn");
                    gameData.strData.CallValueStr = "";
                }
                else
                {
                    //print("當前加注小於玩家籌碼: " + (gameData.thisData.CurrRaiseValue < gameData.thisData.LocalPlayerChips));
                    gameData.strData.CallStr = (gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips) <= 0 ? LanguageManager.Instance.GetText("Check") : LanguageManager.Instance.GetText("Call");
                    gameData.strData.CallValueStr = $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                    //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                }
            }
        }
        else
        {
            if (isBigBlind)
            {
                // Big Blind sees Check/Fold and Check pre-flop with no raise
                print("大盲注看到Check/Fold和Check翻牌前沒有加注");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
            else if (isSmallBlind)
            {
                // Small Blind sees Fold and Call + amount pre-flop
                print("小盲注看到Fold和Call + 金額 翻牌前沒有加注");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Call");
                gameData.strData.CallValueStr = $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
            }
            else
            {
                // Other players see Fold and Call + amount pre-flop
                print("每個人都看到Fold和跟注 + 金額在翻牌前");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Call");
                gameData.strData.CallValueStr = $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
            }
        }

        // Update button texts for the local player
        UpdateActionButtonTexts(isRaised, true);
    }

    private void SetLocalPlayerPostFlopActions(bool isRaised, GameRoomPlayerData localPlayer)
    {
        if (isRaised)
        {
            if (localPlayer.currAllBetChips >= gameData.gameRoomData.currCallValue)
            {
                // Player who raised sees Fold and an empty Call text
                print("如果加注了，每個人都看到Fold和空跟注按鈕");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
            else
            {
                // Other players see Fold and Call + amount
                print("如果加注了，小盲注看到Fold和跟注 + 金額");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                if(gameData.thisData.CurrCallValue > gameData.thisData.LocalPlayerChips)
                {
                    gameData.strData.CallStr = LanguageManager.Instance.GetText("AllIn");
                    gameData.strData.CallValueStr = "";
                }
                else
                {
                    gameData.strData.CallStr = LanguageManager.Instance.GetText("Call");
                    gameData.strData.CallValueStr = $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                    //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                }
            }
        }
        else
        {
            // No raise: Everyone sees Check/Fold and Check post-flop
            if (localPlayer.userId == DataManager.UserId)
            {
                print("本地玩家看到Fold和Check");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
            else
            {
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("CheckOrFold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
        }

        // Update button texts for the local player
        UpdateActionButtonTexts(isRaised, true);
    }

    private void SetPreFlopActionsForOthers(bool isBigBlind, bool isSmallBlind, bool isRaised, GameRoomPlayerData localPlayer)
    {
        GamePlayerInfo locPlayer = GetPlayer(DataManager.UserId);

        if (isRaised)
        {
            // If raised, everyone sees Fold and Call + amount
            if (isBigBlind)
            {
                //設成All In
                print("如果加注，每個人都會看到 Fold 和 Call + 金額");
                if (locPlayer.PlayerRoomChips > 0)
                {
                    gameData.strData.FoldStr = LanguageManager.Instance.GetText("CheckOrFold");
                    bool check = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0;
                    if (check)
                    {
                        gameData.strData.CallStr = (gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue && gameData.gameRoomData.currGameFlow != (int)GameFlowEnum.SetBlind) ? "" : LanguageManager.Instance.GetText("Check");
                        gameData.strData.CallValueStr = "";
                    }
                    else
                    {
                        if (gameData.thisData.CurrRaiseValue < gameData.thisData.LocalPlayerChips || (gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips) < locPlayer.CurrRoomChips)
                        {
                            print("當前加注小於玩家籌碼: " + (gameData.thisData.CurrRaiseValue <= gameData.thisData.LocalPlayerChips));
                            gameData.strData.CallStr = (gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue) ? LanguageManager.Instance.GetText("Check") : LanguageManager.Instance.GetText("Call");
                            gameData.strData.CallValueStr = (gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue) ? "" : $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                            //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                        }
                        else
                        {
                            gameData.strData.CallStr = LanguageManager.Instance.GetText("AllIn");
                            gameData.strData.CallValueStr = "";
                        }
                        //print($"玩家籌碼:{gameData.thisData.LocalPlayerChips}, 當前跟注:{gameData.thisData.CurrCallValue}, 當前加注: {gameData.thisData.CurrRaiseValue}, 本地玩家所有下注: {localPlayer.currAllBetChips}");
                    }
                }
                else
                {
                    print(locPlayer.PlayerRoomChips);
                    print("玩家已All In");
                    gameData.strData.FoldStr = "";
                    gameData.strData.CallStr = "";
                    gameData.strData.CallValueStr = "";
                }
            }
            else
            {
                print("小盲: 每個人都會看到 Fold 和 Call + 金額");
                if ((gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips) < locPlayer.CurrRoomChips)
                {
                    bool check = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0;
                    gameData.strData.CallStr = (check || gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue) ? "" : LanguageManager.Instance.GetText("Call");
                    gameData.strData.CallValueStr = (check || gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue) ? "" : $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                    //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                }
                else
                {
                    if (locPlayer.CurrRoomChips > 0)
                    {
                        gameData.strData.CallStr = LanguageManager.Instance.GetText("AllIn");
                        gameData.strData.CallValueStr = "";
                    }
                    else
                    {
                        print("玩家已All In");
                        gameData.strData.FoldStr = "";
                        gameData.strData.CallStr = "";
                        gameData.strData.CallValueStr = "";
                    }
                }
            }
        }
        else
        {
            if (isBigBlind)
            {
                // Big Blind sees Check/Fold and Check pre-flop with no raise
                print("大盲注看到Check/Fold和Check翻牌前沒有加注");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("CheckOrFold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
            else if (isSmallBlind)
            {
                // Small Blind sees Fold and Call + amount pre-flop
                print("小盲注看到Fold和跟注 + 金額在翻牌前");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0 ? "" : LanguageManager.Instance.GetText("Call");
                gameData.strData.CallValueStr = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0
                    ? ""
                    : $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
            }
            else
            {
                // Other players see Fold and Call + amount pre-flop
                print("每個人都看到Fold和跟注 + 金額in翻牌前");
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0 ? "" : LanguageManager.Instance.GetText("Call");
                gameData.strData.CallValueStr = gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips == 0
                    ? ""
                    : $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}";
                //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
            }
        }

        // Update button texts for other players
        UpdateActionButtonTexts(isRaised);
    }

    private void SetPostFlopActionsForOthers(bool isRaised, GameRoomPlayerData localPlayer)
    {
        GamePlayerInfo locPlayer = GetPlayer(DataManager.UserId);

        if (isRaised)
        {
            if (localPlayer.currAllBetChips >= gameData.gameRoomData.currCallValue)
            {
                // Player who raised sees Fold and empty Call text
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                gameData.strData.CallStr = "";
                gameData.strData.CallValueStr = "";
            }
            else
            {
                // Other players see Fold and Call + amount post-flop
                print("每個人都看到Fold和跟注 + 金額");
                if (locPlayer.PlayerRoomChips > 0)
                {
                    //print(locPlayer.PlayerRoomChips);
                    gameData.strData.FoldStr = LanguageManager.Instance.GetText("Fold");
                    gameData.strData.CallStr = LanguageManager.Instance.GetText("Call"); ;
                    gameData.strData.CallValueStr = (gameData.gameRoomData.currGameFlow != 1) ? $" {gameData.gameRoomData.currCallValue - localPlayer.currAllBetChips}" : "";
                    //print("設置跟注文字: " + gameData.strData.CallStr + gameData.strData.CallValueStr);
                }
                else
                {
                    print("玩家已All In");
                    gameData.strData.FoldStr = "";
                    gameData.strData.CallStr = "";
                    gameData.strData.CallValueStr = "";
                }
            }
        }
        else
        {
            // No raise: everyone sees Check/Fold and Check post-flop
            print("每個人都看到Check/Fold和Check");
            if (locPlayer != null && locPlayer.PlayerRoomChips > 0)
            {
                gameData.strData.FoldStr = LanguageManager.Instance.GetText("CheckOrFold");
                gameData.strData.CallStr = LanguageManager.Instance.GetText("Check");
                gameData.strData.CallValueStr = "";
            }
            else
            {
                print("玩家已All In");
                gameData.strData.FoldStr = "";
                gameData.strData.CallStr = "";
                gameData.strData.CallValueStr = "";
            }
        }

        // Update button texts for other players
        UpdateActionButtonTexts(isRaised);
    }

    private void UpdateActionButtonTexts(bool isRaised, bool localPlayerTurn = false)
    {
        int keyF = 0;
        int keyC = 0;

        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            keyF = gameData.betStringsE.FirstOrDefault(x => x.Value == gameData.strData.FoldStr).Key;
            keyC = gameData.betStringsE.FirstOrDefault(x => x.Value == gameData.strData.CallStr).Key;
            actionButtons.SetCallFoldBetStr("Call", keyC);
            //print("跟注按鈕文字: " + betStringsE[keyC] + " " + gameData.strData.CallStr);
            actionButtons.SetCallFoldBetStr("Fold", keyF);
        }
        else
        {
            keyF = gameData.betStringsC.FirstOrDefault(x => x.Value == gameData.strData.FoldStr).Key;
            keyC = gameData.betStringsC.FirstOrDefault(x => x.Value == gameData.strData.CallStr).Key;
            actionButtons.SetCallFoldBetStr("Call", keyC);
            print("跟注按鈕文字: " + gameData.betStringsC[keyC] + " " + gameData.strData.CallStr);
            actionButtons.SetCallFoldBetStr("Fold", keyF);
        }

        //CallBtn_Txt.text = LanguageManager.Instance.GetText(gameData.strData.CallStr) + gameData.strData.CallValueStr;
        if (gameData.strData.CallValueStr != "" && int.Parse(gameData.strData.CallValueStr) > 0 && keyC == 0)
        //CallBtn_Txt.text = "$" + gameData.strData.CallValueStr;
        {
            print(keyC);
            actionButtons.CallBtnText = "$" + gameData.strData.CallValueStr;
        }
        else
            actionButtons.CallBtnText = "";
        //coinIconObj.SetActive(false);
        UpdateRaiseBtn(localPlayerTurn, isRaised);
    }

    public void UpdateRaiseBtn(bool localPlayerTurn = false, bool isRaised = false)
    {
        if (gameControl.GetLocalPlayer().gameState != (int)PlayerStateEnum.AllIn)
        {
            if (!localPlayerTurn)
            {
                actionButtons.RaiseBtnText = LanguageManager.Instance.GetText("CallAny");
            }
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        GameRoomManager.Instance.EnanbleBtn(false);
        Menu_Btn.interactable = true;
    }

    /// <summary>
    /// 遊戲初始化
    /// </summary>
    public void GameInit()
    {
        print("初始化");
        communityPoker.Init();
        actionButtons.Init(roomName);
        gamePot.Init(roomName);
        gameChat.Init(roomName);
        gamePot.ShowWaitingTip = true;
        gamePot.TotalPot = 0;
        chipsTween.GameInit();
        foreach (var player in gameData.gamePlayerInfoList)
        {
            //player.SetPokerShapeTxtStr = "";
            player.SetPokerShapeImage = null;
            player.IsWinnerActive = false;
            player.SetBackChips = 0;
            player.GetHandPoker[0].gameObject.SetActive(false);
            player.GetHandPoker[1].gameObject.SetActive(false);
            player.IsOpenInfoMask = true;
            player.IsPlaying = false;
            player.SetSeatCharacter(SeatCharacterEnum.None);
        }

        gamePot.TotalPot = 0;
        gamePot.PotActive = false;

        gameData.thisData.IsPlaying = false;
        gameData.thisData.isFold = false;
        gameData.thisData.CurrCommunityPoker = new List<int>();
        tweenManager.inst.initDPos();
        gameData.isDealed = false;
        gameData.playerWinValueList.Clear();
    }

    /// <summary>
    /// 顯示棄牌手牌
    /// </summary>
    public void ShowFoldPoker()
    {
        foreach (var player in gameData.gameRoomData.playerDataDic.Values)
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
    /// 輪到本地玩家檢查下注區域狀態
    /// </summary>
    public void CheckActionArea(GameRoomData gameRoomData)
    {
        //顯示異常
        if (actionButtons.FoldBtnImage() == AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteEnglish).album[3] ||
            actionButtons.FoldBtnImage() == AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteChinese).album[3])
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
        bool IsUnableRaise = (gameControl.GetAllInPlayer().Count() > 0 && gameRoomData.playingPlayersIdList != null &&
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

        actionButtons.SetActionButton = true;

        //當前小盲值
        gameData.thisData.SmallBlindValue = gameRoomData.smallBlind;
        //當前底池
        gameData.thisData.TotalPot = gameRoomData.potChips;
        //玩家籌碼
        gameData.thisData.LocalPlayerChips = gameRoomPlayerData.carryChips;
        //首位加注玩家
        gameData.thisData.IsFirstRaisePlayer = isFirst;
        //當前跟注值
        gameData.thisData.CurrCallValue = gameRoomData.currCallValue;
        //跟注差額
        gameData.thisData.CallDifference = gameRoomData.currCallValue - gameRoomPlayerData.currAllBetChips;
        //玩家當前下注值
        gameData.thisData.LocalPlayerCurrBetValue = gameRoomPlayerData.currAllBetChips;
        //無法加注
        gameData.thisData.IsUnableRaise = IsUnableRaise;
        //是否無法跟注
        gameData.thisData.isCanCall = isCanCall;
        //最小加注
        gameData.thisData.MinRaiseValue = gameData.thisData.CurrCallValue == 0 ? gameData.thisData.SmallBlindValue * 2 : gameData.thisData.CurrCallValue * 2;
        gameData.thisData.CurrRaiseValue = gameData.thisData.MinRaiseValue;


        if (actionButtons.AutoActionState != AutoActingEnum.None)
        {
            StartCoroutine(actionButtons.JudgeAutoAction());
            return;
        }

        actionButtons.ShowBetArea();
        gamePot.SBBlinds = $"BLINDS: ${gameData.thisData.SmallBlindValue}/{gameData.thisData.SmallBlindValue * 2}";
    }

    /// <summary>
    /// 更新遊戲房間訊息
    /// </summary>
    /// <param name="gameRoomData">遊戲房間資料</param>
    public void UpdateGameRoomInfo(GameRoomData gameRoomData)
    {
        // Clear seated players, starting from index 1 (assuming index 0 might be reserved)
        for (int i = 1; i < SeatGamePlayerInfoList.Count; i++)
        {
            SeatGamePlayerInfoList[i].gameObject.SetActive(false);
        }

        // Reset game player list
        gameData.gamePlayerInfoList = new List<GamePlayerInfo>();

        // Get local player data
        GameRoomPlayerData localData = gameControl.GetLocalPlayer();

        // If no local player data, exit early
        if (localData == null)
        {
            print("找不到本地玩家");
            return;
        }

        // Set local player seat
        gameData.thisData.LocalPlayerSeat = localData.gameSeat;

        // Update player info for all players
        foreach (var player in gameRoomData.playerDataDic.Values)
        {
            // if (player.isPlayerLeft)
            // {
            //     continue;
            // }
            GamePlayerInfo gamePlayerInfo = AddPlayer(player, gameRoomData);
            gamePlayerInfo.CloseChatInfo();

            // If it's not the local player
            if (player.userId != DataManager.UserId &&
                gameRoomData.playingPlayersIdList != null &&
                gameRoomData.playingPlayersIdList.Count >= 2 &&
                gameRoomData.playingPlayersIdList.Contains(player.userId))
            {
                // Hide other players' hands
                gamePlayerInfo.SetPokerShapeImage = null;
                gamePlayerInfo.SetHandPoker(-1, -1, "hideCard");
            }
            else
            {
                // Local player logic
                if (!player.isSitOut &&
                    (PlayerStateEnum)player.gameState != PlayerStateEnum.Waiting &&
                    (PlayerStateEnum)player.gameState != PlayerStateEnum.Fold)
                {
                    // The local player is actively playing
                    gameData.thisData.IsPlaying = true;

                    // Hide the waiting tip
                    gamePot.ShowWaitingTip = false;

                    // Ensure no info mask is shown
                    gamePlayerInfo.IsOpenInfoMask = false;

                    // Only for the local player, display their hand and judge poker shape
                    if (player.userId == DataManager.UserId)
                    {
                        if (!gameData.isDealed)
                        {
                            print("尚未開牌");
                            gamePlayerInfo.SetHandPoker(-1, -1, "hideCard");
                        }
                        else
                        {
                            print("已開牌");
                            gamePlayerInfo.SetHandPoker(player.handPoker[0], player.handPoker[1], "Normal");
                        }

                        // Judge the local player's poker hand shape
                        //JudgePokerShapeUI(gamePlayerInfo, true);
                    }
                }

                // If the player is waiting, hide their hand and poker shape image
                if ((PlayerStateEnum)player.gameState == PlayerStateEnum.Waiting)
                {
                    gamePlayerInfo.SetPokerShapeImage = null;
                    gamePlayerInfo.GetHandPoker[0].gameObject.SetActive(false);
                    gamePlayerInfo.GetHandPoker[1].gameObject.SetActive(false);
                }
            }

            // Check if the current player is the one to act and the game flow allows for action
            if (player.userId == gameRoomData.currActionerId &&
                gameRoomData.currGameFlow > (int)GameFlowEnum.Licensing &&
                gameRoomData.actionCD > 0)
            {
                // Highlight the current action frame and start countdown
                gamePlayerInfo.ActionFrame = true;
                gamePlayerInfo.CountDown(DataManager.StartCountDownTime, gameRoomData.actionCD);
            }
        }

        // Update total pot if not in PotResult or SideResult state
        if (gameRoomData.currGameFlow != (int)GameFlowEnum.PotResult &&
            gameRoomData.currGameFlow != (int)GameFlowEnum.SideResult)
        {
            gamePot.TotalPotText = $"${StringUtils.SetChipsUnit(Math.Floor(gameRoomData.potChips))}";
            gameData.thisData.TotalPot = gameRoomData.potChips;
        }

        // Update community cards
        List<int> currCommunityPoker = gameRoomData.currCommunityPoker;
        if (currCommunityPoker != null)
        {
            for (int i = 0; i < currCommunityPoker.Count; i++)
            {
                communityPoker.Show(i, true);
                communityPoker.Set(i, currCommunityPoker[i]);
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
            if (gameData.RoomType == TableTypeEnum.IntegralTable)
            {
                seatIndex = 3;
            }
            else
            {
                seatIndex = playerData.gameSeat > gameData.thisData.LocalPlayerSeat ?
                            playerData.gameSeat - gameData.thisData.LocalPlayerSeat :
                            SeatButtonList.Count - (gameData.thisData.LocalPlayerSeat - playerData.gameSeat);

            }

            gamePlayerInfo = SeatGamePlayerInfoList[seatIndex];
            SeatButtonList[seatIndex].image.enabled = false;
        }
        else
        {
            //本地玩家
            gamePlayerInfo = SeatGamePlayerInfoList[0];
            gameData.thisData.LocalGamePlayerInfo = gamePlayerInfo;
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

        gameData.gamePlayerInfoList.Add(gamePlayerInfo);
        return gamePlayerInfo;
    }

    /// <summary>
    /// 有玩家退出房間
    /// </summary>
    /// <param name="id">退出玩家ID</param>
    /// <returns></returns>
    public GamePlayerInfo PlayerExitRoom(string id, bool allPlayersLeft = false)
    {
        GameRoomPlayerData playerLeft = gameControl.GetPlayerData(DataManager.UserId);
        if (playerLeft != null)
        {
            playerLeft.isPlayerLeft = true;
            Debug.Log("GameControl :: Player Who Left : " + playerLeft.nickname);
            var exitPlayer1 = new Dictionary<string, object>()
            {
                { DataManager.UserId, playerLeft},                 //遊戲中玩家ID
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{gameControl.QueryRoomPath}/{FirebaseManager.PLAYERS_WHO_LEFT}", exitPlayer1);

            if (allPlayersLeft)
            {
                OnAllPlayerLeft();
            }
        }
        else
        {
            Debug.Log("GameControl :: Left Player Not Found");
        }

        GamePlayerInfo exitPlayer = GetPlayer(id);

        gameData.gamePlayerInfoList.Remove(exitPlayer);

        gameData.exitPlayerSeatList.Add(exitPlayer.SeatIndex);

        exitPlayer.gameObject.SetActive(false);

        //if (RoomType == TableTypeEnum.IntegralTable)
        //{
        //    SetBattleResult(true);
        //}

        return exitPlayer;
    }

    void OnAllPlayerLeft()
    {
        print("所有玩家離線");
        GetRoundCount();
        StartCoroutine(SaveResult(gameData.gameRoomData, true));
        print("遊戲結果(所有玩家離開): " + gameData.saveResultData);
        AppApi.OnRoundFinish(gameData.saveResultData, (x) => { Debug.Log("Round Finished"); });
        SaveResultDataToFirebase();
        IncrementRoundCount();
    }

    /// <summary>
    /// 獲取玩家
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GamePlayerInfo GetPlayer(string id)
    {
        return gameData.gamePlayerInfoList.Where(x => x.UserId == id).FirstOrDefault();
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
        double buyValue = gameRoomData.betActionDataDic.buyActionValue;
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
            case BetActingEnum.AddChip:
                PlaySound("SoundCall");
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
                    actionButtons.SetAutoAction(false);
                    actionButtons.SetActingButtonEnable = false;
                    actionButtons.ShowRaise = false;
                    GameRoomManager.Instance.EnanbleBtn(false);
                    Menu_Btn.interactable = true;
                    gameData.thisData.isFold = true;
                    gameData.thisData.IsPlaying = false;
                    break;

                //All In
                case BetActingEnum.AllIn:
                    actionButtons.SetActingButtonEnable = false;
                    break;
            }
        }

        GamePlayerInfo playerInfo = GetPlayer(id);

        playerInfo.InitCountDown();

        if (playerInfo != null &&
            playerInfo.gameObject.activeSelf)
        {
            if (actionEnum != BetActingEnum.AddChip)
            {
                playerInfo.PlayerAction(actionEnum,
                                        betValue,
                                        chips);
            }
            else
            {
                playerInfo.PlayerAction(actionEnum,
                                        buyValue,
                                        chips);
            }

            if (actionEnum == BetActingEnum.AllIn)
                playerInfo.allInHalo.Play();
        }

        //本地玩家有參與
        if (gameData.thisData.LocalGamePlayerInfo.IsPlaying &&
            playerInfo != null)
        {
            //紀錄存檔
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = playerInfo.SeatIndex;
            processStepHistoryData.ActionIndex = (int)actionEnum;

            gameData.processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }
    }

    /// <summary>
    /// 下注籌碼集中
    /// </summary>
    /// <returns></returns>
    public IEnumerator IConcentrateBetChips()
    {
        for (int i = 0; i < gameData.gamePlayerInfoList.Count; i++)
        {
            if (gameData.gamePlayerInfoList[i].GetBetChipsActive == true)
            {
                yield return new WaitForSeconds(1);

                foreach (var player in gameData.gamePlayerInfoList)
                {
                    player.ConcentrateBetChips(gamePot.PotTransform.position);
                }
                chipsTween.ConcentrateChips();

                yield return new WaitForSeconds(0.5f);

                break;
            }
        }

        //顯示底池籌碼
        gamePot.PotActive = true;
    }
    /// <summary>
    /// 翻開公共牌
    /// </summary>
    /// <param name="currCommunityPoker"></param>
    /// <returns></returns>
    public IEnumerator IFlopCommunityPoker(List<int> currCommunityPoker)
    {
        foreach (var player in gameData.gamePlayerInfoList)
        {
            if (!player.IsFold && !player.IsAllIn)
            {
                player.CurrBetAction = BetActionEnum.None;
            }
        }

        if (currCommunityPoker != null)
        {
            gameData.thisData.CurrCommunityPoker = currCommunityPoker;
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

            gameData.processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }
        Debug.Log("Collecting Pot");
        yield return IConcentrateBetChips();

        //播放翻牌動畫
        if (gameData.gameRoomData.currCommunityPoker?.Count == 3)
        {
            if (!communityPoker.GetPoker(0).gameObject.activeSelf)
            {
                tweenManager.inst.playCommunity();
                yield return new WaitForSeconds(0.25f * gameData.gameRoomData.currCommunityPoker.Count);
            }
        }
        else if (gameData.gameRoomData.currCommunityPoker?.Count == 5)
        {
            if (!communityPoker.GetPoker(0).gameObject.activeSelf)
            {
                tweenManager.inst.playCommunity5();
                yield return new WaitForSeconds(0.15f * gameData.gameRoomData.currCommunityPoker.Count);
            }
        }

        if (currCommunityPoker != null)
        {
            for (int i = 0; i < currCommunityPoker.Count; i++)
            {
                if (communityPoker.GetPoker(i).gameObject.activeSelf == false)
                {
                    PlaySound("SoundShowCard");
                    communityPoker.Show(i, true);
                    communityPoker.Set(i, currCommunityPoker[i]);
                    StartCoroutine(communityPoker.GetPoker(i).IHorizontalFlopEffect(currCommunityPoker[i]));
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        yield return new WaitForSeconds(0.6f);

        //判斷牌行
        if (gameData.thisData.IsPlaying == true)
        {
            GamePlayerInfo localPlayerInfo = gameData.gamePlayerInfoList.Where(x => x.UserId == DataManager.UserId)
                                                               .FirstOrDefault();
            JudgePokerShapeUI(localPlayerInfo,
                            true);
        }
    }

    /// <summary>
    /// 判斷牌型
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isOpenMatchPokerFrame">是否開啟符合的撲克框</param>
    /// <param name="isWinEffect">贏家效果</param>
    private void JudgePokerShapeUI(GamePlayerInfo player, bool isOpenMatchPokerFrame, bool isWinEffect = false)
    {
        // Get player's hand cards
        Poker[] handPoker = player.GetHandPoker;
        List<int> judgePoker = handPoker.Select(p => p.PokerNum).ToList();

        Debug.Log($"[JudgePokerShapeUI] Player: {player.Nickname} | Hand Cards Count: {handPoker.Length}");

        // Validate hand cards and community cards
        if (judgePoker != null && gameData.thisData.CurrCommunityPoker != null)
        {
            // Combine player's hand cards with community cards if when game is after flop
            //judgePoker = isStart ? judgePoker : judgePoker.Concat(gameData.thisData.CurrCommunityPoker).ToList();
            judgePoker = judgePoker.Concat(gameData.thisData.CurrCommunityPoker).ToList();
            Debug.Log($"[JudgePokerShapeUI] Combined Cards: {string.Join(", ", judgePoker)}");

            // Combine hand cards and community cards as Poker objects
            List<Poker> allPokers = handPoker.Concat(communityPoker.GetList()).ToList();

            // Disable visual effects for all cards
            foreach (var poker in allPokers)
            {
                poker.PokerEffectEnable = true;
            }

            // Call JudgePokerShape to determine the hand shape
            PokerShape.JudgePokerShape(judgePoker, (resultIndex, matchPokerList) =>
            {
                Debug.Log($"[JudgePokerShapeUI] Player: {player.Nickname} | Result Index: {resultIndex} | Matched Cards Count: {matchPokerList.Count}");
                // Verify if the player's cards are active
                if (player.GetHandPoker[0].gameObject.activeSelf)
                {
                    // Set player's poker shape
                    player.SetPokerShapeStr(resultIndex);
                    var winRateCalc = new PokerWinRateCalculator(handPoker.Select(p => p.PokerNum).ToList(), gameData.thisData.CurrCommunityPoker);
                    if (gameData.thisData.CurrCommunityPoker.Count != 0)
                    {
                        winRateCalc.CalculateWinRate((res) =>
                        {
                            print("勝率: " + res);
                            GetPlayer(DataManager.UserId).setWinRate = res;
                        });
                    }

                    if (resultIndex < PokerShape.HandRanks.Count)
                    {
                        Debug.Log($"[JudgePokerShapeUI] Valid Result Index: {resultIndex} : ");

                        // Open Match Poker Frame if enabled
                        if (isOpenMatchPokerFrame)
                        {
                            bool isStraight = resultIndex == 6 || resultIndex == 2 || resultIndex == 1 ? true : false;
                            bool _isFlush = resultIndex == 5 || resultIndex == 2 || resultIndex == 1 ? true : false;
                            PokerShape.OpenMatchPokerFrame(allPokers, HighlightCard(allPokers, matchPokerList, isStraight, _isFlush), isWinEffect);

                            // Set winner details if win effects are enabled
                            if (isWinEffect)
                            {
                                player.PokerShapeIndex = resultIndex;
                                gamePot.SetWinnerStringTxt(LanguageManager.Instance.GetText(
                                    AssetsManager.Instance.GetStringAlbumAsset(StringAlbumEnum.HandRanksStringAlbum).strAlbum[resultIndex]));
                                foreach (var player in SeatGamePlayerInfoList)
                                    player.IsWinEffect = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[JudgePokerShapeUI] Invalid Result Index: {resultIndex}. HandRank not found.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[JudgePokerShapeUI] Player: {player.name} | Hand is inactive. Skipping judgment.");
                }
            });
        }
        else
        {
            Debug.LogError($"[JudgePokerShapeUI] Invalid data: judgePoker or CurrCommunityPoker is null for player {player.Nickname}.");
        }
    }

    List<int> HighlightCard(List<Poker> allPokers, List<int> cards, bool isStraight, bool isFlush)
    {
        List<int> resList;
        var data = gameControl.judgePoker.CalculateRank(cards, out resList, isStraight, isFlush);
        gameControl.judgePoker.OpenMatchPokerFrame(allPokers, resList);

        List<int> myCards = new List<int>();
        List<int> myRank = new List<int>();

        foreach (var item in data)
        {
            myRank = item.Key; // Rank (e.g., [14, 13, 12, 11, 10])
            myCards = item.Value; // Cards used to form the rank
            break; // Exit after the first item
        }

        Debug.Log("My Cards :: " + string.Join(",", myCards));
        Debug.Log("My Ranks :: " + string.Join(",", myRank));

        return myCards.Take(5).ToList();
    }

    /// <summary>
    /// 主池結果
    /// </summary>
    /// <param name="gameRoomData"></param>
    /// <returns></returns>
    public IEnumerator IPotResult(GameRoomData gameRoomData)
    {
        if (gameRoomData == null)
        {
            Debug.LogError("IPotResult: gameRoomData is null.");
            yield break;
        }

        gameData.thisData.IsPlaying = false;
        gameData.isOnFold = true;
        actionButtons.SetActingButtonEnable = false;
        gameData.thisData.CurrCommunityPoker.Clear();

        // Execute betting and reveal community cards
        yield return IConcentrateBetChips();

        yield return IFlopCommunityPoker(gameRoomData.currCommunityPoker);

        // Check if only one player remains
        bool isOnePlayerLeft = gameRoomData.playingPlayersIdList.Count - gameControl.GetFoldPlayer().Count == 1;

        if (!isOnePlayerLeft)
        {
            // Show each player's hand and evaluate hand shapes
            foreach (var playerId in gameRoomData.playingPlayersIdList)
            {
                if (gameRoomData.playerDataDic.TryGetValue(playerId, out var playerData) &&
                    playerData.gameState != (int)PlayerStateEnum.Waiting &&
                    playerData.gameState != (int)PlayerStateEnum.Fold)
                {
                    var player = GetPlayer(playerId);
                    if (player != null)
                    {
                        player.SetHandPoker(playerData.handPoker[0], playerData.handPoker[1], "PotResult");
                        JudgePokerShapeUI(player, false);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        BGMask.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        if (!gameRoomData.potWinData?.isHaveSide ?? true)
        {
            yield return DisplayAndDistributeMainPot();
            yield return DisplayRoomFeeAll();
            yield return ShowResult();
            yield return new WaitForSeconds(0.8f);
            yield return SaveResult(gameRoomData);
        }
    }

    IEnumerator DisplayRoomFeeAll()
    {
        foreach (var winner in gameControl.winnersRoomFee)
        {
            if (winner == null || winner.roomFee <= 0) continue;

            var player = GetPlayer(winner.userId);
            if (player != null)
            {
                //player.SetRoomFee($"Room Fee - ${winner.roomFee:f2}");
                yield return new WaitForSeconds(0.1f);
                //player.HideRoomFee();
                //playerWinValueList[player.UserId] = playerWinValueList[player.UserId] - winner.roomFee;
            }
        }
        gamePot.TotalPot = 0;
    }

    IEnumerator DisplayAndDistributeMainPot()
    {
        double changeValue = 0;
        // Set the main pot win chips value
        gameData.thisData.PowWinChips = gameData.gameRoomData.potWinData.potWinChips;
        var sideWinChips = gameData.gameRoomData.sideWinData?.sideWinChips ?? 0;
        var totalPot = gameData.gameRoomData.potWinData.potWinChips + sideWinChips;

        // Open player info masks and display total pot
        foreach (var player in gameData.gamePlayerInfoList)
        {
            player.IsOpenInfoMask = true;
        }

        print("底池金額: " + totalPot);
        gamePot.TotalPotText = $"{LanguageManager.Instance.GetText("Pot")} {totalPot}";
        LayoutRebuilder.ForceRebuildLayoutImmediate(gamePot.TotalPot_Txt.GetComponentInParent<Image>().rectTransform);

        yield return new WaitForSeconds(0.5f);

        // Display the winning players and distribute the pot
        int index = 0;
        foreach (var potWinnerId in gameData.gameRoomData.potWinData.potWinnersId)
        {
            changeValue = gameData.gameRoomData.potWinData.potWinChips / gameData.gameRoomData.potWinData.potWinnersId.Count();
            if (potWinnerId == DataManager.UserId)
            {
                // Update local player's chips if they are a winner
                gameControl.UpdateLocalChips(changeValue);
            }

            CloseAllPokerEffect();
            GameRoomPlayerData playerData = gameData.gameRoomData.playerDataDic[potWinnerId];
            GamePlayerInfo player = GetPlayer(potWinnerId);
            player.IsOpenInfoMask = false;

            JudgePokerShapeUI(player, true, true);

            gameData.playerWinValueList.Add(potWinnerId, playerData.carryChips-player.PlayerRoomChips);

            Vector2 winnerSeatPos = player.gameObject.transform.position;
            chipsTween.Result(gameData.gameRoomData.potWinData.potWinnersId.Count, index, winnerSeatPos);
            gamePot.TotalPot = (int)(gameData.gameRoomData.sideWinData?.sideWinChips ?? 0);
            yield return new WaitForSeconds(0.1f);
            player.IsWinnerActive = false;
            index++;
        }

        yield return new WaitForSeconds(0.1f);

        // Display room fee for local player winners
        foreach (var potWinnerId in gameData.gameRoomData.potWinData.potWinnersId)
        {
            if (gameData.thisData.LocalGamePlayerInfo.IsPlaying)
            {
                ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
                processStepHistoryData.ActionPlayerIndex = -1;
                processStepHistoryData.ActionIndex = -1;

                processStepHistoryData.PotWinnerSeatList = gameData.gameRoomData.potWinData.potWinnersId
                    .Select(id => GetPlayer(id).SeatIndex).ToList();
                processStepHistoryData.PotWinChips = gameData.thisData.PowWinChips;

                gameData.processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
            }
        }
    }

    IEnumerator SaveResult(GameRoomData gameRoomData, bool isAllPlayerLeft = false)
    {
        gamePot.SetWinnerStringTxt("");
        BGMask.SetActive(false);
        if (gameRoomData == null || gameRoomData.playingPlayersIdList == null)
        {
            print (gameRoomData == null);
            print (gameRoomData.playingPlayersIdList == null);
            Debug.LogError("SaveResult: Invalid gameRoomData or missing player list.");
            yield break;
        }
        Debug.Log("GameView :: Init Save Result :: " + gameRoomData.playerDataDic.Count());
        // Initialize result data for saving
        print("遊戲結果(儲存遊戲結果): " + gameData.saveResultData);
        gameData.saveResultData = InitializeResultData(gameRoomData, isAllPlayerLeft);
        var allPlayers = gameRoomData.playerDataDic.AsEnumerable();

        if (gameRoomData.playersWhoLeft != null)
        {
            allPlayers = allPlayers.Concat(gameRoomData.playersWhoLeft);
        }

        foreach (var playerData in allPlayers)
        {
            if (playerData.Value == null)
                continue;

            // Check if the player is in the winners' room fee list
            var playerRoomFee = gameControl.winnersRoomFee.FirstOrDefault(x => x.userId == playerData.Value.userId);

            // Determine if the player is a pot winner
            bool isPotWinner = (gameRoomData.potWinData == null) ? false : gameRoomData.potWinData.potWinnersId.Contains(playerData.Value.userId);

            // Create player details based on their data
            var playerDetails = CreatePlayerDetails(playerData.Value, playerRoomFee, isPotWinner, isAllPlayerLeft);
            gameData.saveResultData.playerDetails.Add(playerDetails);
        }

        Debug.Log("SaveResult: Player details saved. Finalizing...");
        yield return new WaitForEndOfFrame();
    }

    private ResultHistoryData InitializeResultData(GameRoomData gameRoomData, bool isAllPlayerLeft)
    {
        string roomName = gameData.RoomType switch
        {
            TableTypeEnum.IntegralTable => "Integral",
            TableTypeEnum.Cash => "High Roller Battleground",
            TableTypeEnum.VCTable => "Classic Battle",
            _ => "Unknown Room"
        };

        return new ResultHistoryData
        {
            uniqueSerial = Guid.NewGuid().ToString(),
            roomType = roomName,
            smallBlind = gameRoomData.smallBlind,
            communityPoker = gameRoomData.currCommunityPoker ?? new List<int>(),
            dateTime = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"),
            roomId = DataManager.RoomId,
            tableId = DataManager.TableId,
            roundId = roundId,
            roundInsuranceFee = 0,
            roundInsurancePayAmount = 0,
            roundInsurancePayRate = 0,
            roundInsuranceResult = "",
            playerDetails = new List<PlayerDetails>()
        };
    }

    private PlayerDetails CreatePlayerDetails(GameRoomPlayerData playerData, RoomFee roomFeeData, bool isWinner = false, bool isPlayerLeft = false)
    {
        double potWinChips = 0;
        double sideWinChips = 0;
        double roomFee = 0;

        bool isBot = DataManager.UserId.Trim().StartsWith(FirebaseManager.ROBOT_ID.Trim(), StringComparison.OrdinalIgnoreCase);

        if (roomFeeData != null)
        {
            potWinChips = roomFeeData.potWinAmount;
            sideWinChips = roomFeeData.sidePotAmount;
            roomFee = Math.Round(roomFeeData.roomFee, 2);
        }

        if (playerData.userId == DataManager.UserId)
        {
            double totalSendAPI = potWinChips + sideWinChips - roomFee;
            print("遊戲結果(創建玩家資料): " + gameData.saveResultData);
            try { 
            NoodleApi.PostTableChipsTransaction(DataManager.UserId, gameData.saveResultData.roundId.ToString(),
                    totalSendAPI, 12, ChipTransactionType.Win, (x) =>
                    {
                        Debug.Log("Player Win ChipsTransaction Success");
                    },
                    (error) =>
                    {
                        Debug.LogError($"Player Win ChipsTransaction Failed Error: {error}");
                    });
            }
            catch
            {
                print("Result Data為空");
            }
        }


        return new PlayerDetails
        {
            playerId = playerData.userId,
            playerName = playerData.nickname,
            playerHandId = "",
            playerValidBetAmount = playerData.playerValidBetAmount,
            playerRoomFee = roomFee,
            tenantName = DataManager.TenantName,
            isBot = isBot,
            isPlayerLeft = isPlayerLeft,
            playerHandData = new PlayerHand
            {
                playerHand = playerData.handPoker ?? new List<int>(),
                playerCurrHandShape = GetPlayer(playerData.userId)?.pokerCurrShapeIndex ?? -1,
                potWinChips = potWinChips,
                sideWinChips = sideWinChips,
                isWinner = isWinner,
                seat = playerData.gameSeat.ToString(),
            }
        };
    }

    /// <summary>
    /// 邊池結果
    /// </summary>
    /// <param name="gameRoomData"></param>
    public IEnumerator SideResult(GameRoomData gameRoomData)
    {
        yield return DisplayAndDistributeMainPot();
        yield return DisplayAndDistributeSidePot();
        yield return DisplayRoomFeeAll();
        yield return ShowResult();
        yield return SaveResult(gameRoomData);
    }

    IEnumerator ShowResult()
    {
        foreach (KeyValuePair<string, double> playerWin in gameData.playerWinValueList)
        {
            GamePlayerInfo player = GetPlayer(playerWin.Key);
            player.IsWinnerActive = true;
            player.setWinnerDisplay($"WIN + ${playerWin.Value:f2}");
            print($"WIN: {playerWin.Value}");
            //獲勝籌碼物件
            //RectTransform rt = Instantiate(WinChipsObj, gamePot.PotTransform).GetComponent<RectTransform>();
            //rt.anchoredPosition = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator DisplayAndDistributeSidePot()
    {
        double changeValue = 0;
        gameData.thisData.SideWinnerList = new List<string>();
        gameData.thisData.SideWinChips = gameData.gameRoomData.sideWinData.sideWinChips / gameData.gameRoomData.sideWinData.sideWinnersId.Count();

        //開啟遮罩
        foreach (var player in gameData.gamePlayerInfoList)
        {
            player.IsOpenInfoMask = true;
            player.IsWinnerActive = false;
        }

        //邊池贏家效果
        //gameData.thisData.SideWinnerList = new List<string>();

        if (gameData.gameRoomData.sideWinData.sideWinChips > 0)
        {
            foreach (var sideWinnerId in gameData.gameRoomData.sideWinData.sideWinnersId)
            {
                changeValue = gameData.gameRoomData.sideWinData.sideWinChips / gameData.gameRoomData.sideWinData.sideWinnersId.Count();
                //本地玩家
                if (sideWinnerId == DataManager.UserId)
                {
                    //更新用戶籌碼資料
                    gameControl.UpdateLocalChips(changeValue);
                }

                CloseAllPokerEffect();

                GameRoomPlayerData playerData = gameData.gameRoomData.playerDataDic
                                                             .FirstOrDefault(x => x.Value.userId == sideWinnerId)
                                                             .Value;

                if (playerData == null)
                {
                    continue; // Skip if playerData is null
                }

                gameData.thisData.SideWinnerList.Add(sideWinnerId);

                GamePlayerInfo player = GetPlayer(sideWinnerId);

                player.IsOpenInfoMask = false;

                if (gameData.playerWinValueList.ContainsKey(sideWinnerId))
                {
                    continue;
                }
                else
                    gameData.playerWinValueList.Add(sideWinnerId, playerData.carryChips - player.PlayerRoomChips);
            }

            yield return new WaitForSeconds(0.1f);
        }

        //顯示退回籌碼
        gameData.thisData.BackChipsDic = new Dictionary<int, double>();
        if (gameData.gameRoomData.sideWinData.backChipsData != null)
        {
            foreach (var backChipsData in gameData.gameRoomData.sideWinData.backChipsData.Values)
            {
                if (backChipsData.backChipsValue > 0)
                {
                    //本地玩家
                    if (backChipsData.backUserId == DataManager.UserId)
                    {
                        //更新用戶籌碼資料
                        changeValue = backChipsData.backChipsValue;
                        gameControl.UpdateLocalChips(changeValue);
                    }

                    GamePlayerInfo player = GetPlayer(backChipsData.backUserId);
                    if (player != null)
                    {
                        player.PlayerRoomChips += backChipsData.backChipsValue;
                        player.SetBackChips = backChipsData.backChipsValue;
                        gameData.thisData.BackChipsDic.Add(player.SeatIndex, backChipsData.backChipsValue);

                        GameRoomPlayerData playerData = gameData.gameRoomData.playerDataDic
                                                                     .FirstOrDefault(x => x.Value.userId == backChipsData.backUserId)
                                                                     .Value;

                        if (playerData != null)
                        {
                            player.PlayerRoomChips = playerData.carryChips;
                        }
                    }
                }
            }
        }

        //邊池紀錄存檔
        if (gameData.thisData.LocalGamePlayerInfo.IsPlaying &&
            gameData.thisData.SideWinnerList.Count > 0)
        {
            ProcessStepHistoryData processStepHistoryData = AddNewStepHistory();
            processStepHistoryData.ActionPlayerIndex = -1;
            processStepHistoryData.ActionIndex = -1;

            processStepHistoryData.SildWinnerSeatList = new List<int>();
            foreach (var id in gameData.thisData.SideWinnerList)
            {
                GamePlayerInfo winnerPlayer = GetPlayer(id);
                if (winnerPlayer != null)
                {
                    int sideWinSeat = winnerPlayer.SeatIndex;
                    processStepHistoryData.SildWinnerSeatList.Add(sideWinSeat);
                }
            }
            processStepHistoryData.SildWinChips = gameData.thisData.SideWinChips;
            processStepHistoryData.BackChipsDic = gameData.thisData.BackChipsDic;

            gameData.processHistoryData.processStepHistoryDataList.Add(processStepHistoryData);
        }
    }

    /// <summary>
    /// 關閉所有撲克效果
    /// </summary>
    private void CloseAllPokerEffect()
    {
        List<Poker> playersPoker = new List<Poker>();
        foreach (var p in gameData.gamePlayerInfoList)
        {
            foreach (var poker in p.GetHandPoker)
            {
                playersPoker.Add(poker);
            }
        }
        List<Poker> allPokerList = communityPoker.GetList().Concat(playersPoker.ToList()).ToList();
        foreach (var poker in allPokerList)
        {
            poker.PokerEffectEnable = true;
        }
    }

    /// <summary>
    /// 遊戲階段
    /// </summary>
    /// <param name="gameRoomData">遊戲房間資料</param>
    /// <param name="smallBlind">小盲值</param>
    /// <returns></returns>
    public IEnumerator IGameStage(GameRoomData gameRoomData, double smallBlind)
    {
        actionButtons.AutoActionState = AutoActingEnum.None;
        gameData.thisData.SmallBlindValue = smallBlind;
        gameData.thisData.CurrRaiseValue = gameData.thisData.SmallBlindValue * 2;

        //重製玩家行動文字顯示
        if ((GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.PotResult)
        {
            //階段=遊戲結果
            foreach (var player in gameData.gamePlayerInfoList)
            {
                player.DisplayBetAction(false);
            }
        }
        else
        {
            //翻牌階段
            foreach (var player in gameData.gamePlayerInfoList)
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

                actionButtons.SetSitOutDisplay();

                break;

            //大小盲
            case GameFlowEnum.SetBlind:
                actionButtons.SetActingButtonEnable = gameData.thisData.IsPlaying;
                actionButtons.ShowActionBtns = gameData.thisData.IsPlaying;

                break;

            //翻牌
            case GameFlowEnum.Flop:

                break;

            //轉牌
            case GameFlowEnum.Turn:

                break;

            //河牌
            case GameFlowEnum.River:

                break;

            //主池結果
            case GameFlowEnum.PotResult:
                //棄牌顯示手牌按鈕
                if (gameData.thisData.isFold == true)
                {
                    actionButtons.ShowPokerList();
                }
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
        gameData.thisData.IsPlaying = false;

        HistoryVideoView historyVideoView = GameObject.FindAnyObjectByType<HistoryVideoView>();
        if (historyVideoView != null)
        {
            Destroy(historyVideoView.gameObject);
        }

        //顯示購買金幣/積分結果
        if (gameData.RoomType == TableTypeEnum.Cash ||
            gameData.RoomType == TableTypeEnum.VCTable)
        {
            gameMenu.OpenBuyChipView(gameControl, false, gameData.gameRoomData.smallBlind, transform.name, gameData.RoomType, InsufficientChipsBuyChipsCallback);

            gameData.thisData.LocalGamePlayerInfo.Init();
            gameData.thisData.LocalGamePlayerInfo.IsOpenInfoMask = true;
            gamePot.ShowWaitingTip = true;
        }
    }

    /// <summary>
    /// 籌碼不足購買籌碼回傳
    /// </summary>
    /// <param name="buyValue"></param>
    private void InsufficientChipsBuyChipsCallback(double buyValue)
    {
        gameMenu.CloseBuyChipView();
        gameControl.PreBuyChipsValue = Math.Floor(buyValue);
        gameControl.UpdateCarryChips();

        if (gameData.gameRoomData.hostId != DataManager.UserId)
        {
            gameControl.JudgeHost();
        }
    }

    /// <summary>
    /// 購買籌碼
    /// </summary>
    /// <param name="buyValue"></param>
    public void BuyChips(double buyValue)
    {
        gameMenu.CloseBuyChipView();
        ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Sending,
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
        gameMenu.CloseBuyChipView();

        double newChips = gameData.gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                    .FirstOrDefault()
                                                    .Value
                                                    .carryChips;
        gameData.thisData.LocalGamePlayerInfo.PlayerRoomChips = newChips;
    }

    #region 聊天

    public void ReciveChat(ChatData chatData)
    {
        gameChat.ReciveChat(chatData);
    }

    #endregion

    #region 記錄存檔

    /// <summary>
    /// 上一局遊戲紀錄存檔
    /// </summary>
    private void SavePreGame()
    {
        // Check if the local player is playing and all necessary data is available
        print("遊戲結果(上一局遊戲紀錄存檔): " + gameData.saveResultData);
        if (gameData.thisData?.LocalGamePlayerInfo?.IsPlaying == true &&
            gameData.saveResultData != null &&
            gameData.gameInitHistoryData != null &&
            gameData.processHistoryData != null)
        {
            // If the current player is the host, save data to Firebase
            if (gameData.gameRoomData.hostId == DataManager.UserId)
            {
                print("本地玩家是否為房主: " + gameData.gameRoomData.hostId == DataManager.UserId);
                GetRoundCount();
                AppApi.OnRoundFinish(gameData.saveResultData, (x) => { Debug.Log("Round Finished"); });
                SaveToFirebase(nameof(gameData.saveResultData), gameData.saveResultData, nameof(GameResultDataSaveToFirebase));
                SaveToFirebase(nameof(gameData.gameInitHistoryData), gameData.gameInitHistoryData, nameof(GameInitDataSaveToFirebase));
                SaveToFirebase(nameof(gameData.processHistoryData), gameData.processHistoryData, nameof(GameProcessDataSaveToFirebase));
                IncrementRoundCount();
            }
            // Save data by player on Firebase
            HandHistoryManager.Instance.SaveResult(gameData.saveResultData);
            HandHistoryManager.Instance.SaveGameInit(gameData.gameInitHistoryData);
            HandHistoryManager.Instance.SaveProcess(gameData.processHistoryData);
        }

        // Reset exit player seat list and process history data
        if (gameData.gameRoomData.playersWhoLeft != null) //防clear失敗
        {
            gameData.gameRoomData.playersWhoLeft.Clear();
        }
        var gameRoomData1 = new Dictionary<string, object>()
        {
            { FirebaseManager.PLAYERS_WHO_LEFT, gameData.gameRoomData.playersWhoLeft},                 //遊戲中玩家ID
        };
        gameControl.UpdateGameRoomData(gameRoomData1, () =>
        {
            gameData.exitPlayerSeatList = new List<int>();
            gameData.processHistoryData = new ProcessHistoryData
            {
                processStepHistoryDataList = new List<ProcessStepHistoryData>()
            };

            // Update hand history view if available
            GameObject.FindAnyObjectByType<HandHistoryView>()?.UpdateHitoryDate();
        });
    }

    public void SaveResultDataToFirebase()
    {
        print("遊戲結果(儲存結果進火庫): " + gameData.saveResultData);
        SaveToFirebase(nameof(gameData.saveResultData), gameData.saveResultData, nameof(GameResultDataSaveToFirebase));
    }

    private void SaveToFirebase(string dataName, object data, string callbackMethodName)
    {
        string json = JsonConvert.SerializeObject(data);
        JSBridgeManager.Instance.WriteDataToFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.ROUND_DATA_PATH}/{DataManager.RoomId}/rounds/round_{roundId}/{dataName}",
            json,
            gameObject.name,
            callbackMethodName, true);
    }

    // Callbacks for Firebase save completion
    void GameInitDataSaveToFirebase() => Debug.Log("GameView :: GameInitDataSavedToFirebase");
    void GameProcessDataSaveToFirebase() => Debug.Log("GameView :: GameProcessDataSavedToFirebase");
    void GameResultDataSaveToFirebase() => Debug.Log("GameView :: GameResultDataSavedToFirebase");

    private int roundId = 0; // This could be loaded from Firebase if persistent

    // Function to get the round count
    public void GetRoundCount()
    {
        Debug.Log(nameof(GetRoundCount));
        JSBridgeManager.Instance.ReadDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.ROUND_DATA_PATH}/{DataManager.RoomId}/roundCount", roomName, nameof(OnGetRoundCount));
    }

    // Callback for getting the round count
    public void OnGetRoundCount(string data)
    {
        if (int.TryParse(data, out int roundCount))
        {
            roundId = roundCount; // Set roundId to the current count
            Debug.Log($"Current roundId set to: {roundId}");
        }
        else
        {
            roundId = 0;
            Debug.Log("Failed to parse round count.");
        }
    }
    private void IncrementRoundCount()
    {
        int newCount = (roundId + 1) % 9999; // Increment and wrap around at 10000

        // Prepare the new round count to update
        Dictionary<string, object> roundCountUpdate = new Dictionary<string, object>
    {
        { "roundCount", newCount }
    };

        // Update the round count in Firebase
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.ROUND_DATA_PATH}/{DataManager.RoomId}", roundCountUpdate, nameof(OnRoundCountUpdated));

        Debug.Log($"Round count updated to {newCount}.");
    }

    // Callback for when the round count is updated
    public void OnRoundCountUpdated()
    {
        Debug.Log("Round count updated successfully.");
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
        foreach (var player in gameData.gamePlayerInfoList)
        {
            processStepHistoryData.SeatList.Add(player.SeatIndex);
            processStepHistoryData.ChipsList.Add(player.CurrRoomChips);
            processStepHistoryData.BetChipsList.Add(player.CurrBetValue);
            processStepHistoryData.HandPoker1.Add(player.GetHandPoker[0].PokerNum);
            processStepHistoryData.HandPoker2.Add(player.GetHandPoker[1].PokerNum);
            processStepHistoryData.BetActionEnumIndex.Add(Convert.ToInt32(player.CurrBetAction));
        }
        processStepHistoryData.CommunityPoker = gameData.thisData.CurrCommunityPoker;
        processStepHistoryData.TotalPot = gameData.gameRoomData.potChips;
        processStepHistoryData.ExitPlayerSeatList = gameData.exitPlayerSeatList;
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
        if (gameData.gameRoomData == null ||
            gameData.gameRoomData.playerDataDic == null)
        {
            return;
        }

        foreach (var player in gameData.gameRoomData.playerDataDic.Values)
        {
            GamePlayerInfo playerInfo = gameData.gamePlayerInfoList.Where(x => x.UserId == player.userId)
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
        gameData.gameRoomData = gameRoomData;
        //當前小盲值
        gameData.thisData.SmallBlindValue = gameRoomData.smallBlind;
        gamePot.SBBlinds = $"BLINDS: ${gameData.thisData.SmallBlindValue}/{gameData.thisData.SmallBlindValue * 2}";

        //底池
        if (gameRoomData.currGameFlow != (int)GameFlowEnum.PotResult &&
            gameRoomData.currGameFlow != (int)GameFlowEnum.SideResult)
        {
            if (gamePot.TotalPotText != StringUtils.SetChipsUnit(Math.Floor(gameRoomData.potChips)))
            {
                StringUtils.ChipsChangeEffect(gamePot.TotalPotTextUI, Math.Floor(gameRoomData.potChips), "$");
            }
        }

        if (gameRoomData.currGameFlow < (int)GameFlowEnum.Flop)
        {
            //公共牌
            for (int i = 0; i < communityPoker.GetList().Count(); i++)
            {
                communityPoker.Show(i, false);
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

            // Initialize player's hand and seat character
            gamePlayerInfo.SwitchShoHandPoker(new List<int> { -1, -1 });
            gamePlayerInfo.SetShowHandPoker(false, new List<int> { -1, -1 });
            gamePlayerInfo.Init();
            gamePlayerInfo.SetSeatCharacter(SeatCharacterEnum.None); // Reset seat character

            print($"正在處理玩家: {gamePlayerInfo.Nickname}");
            if (gamePlayerInfo.CurrRoomChips <= 0)
            {
                print($"玩家 {gamePlayerInfo.Nickname} 無法完成動畫流程，跳過處理！");
                continue;
            }
            //Play Dealcard anim
            tweenManager.inst.setSeats(() =>
            {
                // Set hand cards for local player (UserId matches local player)
                if (userId == DataManager.UserId)
                {
                    GameRoomPlayerData playerData = gameRoomData.playerDataDic.FirstOrDefault(x => x.Value.userId == DataManager.UserId).Value;

                    if (playerData != null && !playerData.isSitOut && playerData.gameState != (int)PlayerStateEnum.Waiting)
                    {
                        // Local player is actively playing
                        gameData.thisData.IsPlaying = true;

                        // Set local player's hand poker cards
                        if (!gameData.isDealed)
                        {
                            print("開牌");
                            gamePlayerInfo.SetHandPoker(playerData.handPoker[0], playerData.handPoker[1], "OnLicensing");
                            gameData.isDealed = true;
                        }

                        // Hide waiting tip
                        gamePot.ShowWaitingTip = false;

                        // Judge the local player's poker hand shape
                        if (gameRoomData.playingPlayersIdList.Contains(DataManager.UserId))
                        {
                            this.iInvoke(nameof(delay2JudgeHand), 0.5f, gamePlayerInfo);
                        }
                    }
                }
                else
                {
                    // For other players, hide their hand and poker shape
                    gamePlayerInfo.SetHandPoker(-1, -1, "hideCard");
                    gamePlayerInfo.SetPokerShapeImage = null;
                }
            });
        }

        // If the local player is the host
        if (gameRoomData.hostId == DataManager.UserId)
        {
            // Set Button seat
            GameRoomPlayerData buttonPlayerData = gameRoomData.playerDataDic.FirstOrDefault(x => x.Value.gameSeat == gameRoomData.buttonSeat).Value;
            UpdatePlayerSeat(buttonPlayerData.userId, SeatCharacterEnum.Button);

            GameRoomPlayerData sbPlayerData;
            GameRoomPlayerData bbPlayerData;

            // Only one active player, assign SB and BB to button player
            if (gameRoomData.playingPlayersIdList.Count == 1)
            {
                sbPlayerData = buttonPlayerData;
                bbPlayerData = buttonPlayerData;
            }
            // If there are only two players, assign SB to Button player and BB to the next player
            else if (gameRoomData.playingPlayersIdList.Count == 2)
            {
                bbPlayerData = buttonPlayerData;
                sbPlayerData = gameControl.GetNextPlayer(gameRoomData.buttonSeat);
            }
            // For three or more players, assign SB and BB accordingly
            else
            {
                sbPlayerData = gameControl.GetNextPlayer(gameRoomData.buttonSeat);
                bbPlayerData = gameControl.GetNextPlayer(sbPlayerData.gameSeat);
            }

            // Update SB and BB seats in the database
            UpdatePlayerSeat(sbPlayerData.userId, SeatCharacterEnum.SB);
            UpdatePlayerSeat(bbPlayerData.userId, SeatCharacterEnum.BB);

            // Set the current bet amounts for SB and BB
            gameRoomData.playerDataDic[sbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind;
            gameRoomData.playerDataDic[bbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind * 2;
        }
    }
    void delay2JudgeHand(GamePlayerInfo gamePlayerInfo)
    {
        JudgePokerShapeUI(gamePlayerInfo, true);
    }

    // Helper method to update the player's seat character in Firebase
    private void UpdatePlayerSeat(string userId, SeatCharacterEnum seatCharacter)
    {
        var dataDic = new Dictionary<string, object>
    {
        { FirebaseManager.SEAT_CHARACTER, (int)seatCharacter }
    };
        gameControl.UpdataPlayerData(userId, dataDic);
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
        tweenManager.inst.DPosAnim(buttonPlayer.SeatIndex);

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
        if (sbPlayer.UserId == DataManager.UserId)
        {
            print("遊戲結果(小盲流程): " + gameData.saveResultData);
            try { 
            NoodleApi.PostTableChipsTransaction(DataManager.UserId, gameData.saveResultData.roundId.ToString(), gameData.thisData.SmallBlindValue, 3, ChipTransactionType.SmallBlind, (x) =>
            {
                Debug.Log("SB Table ChipsTransaction Success");
            },
                 (error) =>
                 {
                     Debug.LogError($"SB Table ChipsTransaction Failed Error: {error}");
                 });
            }
            catch
            {
                print("Result Data為空");
            }
        }

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
        BetActingEnum betActingEnum = BetActingEnum.Blind;
        bool bbIsAllIn = false;
        if (bbPlayerData.carryChips - (gameRoomData.smallBlind * 2) == 0)
        {
            betActingEnum = BetActingEnum.AllIn;
            bbIsAllIn = true;
            //gameControl.UpdateBetAction(bbPlayer.UserId, betActingEnum, bbPlayerData.carryChips);
        }

        bbPlayer.PlayerAction(betActingEnum,
                              gameRoomData.smallBlind * 2,
                              bbPlayerData.carryChips - (gameRoomData.smallBlind * 2));

        if (bbPlayer.UserId == DataManager.UserId)
        {
            print("遊戲結果(大盲流程): " + gameData.saveResultData);
            try{
                NoodleApi.PostTableChipsTransaction(DataManager.UserId, gameData.saveResultData.roundId.ToString(), gameData.thisData.SmallBlindValue * 2, 2, ChipTransactionType.BigBlind, (x) =>
                {
                    Debug.Log("BB Table ChipsTransaction Success");
                },
                    (error) =>
                    {
                        Debug.LogError($"Call Table ChipsTransaction Failed Error: {error}");
                    });
            }
            catch
            {
                print("沒有Result Data");
            }
        }

        if (DataManager.UserId == bbPlayerData.userId)
        {
            gameControl.UpdateLocalChips(-gameRoomData.smallBlind * 2);
        }

        gameRoomData.playerDataDic[sbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind;
        gameRoomData.playerDataDic[bbPlayerData.userId].currAllBetChips = gameRoomData.smallBlind * 2;
        actionButtons.SetActionButton = false;

        gameData.thisData.TotalPot = gameRoomData.smallBlind + (gameRoomData.smallBlind * 2);

        //遊戲紀錄
        gameData.gameInitHistoryData = HandHistoryManager.Instance.SetGameInitData(gameData.gamePlayerInfoList,
                                                                          gameData.thisData.TotalPot);

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
            if (!bbIsAllIn)
            {
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CARRY_CHIPS, bbNewCarryChips },                           //攜帶籌碼
                    { FirebaseManager.CURR_ALL_BET_CHIPS, gameRoomData.smallBlind * 2},         //當前流程總下注籌碼
                    { FirebaseManager.ALL_BET_CHIPS, gameRoomData.smallBlind * 2},              //該局總下注籌碼
                };
            }
            else
            {
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CARRY_CHIPS, bbNewCarryChips },                           //攜帶籌碼
                    { FirebaseManager.CURR_ALL_BET_CHIPS, gameRoomData.smallBlind * 2},         //當前流程總下注籌碼
                    { FirebaseManager.ALL_BET_CHIPS, gameRoomData.smallBlind * 2},              //該局總下注籌碼
                    { FirebaseManager.GAME_STATE, (int)PlayerStateEnum.AllIn},              //該局總下注籌碼
                };
                if (bbPlayer.UserId == DataManager.UserId)
                    actionButtons.ShowActionBtns = false;
            }
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
        gameData.thisData = null;
        StopAllCoroutines();
    }

    void checkIsIdle()
    {
        if (PlayerPrefs.GetString("PlayerIsOnline") == "False")
        {
            int idleC = PlayerPrefs.GetInt("idleCount");
            PlayerPrefs.SetString("PlayerIsOnline", "True");
            idleC++;
            PlayerPrefs.SetInt("idleCount", idleC);
            PlayerPrefs.Save();

            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.IS_SIT_OUT, true},         //是否保留座位離開
            };
            gameControl.UpdataPlayerData(DataManager.UserId,
                                            data);

            print("是否閒置");
            gameControl.idleExit();
        }
    }


    //多桌遮罩
    public void SetGameMask(bool isShow)
    {
        GameMask.SetActive(isShow);
    }

    public void SetTopBar(bool isShow)
    {
        if (!Application.isMobilePlatform)
        {
            TopBar.SetActive(false);
        }
        else
        {
            TopBar.SetActive(isShow);
        }
    }
#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            gameControl.JudgeHost();
            print("編輯器離房");
            gameControl.ExitGame();
        }
    }
#endif
}

public static class customMono
{
    /// <summary>
    /// 擴充Invoke
    /// </summary>
    /// <param name="target"></param>
    /// <param name="methodName"></param>
    /// <param name="time"></param>
    /// <param name="value"></param>
    /// <returns>void</returns>
    public static void iInvoke(this MonoBehaviour target, string methodName, float time, object value)
    {
        if (target == null)
        {
            Debug.LogError("Target MonoBehaviour is null.");
            return;
        }
        target.StartCoroutine(InvokeWithParams(target, methodName, time, value));
    }

    private static IEnumerator InvokeWithParams(MonoBehaviour target, string methodName, float time, object value)
    {
        // 等待指定時間
        yield return new WaitForSeconds(time);

        // 使用 SendMessage 調用指定方法，並傳遞參數
        if (!string.IsNullOrEmpty(methodName))
        {
            target.gameObject.SendMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogError("Method name cannot be null or empty.");
        }
    }
}