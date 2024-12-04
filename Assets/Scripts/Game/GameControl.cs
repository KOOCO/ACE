using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices;
public enum WinnerEnum
{
    MAIN,
    SIDE,
    BOTH
}

public class GameControl : MonoBehaviour
{
    [SerializeField]
    GameView gameView;
    [SerializeField]
    RobotControl RobotControl;

    public string QueryRoomPath { get; set; }                   //查詢房間資料路徑
    public double SmallBlind { get; set; }                      //小盲值
    public TableTypeEnum RoomType { get; set; }                 //房間類型
    public SwitchRoomBtn switchRoomBtn { get; set; }            //切換房間按鈕
    public int MaxRoomPeople { get; set; }                      //房間最大人數
    public double PreBuyChipsValue { get; set; }                //下一手購買籌碼值
    public double leastChips { get; set; }                      //最少所需籌碼

    GameRoomData gameRoomData;                                  //房間資料
    Coroutine cdCoroutine;                                      //倒數Coroutine

    int prePlayerCount { get; set; }                            //上個紀錄的遊戲人數
    bool isWaitingCreateRobot { get; set; }                     //是否等待產生機器人
    bool isGameStart { get; set; }                              //是否遊戲開始
    public GameFlowEnum preUpdateGameFlow { get; set; }         //上個更新遊戲流程
    public GameFlowEnum preLocalGameFlow { get; set; }          //上個本地遊戲流程
    string preBetActionerId { get; set; }                       //上個下注玩家
    int preCD { get; set; }                                     //當前行動倒數時間
    bool isCloseAllCdInfo { get; set; }                         //是否關閉倒數訊息
    List<int> localHand { get; set; }                           //本地玩家手牌
    int cdSound { get; set; }                                   //倒數聲音計時器



    private void OnDestroy()
    {
#if UNITY_EDITOR

        JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}");

#endif

        StopAllCoroutines();
    }

    private void Start()
    {

#if UNITY_EDITOR

        EditorReadRoomData();
        InvokeRepeating(nameof(EditorReadRoomData), 1, 1f);
        return;
#endif

        //判斷玩家在線狀態
        InvokeRepeating(nameof(JudgePlayersOnline), 5, 5);
    }

    private void Update()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreateRobot(false);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RemoveRobot();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            string id = gameRoomData.currActionerId;
            UpdateBetAction(id,
                            BetActingEnum.Call,
                            gameRoomData.currCallValue);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            string id = gameRoomData.currActionerId;
            UpdateBetAction(id,
                            BetActingEnum.Check,
                            0);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            string id = gameRoomData.currActionerId;
            UpdateBetAction(id,
                            BetActingEnum.Raise,
                            gameRoomData.currCallValue + gameRoomData.smallBlind);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            string id = gameRoomData.currActionerId;
            UpdateBetAction(id,
                            BetActingEnum.Fold,
                            0);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            string id = gameRoomData.currActionerId;
            GameRoomPlayerData p = gameRoomData.playerDataDic.Where(x => x.Value.userId == id)
                                                             .FirstOrDefault()
                                                             .Value;
            UpdateBetAction(id,
                            BetActingEnum.AllIn,
                            p.carryChips);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //更新房主
            var dataDic = new Dictionary<string, object>()
                {
                     { FirebaseManager.ROOM_HOST_ID, "robot1"},
                };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}",
                                                            dataDic);
        }

#endif

        if (gameRoomData != null &&
            gameRoomData.playerDataDic != null)
        {
            //初始遊戲開始
            if (isGameStart == false &&
                gameRoomData.playerDataDic.Count >= 2 &&
                gameRoomData.hostId == DataManager.UserId)
            {
                isGameStart = true;
                Debug.Log("IStartGameFlow :: Update Licensing");
                isLicense = false;
                StartCoroutine(IStartGameFlow(GameFlowEnum.Licensing));
            }

            //關閉所有玩家倒數
            gameView.CloseCDInfo(isCloseAllCdInfo ? "" : gameRoomData.currActionerId);
        }
    }

    #region 起始

    /// <summary>
    /// 讀取遊戲資料
    /// </summary>
    public void ReadGameData()
    {
        //讀取房間資料
        JSBridgeManager.Instance.ReadDataFromFirebase($"{QueryRoomPath}",
                                                      gameObject.name,
                                                      nameof(ReadGameRoomDataCallback));
    }

    /// <summary>
    /// 編輯器讀取房間資料
    /// </summary>
    public void EditorReadRoomData()
    {
        //讀取房間資料
        JSBridgeManager.Instance.ReadDataFromFirebase($"{QueryRoomPath}",
                                                      gameObject.name,
                                                      nameof(GameRoomDataCallback));
    }

    /// <summary>
    /// 讀取房間資料回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void ReadGameRoomDataCallback(string jsonData)
    {
        var data = FirebaseManager.Instance.OnFirebaseDataRead<GameRoomData>(jsonData);
        gameRoomData = data;

        leastChips = gameRoomData.smallBlind * 2;

        //更新房間玩家訊息
        gameView.UpdateGameRoomInfo(gameRoomData);

#if UNITY_EDITOR

        //產生機器人
        if (isWaitingCreateRobot)
        {
            isWaitingCreateRobot = false;
            CreateRobot(true);
        }

        return;
#endif

        //開始監聽遊戲房間資料
        JSBridgeManager.Instance.StartListeningForDataChanges($"{QueryRoomPath}",
                                                              gameObject.name,
                                                              nameof(GameRoomDataCallback));

        //開始監聽連線狀態
        JSBridgeManager.Instance.StartListenerConnectState($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{DataManager.UserId}");

        //產生機器人
        if (isWaitingCreateRobot)
        {
            isWaitingCreateRobot = false;
            CreateRobot(true);
        }
    }

    #endregion

    #region 玩家進出房間

    /// <summary>
    /// 創建首個玩家
    /// </summary>
    /// <param name="carryChips">攜帶籌碼</param>
    /// <param name="seatIndex">遊戲座位</param>
    /// <param name="pairPlayerId">積分被配對上的玩家ID</param>
    /// <param name="integralRoomName">積分房間名稱</param>
    public void CreateFirstPlayer(double carryChips, int seatIndex, string pairPlayerId = null, string integralRoomName = null)
    {
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.USER_ID, DataManager.UserId},                         //用戶ID
            { FirebaseManager.NICKNAME, DataManager.UserNickname},                  //暱稱
            { FirebaseManager.AVATAR_INDEX, DataManager.UserAvatarIndex},           //頭像編號
            { FirebaseManager.CARRY_CHIPS, Math.Floor(carryChips)},                 //攜帶籌碼
            { FirebaseManager.GAME_SEAT, seatIndex},                                //遊戲座位
            { FirebaseManager.GAME_STATE, (int)PlayerStateEnum.Waiting},            //遊戲狀態(等待下局/遊戲中/All In/棄牌)
            { FirebaseManager.IS_PLAYER_LEFT, false},            //遊戲狀態(等待下局/遊戲中/All In/棄牌)
        };
        UpdataPlayerData(DataManager.UserId,
                         data,
                         GameStart);

        //積分配對上的玩家
        if (RoomType == TableTypeEnum.IntegralTable &&
            !string.IsNullOrEmpty(pairPlayerId) &&
            !string.IsNullOrEmpty(integralRoomName))
        {
            //更新被配對玩家資料
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.PAIR_ROOM_NAME, integralRoomName},                //配對成功房間名稱
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase(
                $"{Entry.Instance.releaseType}/{TableTypeEnum.IntegralTable}/{FirebaseManager.INTEGRAL_WAIT_DATA}/{pairPlayerId}",
                data);
        }
    }

    /// <summary>
    /// 遊戲開始
    /// </summary>
    public void GameStart(string isSuccess)
    {
        if (RoomType != TableTypeEnum.IntegralTable)
        {
            isWaitingCreateRobot = true;
        }

        ReadGameData();
    }

    /// <summary>
    /// 新玩家加入房間
    /// </summary>
    /// <param name="carryChips">攜帶籌碼</param>
    /// <param name="seatIndex">遊戲座位</param>
    public void NewPlayerInRoom(double carryChips, int seatIndex)
    {
        isGameStart = true;

        //添加新玩家
        var dataDic = new Dictionary<string, object>()
        {
            { FirebaseManager.USER_ID, DataManager.UserId},                         //用戶ID
            { FirebaseManager.NICKNAME, DataManager.UserNickname},                  //暱稱
            { FirebaseManager.AVATAR_INDEX, DataManager.UserAvatarIndex},           //頭像編號
            { FirebaseManager.CARRY_CHIPS, Math.Floor(carryChips)},                 //攜帶籌碼
            { FirebaseManager.GAME_SEAT, seatIndex},                                //遊戲座位
            { FirebaseManager.GAME_STATE, (int)PlayerStateEnum.Waiting},            //遊戲狀態(等待下局/遊戲中/All In/棄牌)
            { FirebaseManager.IS_PLAYER_LEFT, false},            //遊戲狀態(等待下局/遊戲中/All In/棄牌)
        };
        UpdataPlayerData(DataManager.UserId,
                         dataDic);

        ReadGameData();
    }

    /// <summary>
    /// 離開遊戲
    /// </summary>
    public void ExitGame()
    {
        if (DataManager.UserId == null)
        {
            return;
        }

        LeaveRoom leaveRoom = new LeaveRoom
        {
            memberId = DataManager.UserId,
            roomId = long.Parse(DataManager.RoomId),
            amount = GetPlayerData(DataManager.UserId).carryChips,
            type = DataManager.CurrencyType.ToString(),
            rankPoint = 10
        };

        NoodleApi.PostTableCashOut((data) =>
        {
            Debug.Log("Table CashOut SuccessFull.");
        },
        (error) =>
        {
            Debug.LogError($"Table CashOut Failed Error: {error}");
        });

        AppApi.OnLeaveRoom(leaveRoom, (data) =>
        {
            Debug.Log("Player successfully left the room.");
            DataManager.UserChips += leaveRoom.amount;
            DataManager.DataUpdated = true;
            OnLeaveTable();
            ClearRoomDataFromJS();
        },
        (error) =>
        {
            Debug.LogError($"Failed to leave the room. Error: {error}");
        });

        DataManager.isInRoom = false;
    }
    public void idleExit()
    {
        LeaveRoom leaveRoom = new LeaveRoom
        {
            memberId = DataManager.UserId,
            roomId = long.Parse(DataManager.RoomId),
            amount = GetPlayerData(DataManager.UserId).carryChips,
            type = DataManager.CurrencyType.ToString(),
            rankPoint = 10
        };

        NoodleApi.PostTableCashOut((data) =>
        {
            Debug.Log("Table CashOut SuccessFull.");
        },
        (error) =>
        {
            Debug.LogError($"Table CashOut Failed Error: {error}");
        });

        AppApi.OnLeaveRoom(leaveRoom, (data) =>
        {
            Debug.Log("Player successfully left the room.");
            DataManager.UserChips += leaveRoom.amount;
            DataManager.DataUpdated = true;
            OnLeaveTable();
            ClearRoomDataFromJS();
        },
        (error) =>
        {
            Debug.LogError($"Failed to leave the room. Error: {error}");
        });

        LobbyView lobbyView = GameObject.Find("LobbyView").GetComponent<LobbyView>();
        lobbyView.checkIsIdle();
    }

    [DllImport("__Internal")]
    private static extern void clearStoredVariable();

    public void ClearRoomDataFromJS()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        clearStoredVariable();
        Debug.Log("Unity: clearStoredVariable called");
