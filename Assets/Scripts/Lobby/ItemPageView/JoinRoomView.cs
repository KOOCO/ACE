using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Events;
using System;
using Proyecto26;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class JoinRoomView : MonoBehaviour
{
    [SerializeField]
    Request_JoinRoom baseRequest;
    [SerializeField]
    Image BlindACoin_Img, BlindUCoin_Img,
          MinBuyACoin_Img, MinBuyUCoin_Img,
          MaxBuyACoin_Img, MaxBuyUCoin_Img;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Close_Btn, Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, BlindsTitle_Txt,
                    Blind_Txt, PreBuyChips_Txt,
                    MinBuyChips_Txt, MaxBuyChips_Txt,
                    CancelBtn_Txt, BuyBtn_Txt;
    [SerializeField]
    SliderClickDetection sliderClickDetection;

    LobbyView lobbyView;
    string dataRoomName;                 //查詢資料的房間名稱
    double smallBlind;                   //小盲值
    TableTypeEnum tableType;             //房間類型
    double newCarryChipsValue;           //更新後的購買籌碼

    bool isClassic;

    string actionType;
    GameRoom previousRoom;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        BlindsTitle_Txt.text = LanguageManager.Instance.GetText("Blind Bet");
        CancelBtn_Txt.text = LanguageManager.Instance.GetText("CANCEL");
        BuyBtn_Txt.text = LanguageManager.Instance.GetText("CONFIRM");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    public void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //關閉
        Close_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = DataManager.isInRoom;
            gameObject.SetActive(false);
        });

        //取消
        Cancel_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = DataManager.isInRoom;
            gameObject.SetActive(false);
        });

        //購買
        Buy_Btn.onClick.AddListener(() =>
        {
            //if status is 'banned' than cancel join room
            if (PlayerPrefs.GetString("PlayerStatus") == playerStatus.banned.ToString())
            {
                ViewManager.Instance.OpenTipMsgView(lobbyView.transform, messageStatus.Failed, LanguageManager.Instance.GetText("Account abnormal"));
                return;
            }

            //籌碼不足
            if (tableType == TableTypeEnum.Cash &&
                newCarryChipsValue > DataManager.UserChips)
            {
                ViewManager.Instance.OpenTipMsgView(lobbyView.transform, messageStatus.Failed, LanguageManager.Instance.GetText("Purchase Unsuccessful, Please Try Again!"));
                gameObject.SetActive(false);
                return;
            }
            else if (tableType == TableTypeEnum.VCTable &&
                     newCarryChipsValue > DataManager.UserAChips)
            {
                ViewManager.Instance.OpenTipMsgView(lobbyView.transform, messageStatus.Failed, LanguageManager.Instance.GetText("Purchase Unsuccessful, Please Try Again!"));
                gameObject.SetActive(false);
                return;
            }

            ViewManager.Instance.OpenWaitingView(transform);
            //進入房間停播音樂
            //lobbyView.audioSource.Stop();
            JoinRoom newRound = new JoinRoom
            {
                memberId = DataManager.UserId,
                tableId = DataManager.TableId,
                amount = newCarryChipsValue
            };
            Debug.Log($"MemberId {newRound.memberId} :: TableId {newRound.tableId} :: Amount {newRound.amount}");

            //ViewManager.Instance.OpenWaitingView(transform);
            AppApi.OnJoinRoom(newRound, OnJoinRoomSuccess, OnJoinRoomFail);
        });

        //購買Slider單位設定
        BuyChips_Sli.onValueChanged.AddListener((value) =>
        {
            newCarryChipsValue = TexasHoldemUtil.SliderValueChange(BuyChips_Sli,
                                                        value,
                                                        smallBlind * 2,
                                                        BuyChips_Sli.minValue,
                                                        BuyChips_Sli.maxValue,
                                                        sliderClickDetection);
            PreBuyChips_Txt.text = StringUtils.SetChipsUnit(newCarryChipsValue);
        });

        //購買+按鈕
        BuyPlus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newCarryChipsValue + smallBlind * 2);
        });

        //購買-按鈕
        BuyMinus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newCarryChipsValue - smallBlind * 2);
        });
    }


    void OnJoinRoomSuccess(string data)
    {
        GameRoom gameRound = JsonConvert.DeserializeObject<GameRoom>(data);
        Debug.Log("Join Round Response :: " + data);
        //if (previousRoom == null || previousRoom.id == gameRound.id)
        //{
        //    previousRoom = gameRound;
        //    Debug.Log("same Room id ");
        //}
        //else
        //{
        //    CreateNewRoom();
        //    Debug.Log("Room id different");
        //    return;
        //}
        DataManager.TableType = gameRound.tableType;
        DataManager.Rebate = gameRound.table.rebateSetting;
        DataManager.RoundId = gameRound.roundId;
        DataManager.RoomId = gameRound.roomId;
        actionType = gameRound.actionType;

        SendRoomDataToJS(DataManager.UserId);

#if UNITY_EDITOR

        /*dataRoomName = DataManager.RoomId;
        //創新房間資料
        var dataDic = new Dictionary<string, object>()
        {
            { FirebaseManager.SMALL_BLIND, smallBlind},                         //小盲值
            { FirebaseManager.ROOM_HOST_ID, DataManager.UserId},                //房主ID
            { FirebaseManager.POT_CHIPS, 0},                                    //底池總籌碼
            { FirebaseManager.COMMUNITY_POKER, new List<int>()},                //公共牌
            { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>()},           //當前公共牌
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}/{tableType}/{smallBlind}",
            dataDic,
            gameObject.name,
            nameof(JoinRoomQueryCallback));*/
        JoinRoomQueryCallback();
        Debug.LogError("Cause Editor can't play game, so cancel join/create room, please 'Build First'.");
        return;
#endif
        //CreateOrJoinRoom();
        JoinRoomQueryCallback();

    }
    void OnJoinRoomFail(string error)
    {
        Debug.LogError("JoinRoomView :: OnJoinRoomFail : " + error);
    }

    private string BASE_URL = "https://admin-d.jf588.com";  // API Base URL

    public void SendRoomDataToJS(string _memberId)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // Set the URL and query parameters
    string apiEndpoint = "/api/app/rooms/player-disconnected?memberId="+_memberId;
    string memberId = _memberId;
    // string pipfullUrl = "https://f9de149c298966a11d6f15feb43b45f9.m.pipedream.net";
    string fullUrl = BASE_URL + apiEndpoint;
    // Store the URL in JavaScript
    StoreVariableJS(fullUrl,memberId);

    // Call page visibility-related functions
    PageChangeVisibility();
    AddEventListeners();

