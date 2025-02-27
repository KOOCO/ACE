using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Newtonsoft.Json;
using DG.Tweening;
using UnityEngine.Networking;
using System.Text;

public class LobbyView : MonoBehaviour
{
    [SerializeField]
    public Request_LobbyView baseRequest;

    [Header("遊戲測試")]
    [SerializeField]
    Button OpenGameTest_Btn;
    [SerializeField]
    Toggle GameTest_Tog, EditorTest_Tog;
    [SerializeField]
    TMP_InputField IPT;
    [SerializeField]
    Button customJoin_Btn;

    [Header("用戶訊息")]
    [SerializeField]
    TextMeshProUGUI Nickname_Txt,
                    CryptoChips_Txt;

    [Header("用戶資源列表")]
    [SerializeField]
    Button Avatar_Btn;
    [SerializeField]
    TextMeshProUGUI Assets_CryptoChipsValue_Txt;

    [Header("項目按鈕")]
    [SerializeField]
    RectTransform Floor3;
    [SerializeField]
    Button Mine_Btn, Main_Btn, Ranking_Btn, t_History_Btn, Settings_Btn, Refresh_Btn, Report_Btn;
    [SerializeField]
    GameObject LobbyMainPageView, LobbyMinePageView, LobbyRankingView, LobbyShopView, LobbyActivityView, LobbySettingsView, LobbyReportView;

    [Header("任務介面")]
    [SerializeField]
    RectTransform Floor4;
    [SerializeField]
    GameObject QuestView;

    [Header("設置暱稱")]
    [SerializeField]
    GameObject SetNicknameViewObj;

    [Header("提示POP")]
    public GameObject Notice;
    public TextMeshProUGUI noticeText;
    public Button ConfirmBtn;

    //[Header("背景音樂")][SerializeField] public AudioSource audioSource;

    bool isFirstIn;
    bool isListenered;

    DateTime gameTestCountTime;             //開啟遊戲測試點擊時間
    int gameTestTouchCount;                 //開啟遊戲測試點擊次數

    /// <summary>
    /// 項目按鈕類型
    /// </summary>
    public enum ItemType
    {
        None,
        Mine,
        Shop,
        Main,
        Activity,
        Ranking,
        t_History,
        Settings,
        Report
    }

    bool isShowAssetList;               //是否顯示用戶資源列表

    private void OnDestroy()
    {

        /*
        //移除監聽在線狀態
        JSBridgeManager.Instance.RemoveListenerConnectState($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        JSBridgeManager.Instance.StopListeningForDataChanges($"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}");
        WalletManager.Instance.CancelCheckConnect();*/
    }

    private void Awake()
    {
        isFirstIn = true;
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
        EditorTest_Tog.onValueChanged.AddListener((isOn) =>
        {
            DataManager.IsTestWithEditor = isOn;
        });

        //指定加入房間(需先打開對應桌的入桌介面)
        customJoin_Btn.onClick.AddListener(() =>
        {
            JoinRoomView joinRoom = FindAnyObjectByType<JoinRoomView>();
            if (joinRoom != null)
                joinRoom.joinCustomRoom(IPT.text);
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
        Report_Btn.onClick.AddListener(() =>
       {
           OpenItemPage(ItemType.Report);
       });

        //設定
        Settings_Btn.onClick.AddListener(() =>
        {
            OpenItemPage(ItemType.Settings);
        });

        //刷新
        Refresh_Btn.onClick.AddListener(() =>
        {
            Refresh_Btn.GetComponent<Transform>().DORotate(new Vector3(0, 0, -360), 0.5f).SetRelative(true).SetEase(Ease.Linear);
            UpdateUserData();
            Refresh_Btn.interactable = false;
            StartCoroutine(openRefreshBtn());
            NoodleApi.GetBalance();
        });

        #endregion

        ConfirmBtn.onClick.AddListener(() =>
        {
            if (PlayerPrefs.GetInt("nullData") >= 3)
            {
                PlayerPrefs.SetInt("nullData", 0);
                PlayerPrefs.Save();
                JSBridgeManager.Instance.WindowClose();
            }
            else
            {
                DataManager.istipAppear = false;
            }
        });
    }
    private void OnEnable()
    {
        GameTest_Tog.gameObject.SetActive(false);
        EditorTest_Tog.gameObject.SetActive(false);
        IPT.gameObject.SetActive(false);

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

        //ViewManager.Instance.OpenWaitingView(transform);
        UpdateUserData();

        Refresh_Btn.onClick.Invoke();

        //InvokeRepeating(nameof(checkIsMaintenance), 0 , 5);
        InvokeRepeating(nameof(checkIsOffline), 0 , 5);
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

        //播放音樂
        //SoundToggleGroup.IsPlayAudio(audioSource);
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
                EditorTest_Tog.gameObject.SetActive(!EditorTest_Tog.gameObject.activeSelf);
                IPT.gameObject.SetActive(!IPT.gameObject.activeSelf);
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
        print("Get user Data");
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
            !string.IsNullOrEmpty(loginData.userId) || //Cause some player has not nickname, so this place must become to 'or', otherwise client will call Firebase unstopable.
            !string.IsNullOrEmpty(loginData.nickname))
        {
            ViewManager.Instance.CloseWaitingView(transform);

            DataManager.UserNickname = loginData.nickname;
            DataManager.UserAvatarIndex = loginData.avatarIndex;
            DataManager.UserStatus = loginData.online;

            //StartHeartbeat();

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
            print(loginData.userId + " " + loginData.nickname);

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
            // if (isFirstIn)
            // {
            //     Instantiate(SetNicknameViewObj, transform);
            // }
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

        UpdateUserInfo();
        HandHistoryManager.Instance.LoadHandHistoryData();

        isFirstIn = false;
    }

    public void checkIsMaintenance(string data)
    {
        if (data == "normal")
            return;
        else if (data == serverStatus.maintenance.ToString())
            JSBridgeManager.Instance.WindowClose();
        else
            Debug.LogError("Error status!!! Please check dataBase");
    }
    void checkIsOffline()
    {
        if (PlayerPrefs.GetInt("nullData") >= 3)
        {
            DataManager.istipAppear=true;
            DataManager.TipText = LanguageManager.Instance.GetText("Network offline");

            GameControl gameControl = FindAnyObjectByType<GameControl>();
            if (gameControl != null)
                gameControl.RemovePlayer(DataManager.UserId);
        }
    }

    public void checkIsIdle()
    {
        if (PlayerPrefs.GetInt("idleCount") > 1)
        {
            JSBridgeManager.Instance.WindowClose();
        }else if(PlayerPrefs.GetInt("idleCount") > 0)
        {
            DataManager.istipAppear = true;
            DataManager.TipText = LanguageManager.Instance.GetText("Idle warn");
        }
    }

    #region HB in Unity
    //Test callBack in Unity
    void StartHeartbeat()
    {
#if UNITY_EDITOR
        JSBridgeManager.Instance.ReadDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{DataManager.UserId}",
                gameObject.name,
                nameof(delayCallHeartbeat));
#endif
    }
    void delayCallHeartbeat(string jsonData)
    {
        //print("After 5 second: " + jsonData);

        if (!string.IsNullOrEmpty(jsonData) && jsonData != "null")
        {
            var hb = JsonConvert.DeserializeObject<heartbeatData>(jsonData);
            string pStatus = hb.playerStatus;
            string sStatus = hb.serverStatus;
            PlayerPrefs.SetString("PlayerStatus", pStatus);
            PlayerPrefs.SetString("ServerStatus", sStatus);
            PlayerPrefs.Save();
            //print($"PS: {PlayerPrefs.GetString("PlayerStatus")}, SS: {PlayerPrefs.GetString("ServerStatus")}");
        }
        else
        {
            PlayerPrefs.SetString("PlayerStatus", "normal");
            PlayerPrefs.SetString("ServerStatus", "normal");
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        heartbeatData HB = null;
        HB = new heartbeatData(true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(), PlayerPrefs.GetString("PlayerStatus"), PlayerPrefs.GetString("ServerStatus"));

        string data = JsonConvert.SerializeObject(HB);

        JSBridgeManager.Instance.UpdateDataToFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.HEARTBEAT_DATA_PATH}/{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}/{DataManager.UserId}",
                data,
                gameObject.name);
        Invoke("StartHeartbeat", 5);
#endif
    }
    #endregion

