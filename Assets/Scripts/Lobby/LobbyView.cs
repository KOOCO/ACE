using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Thirdweb;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;

public class LobbyView : MonoBehaviour
{
    [SerializeField]
    public Request_LobbyView baseRequest;

    [Header("用戶訊息")]
    [SerializeField]
    TextMeshProUGUI Nickname_Txt, Stamina_Txt, CryptoChips_Txt;

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
    Button Mine_Btn, Shop_Btn, Main_Btn, Activity_Btn, Ranking_Btn;
    [SerializeField]
    GameObject LobbyMainPageView, LobbyMinePageView, LobbyRankingView, LobbyShopView,LobbyActivityView;
    [SerializeField]
    TextMeshProUGUI MineBtn_Txt, ShopBtn_Txt, ActivityBtn_Txt, RankingBtn_Txt;

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

    bool isFirstIn;                         //是否首次登入

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
        ShopBtn_Txt.text = LanguageManager.Instance.GetText("Shop");
        ActivityBtn_Txt.text = LanguageManager.Instance.GetText("Activity");
        RankingBtn_Txt.text = LanguageManager.Instance.GetText("Ranking");

        #endregion

        #region 存提款

        TransfersBtn_Txt.text = LanguageManager.Instance.GetText("Transfers");

        #endregion
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
        JSBridgeManager.Instance.RemoveListenerConnectState($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        JSBridgeManager.Instance.StopListeningForDataChanges($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        WalletManager.Instance.CancelCheckConnect();
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

        //商店
        Shop_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Shop);
        });

        //活動
        Activity_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Activity);
        });

        #endregion

        Transfers_Btn.onClick.AddListener(() =>
        {
            DisplayFloor4UI(Transfers_AnteView);
        });
    }

    private void OnEnable()
    {
        isShowAssetList = false;
        SetIsShowAssetList = isShowAssetList;

        HandHistoryManager.Instance.LoadHandHistoryData();

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
    }

    private void Update()
    {

        #region 測試

        if (Entry.Instance.releaseType == ReleaseEnvironmentEnum.Test)
        {
            //測試_返回登入
            if (Input.GetKeyDown(KeyCode.E))
            {
                WalletManager.Instance.OnWalletDisconnect();
                LoadSceneManager.Instance.LoadScene(SceneEnum.Login);
            }
        }

        #endregion
    }

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    public void UpdateUserData()
    {
        //讀取用戶資料
        JSBridgeManager.Instance.ReadDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
            gameObject.name,
            nameof(GetDataCallback));
    }

    /// <summary>
    /// 獲取資料回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void GetDataCallback(string jsonData)
    {
        ViewManager.Instance.CloseWaitingView(transform);
        AccountData loginData = FirebaseManager.Instance.OnFirebaseDataRead<AccountData>(jsonData);

        //DataManager.UserId = loginData.userId;
        //DataManager.UserLoginPhoneNumber = loginData.phoneNumber;
        //DataManager.UserNickname = loginData.nickname;
        //DataManager.UserAvatarIndex = loginData.avatarIndex;
        //DataManager.UserInvitationCode = loginData.invitationCode;
        //DataManager.UserBoundInviterId = loginData.boundInviterId;
        //DataManager.UserLineToken = loginData.lineToken;
        DataManager.UserUChips = Math.Round(DataManager.InitGiveUChips);
        DataManager.UserAChips = Math.Round(DataManager.InitGiveAChips);
        DataManager.UserGold = Math.Round(DataManager.InitGiveGold);


        DataManager.UserAvatarIndex = 0;

        if (string.IsNullOrEmpty(DataManager.UserId))
        {
            DataManager.UserId = StringUtils.GenerateRandomString(15);
        }
        DataManager.UserLoginPhoneNumber = DataManager.UserId;

        if (string.IsNullOrEmpty(DataManager.UserInvitationCode))
        {
            DataManager.UserInvitationCode = StringUtils.GenerateRandomString(15);
        }

        //開啟設置暱稱
        if (isFirstIn &&
            string.IsNullOrEmpty(DataManager.UserNickname))
        {
            Instantiate(SetNicknameViewObj, transform);
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

        #region 測試

        if (isFirstIn)
        {
            HandHistoryManager.Instance.OnDeleteHistoryData();
        }

        #endregion

        isFirstIn = false;
    }

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    public void UpdateUserInfo()
    {
        Nickname_Txt.text = $"@{DataManager.UserNickname}";
        Avatar_Btn.image.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];
        Stamina_Txt.text = $"{DataManager.UserStamina}/{DataManager.MaxStaminaValue}";
        CryptoChips_Txt.text = string.IsNullOrEmpty(DataManager.UserWalletBalance) ? "0 ETH" : DataManager.UserWalletBalance;

        //資源列表
        Assets_CryptoChipsValue_Txt.text = string.IsNullOrEmpty(DataManager.UserWalletBalance) ? "0 ETH" : DataManager.UserWalletBalance;
        Assets_VCValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserAChips);
        Assets_GoldValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserGold);
        Assets_StaminaValue_Txt.text = $"{DataManager.UserStamina}/{DataManager.MaxStaminaValue}";
        Assets_OTPropsValue_Txt.text = $"{DataManager.UserOTProps}";
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
            AssetList_Obj.SetActive(value);
        }
    }

    /// <summary>
    /// 開啟項目頁面
    /// </summary>
    /// <param name="itemType"></param>
    private void OpenItemPage(ItemType itemType)
    {
        LobbyMainPageView mainPageView = null;
        for (int i = 0; i < Floor3.childCount; i++)
        {
            if (Floor3.GetChild(i).GetComponent<LobbyMainPageView>() != null)
            {
                mainPageView = Floor3.GetChild(i).GetComponent<LobbyMainPageView>();
                mainPageView.SwitchBg = true;
                continue;
            }

            Destroy(Floor3.GetChild(i).gameObject);
        }

        GameObject itemObj = null;
        switch (itemType)
        {
            //主頁
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

            //用戶訊息
            case ItemType.Mine:
                itemObj = LobbyMinePageView;
                break;

            //排名
            case ItemType.Ranking:
                itemObj = LobbyRankingView;
                break;

            //商店
            case ItemType.Shop:
                itemObj = LobbyShopView;
                break;
            
            //活動
            case ItemType.Activity:
                itemObj = LobbyActivityView;
                break;
        }

        if (itemObj != null)
        {
            RectTransform itemPageView = Instantiate(itemObj, Floor3).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(itemPageView, itemType.ToString());
        }
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
}
