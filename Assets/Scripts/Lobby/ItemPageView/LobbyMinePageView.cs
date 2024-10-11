using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Linq;
using TMPro;
using Thirdweb;

public class LobbyMinePageView : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField]
    Mask Viewport;

    [Header("用戶訊息")]
    [SerializeField]
    GameObject UserPorfile_Obj, WalletAddressBg_Obj;
    [SerializeField]
    Image playerAvatar_Img;
    [SerializeField]
    Button EditorAvatar_Btn, CopyWalletAddress_Btn;
    [SerializeField]
    TextMeshProUGUI Nickname_Txt, WalletAddress_Txt, CopiedWalletAddress_Txt;

    [Header("更換頭像")]
    [SerializeField]
    RectTransform ChangeAvatar_Tr, AvatarListParent_Tr, SelectAvatarIcon_Tr;
    [SerializeField]
    GameObject AvatarSapmle;
    [SerializeField]
    Button CloseChangeAvatar_Btn, ChangeAvatarSubmit_Btn;
    [SerializeField]
    TextMeshProUGUI ChangeAvatarTitle_Txt, ChangeAvatarSubmitBtn_Txt;

    [Header("帳戶餘額")]
    [SerializeField]
    RectTransform AccountBalance_Obj;
    [SerializeField]
    Button AccountBalanceExpand_Btn, AccountBalanceReflash_Btn;
    [SerializeField]
    Image AccountBalanceExpand_Img;
    [SerializeField]
    TextMeshProUGUI AccountBalanceTitle_Txt, AccountBalanceReflashBtn_Txt,
                    CryptoTable_Txt, CryptoTableValue_Txt,
                    VCTable_Txt, VCTableValue_Txt,
                    Gold_Txt, GoldValue_Txt,
                    Stamina_Txt, StaminaValue_Txt,
                    OTProps_Txt, OTPropsValue_Txt;

    [Header("分數紀錄")]
    [SerializeField]
    RectTransform ScoreRecord_Obj;
    [SerializeField]
    Button ScoreRecordExpand_Btn, ScoreRecordReflash_Btn;
    [SerializeField]
    Image ScoreRecordExpand_Img;
    [SerializeField]
    Slider VPIP_Sli, PFR_Sli, ATS_Sli, ThreeBET_Sli;
    [SerializeField]
    TextMeshProUGUI ScoreRecordTitle_Txt, ScoreRecordReflashBtn_Txt,
                    VPIPpercent_Txt, PFRpercent_Txt, ATSpercent_Txt, ThreeBETpercent_Txt;

    [Header("第三方連接")]
    [SerializeField]
    Button IGLink_Btn, LineLink_Btn;
    [SerializeField]
    GameObject IGNotYetLinked_Obj, LineNotYetLinked_Obj;
    [SerializeField]
    Image IGLinked_Img, LineLinked_Img;
    [SerializeField]
    TextMeshProUGUI SocialMediaTitle_Txt,
                    IGNotYetLinked_Txt, LineNotYetLinked_Txt,
                    IGLinked_Txt, LineLinked_Txt;

    [Header("邀請碼")]
    [SerializeField]
    RectTransform Invitation_Obj;
    [SerializeField]
    Image Invitation_Img;
    [SerializeField]
    Button InvitationCodeShare_Btn, InviationCodeSubmit_Btn, CopyInvitationCode_Btn, InviteUIArrow_Btn;
    [SerializeField]
    TMP_InputField BoundInviterId_If;
    [SerializeField]
    Image InvitationQRCode_Img;
    [SerializeField]
    TextMeshProUGUI InvitationCodeTitle_Txt, InvitationCodeShareBtn_Txt, MyInvitationCode_Txt,
                    InvitationCode_Txt, CopiedInvitationCode_Txt,
                    BoundInviterTitle_Txt, BoundInviterIdf_Placeholder, InviationCodeSubmitBtn_Txt,
                    InviationCodeError_Txt, BringFriends_Text, InviteCodeTitle_Text, Copy_Text,
                    InvitCode_Txt;
    
   
   


    [Header("交易紀錄")]
    [SerializeField]
    Button TransactionHistory_Btn;
    [SerializeField]
    GameObject TransactionHistoryViewObj;
    [SerializeField]
    TextMeshProUGUI TransactionHistoryTitle_Txt;

    [Header("My NFT")]
    [SerializeField]
    Button MyNFT_Btn;
    [SerializeField]
    GameObject MyNFTViewObj;
    [SerializeField]
    TextMeshProUGUI MyNftTitle_Txt;

    [Header("手牌紀錄")]
    [SerializeField]
    Button HandHistory_Btn;
    [SerializeField]
    GameObject LobbyHandHistoryViewObj;
    [SerializeField]
    TextMeshProUGUI HandHistoryTitle_Txt;

    [Header("設定")]
    [SerializeField]
    RectTransform Settings_Obj;
    [SerializeField]
    Image settings_Img;
    [SerializeField]
    Button Settings_Btn_Expance;

    [SerializeField]
    GameObject SettingsViewObj;
    [SerializeField]
    Button Settings_Btn;
    [SerializeField]
    TextMeshProUGUI SettingsTitle_Txt;

    const string expandContentName = "Content";                                 //展開內容物件名稱
    const string expandTopBgName = "TopBg";                                     //收起上方物件名稱
    const float expandTIme = 0.1f;                                              //內容展開時間

    string invitationCodeUrl;                                                   //邀請碼URL
    List<Button> avatarBtnList;                                                 //頭像按鈕
    int tempAvatarIndex;                                                        //零時頭像index
    bool isAccountBalanceExpand;                                                //是否展開帳戶餘額
    bool isScoreRecordExpand;                                                   //是否展開分數紀錄
    bool isInviteUIExpand;                                                      //是否展開邀請碼介面
    bool isSettingExpand;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 用戶訊息

        CopiedWalletAddress_Txt.text = LanguageManager.Instance.GetText("Copied!");

        #endregion

        #region 更換頭像

        ChangeAvatarTitle_Txt.text = LanguageManager.Instance.GetText("Change Avatar");
        ChangeAvatarSubmitBtn_Txt.text = LanguageManager.Instance.GetText("SUBMIT");

        #endregion

        #region 帳戶餘額

        AccountBalanceTitle_Txt.text = LanguageManager.Instance.GetText("Account Balance");
        AccountBalanceReflashBtn_Txt.text = LanguageManager.Instance.GetText("REFLASH");
        CryptoTable_Txt.text = LanguageManager.Instance.GetText("U point");
        VCTable_Txt.text = LanguageManager.Instance.GetText("A point");
        Gold_Txt.text = LanguageManager.Instance.GetText("Gold");
        Stamina_Txt.text = LanguageManager.Instance.GetText("Stamina");
        OTProps_Txt.text = LanguageManager.Instance.GetText("OT Props");

        #endregion

        #region 分數紀錄

        ScoreRecordTitle_Txt.text = LanguageManager.Instance.GetText("Score Record");
        ScoreRecordReflashBtn_Txt.text = LanguageManager.Instance.GetText("REFLASH");

        #endregion

        #region 第三方連接

        SocialMediaTitle_Txt.text = LanguageManager.Instance.GetText("Social Media");
        IGNotYetLinked_Txt.text = LanguageManager.Instance.GetText("Not Yet Linked");
        LineNotYetLinked_Txt.text = LanguageManager.Instance.GetText("Not Yet Linked");

        #endregion

        #region 邀請碼

        InvitationCodeTitle_Txt.text = LanguageManager.Instance.GetText("Invite Friends");
        InvitationCodeShareBtn_Txt.text = LanguageManager.Instance.GetText("Share");
        BoundInviterIdf_Placeholder.text = LanguageManager.Instance.GetText("Please Enter Code");
        InviationCodeSubmitBtn_Txt.text = LanguageManager.Instance.GetText("LINK NOW");
        MyInvitationCode_Txt.text = LanguageManager.Instance.GetText("Share QR Code");
        BringFriends_Text.text = LanguageManager.Instance.GetText("Bring Friends");
        //BoundInviterTitle_Txt.text = LanguageManager.Instance.GetText("InviteCode");
        InviteCodeTitle_Text.text = LanguageManager.Instance.GetText("InviteCode");
        Copy_Text.text = LanguageManager.Instance.GetText("Copy");

        #endregion

        #region 交易紀錄

        TransactionHistoryTitle_Txt.text = LanguageManager.Instance.GetText("Transaction History");

        #endregion

        #region My NFT

        MyNftTitle_Txt.text = LanguageManager.Instance.GetText("My NFT");

        #endregion

        #region 手牌紀錄

        HandHistoryTitle_Txt.text = LanguageManager.Instance.GetText("Hand History");

        #endregion

        #region 設定

        SettingsTitle_Txt.text = LanguageManager.Instance.GetText("Settings");

        #endregion

        SetUserInfo();
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        //錢包地址已複製文字
        Color color = CopiedWalletAddress_Txt.color;
        color.a = 0;
        CopiedWalletAddress_Txt.color = color;
        //邀請碼已複製文字
        CopiedInvitationCode_Txt.color = color;

        ChangeAvatar_Tr.gameObject.SetActive(false);

        //紀錄展開物件初始化(已拿掉)
        /*List<RectTransform> expandObjList = new()
        {
            AccountBalance_Obj,     //帳戶餘額
            ScoreRecord_Obj,        //分數紀錄
            Invitation_Obj,
            Settings_Obj
        };
        foreach (var expandObj in expandObjList)
        {
            RectTransform contentObj = expandObj.Find(expandContentName).GetComponent<RectTransform>();
            RectTransform TopBgObj = expandObj.Find(expandTopBgName).GetComponent<RectTransform>();
            contentObj.gameObject.SetActive(false);
            expandObj.sizeDelta = new Vector2(expandObj.rect.width, TopBgObj.rect.height);
        }
        AccountBalanceExpand_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[3];
        ScoreRecordExpand_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[3];*/
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        #region 用戶訊息

        //複製錢包地址
        CopyWalletAddress_Btn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(WalletAddress_Txt.text))
            {
                StringUtils.CopyText(DataManager.UserWalletAddress);
                UnityUtils.Instance.ColorFade(CopiedWalletAddress_Txt,
                                              null,
                                              0.2f,
                                              0.5f,
                                              1.5f);
            }
        });

        //開啟更換頭像
        EditorAvatar_Btn.onClick.AddListener(() =>
        {
            //UserPorfile_Obj.SetActive(false);
            ChangeAvatar_Tr.gameObject.SetActive(true);
        });

        //關閉選擇頭像
        CloseChangeAvatar_Btn.onClick.AddListener(() =>
        {
            UserPorfile_Obj.SetActive(true);
            ChangeAvatar_Tr.gameObject.SetActive(false);
        });

        //提交更換頭像
        ChangeAvatarSubmit_Btn.onClick.AddListener(() =>
        {
            DataManager.UserAvatarIndex = tempAvatarIndex;

            //寫入資料
            Dictionary<string, object> dataDic = new()
            {
                { FirebaseManager.AVATAR_INDEX, DataManager.UserAvatarIndex },
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserId}",
                dataDic);

            playerAvatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];
            GameObject.FindAnyObjectByType<LobbyView>().UpdateUserData();

            UserPorfile_Obj.SetActive(true);
            ChangeAvatar_Tr.gameObject.SetActive(false);
        });

        #endregion

        #region 帳戶餘額

        //帳戶餘額展開
        //AccountBalanceExpand_Btn.onClick.AddListener(() =>
        //{
        //    isAccountBalanceExpand = !isAccountBalanceExpand;
        //    StartCoroutine(ISwitchContent(isAccountBalanceExpand,
        //                                  AccountBalance_Obj,
        //                                  AccountBalanceExpand_Img));
        //});

        //帳戶餘額刷新
        AccountBalanceReflash_Btn.onClick.AddListener(() =>
        {
            //UpdatetAccountBalance("4,300 ETH", 40000, 3000, 5, 30);
            FindAnyObjectByType<LobbyView>().UpdateUserData();
        });

        #endregion

        #region 分數紀錄

        //分數紀錄展開
        //ScoreRecordExpand_Btn.onClick.AddListener(() =>
        //{
        //    isScoreRecordExpand = !isScoreRecordExpand;
        //    StartCoroutine(ISwitchContent(isScoreRecordExpand,
        //                                  ScoreRecord_Obj,
        //                                  ScoreRecordExpand_Img));
        //});

        //紀錄分數刷新
        ScoreRecordReflash_Btn.onClick.AddListener(() =>
        {
            UpdateScoreRecord(70, 33, 25, 60);
        });

        //  邀請碼介面展開
        InviteUIArrow_Btn.onClick.AddListener(() =>
        {
            isInviteUIExpand = !isInviteUIExpand;
            StartCoroutine(ISwitchContent(isInviteUIExpand,
                                            Invitation_Obj,
                                            Invitation_Img));
        });

        // 設定介面展開
        Settings_Btn_Expance.onClick.AddListener(() =>
        {
            isSettingExpand = !isSettingExpand;
            StartCoroutine(ISwitchContent(isSettingExpand,
                                          Settings_Obj,
                                          settings_Img));


        });

        #endregion

        #region 邀請碼

        //邀請碼分享
        InvitationCodeShare_Btn.onClick.AddListener(() =>
        {
            string title = LanguageManager.Instance.GetText("Invitation Code");
            string content = $"{LanguageManager.Instance.GetText("Asia Poker")}\n" +
                             $"{LanguageManager.Instance.GetText("Invitation Code")} : {DataManager.UserInvitationCode}";

            JSBridgeManager.Instance.Share(title,
                                           content,
                                           invitationCodeUrl);
        });

        //提交邀請碼
        InviationCodeSubmit_Btn.onClick.AddListener(() =>
        {
            InviationCodeError_Txt.text = "";

            if (BoundInviterId_If.text == DataManager.UserInvitationCode)
            {
                InviationCodeError_Txt.text = LanguageManager.Instance.GetText("Wrong Invitation Code.");
                return;
            }

            //顯示提示框
            ConfirmView confirmView = ViewManager.Instance.OpenConfirmView();
            confirmView.SetContent(LanguageManager.Instance.GetText("Notice"),
                                   LanguageManager.Instance.GetText("Cannot Be Changed After Linking."));
            confirmView.SetBnt(() =>
            {
                ViewManager.Instance.OpenWaitingView(transform);
                JSBridgeManager.Instance.CheckUserDataExist(FirebaseManager.INVITATION_CODE,
                                                            BoundInviterId_If.text,
                                                            gameObject.name,
                                                            nameof(SubmitInvitationCodeCallback));
            });
        });

        //複製邀請碼
        CopyInvitationCode_Btn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(InvitCode_Txt.text))
            {
                StringUtils.CopyText(DataManager.UserInvitationCode);
                UnityUtils.Instance.ColorFade(CopiedInvitationCode_Txt,
                                              null,
                                              0.2f,
                                              0.5f,
                                              1.5f);
            }
        });

        #endregion

        #region 第三方連接

        //IG連接
        IGLink_Btn.onClick.AddListener(() =>
        {
            StartInstagram();
        });

        //Line連接
        LineLink_Btn.onClick.AddListener(() =>
        {
            StartLineLogin();
        });

        #endregion

        #region 交易紀錄

        //交易紀錄按鈕
        TransactionHistory_Btn.onClick.AddListener(() =>
        {
            Transform lobbyView = GameObject.Find("LobbyView").transform;
            RectTransform transactionHistoryView = Instantiate(TransactionHistoryViewObj, lobbyView).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(transactionHistoryView, "TransactionHistoryView");
        });

        #endregion

        #region My NFT

        MyNFT_Btn.onClick.AddListener(() =>
        {
            Transform lobbyView = GameObject.Find("LobbyView").transform;
            RectTransform MyNFTView = Instantiate(MyNFTViewObj, lobbyView).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(MyNFTView, "MyNFTView");
        });

        #endregion

        #region 手牌紀錄

        HandHistory_Btn.onClick.AddListener(() =>
        {
            Transform lobbyView = GameObject.Find("LobbyView").transform;
            RectTransform lobbyHandHistoryView = Instantiate(LobbyHandHistoryViewObj, lobbyView).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(lobbyHandHistoryView, "LobbyHandHistoryView");
        });

        #endregion

        #region 設定

        //開啟設定
        /*  Settings_Btn.onClick.AddListener(() =>
          {
              LobbyView lobbyView = GameObject.FindFirstObjectByType<LobbyView>();
              if (lobbyView != null)
              {
                  Instantiate(SettingsViewObj, lobbyView.transform);
              }
          });
        */

        #endregion
    }

    private void Start()
    {
        InviationCodeError_Txt.text = "";

        //產生選擇的頭像
        avatarBtnList = new List<Button>();
        AvatarSapmle.gameObject.SetActive(false);
        Sprite[] avatars = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album;
        for (int i = 0; i < avatars.Length; i++)
        {
            Button avatarBtn = Instantiate(AvatarSapmle, AvatarListParent_Tr).GetComponent<Button>();
            avatarBtn.gameObject.SetActive(true);
            avatarBtn.image.sprite = avatars[i];
            int index = i;

            avatarBtn.onClick.AddListener(() =>
            {
                SelectAvatarIcon_Tr.SetParent(avatarBtn.transform);
                SelectAvatarIcon_Tr.anchoredPosition = Vector2.zero;
                tempAvatarIndex = index;
            });

            avatarBtnList.Add(avatarBtn);
        }
        SelectAvatarIcon_Tr.SetParent(avatarBtnList[DataManager.UserAvatarIndex].transform);
        SelectAvatarIcon_Tr.anchoredPosition = Vector2.zero;

        //Viewport.enabled = true;

        UpdatetAccountBalance(string.IsNullOrEmpty(DataManager.UserWalletBalance) ? "0 " : DataManager.UserWalletBalance,
                              DataManager.UserAChips,
                              DataManager.UserGold,
                              DataManager.UserEnergy,
                              DataManager.UserTimer);

        UpdateScoreRecord(50, 60, 70, 80);
        UpdateInvitationCodeInfo();
    }

    //外部調用方法
    ///<summary>
    ///開啟交易報表
    ///</summary>
    public void openTransactionHistoryView()
    {
        Transform lobbyView = GameObject.Find("LobbyView").transform;
        RectTransform transactionHistoryView = Instantiate(TransactionHistoryViewObj, lobbyView).GetComponent<RectTransform>();
        ViewManager.Instance.InitViewTr(transactionHistoryView, "TransactionHistoryView");
    }
    ///<summary>
    ///開啟設定選單
    ///</summary>
    public void openSettingsView()
    {
        isSettingExpand = !isSettingExpand;
        StartCoroutine(ISwitchContent(isSettingExpand,
                                      Settings_Obj,
                                      settings_Img));
    }

    /// <summary>
    /// 設置用戶訊息
    /// </summary>
    private void SetUserInfo()
    {
        //頭像
        playerAvatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];

        //暱稱
        Nickname_Txt.text = $"@{DataManager.UserNickname}";

        //錢包地址 /*先呈現畫面之後再寫回來*/
        //WalletAddress_Txt.text = "TTerwE2220ba3fffba745R...";

        //邀請碼
        InvitCode_Txt.text = $"{DataManager.UserInvitationCode}";

        StringUtils.StrExceedSize(DataManager.UserWalletAddress, WalletAddress_Txt);


        //WalletAddressBg_Obj.SetActive(true);
        WalletAddressBg_Obj.SetActive(!string.IsNullOrEmpty(WalletAddress_Txt.text));

        //IG連接
        IGNotYetLinked_Obj.SetActive(string.IsNullOrEmpty(DataManager.IGIUserIdAndName));
        IGLink_Btn.interactable = string.IsNullOrEmpty(DataManager.IGIUserIdAndName);
        IGLinked_Txt.text = string.IsNullOrEmpty(DataManager.IGIUserIdAndName) ?
                            LanguageManager.Instance.GetText("LINK NOW") :
                            LanguageManager.Instance.GetText("LINKED");
        IGLinked_Img.sprite = string.IsNullOrEmpty(DataManager.LineMail) ?
                        AssetsManager.Instance.GetAlbumAsset(AlbumEnum.LinkAlbum).album[0] :
                        AssetsManager.Instance.GetAlbumAsset(AlbumEnum.LinkAlbum).album[1];

        //Line連接
        LineNotYetLinked_Obj.SetActive(string.IsNullOrEmpty(DataManager.GetLineToken));
        LineLink_Btn.interactable = string.IsNullOrEmpty(DataManager.GetLineToken);
        LineLinked_Txt.text = string.IsNullOrEmpty(DataManager.GetLineToken) ?
                              LanguageManager.Instance.GetText("LINK NOW") :
                              LanguageManager.Instance.GetText("LINKED");
        LineLinked_Img.sprite = string.IsNullOrEmpty(DataManager.GetLineToken) ?
                                AssetsManager.Instance.GetAlbumAsset(AlbumEnum.LinkAlbum).album[0] :
                                AssetsManager.Instance.GetAlbumAsset(AlbumEnum.LinkAlbum).album[1];

        invitationCodeUrl = $"{DataManager.GetRedirectUri()}" +
                            $"?invitationCode={DataManager.UserInvitationCode}" +
                            $"&inviterId={DataManager.UserId}";
    }

    /// <summary>
    /// 更新帳號餘額
    /// </summary>
    /// <param name="crypto"></param>
    /// <param name="vc"></param>
    /// <param name="gold"></param>
    /// <param name="Stamina"></param>
    /// <param name="ot"></param>
    private void UpdatetAccountBalance(string crypto, double vc, double gold, int Stamina, int ot)
    {
        DataManager.UserWalletBalance = crypto.ToString();
        DataManager.UserAChips = vc;
        DataManager.UserGold = gold;
        DataManager.UserEnergy = Stamina;
        DataManager.UserTimer = ot;

        CryptoTableValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserUChips);
        VCTableValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserAChips);
        GoldValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserGold);
        StaminaValue_Txt.text = $"{DataManager.UserEnergy}/{DataManager.UserMaxEnrtgy}";
        OTPropsValue_Txt.text = $"{DataManager.UserTimer}";

        GameObject.FindAnyObjectByType<LobbyView>().UpdateUserData();
    }

    /// <summary>
    /// 更新分數紀錄
    /// </summary>
    /// <param name="vpip"></param>
    /// <param name="pfr"></param>
    /// <param name="ats"></param>
    /// <param name="threeBet"></param>
    private void UpdateScoreRecord(float vpip, float pfr, float ats, float threeBet)
    {
        VPIP_Sli.value = vpip / 100;
        VPIPpercent_Txt.text = $"{vpip}%";

        PFR_Sli.value = pfr / 100;
        PFRpercent_Txt.text = $"{pfr}%";

        ATS_Sli.value = ats / 100;
        ATSpercent_Txt.text = $"{ats}%";

        ThreeBET_Sli.value = threeBet / 100;
        ThreeBETpercent_Txt.text = $"{threeBet}%";
    }

    /// <summary>
    /// 介面內容展開縮放
    /// </summary>
    /// <param name="isExpand">是否展開</param>
    /// <param name="rt">展開內容物件</param>
    /// <param name="img">展開按鈕圖</param>
    /// <param name="completeCallback">完成回傳</param>
    /// <returns></returns>
    private IEnumerator ISwitchContent(bool isExpand, RectTransform rt, Image img, UnityAction completeCallback = null)
    {
        //展開內容物件
        RectTransform contentObj = rt.Find(expandContentName).GetComponent<RectTransform>();
        //收回高度
        float pullbackHeight = rt.Find(expandTopBgName).GetComponent<RectTransform>().rect.height;
        //目標高度
        float targetHeight = isExpand == true ?
                             contentObj.rect.height :
                             pullbackHeight;
        //初始高度
        float initHeight = isExpand == true ?
                           pullbackHeight :
                           rt.rect.height;

        contentObj.gameObject.SetActive(false);
        rt.sizeDelta = new Vector2(rt.rect.width, initHeight);

        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < expandTIme)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / expandTIme;
            float height = Mathf.Lerp(initHeight, targetHeight, progress);
            rt.sizeDelta = new Vector2(rt.rect.width, height);

            yield return null;
        }

        contentObj.gameObject.SetActive(isExpand);
        rt.sizeDelta = new Vector2(rt.rect.width, targetHeight);
        img.sprite = isExpand == true ?
                     AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[1] :
                     AssetsManager.Instance.GetAlbumAsset(AlbumEnum.ArrowAlbum).album[3];

        completeCallback?.Invoke();
    }

    #region 第三方連接

    /// <summary>
    /// 開始Instagram登入
    /// </summary>
    public void StartInstagram()
    {
        string authUrl = $"https://api.instagram.com/oauth/authorize?client_id=" +
                         $"{DataManager.InstagramChannelID}&redirect_uri={DataManager.InstagramRedirectUri}" +
                         $"&scope=user_profile,user_media&response_type=code";
        JSBridgeManager.Instance.LocationHref(authUrl);
    }

    /// <summary>
    /// 開始Line登入
    /// </summary>
    public void StartLineLogin()
    {
        string state = GenerateRandomString();
        string nonce = GenerateRandomString();
        string authUrl = $"https://access.line.me/oauth2/v2.1/authorize?response_type=code&" +
                         $"client_id={DataManager.LineChannelId}&" +
                         $"redirect_uri={DataManager.GetRedirectUri()}&" +
                         $"state={state}&" +
                         $"scope=profile%20openid%20email&nonce={nonce}";

        JSBridgeManager.Instance.LocationHref(authUrl);
    }
    private string GenerateRandomString(int length = 16)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion

    #region 邀請碼

    /// <summary>
    /// 提交邀請碼回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void SubmitInvitationCodeCallback(string jsonData)
    {
        ViewManager.Instance.CloseWaitingView(transform);

        var data = JsonUtility.FromJson<CheckUserData>(jsonData);

        //查詢失敗/沒有資料
        if (!string.IsNullOrEmpty(data.error) ||
            data.exists == "false")
        {
            InviationCodeError_Txt.text = LanguageManager.Instance.GetText("Wrong Invitation Code.");
            return;
        }

        JSBridgeManager.Instance.ReadDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{data.phoneNumber}",
                                                       gameObject.name,
                                                       nameof(SubmitGetUserDataCallback));
    }

    /// <summary>
    /// 提交獲取用戶資料回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void SubmitGetUserDataCallback(string jsonData)
    {
        var data = JsonUtility.FromJson<AccountData>(jsonData);

        //寫入資料
        Dictionary<string, object> dataDic = new()
        {
            { FirebaseManager.BOUND_INVITER_ID, data.userId },
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
                                                        dataDic);

        ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Succesful, LanguageManager.Instance.GetText("Binding Successful"));
    }

    /// <summary>
    /// 更新邀請碼訊息
    /// </summary>
    public void UpdateInvitationCodeInfo()
    {

        //邀請人Title
        BoundInviterTitle_Txt.text = string.IsNullOrEmpty(DataManager.UserBoundInviterId) ?
                                     LanguageManager.Instance.GetText("Enter Invitation Code") :
                                     LanguageManager.Instance.GetText("Bound Inviter");

        //邀請人ID
        BoundInviterId_If.text = !string.IsNullOrEmpty(DataManager.UserBoundInviterId) ?
                                  DataManager.UserBoundInviterId :
                                  "";
        BoundInviterId_If.interactable = string.IsNullOrEmpty(DataManager.UserBoundInviterId);

        //提交按鈕
        InviationCodeSubmit_Btn.gameObject.SetActive(string.IsNullOrEmpty(DataManager.UserBoundInviterId));

        //產生分享RQ Code
        InvitationQRCode_Img.sprite = Utils.GenerateQRCodeTexture($"{DataManager.GetRedirectUri()}?{FirebaseManager.INVITATION_CODE}={DataManager.UserInvitationCode}");

        //邀請碼
        InvitationCode_Txt.text = DataManager.UserInvitationCode;
    }

    #endregion
}
