using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LobbyView : MonoBehaviour
{
    [SerializeField]
    public Request_LobbyView baseRequest;

    [Header("遊戲測試")]
    [SerializeField]
    Button OpenGameTest_Btn;
    [SerializeField]
    Toggle GameTest_Tog;

    [Header("用戶訊息")]
    [SerializeField]
    TextMeshProUGUI Nickname_Txt, Stamina_Txt,
                    CryptoChips_Txt;

    [Header("用戶資源列表")]
    [SerializeField]
    Button Avatar_Btn;
    [SerializeField]
    GameObject AssetList_Obj;
    [SerializeField]
    TextMeshProUGUI Assets_CryptoChips_Txt, Assets_CryptoChipsValue_Txt,
                    Assets_VCChips_Txt, Assets_VCValue_Txt,
                    Assets_Gold_Txt, Assets_GoldValue_Txt,
                    Assets_Stamina_Txt, Assets_StaminaValue_Txt,
                    Assets_OTProps_Txt, Assets_OTPropsValue_Txt;

    [Header("項目按鈕")]
    [SerializeField]
    RectTransform Floor3;
    [SerializeField]
    Button Mine_Btn, Shop_Btn, Main_Btn, Activity_Btn, Ranking_Btn, t_History_Btn, Settings_Btn;
    [SerializeField]
    GameObject LobbyMainPageView, LobbyMinePageView, LobbyRankingView, LobbyShopView, LobbyActivityView;
    [SerializeField]
    TextMeshProUGUI MineBtn_Txt, ShopBtn_Txt, ActivityBtn_Txt, RankingBtn_Txt, t_HistoryBtn_Txt, SettingsBtn_Txt;

    [Header("任務介面")]
    [SerializeField]
    RectTransform Floor4;
    [SerializeField]
    GameObject QuestView;

    [Header("設置暱稱")]
    [SerializeField]
    GameObject SetNicknameViewObj;

    [Header("存提款介面")]
    [SerializeField]
    GameObject Transfers_AnteView;
    [SerializeField]
    Button Transfers_Btn;
    [SerializeField]
    TextMeshProUGUI TransfersBtn_Txt;

    public GameObject Notice;
    public TextMeshProUGUI noticeText;

    [Header("背景音樂")][SerializeField] public AudioSource audioSource;

    bool isFirstIn;
    bool isListenered;

    DateTime gameTestCountTime;             //開啟遊戲測試點擊時間
    int gameTestTouchCount;                 //開啟遊戲測試點擊次數

    /// <summary>
    /// 項目按鈕類型
    /// </summary>
    enum ItemType
    {
        Mine,
        Shop,
        Main,
        Activity,
        Ranking,
        t_History,
        Settings
    }

    bool isShowAssetList;               //是否顯示用戶資源列表

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 用戶資源列表

        Assets_CryptoChips_Txt.text = LanguageManager.Instance.GetText("Crypto Table");
        Assets_VCChips_Txt.text = LanguageManager.Instance.GetText("VC Table");
        Assets_Gold_Txt.text = LanguageManager.Instance.GetText("Gold");
        Assets_Stamina_Txt.text = LanguageManager.Instance.GetText("Stamina");
        Assets_OTProps_Txt.text = LanguageManager.Instance.GetText("OT Props");

        #endregion

        #region 項目按鈕

        MineBtn_Txt.text = LanguageManager.Instance.GetText("Mine");
        //ShopBtn_Txt.text = LanguageManager.Instance.GetText("Shop");
        //ActivityBtn_Txt.text = LanguageManager.Instance.GetText("Activity");
        RankingBtn_Txt.text = LanguageManager.Instance.GetText("Ranking");
        t_HistoryBtn_Txt.text = LanguageManager.Instance.GetText("Transaction History");
        SettingsBtn_Txt.text = LanguageManager.Instance.GetText("Settings");

        #endregion

        #region 存提款

        TransfersBtn_Txt.text = LanguageManager.Instance.GetText("Transfers");

        #endregion
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);

        /*
        //移除監聽在線狀態
        JSBridgeManager.Instance.RemoveListenerConnectState($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        JSBridgeManager.Instance.StopListeningForDataChanges($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        WalletManager.Instance.CancelCheckConnect();*/
    }

    private void Awake()
    {
        isFirstIn = true;
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        #region 遊戲測試

        //開啟遊戲測試
        OpenGameTest_Btn.onClick.AddListener(() =>
        {
            gameTestCountTime = DateTime.Now;
            gameTestTouchCount++;
        });

        //遊戲測試開關
        GameTest_Tog.onValueChanged.AddListener((isOn) =>
        {
            DataManager.IsOpenGameTest = isOn;
        });

        #endregion

        //顯示用戶資源列表
        Avatar_Btn.onClick.AddListener(() =>
        {
            isShowAssetList = !isShowAssetList;
            SetIsShowAssetList = isShowAssetList;
        });

        #region 項目按鈕

        //主頁
        Main_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Main);
        });

        //用戶訊息
        Mine_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Mine);
        });

        //排名
        Ranking_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Ranking);
        });
        
        //報表
        t_History_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.t_History);
        });
        
        //設定
        Settings_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Settings);
        });

        //商店
        /*Shop_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Shop);
        });*/

        //活動
        /*Activity_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Activity);
        });*/

        #endregion

        Transfers_Btn.onClick.AddListener(() =>
        {
            DisplayFloor4UI(Transfers_AnteView);
        });
    }

    private void OnEnable()
    {
        GameTest_Tog.gameObject.SetActive(false);

        isShowAssetList = false;
        SetIsShowAssetList = isShowAssetList;

        OpenItemPage(ItemType.Main);
    }

    private void Start()
    {
        #region 測試

        /*//寫入資料
        Dictionary<string, object> dataDic = new()
        {
            { FirebaseManager.U_CHIPS, Math.Round(DataManager.InitGiveUChips) },
            { FirebaseManager.A_CHIPS, Math.Round(DataManager.InitGiveAChips) },
            { FirebaseManager.GOLD, Math.Round(DataManager.InitGiveGold) },
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
            dataDic);*/

        #endregion

        ViewManager.Instance.OpenWaitingView(transform);
        DataManager.ReciveRankData();
        UpdateUserData();

        /*
#if UNITY_EDITOR

        //刷新用戶資料
        //InvokeRepeating(nameof(UpdateUserData), 1, 30);

        return;
#endif

        JSBridgeManager.Instance.StartListenerConnectState(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        JSBridgeManager.Instance.StartListeningForDataChanges(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
            gameObject.name,
            nameof(GetDataCallback));

        //刷新用戶資料
        //InvokeRepeating(nameof(UpdateUserData), 30, 30);*/
        SoundToggleGroup.IsPlayAudio(audioSource);
    }

    private void Update()
    {
        #region 開啟遊戲測試

        //開啟遊戲測試
        if ((DateTime.Now - gameTestCountTime).TotalSeconds < 2)
        {
            if (gameTestTouchCount >= 3)
            {
                gameTestTouchCount = 0;
                GameTest_Tog.gameObject.SetActive(!GameTest_Tog.gameObject.activeSelf);
            }
        }
        else
        {
            gameTestTouchCount = 0;
        }
        if (DataManager.DataUpdated)
        {
            UpdateUserInfo();
            DataManager.DataUpdated = false;
        }
        #endregion


        Notice.gameObject.SetActive(DataManager.istipAppear);
        noticeText.text = DataManager.TipText;
    }

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    public void UpdateUserData()
    {
        //讀取用戶資料
        JSBridgeManager.Instance.ReadDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
            gameObject.name,
            nameof(GetDataCallback));
    }

    /// <summary>
    /// 獲取資料回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void GetDataCallback(string jsonData)
    {
        AccountData loginData = FirebaseManager.Instance.OnFirebaseDataRead<AccountData>(jsonData);

        if (loginData != null &&
            !string.IsNullOrEmpty(loginData.userId) &&
            !string.IsNullOrEmpty(loginData.nickname))
        {
            ViewManager.Instance.CloseWaitingView(transform);

            DataManager.UserNickname = loginData.nickname;
            DataManager.UserAvatarIndex = loginData.avatarIndex;

#if !UNITY_EDITOR

            if (!isListenered)
            {
                isListenered = true;

                //監聽在線狀態
                JSBridgeManager.Instance.StartListenerConnectState(
                    $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}");

                /*//監聽用戶資料
                JSBridgeManager.Instance.StartListeningForDataChanges(
                    $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
                gameObject.name,
                nameof(GetDataCallback));*/
            }
#endif
        }
        else
        {
            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.USER_ID, DataManager.UserId},
                { FirebaseManager.AVATAR_INDEX, 0},
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
                data,
                gameObject.name,
                nameof(UpdateUserData));

            //開啟設置暱稱
            if (isFirstIn)
            {
                Instantiate(SetNicknameViewObj, transform);
            }
        }

        //使用邀請碼登入
        if (string.IsNullOrEmpty(DataManager.UserBoundInviterId) &&
            !string.IsNullOrEmpty(DataManager.GetInvitationCode) &&
            !string.IsNullOrEmpty(DataManager.GetInviterId))
        {
            //寫入資料
            Dictionary<string, object> dataDic = new()
            {
                { FirebaseManager.BOUND_INVITER_ID, DataManager.GetInviterId },
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
                                                            dataDic);

            //清除紀錄資料
            DataManager.GetInvitationCode = "";
            DataManager.GetInviterId = "";
            JSBridgeManager.Instance.ClearUrlQueryString();
        }

        //有Line Toke
        if (string.IsNullOrEmpty(DataManager.UserLineToken) &&
            !string.IsNullOrEmpty(DataManager.GetLineToken))
        {
            //修改資料
            Dictionary<string, object> dataDic = new()
            {
                { FirebaseManager.LINE_TOKEN, DataManager.GetLineToken },
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
                                                            dataDic);

            JSBridgeManager.Instance.ClearUrlQueryString();
        }

        //更新主頁邀請碼訊息
        LobbyMinePageView lobbyMinePageView = GameObject.FindAnyObjectByType<LobbyMinePageView>();
        if (lobbyMinePageView != null)
        {
            lobbyMinePageView.UpdateInvitationCodeInfo();
        }

        UpdateUserInfo();
        HandHistoryManager.Instance.LoadHandHistoryData();

        isFirstIn = false;
    }

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    public void UpdateUserInfo()
    {
        Nickname_Txt.text = $"@{DataManager.UserNickname}";
        Avatar_Btn.image.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];
        Stamina_Txt.text = $"{DataManager.UserEnergy}/{DataManager.UserMaxEnrtgy}";

        Assets_CryptoChipsValue_Txt.text = $"{StringUtils.SetChipsUnit(DataManager.UserUChips)}";
        Assets_VCValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserAChips);
        Assets_GoldValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserGold);
        Assets_StaminaValue_Txt.text = $"{DataManager.UserEnergy}/{DataManager.UserMaxEnrtgy}";
        Assets_OTPropsValue_Txt.text = $"{DataManager.UserTimer}";
    }

    /// <summary>
    /// 頭像更換成Line頭貼
    /// </summary>
    /// <param name="linePicture">Line頭貼</param>
    public void AvatarChangeToLinePicture(Sprite linePicture)
    {
        Avatar_Btn.image.sprite = linePicture;
    }

    /// <summary>
    /// 是否顯示用戶資源列表
    /// </summary>
    private bool SetIsShowAssetList
    {
        set
        {
            ///  AssetList_Obj.SetActive(value);
        }
    }

    /// <summary>
    /// 開啟項目頁面
    /// </summary>
    /// <param name="itemType"></param>
    private void OpenItemPage(ItemType itemType)
    {
        LobbyMainPageView mainPageView = null;

        // Find existing pages
        for (int i = 0; i < Floor3.childCount; i++)
        {
            Transform child = Floor3.GetChild(i);
            if (child.TryGetComponent(out LobbyMainPageView foundView))
            {
                if (itemType == ItemType.Main)
                {
                    mainPageView = foundView;
                    mainPageView.SwitchBg = true;
                }
                else
                    Destroy(child.gameObject);

            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        GameObject itemObj = null;
        switch (itemType)
        {
            case ItemType.Main:
                if (mainPageView == null)
                {
                    itemObj = LobbyMainPageView;
                }
                else
                {
                    mainPageView.SwitchBg = false;
                }
                break;
            case ItemType.Mine:
                itemObj = LobbyMinePageView;
                break;
            case ItemType.Ranking:
                itemObj = LobbyRankingView;
                break;
            case ItemType.Shop:
                itemObj = LobbyShopView;
                break;
            case ItemType.Activity:
                itemObj = LobbyActivityView;
                break;
            case ItemType.t_History:
                itemObj = LobbyMinePageView;
                break;
            case ItemType.Settings:
                itemObj = LobbyMinePageView;
                break;
            default:
                Debug.LogWarning("Unknown item type: " + itemType);
                return; // Early exit for unknown types
        }

        if (itemObj != null)
        {
            RectTransform itemPageView = Instantiate(itemObj, Floor3).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(itemPageView, itemType.ToString());

            if(itemType == ItemType.t_History)
            {
                LobbyMinePageView mineView = Floor3.GetComponentInChildren<LobbyMinePageView>();
                mineView.openTransactionHistoryView();
            }else if(itemType == ItemType.Settings)
            {
                StartCoroutine(wait4MineLoad());
            }
        }
    }

    public void CloseNotice()
    {
        DataManager.istipAppear = false;
    }

    /// <summary>
    /// 顯示已達房間數量提示
    /// </summary>
    public void ShowMaxRoomTip()
    {
        ViewManager.Instance.OpenTipMsgView(transform,
                                            LanguageManager.Instance.GetText("MaxRoomTip"));
    }

    /// <summary>
    /// 開啟Floor4介面
    /// </summary>
    public void DisplayFloor4UI(GameObject UIobj)
    {
        if (Floor4.childCount < 1)
        {
            Instantiate(UIobj, Floor4);
        }
        else
        {

            Destroy(Floor4.GetChild(0).gameObject);
        }
    }

    IEnumerator wait4MineLoad()
    {
        LobbyMinePageView mineView = Floor3.GetComponentInChildren<LobbyMinePageView>();
        yield return new WaitUntil(() => mineView!=null);

        if (mineView != null)
        {
            print(mineView.name);
            mineView.openSettingsView();
        }
    }
}