using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using static LoginView;
using Microsoft.AspNet.SignalR.Client.Http;
using Newtonsoft.Json;
//using Org.BouncyCastle.Math.EC.Rfc7748;

public class LobbyMainPageView : MonoBehaviour
{
    [Header("Tables")]
    public List<GameObject> tables;
    [Header("背景")]
    [SerializeField]
    Image Bg_Img;

    [Header("廣告刊版")]
    [SerializeField]
    GameObject BillboardSample;

    [Header("積分房")]
    [SerializeField]
    Button Integral_Btn;
    [SerializeField]
    TextMeshProUGUI IntegralBtn_Txt;


    [Header("Rank Battle")]
    [SerializeField]
    GameObject RankBattleBtnSample;
    [SerializeField]
    RectTransform RankTableParent;

    [Header("加密貨幣桌")]
    [SerializeField]
    GameObject CryptoTableBtnSample, CryptoTableTitle;
    [SerializeField]
    RectTransform CryptoTableParent;

    [Header("虛擬貨幣桌")]
    [SerializeField]
    GameObject VCTableBtnSample;
    [SerializeField]
    RectTransform VCTableParent;

    [Header("Line客服")]
    [SerializeField]
    Button LineService;

    LobbyView lobbyView;

    public Image billboardImg;                    //廣告刊版圖片物件
    public List<Sprite> billboardSprList;                    //廣告刊版圖片

    string dataRoomName;                                    //查詢資料的房間名稱
    string pairPlayerUserId;                                //被配對上的玩家ID

    private void OnEnable()
    {
        CryptoTableTitle.SetActive(false);
    }