#endif
    }

    void OnLeaveTable()
    {
        //移除倒數
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);

        //機器人數量
        int robotCount = gameRoomData.playerDataDic.Where(x => x.Value.userId.StartsWith(FirebaseManager.ROBOT_ID))
                                                   .Count();
        //停止監聽遊戲房間資料
        JSBridgeManager.Instance.StopListeningForDataChanges($"{QueryRoomPath}");

        //移除監測連線狀態
        JSBridgeManager.Instance.RemoveListenerConnectState($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{DataManager.UserId}");

        //移除房間判斷
        // if (gameRoomData.playerDataDic.Count - robotCount == 1 &&
        //     RoomType != TableTypeEnum.IntegralTable)
        // {
        //     //房間剩下1名玩家
        //     Debug.Log("OnLeaveTable :: Single Player : " + QueryRoomPath);

        //     JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}");
        // }
        // else
        // {
        //積分房
        // if (gameRoomData.playerDataDic.Count == 1
        // && RoomType == TableTypeEnum.IntegralTable)
        // {
        //     //房間剩下1名玩家
        //     JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}");
        //     GameRoomManager.Instance.RemoveGameRoom(transform.name);
        //     return;
        // }
        // else
        // {
        if (gameRoomData.playerDataDic.Count > 0)
        {
            string newHostId = "";
            if (gameRoomData.hostId == DataManager.UserId)
            {
                newHostId = gameRoomData.playingPlayersIdList
                                    .FirstOrDefault(x => x != DataManager.UserId && !x.StartsWith(FirebaseManager.ROBOT_ID));
            }

            Debug.Log("GameControl :: OnLeaveTable : more then one Player : " + QueryRoomPath + " New Host : " + newHostId);

            //更新房主
            if (!string.IsNullOrEmpty(newHostId) && newHostId != "")
            {
                Debug.Log("GameControl :: OnLeaveTable : Setting new host : " + newHostId);
                var dataDic = new Dictionary<string, object>()
                    {
                         { FirebaseManager.ROOM_HOST_ID, newHostId},
                    };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}",
                                                                dataDic);
            }
        }
        //}

        //移除玩家
        RemovePlayer(DataManager.UserId);
        //}
        //本地玩家房間關閉
        GameRoomManager.Instance.RemoveGameRoom(transform.name);
    }
    /// <summary>
    /// 移除玩家
    /// </summary>
    /// <param name="id"></param>
    private void RemovePlayer(string id)
    {
        bool allPlayersLeft = gameRoomData.playerDataDic.Count - 1 == 1;

        gameView.PlayerExitRoom(id, allPlayersLeft);

        List<string> playingPlayersId = new();
        foreach (var playerId in gameRoomData.playingPlayersIdList)
        {
            if (playerId != id)
            {
                playingPlayersId.Add(playerId);
            }
        }

        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.PLAYING_PLAYER_ID, playingPlayersId},                 //遊戲中玩家ID
        };
        UpdateGameRoomData(data);

        JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{id}");
    }


    #endregion

    #region 機器人

    /// <summary>
    /// 產生機器人
    /// </summary>
    private void CreateRobot(bool randonSeat = false)
    {
        //設置座位
        int robotSeat = randonSeat == true ?
                        UnityEngine.Random.Range(1, 5) :
                        TexasHoldemUtil.SetGameSeat(gameRoomData);

        //機器人暱稱
        string[] names = {
            "Oliver", "Amelia", "William", "Emma", "James", "Olivia", "Benjamin", "Ava",
            "Lucas", "Sophia", "Henry", "Isabella", "Alexander", "Mia", "Michael", "Charlotte",
            "Elijah", "Harper", "Daniel", "Evelyn", "Matthew", "Abigail", "Joseph", "Emily",
            "David", "Ella", "Jackson", "Lily", "Samuel", "Grace", "Sebastian", "Chloe",
            "Owen", "Victoria", "Jack", "Riley", "Aiden", "Aria", "John", "Scarlett",
            "Luke", "Zoey", "Gabriel", "Lillian", "Anthony", "Aubrey", "Isaac", "Addison",
            "Dylan", "Eleanor", "Wyatt", "Nora", "Carter", "Hannah", "Julian", "Stella",
            "Levi", "Bella", "Isaiah", "Lucy", "Nolan", "Ellie", "Hunter", "Paisley",
            "Caleb", "Audrey", "Christian", "Claire", "Josiah", "Skylar", "Andrew", "Camila",
            "Thomas", "Penelope", "Nathan", "Layla", "Eli", "Anna", "Aaron", "Aaliyah",
            "Charles", "Gabriella", "Connor", "Madelyn", "Jeremiah", "Alice", "Ezekiel", "Ariana",
            "Colton", "Ruby", "Jordan", "Eva", "Cameron", "Serenity", "Nicholas", "Autumn",
            "Adrian", "Quinn", "Grayson", "Peyton"
        };
        string robotName = names[UnityEngine.Random.Range(0, names.Length)];
        while (gameRoomData.playerDataDic.Values.Any(x => x.nickname == robotName))
        {
            robotName = names[UnityEngine.Random.Range(0, names.Length)];
        }

        //機器人頭像
        int avatarLength = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album.Length;
        int robotAvatar = UnityEngine.Random.Range(0, avatarLength);

        //機器人攜帶籌碼
        double robotCarryChips = UnityEngine.Random.Range((int)(SmallBlind * 2) * 20, (int)(SmallBlind * 2) * 80);

        //機器人ID
        string robotId = $"{FirebaseManager.ROBOT_ID}{gameRoomData.robotIndex + 1}";

        //添加機器人
        var dataDic = new Dictionary<string, object>()
        {
            { FirebaseManager.USER_ID, robotId},                             //用戶ID
            { FirebaseManager.NICKNAME, robotName},                          //暱稱
            { FirebaseManager.AVATAR_INDEX, robotAvatar },                   //頭像編號
            { FirebaseManager.CARRY_CHIPS, robotCarryChips},                 //攜帶籌碼
            { FirebaseManager.GAME_SEAT, robotSeat},                         //遊戲座位
            { FirebaseManager.GAME_STATE, PlayerStateEnum.Waiting},          //遊戲狀態(等待下局/遊戲中/All In/棄牌)
        };
        UpdataPlayerData(robotId,
                         dataDic);

        //更新房間機器人編號
        var updateDataDic = new Dictionary<string, object>()
        {
            { FirebaseManager.ROBOT_INDEX, gameRoomData.robotIndex + 1},
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}",
                                                        updateDataDic);
    }

    /// <summary>
    /// 移除機器人
    /// </summary>
    private void RemoveRobot()
    {

        Debug.Log($"{nameof(RemoveRobot)} :: Removing Robot from game");
        string robotId = gameRoomData.playerDataDic.Values.Where(x => x.userId.StartsWith(FirebaseManager.ROBOT_ID))
                                                          .FirstOrDefault()
                                                          .userId;

        if (!string.IsNullOrEmpty(robotId))
        {
            RemovePlayer(robotId);
        }
    }

    #endregion

    #region 斷線判斷

    /// <summary>
    /// 判斷房主
    /// </summary>
    private void JudgeHost()
    {
#if UNITY_EDITOR
        return;
#endif

        if (gameRoomData.playerDataDic == null)
        {
            return;
        }

        //房主離開/斷線
        GameRoomPlayerData host = gameRoomData.playerDataDic.Where(x => x.Value.userId == gameRoomData.hostId)
                                                            .FirstOrDefault()
                                                            .Value;
        if (host == null ||
            host.online == false)
        {
            string oldHostId = gameRoomData.hostId;

            //尋找下位房主
            string newHostID = "";
            foreach (var player in gameRoomData.playerDataDic.Values)
            {
                if (!player.userId.StartsWith(FirebaseManager.ROBOT_ID) &&
                    player.online == true)
                {
                    newHostID = player.userId;
                    break;
                }
            }

            //尋找新房主錯誤
            if (string.IsNullOrEmpty(newHostID))
            {
                return;
            }

            //新房主是本地端
            if (newHostID == DataManager.UserId)
            {
                //更新房主
                var dataDic = new Dictionary<string, object>()
                {
                     { FirebaseManager.ROOM_HOST_ID, DataManager.UserId},
                };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}",
                                                                dataDic);

                //舊房主斷線
                if (host != null &&
                    host.online == false)
                {
                    //移除舊房主
                    RemovePlayer(oldHostId);
                }
            }
        }
    }

    /// <summary>
    /// 判斷玩家在線狀態
    /// </summary>
    private void JudgePlayersOnline()
    {
        if (gameRoomData.hostId == DataManager.UserId)
        {
            foreach (var player in gameRoomData.playerDataDic.Values)
            {
                if (!player.userId.StartsWith(FirebaseManager.ROBOT_ID) &&
                    player.online == false)
                {
                    RemovePlayer(player.userId);
                }
            }
        }
    }

    #endregion

    #region 遊戲流程控制

    /// <summary>
    /// 開始遊戲流程
    /// </summary>
    /// <param name="gameFlow">遊戲流程</param>
    /// 
    double mainPotWinChips = 0;
    double playersWhoLeftBet = 0;
    public List<RoomFee> winnersRoomFee = new();

    public IEnumerator IStartGameFlow(GameFlowEnum gameFlow)
    {
        Debug.Log($"{nameof(IStartGameFlow)} :: {gameFlow} :: hostId :: {gameRoomData.hostId} :: {DataManager.UserId}");
        if (preUpdateGameFlow == gameFlow ||
            gameRoomData.hostId != DataManager.UserId)
        {
            Debug.Log("Game Break Host Not found");
            yield break;
        }
        Debug.Log(nameof(IStartGameFlow) + " Setting New Game Flow");
        preUpdateGameFlow = gameFlow;
        bool isGameStarting = gameFlow == GameFlowEnum.Licensing || gameFlow == GameFlowEnum.SetBlind;
        //重製房間資料
        var roomData = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_CALL_VALUE,isGameStarting? gameRoomData.smallBlind * 2:0},                //當前跟注值
            { FirebaseManager.ACTIONP_PLAYER_COUNT, 0},                                     //當前流程行動玩家次數
            { FirebaseManager.ACTION_CD, -1},                                               //行動倒數時間
        };
        UpdateGameRoomData(roomData);

        //重製所有玩家
        foreach (var item in gameRoomData.playerDataDic.Values)
        {
            var playerData = new Dictionary<string, object>()
            {
                { FirebaseManager.CURR_ALL_BET_CHIPS, 0},             //該回合總下注籌碼
                { FirebaseManager.IS_BET, false},                     //該流程是否已下注
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{item.userId}",
                                                            playerData);
        }

        //重製下注行為
        preBetActionerId = "";
        var betActionData = new Dictionary<string, object>()
        {
            { FirebaseManager.BET_ACTIONER_ID, ""},                 //行動玩家ID
            { FirebaseManager.BET_ACTION, 0},                       //(BetActingEnum)下注行為
            { FirebaseManager.BET_ACTION_VALUE, 0},                 //下注籌碼值
            { FirebaseManager.UPDATE_CARRY_CHIPS, 0},               //更新後的攜帶籌碼
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.BET_ACTION_DATA}",
                                                        betActionData);

        var data = new Dictionary<string, object>();
        var playingPlayers = new List<GameRoomPlayerData>();
        var potWinners = new List<GameRoomPlayerData>();
        switch (gameFlow)
        {
            //發牌
            case GameFlowEnum.Licensing:

                //遊戲資料初始化
                GameDataInit();

                //積分房只剩下玩家1名
                if (RoomType == TableTypeEnum.IntegralTable &&
                    gameRoomData.playingPlayersIdList != null &&
                    gameRoomData.playingPlayersIdList.Count() == 1)
                {
                    //顯示積分結果
                    gameView.SetBattleResult(true);
                    yield break;
                }

                yield return new WaitForSeconds(1);

                //更新遊戲流程
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CURR_GAME_FLOW, (int)GameFlowEnum.Licensing},         //當前遊戲流程
                };
                UpdateGameRoomData(data);

                break;

            //大小盲
            case GameFlowEnum.SetBlind:

                //更新遊戲流程
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CURR_GAME_FLOW, (int)GameFlowEnum.SetBlind},           //當前遊戲流程
                };
                UpdateGameRoomData(data);
                break;

            //翻牌
            case GameFlowEnum.Flop:

                //更新公共牌翻牌流程
                UpdateCommunityFlopSeason(GameFlowEnum.Flop,
                                          3);
                break;

            //轉牌
            case GameFlowEnum.Turn:

                //更新公共牌翻牌流程
                UpdateCommunityFlopSeason(GameFlowEnum.Turn,
                                          4);
                break;

            //河牌
            case GameFlowEnum.River:

                //更新公共牌翻牌流程
                UpdateCommunityFlopSeason(GameFlowEnum.River,
                                          5);
                break;

            //遊戲結果_底池
            case GameFlowEnum.PotResult:
                winnersRoomFee.Clear();
                // Get the players still in the game and order by their total bet chips
                playingPlayers = GetPlayingPlayer().OrderBy(x => x.allBetChips).ToList();

                Debug.Log("GameControl :: " + nameof(IStartGameFlow) + " Before Judging player " + playingPlayers.Count);
                if (playingPlayers.Count == 0)
                    break;

                // Judge the winners based on the remaining players
                potWinners = JudgeWinner(playingPlayers).OrderBy(x => x.allBetChips).ToList();

                // Calculate the minimum amount in the pot and the total winning chips
                double potMin = playingPlayers[0].allBetChips;

                playersWhoLeftBet = gameRoomData.playersWhoLeft == null ? 0 : gameRoomData.playersWhoLeft.Sum(p => p.Value.allBetChips);
                Debug.Log("GameControl :: PlayersWhoLeftBet : " + playersWhoLeftBet);
                mainPotWinChips = potMin * playingPlayers.Count();

                // Update the main pot and check if there are remaining side chips
                double remainingChips = gameRoomData.potChips - (mainPotWinChips + playersWhoLeftBet);

                // Check if there is a side pot, but only if there are remaining chips from bets not fully covered
                bool IsHaveSide = remainingChips > 0;

                double winnerShare = (mainPotWinChips / potWinners.Count) + playersWhoLeftBet;
                // Update the chips for the winning players
                List<string> potWinnerIdList = new List<string>();
                foreach (var potWinner in potWinners)
                {
                    potWinner.winType = WinnerEnum.MAIN;
                    potWinnerIdList.Add(potWinner.userId);
                    RoomFee roomFeeObj = new RoomFee()
                    {
                        nickname = potWinner.nickname,
                        userId = potWinner.userId,
                        winType = WinnerEnum.MAIN,
                        potWinAmount = winnerShare,
                        allBetChips = potWinner.allBetChips,
                        carryChips = potWinner.carryChips,
                    };
                    winnersRoomFee.Add(roomFeeObj);
                }
                if (!IsHaveSide)
                {
                    CalculateRoomFee();
                }

                // Update the game room data (e.g., community cards)
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CURR_COMMUNITY_POKER, gameRoomData.communityPoker.Take(5)},      //當前顯示公共牌
                };
                UpdateGameRoomData(data);

                // Update the pot winner details
                List<string> potWinnersId = potWinners.Select(x => x.userId).ToList();
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.POT_WIN_CHIPS, winnerShare},                      //底池獲得籌碼
                    { FirebaseManager.POT_WINNERS_ID, potWinnerIdList},                 //底池獲得贏家ID
                    { FirebaseManager.IS_HAVE_SIDE, IsHaveSide},                        //是否有邊池
                };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.POT_WIN_DATA}",
                                                                data,
                                                                gameObject.name,
                                                                nameof(PotWinDataCallback));
                break;

            case GameFlowEnum.SideResult:
                Debug.Log("GameControl :: SideResult : === Starting SideResult Flow ===");

                // Update community cards in Firebase
                data = new Dictionary<string, object>
                {
                    { FirebaseManager.CURR_COMMUNITY_POKER, gameRoomData.communityPoker.Take(5).ToList() }, // Ensure ToList to avoid deferred execution
                };
                UpdateGameRoomData(data);
                Debug.Log($"GameControl :: SideResult : Updated community cards: {string.Join(", ", gameRoomData.communityPoker.Take(5))}");

                // Get players still in the game, ordered by their total bet chips
                var newPlayingPlayers = GetPlayingPlayer().OrderBy(x => x.allBetChips).ToList(); // Ensure players are sorted by bet chips
                Debug.Log($"GameControl :: SideResult : Playing players ordered by bet chips: {string.Join(", ", newPlayingPlayers.Select(p => $"{p.nickname}: {p.allBetChips}"))}");
                potWinners = JudgeWinner(newPlayingPlayers).OrderBy(x => x.allBetChips).ToList();
                Debug.Log($"GameControl :: SideResult : PotWinner players ordered by bet chips: {string.Join(", ", potWinners.Select(p => $"{p.nickname}: {p.allBetChips}"))}");
                // Calculate total side pot
                playersWhoLeftBet = gameRoomData.playersWhoLeft == null ? 0 : gameRoomData.playersWhoLeft.Sum(p => p.Value.allBetChips);
                double totalSidePot = gameRoomData.potChips - (mainPotWinChips + playersWhoLeftBet);
                Debug.Log($"GameControl :: SideResult : Total side pot: {totalSidePot}");

                // Calculate individual side pots
                double maxEligibleCriteria = potWinners[0].allBetChips;
                double potMinBet = newPlayingPlayers[0].allBetChips;
                List<GameRoomPlayerData> eligiblePlayers = new();
                Dictionary<List<GameRoomPlayerData>, double> sideWinners1 = new();
                sideWinnersIds = new List<string>();
                List<double> sidePots = new();

                foreach (var player in newPlayingPlayers)
                {
                    if (player.allBetChips >= maxEligibleCriteria && player.allBetChips > newPlayingPlayers[0].allBetChips)
                    {
                        eligiblePlayers.Add(player);
                    }
                }
                Debug.Log($"GameControl :: SideResult : Eligible Player : {eligiblePlayers.Count} :" + string.Join(" , ", eligiblePlayers.Select(p => p.nickname)));
                if (eligiblePlayers.Count > 0 && eligiblePlayers != null)
                {
                    sidePots = CalculatePots(newPlayingPlayers);

                    foreach (var sidePot in sidePots.Skip(1)) // Skip the main pot
                    {
                        if (!eligiblePlayers.Any())
                        {
                            Debug.Log("GameControl :: SideResult : No eligible players remaining for side pots.");
                            break; // Stop if no eligible players remain
                        }

                        double minBet = eligiblePlayers.Min(p => p.allBetChips);
                        Debug.Log($"GameControl :: SideResult : Minimum bet among eligible players: {minBet}");

                        var sideWinner = JudgeWinner(eligiblePlayers);  // Get winner(s) for the side pot
                        Debug.Log($"GameControl :: SideResult : Judged winners for side pot: {string.Join(", ", sideWinner.Select(p => p.nickname))}");

                        sideWinners1.Clear();
                        sideWinners1.Add(sideWinner, sidePot);
                        Debug.Log($"GameControl :: SideResult : Side pot amount: {sidePot}, distributing to winners.");

                        DistributeSidePot(sideWinners1);

                        // Remove players whose chips are less than the minimum bet
                        eligiblePlayers = eligiblePlayers.Where(p => (p.allBetChips - minBet) > 0)?.ToList();
                        Debug.Log($"GameControl :: SideResult : Remaining eligible players: {string.Join(", ", eligiblePlayers.Select(p => p.nickname))}");
                    }
                }
                else
                {
                    Debug.Log("GameControl :: SideResult : No eligible Players");
                    sideWinners1.Clear();
                    sideWinners1.Add(potWinners, totalSidePot);
                    DistributeSidePot(sideWinners1);
                }

                // Update Firebase with side pot data
                var sidePotData = new Dictionary<string, object>
                {
                    { FirebaseManager.SIDE_WIN_CHIPS, totalSidePot },
                    { FirebaseManager.SIDE_WINNERS_ID, sideWinnersIds },
                };

                CalculateRoomFee();

                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.SIDE_WIN_DATA}",
                                                                sidePotData,
                                                                gameObject.name,
                                                                nameof(SideWinDataCallback));
                Debug.Log($"GameControl :: SideResult : Side pot data updated in Firebase. Total side pot: {totalSidePot}, winners: {string.Join(", ", sideWinnersIds)}");
                break;


            //剩餘1名玩家結果
            case GameFlowEnum.OnePlayerLeftResult:

                potWinners = GetPlayingPlayer();

                if (potWinners.Count() > 1)
                {
                    yield break;
                }

                GameRoomPlayerData winner = potWinners[0];

                mainPotWinChips = gameRoomData.potChips;

                // //更新玩家籌碼
                // newCarryChips = winner.carryChips + mainPotWinChips;
                // data = new Dictionary<string, object>()
                // {
                //     { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},   //攜帶籌碼
                // };
                // UpdataPlayerData(winner.userId,
                //                  data);
                Debug.Log("GameControl :: OnePlayerLeftResult : " + winner.nickname);

                playersWhoLeftBet = gameRoomData.playersWhoLeft == null ? 0 : gameRoomData.playersWhoLeft.Sum(p => p.Value.allBetChips);
                remainingChips = gameRoomData.potChips - (mainPotWinChips + playersWhoLeftBet);

                IsHaveSide = remainingChips > 0;

                winnerShare = (mainPotWinChips / potWinners.Count) + playersWhoLeftBet;

                potWinnerIdList = new List<string>();
                foreach (var potWinner in potWinners)
                {
                    potWinner.winType = WinnerEnum.MAIN;
                    potWinnerIdList.Add(potWinner.userId);
                    RoomFee roomFeeObj = new RoomFee()
                    {
                        nickname = potWinner.nickname,
                        userId = potWinner.userId,
                        winType = WinnerEnum.MAIN,
                        potWinAmount = winnerShare,
                        sidePotAmount = 0,
                        allBetChips = potWinner.allBetChips,
                        carryChips = potWinner.carryChips,
                    };
                    winnersRoomFee.Add(roomFeeObj);
                }

                CalculateRoomFee();

                //更新底池獲勝資料
                potWinnerIdList = new List<string>();
                potWinnerIdList.Add(winner.userId);
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.POT_WIN_CHIPS, winnerShare},                    //底池獲得籌碼
                    { FirebaseManager.POT_WINNERS_ID, potWinnerIdList},               //底池獲得贏家ID
                    { FirebaseManager.IS_HAVE_SIDE, false},                           //是否有邊池
                };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.POT_WIN_DATA}",
                                                                data,
                                                                gameObject.name,
                                                                nameof(PotWinDataCallback));
                break;

        }
    }

    List<string> sideWinnersIds;
    public static List<double> CalculatePots(List<GameRoomPlayerData> players)
    {
        Debug.Log("GameControl :: CalculatePots : Start");

        var sortedPlayers = players.OrderBy(p => p.allBetChips).ToList();
        Debug.Log($"GameControl :: CalculatePots : SortedPlayers = {string.Join(", ", sortedPlayers.Select(p => p.nickname + ":" + p.allBetChips))}");

        List<double> pots = new List<double>();
        double previousBet = 0;

        foreach (var player in sortedPlayers)
        {
            double betDifference = player.allBetChips - previousBet;
            Debug.Log($"GameControl :: CalculatePots : Player {player.nickname}, BetDifference = {betDifference}");

            if (betDifference > 0)
            {
                int activePlayers = players.Count(p => p.allBetChips >= player.allBetChips);
                double pot = betDifference * activePlayers;
                pots.Add(pot);
                Debug.Log($"GameControl :: CalculatePots : Pot = {pot}, ActivePlayers = {activePlayers}");

                previousBet = player.allBetChips;
            }
        }

        Debug.Log($"GameControl :: CalculatePots : Pots = {string.Join(", ", pots)}");
        return pots;
    }

    void DistributeSidePot(Dictionary<List<GameRoomPlayerData>, double> sidePotData)
    {
        Debug.Log("GameControl :: DistributeSidePot : Start");

        foreach (var entry in sidePotData)
        {
            var players = entry.Key;
            double sidePot = entry.Value;

            Debug.Log($"GameControl :: DistributeSidePot : SidePot = {sidePot}, Players = {string.Join(", ", players?.Select(p => p.nickname) ?? new List<string>())}");

            if (players == null || players.Count == 0)
            {
                Debug.Log("GameControl :: DistributeSidePot : Skipping empty or null players list.");
                continue;
            }

            double sideWinChips = sidePot / players.Count;
            Debug.Log($"GameControl :: DistributeSidePot : SideWinChips = {sideWinChips}");

            foreach (var player in players)
            {
                var winner = winnersRoomFee.FirstOrDefault(p => p.userId == player.userId);
                if (winner != null)
                {
                    winner.sidePotAmount += sideWinChips;
                    winner.winType = WinnerEnum.BOTH;
                }
                else
                {
                    RoomFee roomFeeObj = new RoomFee()
                    {
                        nickname = player.nickname,
                        userId = player.userId,
                        winType = WinnerEnum.SIDE,
                        potWinAmount = 0,
                        sidePotAmount = sideWinChips,
                        allBetChips = player.allBetChips,
                        carryChips = player.carryChips,
                    };
                    winnersRoomFee.Add(roomFeeObj);
                }
                Debug.Log($"GameControl :: DistributeSidePot : Updated player data for {player.nickname}");
                sideWinnersIds.Add(player.userId);
            }
        }

        Debug.Log("GameControl :: DistributeSidePot : End");
    }

    public void CalculateRoomFee()
    {
        foreach (var winner in winnersRoomFee)
        {
            double winAmount = winner.potWinAmount;
            double sidePotAmount = winner.sidePotAmount;
            double carryChips = winner.carryChips;
            double roomRate = DataManager.Rebate / 100;

            // Calculate winAmount based on winType
            if (winner.winType == WinnerEnum.BOTH || winner.winType == WinnerEnum.SIDE)
            {
                winAmount += sidePotAmount;
            }

            double roomFee = winAmount * roomRate;
            double finalWinnings = winAmount - roomFee;
            double profit = winAmount - winner.allBetChips;

            if (profit <= 0)
            {
                Debug.Log("GameControl :: CalculateRoomFee : Profit is <= 0 " + winner.nickname);
                roomFee = 0;
            }

            // Determine carry chips based on profit
            double newCarryChips = profit > 0 ? carryChips + Math.Floor(finalWinnings) : carryChips + winAmount;

            // Prepare data to update based on winType
            var newData = new Dictionary<string, object>
            {
                { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips) },
                { FirebaseManager.ROOM_FEE, Math.Round(roomFee,2) }
            };
            Debug.Log("GameControl :: CalculateRoomFee : Player : " + winner.nickname + " with room fee: " + Math.Round(roomFee, 2));
            // Add profit type-specific data
            switch (winner.winType)
            {
                case WinnerEnum.BOTH:
                    newData[FirebaseManager.MAIN_PROFIT] = Math.Floor(profit);
                    break;
                case WinnerEnum.MAIN:
                    newData[FirebaseManager.MAIN_PROFIT] = Math.Floor(profit);
                    break;
                case WinnerEnum.SIDE:
                    newData[FirebaseManager.SIDE_PROFIT] = Math.Floor(profit);
                    break;
            }

            UpdataPlayerData(winner.userId, newData);

            if (winner.userId == DataManager.UserId)
            {
                NoodleApi.PostTableChipsTransaction(winner.userId, DataManager.RoundId.ToString(), Math.Round(roomFee, 2), 21, ChipTransactionType.TableFee, (x) =>
                    {
                        Debug.Log("TableFee ChipsTransaction Success");
                    },
                   (error) =>
                   {
                       Debug.LogError($"TableFee ChipsTransaction Failed Error: {error}");
                   });
            }
            // GameRoomPlayerData playerData = GetPlayerData(winner.userId);
            // if (playerData != null)
            // {
            //     playerData.roomFee = roomFee;
            // }
            // Update player data
        }
        CalculateValidBets();
    }
    void CalculateValidBets()
    {
        List<double> allBetChipsList = gameRoomData.playerDataDic
            .Where(x => x.Value != null)
            .Select(x => x.Value.allBetChips)
            .OrderByDescending(chips => chips)
            .ToList();

        foreach (var player in gameRoomData.playerDataDic)
        {
            GameRoomPlayerData playerData = GetPlayerData(player.Value.userId);
            if (playerData != null)
            {
                double effectiveBet = 0;

                // Calculate effective bet by summing only up to the player's total bet amount
                foreach (var level in allBetChipsList)
                {
                    if (playerData.allBetChips > level)
                    {
                        effectiveBet += level;
                    }
                    else
                    {
                        effectiveBet += playerData.allBetChips;
                        break;
                    }


                }

                var newData = new Dictionary<string, object>
                    {
                        { FirebaseManager.VALID_BET, Math.Floor(effectiveBet) },
                    };
                UpdataPlayerData(playerData.userId, newData);

                // Assign effective bet after calculation completes
                //playerData.playerValidBetAmount = effectiveBet;

                Debug.Log($"Player ID: {player.Value.userId}, All Bet Chips: {playerData.allBetChips}, Effective Bet: {effectiveBet}");
            }
            else
            {
                Debug.Log($"Player ID: {player.Value.userId} not found in gameRoomData.");
            }
        }
    }

    // public void ReCalculateProfitForPlayersWhoHaveBothSideAndMainPot(Dictionary<string, double> _mainPotWinnersRoomFee, Dictionary<string, double> _sidePotWinnersRoomFee)
    // {
    //     playersWithTheirRoomFee.Clear();
    //     if (_mainPotWinnersRoomFee != null && _sidePotWinnersRoomFee != null)
    //     {
    //         foreach (var mainPotEntry in _mainPotWinnersRoomFee)
    //         {
    //             string player = mainPotEntry.Key;

    //             // Check if the player also exists in the side pot winners dictionary
    //             if (_sidePotWinnersRoomFee.ContainsKey(player))
    //             {
    //                 // If the player exists in both, add their side pot fee to their main pot fee
    //                 double totalProfit = mainPotEntry.Value + _sidePotWinnersRoomFee[player];
    //                 var playerWithBoth = GetPlayerData(player);

    //                 roomFee = totalProfit * (DataManager.Rebate / 100);

    //                 double finalWinnings = totalProfit - roomFee;

    //                 // You can update the main pot fee or store this in another dictionary
    //                 // For example, if you want to update the mainPotWinnersRoomFee:
    //                 var data = new Dictionary<string, object>()
    //                 {
    //                     { FirebaseManager.CARRY_CHIPS, Math.Floor(playerWithBoth.carryChips+finalWinnings) },  // Update carry chips
    //                     {FirebaseManager.MAIN_PROFIT,Math.Floor(totalProfit)},  // Update carry chips
    //                 };
    //                 //_mainPotWinnersRoomFee[player] = roomFee;
    //                 playersWithTheirRoomFee.Add(player, roomFee);
    //                 _sidePotWinnersRoomFee?.Remove(player);
    //                 _mainPotWinnersRoomFee?.Remove(player);
    //                 // Optionally, you can also remove the player from the sidePotWinnersRoomFee if no longer needed
    //             }
    //         }
    //     }
    // }




    /// <summary>
    /// 底池贏家資料回傳
    /// </summary>
    /// <param name="isSuccess">是否資料更新成功</param>
    public void PotWinDataCallback(string isSuccess)
    {
        //更新遊戲流程
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_GAME_FLOW, (int)GameFlowEnum.PotResult},           //當前遊戲流程
        };
        UpdateGameRoomData(data);
    }

    /// <summary>
    /// 邊池贏家資料回傳
    /// </summary>
    /// <param name="isSuccess">是否資料更新成功</param>
    public void SideWinDataCallback(string isSuccess)
    {
        //更新遊戲流程
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_GAME_FLOW, (int)GameFlowEnum.SideResult},           //當前遊戲流程
        };
        UpdateGameRoomData(data);
    }

    /// <summary>
    /// 更新公共牌翻牌流程
    /// </summary>
    /// <param name="inGameFlow">進入流程</param>
    /// <param name="takeCommunityPoker">顯示的公共牌數量</param>
    private void UpdateCommunityFlopSeason(GameFlowEnum inGameFlow, int takeCommunityPoker)
    {
        //首位行動玩家=小盲座位
        int nextSeat = (gameRoomData.buttonSeat + 1) % DataManager.MaxPlayerCount;
        List<GameRoomPlayerData> players = GetCanActionPlayer().OrderBy(x => x.gameSeat)
                                                               .ToList();
        string nextPlayerId = players.Where(x => x.gameSeat == nextSeat)
                                     .FirstOrDefault()?
                                     .userId ?? "";

        //小盲座位玩家不存在下個座位玩家開始
        int index = 2;
        while (string.IsNullOrEmpty(nextPlayerId))
        {
            nextSeat = (gameRoomData.buttonSeat + index) % DataManager.MaxPlayerCount;
            nextPlayerId = players.Where(x => x.gameSeat == nextSeat)
                                  .FirstOrDefault()?
                                  .userId ?? "";

            index++;
        }

        //更新遊戲流程
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_GAME_FLOW, (int)inGameFlow},                                                 //當前遊戲流程
            { FirebaseManager.CURR_COMMUNITY_POKER, gameRoomData.communityPoker.Take(takeCommunityPoker)},      //當前顯示公共牌
            { FirebaseManager.CURR_ACTIONER_ID, nextPlayerId},                                                  //當前行動玩家Id
            { FirebaseManager.CURR_ACTIONER_SEAT, nextSeat},                                                    //當前行動玩家座位
        };
        UpdateGameRoomData(data);
    }

    /// <summary>
    /// 監聽遊戲房間資料回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void GameRoomDataCallback(string jsonData)
    {
        //同步資料
        var data = FirebaseManager.Instance.OnFirebaseDataRead<GameRoomData>(jsonData);
        gameRoomData = data;

        //遊戲介面更新房間資料
        gameView.UpdateGameRoomData(gameRoomData);

        //判斷房主
        //JudgeHost();

        //積分房未開始牌局只剩1名玩家
        if (RoomType == TableTypeEnum.IntegralTable &&
            gameRoomData.playerDataDic.Count() == 1 &&
            gameRoomData.currGameFlow < (int)GameFlowEnum.Licensing)
        {
            gameView.SetBattleResult(true);
            return;
        }

        //聊天訊息
        ChatMessage();

        //遊戲流程回傳
        LocalGameFlowBehavior();

        //下注行為演出
        ShowBetAction();

        //行動倒數
        CountDown();

        if (gameRoomData.playerDataDic != null &&
            gameRoomData.playingPlayersIdList != null)
        {
            //人數有變化更新房間玩家訊息
            if (gameRoomData.playerDataDic.Count() != prePlayerCount)
            {
                prePlayerCount = gameRoomData.playerDataDic.Count();
                gameView.UpdateGameRoomInfo(gameRoomData);

                if (gameRoomData.playingPlayersIdList.Count() == 1 &&
                    preUpdateGameFlow >= GameFlowEnum.SetBlind)
                {
                    //剩下一名玩家在進行遊戲
                    StartCoroutine(IJudgeNextSeason());
                }
                else
                {
                    //剩下一名玩家在等待遊戲
                    if (!hostUpdated)
                        JudgePauseToStar();
                }
            }
            else
            {
                //剩下一名玩家在等待遊戲
                if (!hostUpdated)
                    JudgePauseToStar();
            }
        }

        //棄牌後顯示手牌
        if ((GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.PotResult ||
            (GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.SideResult)
        {
            GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                                      .FirstOrDefault()
                                                                      .Value;
            if (localHand != null &&
                playerData.handPoker != null &&
                playerData.handPoker.SequenceEqual(localHand))
            {
                gameView.ShowFoldPoker();
            }
        }
    }

    /// <summary>
    /// 剩下一名玩家在等待遊戲
    /// </summary>
    private void JudgePauseToStar()
    {
        if (gameRoomData != null &&
            gameRoomData.playingPlayersIdList != null)
        {
            if (gameRoomData.hostId == DataManager.UserId)
            {
                if (gameRoomData.playingPlayersIdList.Count == 1 &&
                    preUpdateGameFlow != (GameFlowEnum)gameRoomData.currGameFlow &&
                    preUpdateGameFlow <= GameFlowEnum.Licensing &&
                    RoomType != TableTypeEnum.IntegralTable)
                {
                    Debug.Log("IStartGameFlow :: JudgePauseToStart Licensing");
                    StartCoroutine(IStartGameFlow(GameFlowEnum.Licensing));
                }
            }
        }
    }

    /// <summary>
    /// 遊戲流程回傳
    /// </summary>
    private void LocalGameFlowBehavior()
    {
        if (preLocalGameFlow == (GameFlowEnum)gameRoomData.currGameFlow)
        {
            return;
        }

        //本地遊戲流程行為
        StartCoroutine(ILocalGameFlowBehavior());
    }
    /// <summary>
    /// 遊戲流程回傳
    /// </summary>
    public bool isLicense = false;
    public bool hostUpdated = false;
    private IEnumerator ILocalGameFlowBehavior()
    {
        // Store the previous game flow
        preLocalGameFlow = (GameFlowEnum)gameRoomData.currGameFlow;

        Debug.Log($"{nameof(ILocalGameFlowBehavior)} Flow :: {preLocalGameFlow}");

        // Start the game stage in the view
        yield return gameView.IGameStage(gameRoomData, SmallBlind);

        // Create a dictionary for updating data
        var data = new Dictionary<string, object>();

        // Handle different game flow cases
        switch ((GameFlowEnum)gameRoomData.currGameFlow)
        {
            // Licensing (dealing cards)
            case GameFlowEnum.Licensing:
                Debug.Log(nameof(ILocalGameFlowBehavior) + " Licensing");
                gameView.GameStartInit();

                // Get local player data
                GameRoomPlayerData playerData = GetLocalPlayer();

                Debug.Log(nameof(ILocalGameFlowBehavior) + " Licensing");
                // Check if the player has insufficient chips
                if (playerData.carryChips < leastChips && PreBuyChipsValue < leastChips)
                {
                    gameView.OnInsufficientChips();
                    playerData.gameState = (int)PlayerStateEnum.Waiting;
                    data = new Dictionary<string, object>()
                {
                    { FirebaseManager.GAME_STATE, (int)PlayerStateEnum.Waiting },
                };
                    UpdataPlayerData(playerData.userId, data);
                }


                Debug.Log($"{nameof(ILocalGameFlowBehavior)} :: Playing Players : {gameRoomData.playingPlayersIdList.Count()}");
                // Check if there are enough players in the game
                if (gameRoomData.playingPlayersIdList != null && gameRoomData.playingPlayersIdList.Count < 2)
                {
                    if (gameRoomData.hostId == DataManager.UserId)
                    {
                        foreach (var item in gameRoomData.playerDataDic.Values)
                        {
                            item.gameState = (int)PlayerStateEnum.Waiting;
                            data = new Dictionary<string, object>()
                        {
                            { FirebaseManager.GAME_STATE, (int)PlayerStateEnum.Waiting },
                        };
                            UpdataPlayerData(item.userId, data);
                        }
                    }
                    preUpdateGameFlow = GameFlowEnum.None;
                    preLocalGameFlow = GameFlowEnum.None;
                    yield break;
                }

                // Game test check
                if (DataManager.IsOpenGameTest && !gameView.IsStartGameTest)
                {
                    gameView.UpdateGameRoomInfo(gameRoomData);
                    gameView.IsOpenGameTestObj = true;
                    yield break;
                }

                if (DataManager.IsOpenGameTest)
                {
                    gameView.IsStartGameTest = false;
                }

                gameView.UpdateGameRoomInfo(gameRoomData);
                gameView.OnLicensingFlow(gameRoomData);

                yield return new WaitForSeconds(1);

                // Host will start the next game flow
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    Debug.Log($"{nameof(ILocalGameFlowBehavior)} :: Starting New Game : {gameRoomData.playingPlayersIdList}");

                    yield return new WaitForSeconds(1);
                    yield return IStartGameFlow(GameFlowEnum.SetBlind);
                }

                // Store local hand data for the local player
                localHand = playerData.handPoker;

                break;

            // Set the blinds
            case GameFlowEnum.SetBlind:

                gameView.OnBlindFlow(gameRoomData);
                UpdateGameRoom();

                yield return new WaitForSeconds(1);
                isCloseAllCdInfo = false;

                // Host sets the next player action
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    UpdateNextPlayer();
                }
                break;

            // Flop
            case GameFlowEnum.Flop:
                yield return IStartCommunityFlopSeason();
                break;

            // Turn
            case GameFlowEnum.Turn:
                yield return IStartCommunityFlopSeason();
                break;

            // River
            case GameFlowEnum.River:
                yield return IStartCommunityFlopSeason();
                break;

            // Pot result (main pot)
            case GameFlowEnum.PotResult:

                isCloseAllCdInfo = true;
                yield return gameView.IPotResult(gameRoomData);

                // Handle chip purchase if necessary
                if (PreBuyChipsValue > 0 && !gameRoomData.potWinData.isHaveSide)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);

                // Check if any player has insufficient chips
                bool isPotIntegralResult = !gameRoomData.potWinData.isHaveSide &&
                    gameRoomData.playerDataDic.Any(x => x.Value.carryChips < leastChips);

                // Display result for an integral table
                if (RoomType == TableTypeEnum.IntegralTable && isPotIntegralResult)
                {
                    gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                }

                // Host will handle game flow continuation
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    if (gameRoomData.potWinData.isHaveSide)
                    {
                        yield return IStartGameFlow(GameFlowEnum.SideResult);
                        yield break;
                    }
                    else
                    {
                        data = new Dictionary<string, object>()
                    {
                        { FirebaseManager.GAME_END_TIME, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                    };
                        UpdateGameRoomData(data);

                        if (RoomType == TableTypeEnum.IntegralTable && isPotIntegralResult)
                        {
                            yield break;
                        }

                        isLicense = false;
                        yield return IStartGameFlow(GameFlowEnum.Licensing);
                    }
                }

                break;

            // Side pot result
            case GameFlowEnum.SideResult:

                yield return gameView.SideResult(gameRoomData);

                if (PreBuyChipsValue > 0)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);

                bool isSideIntegralResult = gameRoomData.playerDataDic.Any(x => x.Value.carryChips < leastChips);

                if (RoomType == TableTypeEnum.IntegralTable && isSideIntegralResult)
                {
                    gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                }

                if (gameRoomData.hostId == DataManager.UserId)
                {
                    data = new Dictionary<string, object>()
                {
                    { FirebaseManager.GAME_END_TIME, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                };
                    UpdateGameRoomData(data);

                    if (RoomType == TableTypeEnum.IntegralTable && isSideIntegralResult)
                    {
                        yield break;
                    }

                    isLicense = false;
                    yield return IStartGameFlow(GameFlowEnum.Licensing);
                }

                break;

            // One player left result
            case GameFlowEnum.OnePlayerLeftResult:

                yield return gameView.IPotResult(gameRoomData);

                if (PreBuyChipsValue > 0)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);

                if (gameRoomData.hostId == DataManager.UserId)
                {
                    data = new Dictionary<string, object>()
                {
                    { FirebaseManager.GAME_END_TIME, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") },
                };
                    UpdateGameRoomData(data);

                    if (RoomType == TableTypeEnum.IntegralTable && gameRoomData.playingPlayersIdList.Count == 1)
                    {
                        gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                        yield break;
                    }

                    isLicense = false;
                    yield return IStartGameFlow(GameFlowEnum.Licensing);
                }

                break;
        }
    }

    /// <summary>
    /// 開始公共牌翻牌流程
    /// </summary>
    /// <returns></returns>
    private IEnumerator IStartCommunityFlopSeason()
    {
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);
        isCloseAllCdInfo = true;

        //翻開公共牌
        yield return gameView.IFlopCommunityPoker(gameRoomData.currCommunityPoker);

        yield return new WaitForSeconds(1);
        isCloseAllCdInfo = false;

        //房主執行
        if (gameRoomData.hostId == DataManager.UserId)
        {
            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.ACTION_CD, DataManager.StartCountDownTime},           //行動倒數時間
            };
            UpdateGameRoomData(data);
        }
    }

    /// <summary>
    /// 行動倒數
    /// </summary>
    public void CountDown()
    {
        if ((preCD < DataManager.StartCountDownTime && preCD == gameRoomData.actionCD) ||
            gameRoomData.actionCD < 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(gameRoomData.currActionerId))
        {
            return;
        }

        preCD = gameRoomData.actionCD;

        //行動倒數
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);
        cdCoroutine = StartCoroutine(ICountdown());
    }
    /// <summary>
    /// 行動倒數
    /// </summary>
    private IEnumerator ICountdown()
    {
        if (gameRoomData.actionCD < 0 ||
            preCD != gameRoomData.actionCD)
        {
            yield break;
        }

        //找不到玩家(玩家離開/斷線)
        if (!gameRoomData.playerDataDic.ContainsKey(gameRoomData.currActionerId))
        {
            yield return new WaitForSeconds(1);
            StartCoroutine(IJudgeNextSeason());
            yield break;
        }

        //積分房玩家剩下1名
        if (RoomType == TableTypeEnum.IntegralTable &&
            gameRoomData.playerDataDic.Count() == 1)
        {
            StartCoroutine(IJudgeNextSeason());
            yield break;
        }

        GamePlayerInfo player = gameView.GetPlayer(gameRoomData.playerDataDic[gameRoomData.currActionerId].userId);
        if (gameRoomData.actionCD == DataManager.StartCountDownTime)
        {
            yield return new WaitForSeconds(1);

            if (player == null ||
                gameRoomData.actionCD < 0 ||
                preCD != gameRoomData.actionCD)
            {
                yield break;
            }

            player.InitCountDown();

            if (player.UserId == DataManager.UserId)
            {
                gameView.LocalPlayerRound(gameRoomData);
            }

            cdSound = 0;
        }

        if (player == null ||
            gameRoomData.actionCD < 0 ||
            preCD != gameRoomData.actionCD)
        {
            yield break;
        }

        player.ActionFrame = true;
        //gameRoomData.actionCD = 1;
        if (player.UserId == DataManager.UserId)
        {
            print("GameCtrl 執行倒數");
            player.CountDown(DataManager.StartCountDownTime,
                             gameRoomData.actionCD);
        }

        if (player.UserId == DataManager.UserId)
        {
            gameView.CheckActionArea(gameRoomData);
            switchRoomBtn.SetCdTimeText($"{gameRoomData.actionCD}");
        }
        else
        {
            switchRoomBtn.SetCdTimeText("");
        }

        cdSound += 1;
        if (cdSound == 7)
        {
            gameView.PlaySound("NotificationTimeBank");
            cdSound = 0;
        }

        //關閉其他玩家行動框
        foreach (var item in gameRoomData.playerDataDic.Values)
        {
            if (item.userId != player.UserId)
            {
                GamePlayerInfo other = gameView.GetPlayer(item.userId);
                if (other != null)
                {
                    other.ActionFrame = false;
                    other.InitCountDown();
                }
            }
        }

        //非本地玩家更新房間資料
        if (player.UserId != DataManager.UserId)
        {
            gameView.UpdateGameRoomInfo(gameRoomData);
        }

        yield return new WaitForSeconds(1);

        if (gameRoomData.actionCD < 0 ||
            preCD != gameRoomData.actionCD)
        {
            yield break;
        }

        //房主執行
        if (gameRoomData.hostId == DataManager.UserId)
        {
            //時間減少
            int currActionCD = gameRoomData.actionCD - 1;

            if (currActionCD < 0)
            {
                //超過時間棄牌
                string id = gameRoomData.currActionerId;
                UpdateBetAction(id, BetActingEnum.Fold, 0);
                if (player.UserId == DataManager.UserId)
                    gameView.isOnFold = true;
            }
            else
            {
                //機器人動作
                if (player.UserId.StartsWith(FirebaseManager.ROBOT_ID) &&
                    gameRoomData.actionCD == DataManager.RobotActionTime)
                {
                    RobotControl.RobotBet(gameRoomData);

                    yield break;
                }

                //更新倒數
                var data = new Dictionary<string, object>()
                {
                    { FirebaseManager.ACTION_CD, gameRoomData.actionCD - 1},              //行動倒數時間
                };
                UpdateGameRoomData(data);
            }
        }
    }

    /// <summary>
    /// 下注行為演出
    /// </summary>
    public void ShowBetAction()
    {
        gameView.UpdateActionBtns();
        if (gameRoomData.betActionDataDic == null ||
            string.IsNullOrEmpty(gameRoomData.betActionDataDic.betActionerId))
        {
            return;
        }

        if (string.IsNullOrEmpty(gameRoomData.betActionDataDic.betActionerId) ||
            preBetActionerId == gameRoomData.betActionDataDic.betActionerId)
        {
            return;
        }
        preBetActionerId = gameRoomData.betActionDataDic.betActionerId;

        if (string.IsNullOrEmpty(preBetActionerId))
        {
            return;
        }

        switchRoomBtn.SetCdTimeText("");
        gameView.GetPlayerAction(gameRoomData);
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);

        StartCoroutine(IJudgeNextSeason());
    }

    /// <summary>
    /// 判斷是否進入下個流程
    /// </summary>
    /// <returns></returns>
    private IEnumerator IJudgeNextSeason()
    {
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);

        //房主執行
        if (gameRoomData.hostId == DataManager.UserId)
        {
            List<GameRoomPlayerData> canActionPlayers = GetCanActionPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> allInPlayers = GetAllInPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> foldPlayers = GetFoldPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> playingPlayers = GetPlayingPlayer().OrderBy(x => x.currAllBetChips).ToList();

            yield return new WaitForSeconds(1);

            //所有玩家已下注
            bool isAllBet = true;
            if (canActionPlayers.Count() > 0)
            {
                isAllBet = canActionPlayers.All(x => x.isBet == true);
            }

            //所有玩家下注籌碼一致
            bool isAllPlayerBetValueEqual = true;
            if (playingPlayers.Count() > 0 &&
                canActionPlayers.Count() > 0)
            {
                isAllPlayerBetValueEqual = playingPlayers.All(x => x.currAllBetChips == canActionPlayers[0].currAllBetChips);
            }

            //可下注玩家籌碼一致
            bool isBetValueEqual = true;
            if (playingPlayers.Count() > 0 &&
                canActionPlayers.Count() > 0)
            {
                isBetValueEqual = canActionPlayers.All(x => x.currAllBetChips == canActionPlayers[0].currAllBetChips);
            }

            //剩下一名玩家可行動，其他玩家棄牌/離開
            if (foldPlayers.Count() == gameRoomData.playingPlayersIdList.Count() - 1)
            {
                yield return IStartGameFlow(GameFlowEnum.OnePlayerLeftResult);
                yield break;
            }

            //所有玩家AllIn/Fold
            if (canActionPlayers.Count() == 0 ||
                allInPlayers.Count() == gameRoomData.playingPlayersIdList.Count())
            {
                yield return IStartGameFlow(GameFlowEnum.PotResult);
                yield break;
            }

            //剩下一名玩家可行動，其他玩家棄牌/離開，下注值>=當前跟注值
            if (isAllBet &&
                gameRoomData.playingPlayersIdList.Count() - (allInPlayers.Count() + foldPlayers.Count()) == 1 &&
                canActionPlayers.Count() == 1 &&
                canActionPlayers[0].currAllBetChips >= gameRoomData.currCallValue)
            {
                yield return IStartGameFlow(GameFlowEnum.PotResult);
                yield break;
            }

            //所有玩家已下注 & 下注籌碼一致
            if (isAllBet == true &&
                isAllPlayerBetValueEqual == true)
            {
                int nextFlowIndex = (gameRoomData.currGameFlow + 1) % Enum.GetValues(typeof(GameFlowEnum)).Length;
                GameFlowEnum nextFlow = (GameFlowEnum)Mathf.Max(1, nextFlowIndex);
                Debug.Log("IStartGameFlow :: " + nextFlow.ToString() + " IJudge 2");
                yield return IStartGameFlow(nextFlow);
                yield break;
            }

            //所有可下注玩家已下注 & 下注籌碼一致
            if (isAllBet == true &&
                isBetValueEqual == true &&
                canActionPlayers.All(x => x.currAllBetChips >= gameRoomData.currCallValue))
            {
                int nextFlowIndex = (gameRoomData.currGameFlow + 1) % Enum.GetValues(typeof(GameFlowEnum)).Length;
                GameFlowEnum nextFlow = (GameFlowEnum)Mathf.Max(1, nextFlowIndex);
                Debug.Log("IStartGameFlow :: " + nextFlow.ToString() + " IJudge 3");
                yield return IStartGameFlow(nextFlow);
                yield break;
            }

            //設置下位行動玩家
            UpdateNextPlayer();
        }
    }

    #endregion

    #region 遊戲資料更新

    /// <summary>
    /// 刷新房間
    /// </summary>
    public void UpdateGameRoom()
    {
        //讀取房間資料
        JSBridgeManager.Instance.ReadDataFromFirebase($"{QueryRoomPath}",
                                                      gameObject.name,
                                                      nameof(UpdateGameRoomCallBack));
    }

    /// <summary>
    /// 刷新房間回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void UpdateGameRoomCallBack(string jsonData)
    {
        var data = FirebaseManager.Instance.OnFirebaseDataRead<GameRoomData>(jsonData);
        gameRoomData = data;

        //遊戲介面更新房間資料
        gameView.UpdateGameRoomData(gameRoomData);

        gameView.UpdateGameRoomInfo(gameRoomData);
    }

    /// <summary>
    /// 遊戲資料初始化
    /// </summary>
    private void GameDataInit()
    {
        var data = new Dictionary<string, object>();

        //更新玩家個人資料
        foreach (var id in gameRoomData.playerDataDic.Keys)
        {
            //機器人籌碼不足
            if (id.StartsWith(FirebaseManager.ROBOT_ID) &&
                gameRoomData.playerDataDic[id].carryChips < leastChips)
            {
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CARRY_CHIPS, SmallBlind * 100},                     //攜帶籌碼
                };
                UpdataPlayerData(id,
                                 data);

                gameRoomData.playerDataDic[id].carryChips = SmallBlind * 100;
            }

            //更新玩家個人資料
            PlayerStateEnum playerState = PlayerStateEnum.Playing;
            if (gameRoomData.playerDataDic[id].isSitOut == true ||
                gameRoomData.playerDataDic[id].carryChips < leastChips)
            {
                playerState = PlayerStateEnum.Waiting;
            }
            gameRoomData.playerDataDic[id].gameState = (int)playerState;
            data = new Dictionary<string, object>()
                {
                    { FirebaseManager.SEAT_CHARACTER, 0},                                   //(SeatCharacterEnum)座位角色(Button/SB/BB)
                    { FirebaseManager.GAME_STATE, (int)playerState},                        //(PlayerStateEnum)遊戲狀態(等待/遊戲中/棄牌/All In/保留座位離開)
                    { FirebaseManager.ALL_BET_CHIPS, 0},                                    //該局總下注籌碼
                    { FirebaseManager.SHOW_HAND_POKER, new List<int>(){ -1, -1} },          //棄牌後顯示手牌
                    { FirebaseManager.HAND_POKER, new List<int>(){ -1, -1}},
                    { FirebaseManager.ROOM_FEE, 0 },
                    { FirebaseManager.VALID_BET, 0 },
                    { FirebaseManager.PLAYER_HAND_SHAPE, -1 },
                };
            UpdataPlayerData(id,
                             data);
        }

        //遊戲中玩家
        List<string> playingPlayersId = new();
        foreach (var player in gameRoomData.playerDataDic.Values)
        {
            // if (player.isPlayerLeft)
            // {
            //     Debug.Log("GameControl :: Player Removed : " + player.userId);
            //     JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{player.userId}");
            //     continue;
            // }
            // Debug.Log("GameControl :: Player Next Player : " + player.userId);
            // Check if the player is active and has enough chips
            if (player.isSitOut == false &&
                player.carryChips >= leastChips)
            {
                // Add to the list of active player IDs
                playingPlayersId.Add(player.userId);
            }
        }
        gameRoomData.playingPlayersIdList = playingPlayersId;

        //設置Button座位
        int newButtonSeat = SetButtonSeat();

        //更新房間資料
        data = new Dictionary<string, object>()
        {
            { FirebaseManager.POT_CHIPS, 0},                                                    //底池
            { FirebaseManager.PLAYING_PLAYER_ID, playingPlayersId},                             //遊戲中玩家ID
            { FirebaseManager.COMMUNITY_POKER, SetPoker()},                                     //公共牌
            { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>()},                           //當前公共牌座位
            { FirebaseManager.BUTTON_SEAT, newButtonSeat},                                      //Button座位
            { FirebaseManager.GAME_START_TIME, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}    //遊戲開始時間
        };
        UpdateGameRoomData(data);

        //移除底池結果資料
        JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.POT_WIN_DATA}");

        //移除邊池結果資料
        JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.SIDE_WIN_DATA}");
    }

    /// <summary>
    /// 更新玩家個人資料
    /// </summary>
    /// <param name="id">玩家ID</param>
    /// <param name="dataDic">更新資料</param>
    /// <param name="callback">回傳執行</param>
    public void UpdataPlayerData(string id, Dictionary<string, object> dataDic, UnityAction<string> callback = null)
    {
        if (callback == null)
        {
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{id}",
                                                dataDic);
        }
        else
        {
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{id}",
                                                dataDic,
                                                gameObject.name,
                                                callback.Method.Name);
        }
    }

    /// <summary>
    ///更新遊戲房間資料
    /// </summary>
    /// <param name="data"></param>
    public void UpdateGameRoomData(Dictionary<string, object> data)
    {
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}",
                                                        data);
    }

    /// <summary>
    /// 設置下位行動玩家
    /// </summary>
    private void UpdateNextPlayer()
    {
        List<GameRoomPlayerData> players = new List<GameRoomPlayerData>();
        foreach (var item in gameRoomData.playingPlayersIdList)
        {
            GameRoomPlayerData player = gameRoomData.playerDataDic.Where(x => x.Value.userId == item)
                                                                  .FirstOrDefault()
                                                                  .Value;
            players.Add(player);
        }
        players = players.OrderBy(x => x.gameSeat)
                         .Where(x => x.gameState == (int)PlayerStateEnum.Playing &&
                                x.carryChips > 0)
                         .ToList();

        int nextSeat = (gameRoomData.currActionerSeat + 1) % DataManager.MaxPlayerCount;
        string nextPlayerId = players.Where(x => x.gameSeat == nextSeat)
                                     .FirstOrDefault()?
                                     .userId ?? "";

        //小盲座位玩家不存在下個座位玩家開始
        int index = 2;
        while (string.IsNullOrEmpty(nextPlayerId))
        {
            nextSeat = (gameRoomData.currActionerSeat + index) % DataManager.MaxPlayerCount;
            nextPlayerId = players.Where(x => x.gameSeat == nextSeat)
                                  .FirstOrDefault()?
                                  .userId ?? "";

            index++;
        }

        //相同的玩家(遊戲只剩1人進行)
        if (nextPlayerId == preBetActionerId)
        {
            preBetActionerId = "";
        }

        //更新資料
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_ACTIONER_ID, nextPlayerId},                    //當前行動玩家Id
            { FirebaseManager.CURR_ACTIONER_SEAT, nextSeat},                      //當前行動玩家座位
            { FirebaseManager.ACTION_CD, DataManager.StartCountDownTime},         //行動倒數時間
        };
        UpdateGameRoomData(data);
    }

    /// <summary>
    /// 更新下注行為
    /// </summary>
    /// <param name="id">玩家ID</param>
    /// <param name="betActing">下注動作</param>
    /// <param name="betValue">下注值</param>
    public void UpdateBetAction(string id, BetActingEnum betActing, double betValue)
    {
        if (cdCoroutine != null) StopCoroutine(cdCoroutine);
        GameRoomPlayerData roomPlayerData = gameRoomData.playerDataDic[id];

        //玩家狀態
        PlayerStateEnum playerState = PlayerStateEnum.Playing;
        if (betActing == BetActingEnum.Fold)
        {
            playerState = PlayerStateEnum.Fold;
        }
        else if (betActing == BetActingEnum.AllIn)
        {
            playerState = PlayerStateEnum.AllIn;
        }
        //下注差額
        double difference = betActing == BetActingEnum.AllIn ?
                            betValue :
                            Math.Max(0, betValue - roomPlayerData.currAllBetChips);

        //下注值
        betValue = betActing == BetActingEnum.AllIn ?
                   betValue + roomPlayerData.currAllBetChips :
                   betValue;

        //該回合總下注籌碼
        double currAllBetChips = roomPlayerData.currAllBetChips + difference;
        //該局總下注籌碼
        double allBetChips = roomPlayerData.allBetChips + difference;
        //攜帶籌碼
        double carryChips = roomPlayerData.carryChips - difference;
        //該流程是否已下注
        bool isBet = true;

        //更新用戶籌碼資料
        if (id == DataManager.UserId)
        {
            UpdateLocalChips(-difference);

            gameView.postNoodleChip(difference);
        }

        //更新玩家資料
        var playerData = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_ALL_BET_CHIPS, Math.Floor(currAllBetChips)},             //該回合總下注籌碼
            { FirebaseManager.ALL_BET_CHIPS, Math.Floor(allBetChips)},                      //該局總下注籌碼
            { FirebaseManager.CARRY_CHIPS, Math.Floor(carryChips)},                         //攜帶籌碼
            { FirebaseManager.IS_BET, isBet},                                               //該流程是否已下注
            { FirebaseManager.GAME_STATE, (int)playerState},                                //(PlayerStateEnum)遊戲狀態(等待/遊戲中/棄牌/All In)
        };
        UpdataPlayerData(id,
                         playerData);



        //更新下注行為
        var betActionData = new Dictionary<string, object>()
        {
            { FirebaseManager.BET_ACTIONER_ID, id},                                         //下注玩家ID
            { FirebaseManager.BET_ACTION, (int)betActing},                                  //(BetActingEnum)下注行為
            { FirebaseManager.BET_ACTION_VALUE, Math.Floor(betValue)},                      //下注籌碼值
            { FirebaseManager.UPDATE_CARRY_CHIPS, Math.Floor(carryChips)},                  //更新後的攜帶籌碼
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.BET_ACTION_DATA}",
                                                        betActionData);

        //更新底池
        double totalPot = gameRoomData.potChips + difference;
        double currCallValue = Math.Max(betValue, gameRoomData.currCallValue);
        int actionPlayerCount = gameRoomData.actionPlayerCount + 1;
        if (gameRoomData.actionPlayerCount == 0 &&
            betActing == BetActingEnum.Check ||
            betActing == BetActingEnum.Fold)
        {
            actionPlayerCount = 0;
        }

        //更新遊戲房間資料
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.POT_CHIPS, totalPot },                                 //底池
            { FirebaseManager.CURR_CALL_VALUE, currCallValue },                      //當前跟注值
            { FirebaseManager.ACTION_CD, -1},                                        //行動倒數時間
            { FirebaseManager.ACTIONP_PLAYER_COUNT, actionPlayerCount },             //當前流程行動玩家次數
        };
        UpdateGameRoomData(data);
    }

    #endregion

    #region 用戶籌碼更新

    /// <summary>
    /// 更新用戶籌碼資料
    /// </summary>
    /// <param name="changeValue">籌碼增減值</param>
    public void UpdateLocalChips(double changeValue)
    {
#if UNITY_EDITOR
        return;
#endif

        LobbyView lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
        var data = new Dictionary<string, object>();

        //更新用戶籌碼
        double newChips = 0;
        if (RoomType == TableTypeEnum.Cash)
        {
            //現金房
            newChips = DataManager.UserChips + changeValue;
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.U_CHIPS, Math.Round(newChips) },
            };
        }
        else
        {
            //虛擬房
            newChips = DataManager.UserAChips + changeValue;
            data = new Dictionary<string, object>()
            {
                { FirebaseManager.A_CHIPS, Math.Round(newChips) },
            };
        }
        /*JSBridgeManager.Instance.UpdateDataFromFirebase(
            $"{Entry.Instance.releaseType}/{FirebaseManager.USER_DATA_PATH}{DataManager.UserLoginType}/{DataManager.UserLoginPhoneNumber}",
            data,
            nameof(lobbyView.gameObject.name),
            nameof(lobbyView.UpdateUserData));*/
    }

    /// <summary>
    /// 更新攜帶籌碼(購買籌碼)
    /// </summary>
    public void UpdateCarryChips()
    {
        //更新用戶籌碼資料
        //UpdateLocalChips(-buyChipsValue);

        //更新房間籌碼
        GameRoomPlayerData playerData = GetLocalPlayer();
        double newCarryChips = playerData.carryChips + PreBuyChipsValue;
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},     //攜帶籌碼
        };
        UpdataPlayerData(playerData.userId,
                         data,
                         UpdateCarryChipsCallback);

        PreBuyChipsValue = 0;
    }

    /// <summary>
    /// 更新攜帶籌碼(購買籌碼)回傳
    /// </summary>
    /// <param name="isSuccess"></param>
    public void UpdateCarryChipsCallback(string isSuccess)
    {
        gameView.BuyChipsGoBack();
    }

    #endregion

    #region 遊戲工具類

    /// <summary>
    /// 設定玩家手牌與公共牌
    /// </summary>
    public List<int> SetPoker()
    {
        int poker;
        //52張撲克
        List<int> pokerList = new List<int>();
        for (int i = 0; i < 52; i++)
        {
            pokerList.Add(i);
        }

        //公共牌
        List<int> community = new();
        for (int i = 0; i < 5; i++)
        {
            if (DataManager.IsOpenGameTest)
            {
                //測試
                poker = (13 * gameView.CP_SuitTogList[i].value) + gameView.CP_NumTogList[i].value;
                community.Add(poker);
            }
            else
            {
                //正式
                poker = Licensing();
                community.Add(poker);
            }
        }

        //玩家手牌
        foreach (var player in gameRoomData.playerDataDic)
        {
            //保留離座
            if (player.Value.isSitOut == true)
            {
                continue;
            }

            int[] handPoker = new int[2];

            if (DataManager.IsOpenGameTest)
            {
                //測試
                handPoker[0] = (13 * gameView.PH0_SuitTogList[player.Value.gameSeat].value) + gameView.PN0_NumTogList[player.Value.gameSeat].value;
                handPoker[1] = (13 * gameView.PH1_SuitTogList[player.Value.gameSeat].value) + gameView.PN1_NumTogList[player.Value.gameSeat].value;
            }
            else
            {
                //正式
                for (int i = 0; i < 2; i++)
                {
                    poker = Licensing();
                    handPoker[i] = poker;
                }
            }

            //更新玩家資料
            Dictionary<string, object> dataDic = new Dictionary<string, object>()
            {
                { FirebaseManager.HAND_POKER, handPoker.ToList()},              //手牌
                { FirebaseManager.CURR_ALL_BET_CHIPS, 0},                       //該回合總下注籌碼
                { FirebaseManager.ALL_BET_CHIPS, 0},                            //該局總下注籌碼
                { FirebaseManager.GAME_STATE, PlayerStateEnum.Playing},         //遊戲狀態(等待/遊戲中/棄牌/All In)
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{player.Key}",
                                                            dataDic);
        }

        return community;

        //發牌
        int Licensing()
        {
            int index = new System.Random().Next(0, pokerList.Count);
            int poker = pokerList[index];
            pokerList.RemoveAt(index);
            return poker;
        }
    }

    /// <summary>
    /// 設置Botton座位
    /// </summary>
    private int SetButtonSeat()
    {
        List<GameRoomPlayerData> playerOrderSeat = gameRoomData.playerDataDic
            .OrderBy(x => x.Value.gameSeat)
            .Where(x => x.Value.isSitOut == false &&
                   (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.Playing)
            .Select(x => x.Value)
            .ToList();

        // 如果没有玩家符合条件，直接返回 -1 或者其他表示无效的值
        if (playerOrderSeat.Count == 0)
            return 0;

        do
        {
            gameRoomData.buttonSeat = (gameRoomData.buttonSeat + 1) % DataManager.MaxPlayerCount;
        }
        while (!playerOrderSeat.Any(x => x.gameSeat == gameRoomData.buttonSeat));

        return gameRoomData.buttonSeat;
    }

    /// <summary>
    /// 獲取座位角色下一位玩家
    /// </summary>
    /// <param name="currPlayerSeat"></param>
    /// <returns></returns>
    public GameRoomPlayerData GetNextPlayer(int currPlayerSeat)
    {
        List<GameRoomPlayerData> playerOrderSeat = gameRoomData.playerDataDic
            .OrderBy(x => x.Value.gameSeat)
            .Where(x => x.Value.isSitOut == false &&
                    (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.Playing &&
                    x.Value.carryChips >= leastChips)
            .Select(x => x.Value)
            .ToList();

        // 如果没有玩家符合条件，直接返回null
        if (playerOrderSeat.Count == 0)
            return null;

        // 获取当前玩家的索引
        int currentIndex = playerOrderSeat.FindIndex(x => x.gameSeat == currPlayerSeat);

        // 找到下一个玩家
        for (int i = 1; i < playerOrderSeat.Count; i++)
        {
            int nextIndex = (currentIndex + i) % playerOrderSeat.Count;
            if (!playerOrderSeat[nextIndex].isSitOut &&
                (PlayerStateEnum)playerOrderSeat[nextIndex].gameState == PlayerStateEnum.Playing)
            {
                return playerOrderSeat[nextIndex];
            }
        }

        return null; // 如果没有找到合适的玩家，返回null
    }

    /// <summary>
    /// 獲取本地玩家
    /// </summary>
    /// <returns></returns>
    public GameRoomPlayerData GetLocalPlayer()
    {
        if (gameRoomData == null ||
            gameRoomData.playerDataDic == null)
        {
            return null;
        }

        return gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                         .FirstOrDefault()
                                         .Value;
    }

    /// <summary>
    /// 獲取玩家資料
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameRoomPlayerData GetPlayerData(string id)
    {
        return gameRoomData.playerDataDic.Where(x => x.Value.userId == id)
                                         .FirstOrDefault()
                                         .Value;
    }

    /// <summary>
    /// 獲取可下注玩家
    /// </summary>
    /// <returns></returns>
    private List<GameRoomPlayerData> GetCanActionPlayer()
    {
        return gameRoomData.playerDataDic.OrderBy(x => x.Value.gameSeat)
                                         .Where(x => (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.Playing)
                                         .Where(x => gameRoomData.playingPlayersIdList.Contains(x.Value.userId))
                                         .Select(x => x.Value)
                                         .ToList();
    }

    /// <summary>
    /// 獲取遊戲中玩家
    /// </summary>
    /// <returns></returns>
    public List<GameRoomPlayerData> GetPlayingPlayer()
    {
        return gameRoomData.playerDataDic
                           .OrderBy(x => x.Value.gameSeat)
                           .Where(x => (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.Playing ||
                                       (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.AllIn)
                           .Where(x => gameRoomData.playingPlayersIdList.Contains(x.Value.userId))
                           .Select(x => x.Value)
                           .ToList();
    }

    /// <summary>
    /// 獲取AllIn玩家
    /// </summary>
    /// <returns></returns>
    public List<GameRoomPlayerData> GetAllInPlayer()
    {
        return gameRoomData.playerDataDic.OrderBy(x => x.Value.gameSeat)
                                         .Where(x => (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.AllIn)
                                         .Where(x => gameRoomData.playingPlayersIdList.Contains(x.Value.userId))
                                         .Select(x => x.Value)
                                         .ToList();
    }

    /// <summary>
    /// 獲取棄牌玩家
    /// </summary>
    /// <returns></returns>
    public List<GameRoomPlayerData> GetFoldPlayer()
    {
        return gameRoomData.playerDataDic.OrderBy(x => x.Value.gameSeat)
                                         .Where(x => (PlayerStateEnum)x.Value.gameState == PlayerStateEnum.Fold)
                                         .Where(x => gameRoomData.playingPlayersIdList.Contains(x.Value.userId))
                                         .Select(x => x.Value)
                                         .ToList();
    }

    /// <summary>
    /// 判斷結果
    /// </summary>
    /// <param name="judgePlayers">判斷玩家</param>
    /// <returns></returns>

    public List<GameRoomPlayerData> JudgeWinner(List<GameRoomPlayerData> judgePlayers)
    {
        if (judgePlayers == null || judgePlayers.Count == 0)
            return new List<GameRoomPlayerData>();

        // Step 1: Evaluate each player's hand
        var shapeDic = EvaluatePlayerHands(judgePlayers);

        // Step 2: Determine the best hand rank (lower rank is better)
        int bestHandRank = shapeDic.Values.Min(x => x.HandRank);

        // Step 3: Get players with the best hand rank
        var bestPlayers = shapeDic
            .Where(x => x.Value.HandRank == bestHandRank)
            .Select(x => x.Key)
            .ToList();

        // Step 4: Resolve ties if necessary
        return bestPlayers.Count == 1 ? bestPlayers : ResolveTie(bestPlayers, shapeDic, bestHandRank);
    }
    private Dictionary<GameRoomPlayerData, HandEvaluation> EvaluatePlayerHands(List<GameRoomPlayerData> judgePlayers)
    {
        var shapeDic = new Dictionary<GameRoomPlayerData, HandEvaluation>();

        foreach (var player in judgePlayers)
        {
            // Combine player hand and community cards
            var fullHand = new List<int>(player.handPoker);
            fullHand.AddRange(gameRoomData.communityPoker);

            PokerShape.JudgePokerShape(fullHand, (result, matchPoker) =>
            {
                Debug.Log($"GameControl :: {player.nickname} : Input Hand :: {fullHand}");
                // Store hand rank and match poker for tie-breaking
                Debug.Log($"GameControl :: {player.nickname} : Returned Cards :: {string.Join(" , ", matchPoker)}");
                bool isStraight = result == 6 || result == 2 || result == 1 ? true : false;
                bool _isFlush = result == 5 || result == 2 || result == 1 ? true : false;
                var _matchPoker = CalculateRank(matchPoker, isStraight, _isFlush);

                List<int> myCards = new List<int>();
                List<int> myRank = new List<int>();

                foreach (var item in _matchPoker)
                {
                    myRank = item.Key; // Rank (e.g., [14, 13, 12, 11, 10])
                    myCards = item.Value; // Cards used to form the rank
                    break; // Exit after the first item
                }

                Debug.Log($"GameControl :: {player.nickname} : Hand :: {PokerShape.HandRanks[result]} : CardsRank :: {string.Join(" , ", _matchPoker)}");

                shapeDic[player] = new HandEvaluation
                {
                    HandRank = result,
                    MatchPoker = myRank.Take(5).ToList(),
                };
            });
        }

        return shapeDic;
    }

    private List<GameRoomPlayerData> ResolveTie(List<GameRoomPlayerData> tiedPlayers, Dictionary<GameRoomPlayerData, HandEvaluation> shapeDic, int handRank)
    {
        if (tiedPlayers.Count <= 1) return tiedPlayers;

        var winners = new List<GameRoomPlayerData> { tiedPlayers[0] };
        var bestHand = shapeDic[winners[0]].MatchPoker;
        Debug.Log("GameControl :: BestHand : " + string.Join(" , ", bestHand));
        foreach (var player in tiedPlayers.Skip(1))
        {
            var currentHand = shapeDic[player].MatchPoker;

            // Compare hands for tie-breaking
            int comparisonResult = CompareHands(bestHand, currentHand);

            Debug.Log("GameControl :: Comparison Result : " + comparisonResult);

            if (comparisonResult > 0)
            {
                // If current hand is better, reset winners list
                winners.Clear();
                winners.Add(player);
                bestHand = currentHand;
            }
            else if (comparisonResult == 0)
            {
                // If hands are equal, add to winners list
                winners.Add(player);
            }
        }

        return winners;
    }
    public Dictionary<List<int>, List<int>> CalculateRank(List<int> cards, bool isStraight = false, bool isFlush = false)
    {
        Dictionary<List<int>, List<int>> result = new();

        // Group cards by suit and rank for efficient checks
        var cardsBySuit = cards.GroupBy(card => card / 13).ToDictionary(g => g.Key, g => g.ToList());
        var cardsByRank = cards.GroupBy(card => card % 13 + 2).ToDictionary(g => g.Key, g => g.ToList());

        // Handle Straight Flush
        if (isStraight && isFlush)
        {
            Debug.Log("Calculate Rank: Straight Flush Check");
            foreach (var suitGroup in cardsBySuit.Where(g => g.Value.Count >= 5))
            {
                var suitedCards = suitGroup.Value
                    .Select(card => new { Rank = card % 13 + 2, Card = card })
                    .OrderByDescending(x => x.Rank)
                    .ToList();

                if (HasLowStraight(suitedCards.Select(x => x.Rank).ToList()))
                {
                    suitedCards = suitedCards
                        .Select(x => new { Rank = x.Rank == 14 ? 1 : x.Rank, x.Card })
                        .OrderByDescending(x => x.Rank)
                        .ToList();
                }

                var highestStraightFlush = FindHighestConsecutiveSequence(suitedCards.Select(x => x.Rank).ToList());
                if (highestStraightFlush.Count == 5)
                {
                    var cardsUsed = suitedCards
                        .Where(x => highestStraightFlush.Contains(x.Rank))
                        .Select(x => x.Card)
                        .ToList();

                    Debug.Log("Straight Flush Found: " + string.Join(", ", highestStraightFlush));
                    result[highestStraightFlush] = cardsUsed;
                    return result;
                }
            }
        }

        // Handle Flush
        if (isFlush)
        {
            Debug.Log("Calculate Rank: Flush Check");
            var flushSuit = cardsBySuit.FirstOrDefault(g => g.Value.Count >= 5).Value;
            if (flushSuit != null)
            {
                var topFlushCards = flushSuit.OrderByDescending(card => card % 13 + 2).Take(5).ToList();
                var flushRank = topFlushCards.Select(card => card % 13 + 2).ToList();

                result[flushRank] = topFlushCards;
                return result;
            }
        }

        // Handle Straight
        if (isStraight)
        {
            Debug.Log("Calculate Rank: Straight Check");
            var allRanks = cardsByRank.Keys.OrderByDescending(rank => rank == 14 ? 1 : rank).ToList();

            if (HasLowStraight(allRanks))
            {
                allRanks = allRanks.Select(rank => rank == 14 ? 1 : rank).ToList();
            }

            var highestStraight = FindHighestConsecutiveSequence(allRanks);
            if (highestStraight.Count == 5)
            {
                var cardsUsed = cards.Where(card => highestStraight.Contains(card % 13 + 2)).ToList();

                Debug.Log("Straight Found: " + string.Join(", ", highestStraight));
                result[highestStraight] = cardsUsed;
                return result;
            }
        }

        // Handle Default: Pair, Three of a Kind, etc.
        var rankedCards = cardsByRank
            .SelectMany(g => g.Value)
            .OrderByDescending(card => card % 13 + 2)
            .Take(5)
            .ToList();

        var defaultRank = rankedCards.Select(card => card % 13 + 2).ToList();
        result[defaultRank] = rankedCards;

        return result;
    }

    private List<int> FindHighestConsecutiveSequence(List<int> ranks)
    {
        var sortedRanks = ranks.Distinct().OrderBy(rank => rank).ToList();
        var longestSeq = new List<int>();
        var currentSeq = new List<int>();

        foreach (var rank in sortedRanks)
        {
            if (currentSeq.Count == 0 || rank == currentSeq.Last() + 1)
            {
                currentSeq.Add(rank);
            }
            else
            {
                if (currentSeq.Count > longestSeq.Count) longestSeq = new List<int>(currentSeq);
                currentSeq.Clear();
                currentSeq.Add(rank);
            }
        }

        if (currentSeq.Count > longestSeq.Count) longestSeq = currentSeq;

        return longestSeq.TakeLast(5).ToList();
    }

    private bool HasLowStraight(List<int> ranks)
    {
        HashSet<int> lowStraightRanks = new() { 14, 2, 3, 4, 5 };
        return lowStraightRanks.All(ranks.Contains);
    }

    public static int CompareHands(List<int> hand1, List<int> hand2)
    {
        // Compare cards one by one
        for (int i = 0; i < Math.Min(hand1.Count, hand2.Count); i++)
        {
            if (hand1[i] > hand2[i])
                return -1;
            if (hand1[i] < hand2[i])
                return 1;
        }
        // If all cards are equal, the hands are a tie
        return 0;
    }
    // Hand evaluation structure
    private class HandEvaluation
    {
        public int HandRank { get; set; }  // Lower is better (e.g., 1 = Royal Flush)
        public List<int> MatchPoker { get; set; } // Relevant cards for tie-breaking
    }

    #endregion

    #region 聊天

    /// <summary>
    /// 更新聊天訊息
    /// </summary>
    /// <param name="msg"></param>
    public void UpdateChatMsg(string msg)
    {
        //更新聊天資料
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.USER_ID, DataManager.UserId },                    //用戶ID
            { FirebaseManager.NICKNAME, DataManager.UserNickname },             //暱稱
            { FirebaseManager.AVATAR_INDEX, DataManager.UserAvatarIndex },      //頭像編號
            { FirebaseManager.CHAT_MSG, msg },                                  //聊天訊息
        };
        JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.CHAT_DATA}",
                                                        data);
    }

    /// <summary>
    /// 聊天訊息
    /// </summary>
    private void ChatMessage()
    {
        if (gameRoomData.chatData != null &&
            !string.IsNullOrEmpty(gameRoomData.chatData.chatMsg))
        {
            gameView.ReciveChat(gameRoomData.chatData);

            //重製聊天資料
            gameRoomData.chatData.chatMsg = "";
            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.CHAT_MSG, "" },                                  //聊天訊息
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.CHAT_DATA}",
                                                            data);
        }
    }

    #endregion
}
