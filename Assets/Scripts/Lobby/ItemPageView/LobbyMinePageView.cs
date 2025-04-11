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
    GameObject UserPorfile_Obj;
    [SerializeField]
    Image playerAvatar_Img;
    [SerializeField]
    Button EditorAvatar_Btn;
    [SerializeField]
    TextMeshProUGUI Nickname_Txt;

    [Header("更換頭像")]
    [SerializeField]
    RectTransform ChangeAvatar_Tr, AvatarListParent_Tr, SelectAvatarIcon_Tr;
    [SerializeField]
    GameObject AvatarSapmle;
    [SerializeField]
    Button CloseChangeAvatar_Btn, ChangeAvatarSubmit_Btn;

    [Header("帳戶餘額")]
    [SerializeField]
    RectTransform AccountBalance_Obj;
    [SerializeField]
    Button AccountBalanceReflash_Btn;
    [SerializeField]
    TextMeshProUGUI CryptoTableValue_Txt;

    [Header("分數紀錄")]
    [SerializeField]
    Image VPIP_Img, PFR_Img, BET3_Img;
    [SerializeField]
    TextMeshProUGUI VPIP_Txt, PFR_Txt, BET3_Txt;

    [Header("個人資料")]
    [SerializeField]
    TextMeshProUGUI Text_totalTimesValue, Text_averageVicRateValue, Text_vicRateValue, Text_highestVicPriceValue, Text_totalRevenueValue;

    public Transform Refresh;

    List<Button> avatarBtnList;                                                 //頭像按鈕
    int tempAvatarIndex;                                                        //零時頭像index
    bool isAccountBalanceExpand;                                                //是否展開帳戶餘額

    private void Awake()
    {
        SetUserInfo();
        AppApi.PlayerStatistics(UpdatePlayerStatistics);
        ListenerEvent();

        ChangeAvatar_Tr.gameObject.SetActive(false);
        UserPorfile_Obj.gameObject.SetActive(true);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        #region 用戶訊息
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
    }

    /// <summary>
    /// 設置用戶訊息
    /// </summary>
    private void SetUserInfo()
    {
        //頭像
        playerAvatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];

        //暱稱
        Nickname_Txt.text = $"{DataManager.UserNickname}";
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