    /// <summary>
    /// 更新用戶訊息
    /// </summary>
    public void UpdateUserInfo()
    {
        Nickname_Txt.text = $"@{DataManager.UserNickname}";
        Avatar_Btn.image.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[DataManager.UserAvatarIndex];

        Assets_CryptoChipsValue_Txt.text = $"${StringUtils.SetChipsUnit(DataManager.UserChips)}";
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
                break;
            case ItemType.Settings:
                itemObj = LobbySettingsView;
                break;
            case ItemType.Report:
                itemObj = LobbyReportView;
                break;
            default:
                Debug.LogWarning("Unknown item type: " + itemType);
                return; // Early exit for unknown types
        }

        if (itemObj != null)
        {
            RectTransform itemPageView = Instantiate(itemObj, Floor3).GetComponent<RectTransform>();
            ViewManager.Instance.InitViewTr(itemPageView, itemType.ToString());
        }
    }
    //外部調用
    public void callOpenItemPage(ItemType itemType)
    {
        OpenItemPage(itemType);
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
        ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Warning,
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

    /// <summary>
    /// 安卓離桌後重啟心跳
    /// </summary>
    public void reStartHertbeat()
    {
#if UNITY_ANDROID
        AndroidHB.Instance.initHeartBeat(DataManager.UserId);
#endif
    }

    IEnumerator openRefreshBtn()
    {
        yield return new WaitForSeconds(5);

        Refresh_Btn.interactable = true;
    }

    /// <summary>
    /// 入金協程(暫放)
    ///</summary>
   public IEnumerator PostTransactionData(string authorSession, Transaction transacData)
    {
        // 建立 UnityWebRequest，設定請求的 URL
        string url = $"https://noodle-dev.azurewebsites.net/api/transaction";
        string jsonData = JsonConvert.SerializeObject(transacData);
        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        // 設定請求頭
        request.SetRequestHeader("accept", "application/json");
        request.SetRequestHeader("Session", authorSession);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("RequestVerificationToken", "CfDJ8GAhEUSluuBNskVi60eW89y5tH86uVPQpRg45s0KPLVrPy8Kh1GTpIJCVeJd1SI5RKJJX5qAOfP_g7cp7J4N0P8i2DWTVqvTlBtMoN5juBLkGB3NBDf10u5SjUpdGs5nKqx2DDZrFNOb0Mfvhtt9a6c");
        request.SetRequestHeader("X-Requested-With", "XMLHttpRequest");

        request.uploadHandler = new UploadHandlerRaw(jsonToSend);

        // 發送請求並等待回應
        yield return request.SendWebRequest();

        // 檢查請求是否出現錯誤
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // 輸出請求結果
            //Debug.Log("Response: " + request.downloadHandler.text);

            string jsonResponse = request.downloadHandler.text;
            print(jsonResponse);
        }
    }

    public void testAddScene()
    {
        LoadSceneManager.Instance.LoadScene("Game");
    }
}