#else
        Debug.Log("This function only works in a WebGL build.");
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // Define the external JavaScript functions

    [DllImport("__Internal")]
    private static extern void onPageLoadWithVisibilityChange();

    [DllImport("__Internal")]
    private static extern void storeVariable(string value,string memberId);

    [DllImport("__Internal")]
    private static extern void onPageLoad();

  
#endif

    public void PageChangeVisibility()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        onPageLoadWithVisibilityChange();
        Debug.Log("Unity: PageChangeVisibility called");
#endif
    }



    public void StoreVariableJS(string value, string memberId)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        storeVariable(value,memberId);
        Debug.Log("Unity: Stored variable in JavaScript: " + value);
#endif
    }

    public void AddEventListeners()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        onPageLoad();
        Debug.Log("Unity: onPageLoad called");
#endif
    }

    public void CreateOrJoinRoom()
    {
        JSBridgeManager.Instance.JoinRoomQueryData($"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}/{tableType}/{smallBlind}",
                                                    $"{DataManager.MaxPlayerCount}",
                                                    $"{DataManager.UserId}",
                                                    gameObject.name,
                                                    nameof(JoinRoomQueryCallback));
    }



    /// <summary>
    /// 設定創建房間介面
    /// </summary>
    /// <param name="tableType">遊戲桌類型</param>
    /// <param name="smallBlind">小盲值</param>
    public void SetCreatRoomViewInfo(TableTypeEnum tableType, double smallBlind)
    {
        this.smallBlind = smallBlind;
        this.tableType = tableType;

        string titleStr = "";
        switch (tableType)
        {
            //現金桌
            case TableTypeEnum.Cash:
                titleStr = "High Roller Battleground";
                isClassic = false;
                BlindACoin_Img.gameObject.SetActive(true);
                BlindUCoin_Img.gameObject.SetActive(false);
                MinBuyACoin_Img.gameObject.SetActive(true);
                MinBuyUCoin_Img.gameObject.SetActive(false);
                MaxBuyACoin_Img.gameObject.SetActive(true);
                MaxBuyUCoin_Img.gameObject.SetActive(false);
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "Classic Battle";
                isClassic = true;
                BlindACoin_Img.gameObject.SetActive(false);
                BlindUCoin_Img.gameObject.SetActive(true);
                MinBuyACoin_Img.gameObject.SetActive(false);
                MinBuyUCoin_Img.gameObject.SetActive(true);
                MaxBuyACoin_Img.gameObject.SetActive(false);
                MaxBuyUCoin_Img.gameObject.SetActive(true);
                break;
        }
        Title_Txt.text = LanguageManager.Instance.GetText(titleStr);

        Blind_Txt.text = $"${StringUtils.SetChipsUnit(smallBlind)} / " +
                         $"${StringUtils.SetChipsUnit(smallBlind * 2)}";

        if (isClassic)
            TexasHoldemUtil.SetBuySlider(this.smallBlind * 2, DataManager.UserAChips < ((this.smallBlind * 2) * DataManager.MaxMagnification) ? DataManager.UserAChips : (this.smallBlind * 2) * DataManager.MaxMagnification, BuyChips_Sli, tableType);
        else
            TexasHoldemUtil.SetBuySlider(this.smallBlind * 2, DataManager.UserChips < ((this.smallBlind * 2) * DataManager.MaxMagnification) ? DataManager.UserChips : (this.smallBlind * 2) * DataManager.MaxMagnification, BuyChips_Sli, tableType);

        MinBuyChips_Txt.text = $"${StringUtils.SetChipsUnit((this.smallBlind * 2) * DataManager.MinMagnification)}";
        MaxBuyChips_Txt.text = $"${StringUtils.SetChipsUnit((this.smallBlind * 2) * DataManager.MaxMagnification)}"; ;
    }

    /// <summary>
    /// 加入房間查詢回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void JoinRoomQueryCallback()
    {
        // Deserialize JSON data into a QueryRoom object
        // Debug.Log("JoinRoomView :: JoinRoomQueryCallback : " + jsonData);
        // QueryRoom queryRoom = FirebaseManager.Instance.OnFirebaseDataRead<QueryRoom>(jsonData);

        // // Handle errors
        // if (!string.IsNullOrEmpty(queryRoom?.error))
        // {
        //     Debug.LogError($"JoinRoomQueryCallback Error: {queryRoom.error}");
        //     return;
        // }

        // Debug.Log($"JoinRoomQueryCallback :: Room Name: {queryRoom?.getRoomName}, Room Count: {queryRoom?.roomCount}");

        // Validate dataRoomName
        dataRoomName = DataManager.RoomId;

        if (string.IsNullOrEmpty(dataRoomName))
        {
            Debug.LogError("JoinRoomQueryCallback Error: Invalid Room ID.");
            return;
        }

        // Construct the common data path
        string dataPath = $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}/{tableType}/{smallBlind}/{dataRoomName}";

        if (actionType == "Create")
        {
            // Data for creating a new room
            var dataDic = new Dictionary<string, object>
            {
                { FirebaseManager.SMALL_BLIND, smallBlind },                  // Small blind amount
                { FirebaseManager.ROOM_HOST_ID, DataManager.UserId },         // Host ID
                { FirebaseManager.POT_CHIPS, 0 },                             // Total pot chips
                { FirebaseManager.COMMUNITY_POKER, new List<int>() },         // Community cards
                { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>() }     // Current community cards
            };

            Debug.Log($"JoinRoomQueryCallback :: Creating Room at Path: {dataPath}");

            // Write data to Firebase
            JSBridgeManager.Instance.WriteDataFromFirebase(
                dataPath,
                dataDic,
                gameObject.name,
                nameof(CreateNewRoomCallback));
        }
        else if (actionType == "Join")
        {
            Debug.Log($"JoinRoomQueryCallback :: Joining Room at Path: {dataPath}");
            // Read data from Firebase
            JSBridgeManager.Instance.ReadDataFromFirebase(
                dataPath,
                gameObject.name,
                nameof(JoinRoomCallback));
        }
        else
        {
            Debug.LogWarning($"JoinRoomQueryCallback Warning: Invalid actionType '{actionType}' specified.");
        }

        // Reset actionType to avoid accidental reuse
        actionType = string.Empty;
    }


    /// <summary>
    /// 創建新房間回傳
    /// </summary>
    /// <param name="isSuccess">創建/加入房間回傳結果</param>
    public void CreateNewRoomCallback(string isSuccess)
    {
        Debug.Log($"JoinRoomView :: {nameof(CreateNewRoomCallback)} : {isSuccess}");
        //錯誤
        if (isSuccess == "false")
        {
            ViewManager.Instance.CloseWaitingView(transform);
            Debug.LogError("Create Room Error!!!");
            return;
        }

        StartCoroutine(IYieldInCreateRoom());
    }

    /// <summary>
    /// 延遲創建房間
    /// </summary>
    /// <returns></returns>
    private IEnumerator IYieldInCreateRoom()
    {
        Debug.Log($"JoinRoomView :: {nameof(IYieldInCreateRoom)}");
        yield return new WaitForSeconds(0.2f);

        GameRoomManager.Instance.CreateGameRoom(tableType,
                                                smallBlind,
                                                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}/{tableType}/{smallBlind}/{dataRoomName}",
                                                true,
                                                newCarryChipsValue,
                                                0);

        OnEnterTable();
        ViewManager.Instance.CloseWaitingView(transform);

        gameObject.SetActive(false);
        DataManager.isInRoom = true;
        print("Is in Room: " + DataManager.isInRoom);
    }

    /// <summary>
    /// 加入房間回傳
    /// </summary>
    /// <param name="jsonData">房間資料</param>
    public void JoinRoomCallback(string jsonData)
    {
        Debug.Log("JoinRoomView :: JoinRoomCallback : " + jsonData);

        var gameRoomData = FirebaseManager.Instance.OnFirebaseDataRead<GameRoomData>(jsonData);
        int seat = TexasHoldemUtil.SetGameSeat(gameRoomData);

        Debug.Log($"JoinRoomView :: {nameof(CreateNewRoomCallback)} : {jsonData}");

        //本地創建房間
        GameRoomManager.Instance.CreateGameRoom(tableType,
                                                smallBlind,
                                                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}/{tableType}/{smallBlind}/{dataRoomName}",
                                                false,
                                                newCarryChipsValue,
                                                seat);

        OnEnterTable();
        ViewManager.Instance.CloseWaitingView(transform);
        gameObject.SetActive(false);
        DataManager.isInRoom = true;
        print("Is in Room: " + DataManager.isInRoom);
    }

    void OnEnterTable()
    {
        NoodleApi.PostTableBuyIn(newCarryChipsValue, (data) =>
        {
            Debug.Log("Table BuyIn SuccessFull.");
            var _currencyType = DataManager.CurrencyType;
            Debug.Log("Currency Type :: " + _currencyType);
            switch (_currencyType)
            {
                case CurrencyType.Gold:
                    Debug.Log(_currencyType);
                    DataManager.UserGold -= newCarryChipsValue;
                    break;
                case CurrencyType.ACoin:
                    Debug.Log(_currencyType);
                    DataManager.UserAChips -= newCarryChipsValue;
                    break;
                case CurrencyType.UCoin:
                    Debug.Log(_currencyType);
                    DataManager.UserChips -= newCarryChipsValue;
                    break;
            }
            DataManager.DataUpdated = true;
        },
        (error) =>
        {
            Debug.LogError($"Table BuyIn Failed Error: {error}");
        });

    }
}