    /// <summary>
    /// 背景開關
    /// </summary>
    public bool SwitchBg
    {
        set
        {
            Bg_Img.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        IntegralBtn_Txt.text = LanguageManager.Instance.GetText("GO TO INTEGRAL");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        //integralData = new IntegralData();

        ListenerEvent();
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        /*
        //積分房
        Integral_Btn.onClick.AddListener(() =>
        {
            if (integralData.isPairing)
            {
                //正在配對取消配對
                IntegralEndPair();
            }
            else
            {
                //籌碼不足
                if (DataManager.UserAChips < DataManager.IntegralNeedChips)
                {
                    ViewManager.Instance.OpenTipMsgView(transform, messageStatus.Failed, LanguageManager.Instance.GetText("Purchase Unsuccessful, Please Try Again!"));
                }
                else
                {
                    if (GameRoomManager.Instance.JudgeIsCanBeCreateRoom())
                    {
                        //開始配對
                        pairPlayerUserId = "";
                        integralData.isPairing = true;
                        integralData.startPairTime = DateTime.Now;

                        //移除未使用積分房
                        JSBridgeManager.Instance.JoinRoomQueryData($"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_ROOM}",
                                            $"{2}",
                                            $"{DataManager.UserId}",
                                            gameObject.name,
                                            nameof(CheckRoomCallback));

                        //讀取積分房訊息
                        JSBridgeManager.Instance.ReadDataFromFirebase($"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}",
                                                                      gameObject.name,
                                                                      nameof(QueryCallback));
                    }
                    else
                    {
                        //房間數已達上限
                        lobbyView.ShowMaxRoomTip();
                    }
                }
            }
        });

        //  Line客服
        LineService.onClick.AddListener(() =>
        {
            StartLineLogin();
        });
        */
    }

    private void Start()
    {
        lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
        SwitchBg = false;

        ViewManager.Instance.OpenWaitingView(transform.parent);
        SwaggerAPIManager.Instance.SendGetAPI("/api/app/tables", (data) =>
        {
            Debug.Log("Tables data :: " + data);
            tablesData = JsonConvert.DeserializeObject<TableItemList>(data);
            CreateRoomBtn();
        }, null, true);

        billboardImg.sprite = billboardSprList[LanguageManager.Instance.GetCurrLanguageIndex()];
    }

    private void Update()
    {
        #region 積分房

        ////積分配對計時器
        //if (integralData.isPairing)
        //{
        //    TimeSpan waitingTime = DateTime.Now - integralData.startPairTime;
        //    IntegralBtn_Txt.text = $"{LanguageManager.Instance.GetText("Pairing")}:{(int)waitingTime.TotalMinutes} : {waitingTime.Seconds:00}";

        //    //配對中房間已達上限
        //    if (GameRoomManager.Instance.GetRoomCount >= GameRoomManager.Instance.maxRoomCount)
        //    {
        //        IntegralEndPair();
        //    }
        //}

        #endregion
    }

    #region 積分房

    /*
    /// <summary>
    /// 積分房資料
    /// </summary>
    private IntegralData integralData;
    public class IntegralData
    {
        public bool isPairing;              //是否正在配對中
        public DateTime startPairTime;      //開始配對時間
    }

    public void CheckRoomCallback(string jsonData)
    {

    }

    /// <summary>
    /// 查詢積分房房間回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void QueryCallback(string jsonData)
    {
        Debug.Log($"QueryCallback invoked with jsonData: {jsonData}");
        var gameRoomData = FirebaseManager.Instance.OnFirebaseDataRead<IntegralTable>(jsonData);
        Debug.Log($"查詢積分房房間回傳人數: {gameRoomData.integralWaitData.Count}");

        //尋找未配對玩家
        var data = new Dictionary<string, object>();
        foreach (var waitPlayer in gameRoomData.integralWaitData)
        {
            Debug.Log($"Checking player {waitPlayer.Key}, paired: {waitPlayer.Value.paired}, pairRoomName: {waitPlayer.Value.pairRoomName}");

            //配對到玩家
            if (waitPlayer.Value.paired == false &&
                string.IsNullOrEmpty(waitPlayer.Value.pairRoomName))
            {
                //更新被配對玩家資料
                data = new Dictionary<string, object>()
            {
                { FirebaseManager.PAIRED, true},        //是否已被選上配對
            };
                Debug.Log($"Pairing with player {waitPlayer.Key}");
                JSBridgeManager.Instance.UpdateDataFromFirebase(
                    $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{waitPlayer.Key}",
                    data);

                string roomToken = StringUtils.GenerateRandomString(DataManager.RoomTokenLength);
                dataRoomName = $"{FirebaseManager.INTEGRAL_ROOM}_{roomToken}";
                pairPlayerUserId = waitPlayer.Key;

                //創建房間
                data = new Dictionary<string, object>()
            {
                { FirebaseManager.SMALL_BLIND, DataManager.IntegralSmallBlind},         //小盲值
                { FirebaseManager.ROOM_HOST_ID, DataManager.UserId},                    //房主ID
                { FirebaseManager.POT_CHIPS, 0},                                        //底池總籌碼
            };
                Debug.Log($"Creating room: {dataRoomName} with host ID: {DataManager.UserId}");
                JSBridgeManager.Instance.WriteDataFromFirebase(
                    $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_ROOM}/{dataRoomName}",
                    data,
                    gameObject.name,
                    nameof(CreateIntegralRommCallback));

                return;
            }
        }

        Debug.Log($"No available pairs found. Adding user {DataManager.UserId} to waiting list.");
        //加入配對列表
        data = new Dictionary<string, object>()
    {
        { FirebaseManager.USER_ID, DataManager.UserId},             //等待玩家ID
        { FirebaseManager.PAIR_ROOM_NAME, ""},                      //配對成功房間名稱
        { FirebaseManager.PAIRED, false},                           //是否已被選上配對
    };
        JSBridgeManager.Instance.UpdateDataFromFirebase(
            $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{DataManager.UserId}",
            data);

        //開始監聽配對
        Debug.Log($"Starting to listen for pairing changes for user {DataManager.UserId}");
        JSBridgeManager.Instance.StartListeningForDataChanges(
            $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{DataManager.UserId}",
            gameObject.name,
            nameof(ListenerPairCallback));
    }

    /// <summary>
    /// 創建積分房回傳
    /// </summary>
    /// <param name="isSuccess"></param>
    public void CreateIntegralRommCallback(string isSuccess)
    {
        Debug.Log($"CreateIntegralRommCallback invoked with result: {isSuccess}");
        IntegralEndPair();

        //錯誤
        if (isSuccess == "false")
        {
            ViewManager.Instance.CloseWaitingView(transform.parent);
            Debug.LogError("Create Room Error!!!");
            return;
        }

        Debug.Log($"Room created successfully. Joining room: {dataRoomName}");
        GameRoomManager.Instance.CreateGameRoom(TableTypeEnum.IntegralTable,
                                                DataManager.IntegralSmallBlind,
                                                $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_ROOM}/{dataRoomName}",
                                                true,
                                                (int)DataManager.IntegralNeedChips,
                                                0,
                                                pairPlayerUserId,
                                                dataRoomName);
    }

    /// <summary>
    /// 監聽配對回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void ListenerPairCallback(string jsonData)
    {
        Debug.Log($"ListenerPairCallback invoked with jsonData: {jsonData}");
        var loginData = FirebaseManager.Instance.OnFirebaseDataRead<IntefralWaitUserData>(jsonData);

        //被配對到
        if (!string.IsNullOrEmpty(loginData.pairRoomName))
        {
            Debug.Log($"User paired with room: {loginData.pairRoomName}");
            IntegralEndPair();

            //移除監聽
            JSBridgeManager.Instance.StopListeningForDataChanges(
                $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{DataManager.UserId}");

            //加入房間
            dataRoomName = loginData.pairRoomName;
            GameRoomManager.Instance.CreateGameRoom(TableTypeEnum.IntegralTable,
                                                    DataManager.IntegralSmallBlind,
                                                    $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_ROOM}/{dataRoomName}",
                                                    false,
                                                    (int)DataManager.IntegralNeedChips,
                                                    3,
                                                    null,
                                                    loginData.pairRoomName);
        }
        else
        {
            Debug.Log($"User {DataManager.UserId} is still waiting for pairing.");
        }

        Debug.Log("Test");
    }

    /// <summary>
    /// 積分房結束配對
    /// </summary>
    private void IntegralEndPair()
    {
        Debug.Log($"Ending pairing process for user {DataManager.UserId}");
        IntegralBtn_Txt.text = LanguageManager.Instance.GetText("INTEGRAL");
        integralData.isPairing = false;

#if UNITY_EDITOR
        return;
#endif

        //移除監聽
        Debug.Log($"Removing data change listener for user {DataManager.UserId}");
        JSBridgeManager.Instance.StopListeningForDataChanges(
            $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{DataManager.UserId}");

        //從配對中移除
        Debug.Log($"Removing user {DataManager.UserId} from pairing list");
        JSBridgeManager.Instance.RemoveDataFromFirebase(
                $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{DataManager.UserId}");
    }

    */
    #endregion

    /// <summary>
    /// 創建房間按鈕
    /// </summary>
    TableItemList tablesData = new();
    private void CreateRoomBtn()
    {
        // Define each mode with their respective table index, title, sample button, and parent
        SetupTable(0, tables[0], RankBattleBtnSample, RankTableParent);
        SetupTable(1, tables[1], CryptoTableBtnSample, CryptoTableParent);
        SetupTable(2, tables[2], VCTableBtnSample, VCTableParent);

        CryptoTableTitle.SetActive(true);
        ViewManager.Instance.CloseWaitingView(transform.parent);
    }

    // Helper method to set up each table
    private void SetupTable(int mode, GameObject table, GameObject btnSample, Transform parent)
    {
        var selectedData = tablesData.items.Where(x => x.mode == mode && x.isEnable == true).ToList();

        if (selectedData.Count > 0)
        {
            table.SetActive(true);
            btnSample.SetActive(false);

            foreach (var data in selectedData)
            {
                RectTransform rt = Instantiate(btnSample).GetComponent<RectTransform>();
                rt.gameObject.SetActive(true);
                rt.SetParent(parent);
                rt.localScale = Vector3.one;

                if (mode == 0)
                    rt.GetComponent<RankBattleSampleBtn>().SetRankBattleBtnInfo(data.smallStake, lobbyView, data.id);
                else if (mode == 1)
                    rt.GetComponent<CryptoTableBtnSample>().SetCryptoTableBtnInfo(data.smallStake, lobbyView, data.id);
                else if (mode == 2)
                    rt.GetComponent<VCTableBtnSample>().SetVCTableBtnInfo(data.smallStake, lobbyView, data.id);
            }
        }
        else
        {
            table.SetActive(false);
        }
    }


    #region Line客服加好友
    
    /*
    public void StartLineLogin()
    {
        string state = GenerateRandomString();
        string nonce = GenerateRandomString();
        string authUrl = $"https://line.me/ti/p/@309jwned";
                    //+
                    // $"client_id={DataManager.LineChannelId}&" +
                    // $"redirect_uri={DataManager.RedirectUri}&" +
                    // $"state={state}&" +
                    // $"scope=profile%20openid%20email&nonce={nonce}";

        //JSBridgeManager.Instance.LocationHref(authUrl);

        JSBridgeManager.Instance.onLineService(authUrl);
    }
    private string GenerateRandomString(int length = 16)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
    */
    #endregion

}
