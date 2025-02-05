using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Linq;
using TMPro;
//using Thirdweb;
using Newtonsoft.Json;
using DG.Tweening;

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
    Button AccountBalanceReflash_Btn;
    [SerializeField]
    TextMeshProUGUI AccountBalanceTitle_Txt, AccountBalanceReflashBtn_Txt,
                    CryptoTable_Txt, CryptoTableValue_Txt;

    [Header("分數紀錄")]
    [SerializeField]
    Image VPIP_Img, PFR_Img, BET3_Img;
    [SerializeField]
    TextMeshProUGUI VPIP_Txt, PFR_Txt, BET3_Txt;

    [Header("個人資料")]
    [SerializeField]
    TextMeshProUGUI Text_totalTimesValue, Text_averageVicRateValue, Text_vicRateValue, Text_highestVicPriceValue, Text_totalRevenueValue;
    [SerializeField]
    TextMeshProUGUI playerID_Txt, P_Info_Txt, Text_totalTimes_Txt, Text_averageVicRate, Text_vicRate, Text_highestVicPrice, Text_totalRevenue;

    public Transform Refresh;

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
        playerID_Txt.text = LanguageManager.Instance.GetText("Player ID");

        #endregion

        #region 更換頭像

        ChangeAvatarTitle_Txt.text = LanguageManager.Instance.GetText("Change Avatar");
        ChangeAvatarSubmitBtn_Txt.text = LanguageManager.Instance.GetText("SUBMIT");

        #endregion

        #region 帳戶餘額

        AccountBalanceTitle_Txt.text = LanguageManager.Instance.GetText("Account Balance");
        AccountBalanceReflashBtn_Txt.text = LanguageManager.Instance.GetText("REFLASH");
        CryptoTable_Txt.text = LanguageManager.Instance.GetText("U point");

        #endregion

        #region 個人資料

        P_Info_Txt.text = LanguageManager.Instance.GetText("Personal Information");
        Text_totalTimes_Txt.text = LanguageManager.Instance.GetText("TOTAL HANDS PLAYED");
        Text_averageVicRate.text = LanguageManager.Instance.GetText("AVERAGE WINNING");
        Text_vicRate.text = LanguageManager.Instance.GetText("WIN RATE");
        Text_highestVicPrice.text = LanguageManager.Instance.GetText("BIGGEST POT WON");
        Text_totalRevenue.text = LanguageManager.Instance.GetText("TOTAL EARNINGS");

        #endregion

        SetUserInfo();
        AppApi.PlayerStatistics(UpdatePlayerStatistics);
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

        ChangeAvatar_Tr.gameObject.SetActive(false);
        UserPorfile_Obj.gameObject.SetActive(true);
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
            UserPorfile_Obj.gameObject.SetActive(false);
        });

        //關閉選擇頭像
        CloseChangeAvatar_Btn.onClick.AddListener(() =>
        {
            UserPorfile_Obj.SetActive(true);
            ChangeAvatar_Tr.gameObject.SetActive(false);
            UserPorfile_Obj.gameObject.SetActive(true);
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
            UserPorfile_Obj.gameObject.SetActive(true);
        });

        #endregion

        #region 帳戶餘額
        //帳戶餘額刷新
        AccountBalanceReflash_Btn.onClick.AddListener(() =>
        {
            //UpdatetAccountBalance("4,300 ETH", 40000, 3000, 5, 30);
            LobbyView lobbyView = FindAnyObjectByType<LobbyView>();
            lobbyView.UpdateUserData();
            NoodleApi.GetBalance();

            Refresh.DORotate(new Vector3(0, 0, -360), 0.5f).SetRelative(true).SetEase(Ease.Linear);
        });

        #endregion
    }
    private void Start()
    {

        //產生選擇的頭像
        avatarBtnList = new List<Button>();
        AvatarSapmle.gameObject.SetActive(false);
        Sprite[] avatars = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album;
        for (int i = 0; i < avatars.Length; i++)
        {
            Button avatarBtn = Instantiate(AvatarSapmle, AvatarListParent_Tr).GetComponent<Button>();
            avatarBtn.gameObject.SetActive(true);
            avatarBtn.transform.GetChild(0).GetComponent<Image>().sprite = avatars[i];
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

        StringUtils.StrExceedSize(DataManager.UserWalletAddress, WalletAddress_Txt);


        //WalletAddressBg_Obj.SetActive(true);
        WalletAddressBg_Obj.SetActive(!string.IsNullOrEmpty(WalletAddress_Txt.text));

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

        CryptoTableValue_Txt.text = StringUtils.SetChipsUnit(DataManager.UserChips);

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
        VPIP_Img.fillAmount = vpip / 100;
        VPIP_Txt.text = $"{vpip}%";

        PFR_Img.fillAmount = pfr / 100;
        PFR_Txt.text = $"{pfr}%";

        BET3_Img.fillAmount = threeBet / 100;
        BET3_Txt.text = $"{threeBet}%";
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

    }

    #endregion

    public void UpdatePlayerStatistics(string _playerData)
    {
        Debug.Log("Player Statistics :: " + _playerData);
        PlayerStatistics playerData = JsonConvert.DeserializeObject<PlayerStatistics>(_playerData);
        Text_totalTimesValue.text = playerData.totalHandsPlayed.ToString();
        Text_averageVicRateValue.text = $"$ {playerData.averageWinning.ToString("F2")} / {LanguageManager.Instance.GetText("Hand")}";
        Text_vicRateValue.text = $"{playerData.winRate.ToString("F2")} %";
        Text_highestVicPriceValue.text = $"$ {playerData.biggestPotWon.ToString("F2")}";
        Text_totalRevenueValue.text = $"$ {playerData.totalEarnings.ToString("F2")}";
    }
}
