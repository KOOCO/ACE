using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Events;

public class GameControl : MonoBehaviour
{
    [SerializeField]
    GameView gameView;
    [SerializeField]
    RobotControl RobotControl;

    public string QueryRoomPath { get; set; }                   //查詢房間資料路徑
    public double SmallBlind { get; set; }                      //小盲值
    public TableTypeEnum RoomType { get; set; }                 //房間類型
    public int MaxRoomPeople { get; set; }                      //房間最大人數
    public double PreBuyChipsValue { get; set; }                //下一手購買籌碼值

    GameRoomData gameRoomData;                                  //房間資料
    Coroutine cdCoroutine;                                      //倒數Coroutine
    public double leastChips { get; set; }                      //最少所需籌碼

    int prePlayerCount { get; set; }                            //上個紀錄的遊戲人數
    bool isWaitingCreateRobot { get; set; }                     //是否等待產生機器人
    bool isGameStart { get; set; }                              //是否遊戲開始
    GameFlowEnum preUpdateGameFlow { get; set; }                //上個更新遊戲流程
    GameFlowEnum preLocalGameFlow { get; set; }                 //上個本地遊戲流程
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
            CreateRobot();
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
            gameView.UpdateGameRoomInfo(gameRoomData);
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
            CreateRobot();
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
            CreateRobot();
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
        if (gameRoomData.playerDataDic.Count - robotCount == 1)
        {
            //房間剩下1名玩家
            JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}");
        }
        else
        {
            //移除玩家
            RemovePlayer(DataManager.UserId);
        }

        //本地玩家房間關閉
        GameRoomManager.Instance.RemoveGameRoom(transform.name);
    }

    /// <summary>
    /// 移除玩家
    /// </summary>
    /// <param name="id"></param>
    private void RemovePlayer(string id)
    {
        gameView.PlayerExitRoom(id);

        List<string> playingPlayersId = new();
        foreach (var playerId in gameRoomData.playingPlayersIdList)
        {
            if (playerId != id)
            {
                playingPlayersId.Add(playerId);
            }
        }
        //更新房間資料
        var data = new Dictionary<string, object>()
        {
            { FirebaseManager.PLAYING_PLAYER_ID, playingPlayersId},                 //遊戲中玩家ID
        };
        UpdateGameRoomData(data);

        //玩家列表中移除
        if (gameRoomData.playerDataDic.ContainsKey(id))
        {
            JSBridgeManager.Instance.RemoveDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{id}");
        }
    }

    #endregion

    #region 機器人

    /// <summary>
    /// 產生機器人
    /// </summary>
    private void CreateRobot()
    {
        //設置座位
        int robotSeat = TexasHoldemUtil.SetGameSeat(gameRoomData);

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
        double robotCarryChips = (int)(SmallBlind * 2) * 20;

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
    private IEnumerator IStartGameFlow(GameFlowEnum gameFlow)
    {
        if (preUpdateGameFlow == gameFlow ||
            gameRoomData.hostId != DataManager.UserId)
        {
            yield break;
        }
        preUpdateGameFlow = gameFlow;

        //重製房間資料
        var roomData = new Dictionary<string, object>()
        {
            { FirebaseManager.CURR_CALL_VALUE, gameRoomData.smallBlind * 2},                //當前跟注值
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
        double newCarryChips = 0;
        switch (gameFlow)
        {
            //發牌
            case GameFlowEnum.Licensing:

                //遊戲資料初始化
                 GameDataInit();

                //積分房只剩下玩家1名
                if (RoomType == TableTypeEnum.IntegralTable && 
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

                //遊戲中玩家
                playingPlayers = GetPlayingPlayer().OrderBy(x => x.allBetChips)
                                                   .ToList();

                //底池獲勝玩家
                List<GameRoomPlayerData> potWinners = JudgeWinner(playingPlayers).OrderBy(x => x.allBetChips).ToList();

                //底池贏得籌碼
                double potMin = playingPlayers[0].allBetChips;
                double potWinChips = potMin * playingPlayers.Count();

                //更新底池贏家玩家籌碼
                List<string> potWinnerIdList = new List<string>();
                foreach (var potWinner in potWinners)
                {
                    potWinnerIdList.Add(potWinner.userId);
                    newCarryChips = (potWinner.carryChips + potWinChips) / potWinners.Count;
                    data = new Dictionary<string, object>()
                    {
                        { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},   //攜帶籌碼
                    };
                    UpdataPlayerData(potWinner.userId,
                                     data);
                }

                //是否有邊池
                bool IsHaveSide = gameRoomData.potChips - potWinChips > 0;

                //更新遊戲流程
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CURR_COMMUNITY_POKER, gameRoomData.communityPoker.Take(5)},      //當前顯示公共牌
                };
                UpdateGameRoomData(data);

                //更新底池獲勝資料
                List<string> potWinnersId = potWinners.Select(x => x.userId).ToList();
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.POT_WIN_CHIPS, potWinChips},                      //底池獲得籌碼
                    { FirebaseManager.POT_WINNERS_ID, potWinnerIdList},                 //底池獲得贏家ID
                    { FirebaseManager.IS_HAVE_SIDE, IsHaveSide},                        //是否有邊池
                };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.POT_WIN_DATA}",
                                                                data,
                                                                gameObject.name,
                                                                nameof(PotWinDataCallback));
                break;

            //邊池結果
            case GameFlowEnum.SideResult:

                //更新遊戲流程
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CURR_COMMUNITY_POKER, gameRoomData.communityPoker.Take(5)},      //當前顯示公共牌
                };
                UpdateGameRoomData(data);

                //遊戲中玩家
                playingPlayers = GetPlayingPlayer().OrderBy(x => x.allBetChips)
                                                   .ToList();

                //邊池贏家
                List<GameRoomPlayerData> sideWinners = JudgeWinner(playingPlayers).OrderBy(x => x.allBetChips)
                                                                                  .ToList();

                //底池贏家總下注籌碼
                double potMInChips = Math.Floor(gameRoomData.potWinData.potWinChips / playingPlayers.Count());

                //退回籌碼與邊池總籌碼
                double sideWinValue = 0;
                foreach (var player in playingPlayers)
                {
                    //與底池差額
                    double potDifference = player.allBetChips - potMInChips;

                    if (potDifference == 0)
                    {
                        continue;
                    }

                    double backChips = 0;
                    double chips = (sideWinners[0].allBetChips - potMInChips) - (player.allBetChips - potMInChips);
                    if (player.allBetChips <= sideWinners[0].allBetChips)
                    {
                        sideWinValue += potDifference;
                    }
                    else
                    {
                        double lose = (player.allBetChips - potMInChips) - (sideWinners[0].allBetChips - potMInChips) - potDifference;
                        sideWinValue += lose;

                        //退回籌碼
                        backChips = potDifference - lose;

                        //更新玩家籌碼
                        newCarryChips = player.carryChips + backChips;
                        data = new Dictionary<string, object>()
                        {
                            { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},   //攜帶籌碼
                        };
                        UpdataPlayerData(player.userId,
                                         data);

                        //更新退回籌碼資料
                        data = new Dictionary<string, object>()
                        {
                            { FirebaseManager.BACK_USER_ID, player.userId},        //用戶ID
                            { FirebaseManager.BACK_CHIPS_VALUE, backChips},        //退回籌碼值
                        };
                        JSBridgeManager.Instance.UpdateDataFromFirebase(
                            $"{QueryRoomPath}/{FirebaseManager.SIDE_WIN_DATA}/{FirebaseManager.BACK_CHIPS_DATA}/{player.userId}",
                            data);

                        GetPlayerData(player.userId).carryChips = newCarryChips;
                    }
                }

                //更新邊池贏家籌碼
                List<string> sideWinnerIdList = new List<string>();
                foreach (var sidewinner in sideWinners)
                {
                    sideWinnerIdList.Add(sidewinner.userId);

                    //更新玩家籌碼
                    newCarryChips = sidewinner.carryChips + (sideWinValue / sideWinners.Count());
                    data = new Dictionary<string, object>()
                    {
                        { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},   //攜帶籌碼
                    };
                    UpdataPlayerData(sidewinner.userId,
                                     data);

                    GetPlayerData(sidewinner.userId).carryChips = newCarryChips;
                }

                //更新邊池資料
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.SIDE_WIN_CHIPS, sideWinValue},             //邊池獲得籌碼
                    { FirebaseManager.SIDE_WINNERS_ID, sideWinnerIdList},        //邊池贏家ID
                };
                JSBridgeManager.Instance.UpdateDataFromFirebase($"{QueryRoomPath}/{FirebaseManager.SIDE_WIN_DATA}",
                                                                data,
                                                                gameObject.name,
                                                                nameof(SideWinDataCallback));

                break;

            //剩餘1名玩家結果
            case GameFlowEnum.OnePlayerLeftResult:

                potWinners = GetPlayingPlayer();
                if (potWinners.Count() > 1)
                {
                    yield break;
                }

                GameRoomPlayerData winner = potWinners[0];
                potWinChips = gameRoomData.potChips;

                //更新玩家籌碼
                newCarryChips = winner.carryChips + potWinChips;
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.CARRY_CHIPS, Math.Floor(newCarryChips)},   //攜帶籌碼
                };
                UpdataPlayerData(winner.userId,
                                 data);

                GetPlayerData(winner.userId).carryChips = newCarryChips;

                //更新底池獲勝資料
                potWinnerIdList = new List<string>();
                potWinnerIdList.Add(winner.userId);
                data = new Dictionary<string, object>()
                {
                    { FirebaseManager.POT_WIN_CHIPS, potWinChips},                    //底池獲得籌碼
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
        JudgeHost();

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

                //剩下一名玩家在進行遊戲
                if (gameRoomData.playingPlayersIdList.Count() == 1)
                {
                    StartCoroutine(IJudgeNextSeason());
                }
            }
        }

        //棄牌後顯示手牌
        if ((GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.PotResult ||
            (GameFlowEnum)gameRoomData.currGameFlow == GameFlowEnum.SideResult)
        {
            GameRoomPlayerData playerData = gameRoomData.playerDataDic.Where(x => x.Value.userId == DataManager.UserId)
                                                                      .FirstOrDefault()
                                                                      .Value;
            if (playerData.handPoker != null &&
                playerData.handPoker.SequenceEqual(localHand))
            {
                gameView.ShowFoldPoker();
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
    private IEnumerator ILocalGameFlowBehavior()
    {
        preLocalGameFlow = (GameFlowEnum)gameRoomData.currGameFlow;

        yield return gameView.IGameStage(gameRoomData,
                                         SmallBlind);

        var data = new Dictionary<string, object>();
        switch ((GameFlowEnum)gameRoomData.currGameFlow)
        {
            //發牌
            case GameFlowEnum.Licensing:

                //本地玩家資料
                GameRoomPlayerData playerData = GetLocalPlayer();

                //籌碼不足
                if (playerData.carryChips < leastChips)
                {
                    gameView.OnInsufficientChips();
                    playerData.gameState = (int)PlayerStateEnum.Waiting;
                }

                gameView.UpdateGameRoomInfo(gameRoomData);
                gameView.OnLicensingFlow(gameRoomData);

                yield return new WaitForSeconds(1);

                //房主執行
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    yield return new WaitForSeconds(1);
                    yield return IStartGameFlow(GameFlowEnum.SetBlind);
                }

                localHand = playerData.handPoker;

                break;

            //大小盲
            case GameFlowEnum.SetBlind:

                gameView.OnBlindFlow(gameRoomData);
                UpdateGameRoom();

                yield return new WaitForSeconds(1);
                isCloseAllCdInfo = false;
                //房主執行
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    //設置下位行動玩家
                    UpdateNextPlayer();
                }
                break;

            //翻牌
            case GameFlowEnum.Flop:

                //開始公共牌翻牌流程
                yield return IStartCommunityFlopSeason();
                break;

            //轉牌
            case GameFlowEnum.Turn:

                //開始公共牌翻牌流程
                yield return IStartCommunityFlopSeason();
                break;

            //河牌
            case GameFlowEnum.River:

                //開始公共牌翻牌流程
                yield return IStartCommunityFlopSeason();
                break;

            //底池結果
            case GameFlowEnum.PotResult:

                isCloseAllCdInfo = true;
                yield return gameView.IPotResult(gameRoomData);

                //購買籌碼
                if (PreBuyChipsValue > 0 &&
                    !gameRoomData.potWinData.isHaveSide)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);

                //是否有玩家籌碼不足
                bool isPotIntefralResult = gameRoomData.potWinData.isHaveSide == false &&
                                 gameRoomData.playerDataDic.Any(x => x.Value.carryChips < leastChips);

                //顯示積分結果
                if (RoomType == TableTypeEnum.IntegralTable &&
                    isPotIntefralResult)
                {
                    gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                }

                //房主執行
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    if (gameRoomData.potWinData.isHaveSide == true)
                    {
                        //有邊池贏家
                        yield return IStartGameFlow(GameFlowEnum.SideResult);

                        yield break;
                    }
                    else
                    {
                        //積分房
                        if (RoomType == TableTypeEnum.IntegralTable)
                        {
                            //有玩家籌碼不足
                            if (isPotIntefralResult)
                            {
                                yield break;
                            }
                        }

                        //重新遊戲流程
                        yield return IStartGameFlow(GameFlowEnum.Licensing);
                    }
                }              

                break;

            //邊池結果
            case GameFlowEnum.SideResult:

                yield return gameView.SideResult(gameRoomData);

                //購買籌碼
                if (PreBuyChipsValue > 0)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);


                //是否有玩家籌碼不足
                bool isSideIntegralResult = gameRoomData.playerDataDic.Any(x => x.Value.carryChips < leastChips);

                //顯示積分結果
                if (RoomType == TableTypeEnum.IntegralTable &&
                    isSideIntegralResult)
                {
                    gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                }

                //房主執行
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    //積分房
                    if (RoomType == TableTypeEnum.IntegralTable)
                    {
                        //有玩家籌碼不足
                        if (isSideIntegralResult)
                        {
                            yield break;
                        }
                    }

                    //重新遊戲流程
                    yield return IStartGameFlow(GameFlowEnum.Licensing);
                }

                break;

            //剩餘1名玩家結果
            case GameFlowEnum.OnePlayerLeftResult:

                yield return gameView.IPotResult(gameRoomData);

                //購買籌碼
                if (PreBuyChipsValue > 0)
                {
                    UpdateCarryChips();
                }

                yield return new WaitForSeconds(2);

                //房主執行
                if (gameRoomData.hostId == DataManager.UserId)
                {
                    //積分房對手離開/斷線
                    if (RoomType == TableTypeEnum.IntegralTable &&
                        gameRoomData.playingPlayersIdList.Count() == 1)
                    {
                        //顯示積分結果
                        gameView.SetBattleResult(GetLocalPlayer().carryChips >= leastChips);
                    }

                    //重新遊戲流程
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

        GamePlayerInfo player = gameView.GetPlayer(gameRoomData.playerDataDic[gameRoomData.currActionerId].userId);
        if (gameRoomData.actionCD == DataManager.StartCountDownTime)
        {
            yield return new WaitForSeconds(1);

            if (gameRoomData.actionCD < 0 ||
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

        if (gameRoomData.actionCD < 0 ||
            preCD != gameRoomData.actionCD)
        {
            yield break;
        }

        player.ActionFrame = true;
        player.CountDown(DataManager.StartCountDownTime,
                         gameRoomData.actionCD);
        if (player.UserId == DataManager.UserId)
        {
            gameView.CheckActionArea(gameRoomData);
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
            int currActionCD =  gameRoomData.actionCD - 1;

            if (currActionCD < 0)
            {
                //超過時間棄牌
                string id = gameRoomData.currActionerId;
                UpdateBetAction(id,
                                BetActingEnum.Fold,
                                0);
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
            yield return new WaitForSeconds(1);

            List<GameRoomPlayerData> canActionPlayers = GetCanActionPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> allInPlayers = GetAllInPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> foldPlayers = GetFoldPlayer().OrderBy(x => x.currAllBetChips).ToList();
            List<GameRoomPlayerData> playingPlayers = GetPlayingPlayer().OrderBy(x => x.currAllBetChips).ToList();

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
            };
            UpdataPlayerData(id,
                             data);
        }

        //遊戲中玩家
        List<string> playingPlayersId = new();
        foreach (var player in gameRoomData.playerDataDic)
        {
            //離座 / 籌碼不足 不添加
            if (player.Value.isSitOut == false &&
                player.Value.carryChips >= leastChips)
            {
                playingPlayersId.Add(player.Key);
            }
        }

        //設置Button座位
        int newButtonSeat = SetButtonSeat();

        //更新房間資料
        data = new Dictionary<string, object>()
        {
            { FirebaseManager.POT_CHIPS, 0},                                        //底池
            { FirebaseManager.PLAYING_PLAYER_ID, playingPlayersId},                 //遊戲中玩家ID
            { FirebaseManager.COMMUNITY_POKER, SetPoker()},                         //公共牌
            { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>()},               //當前公共牌座位
            { FirebaseManager.BUTTON_SEAT, newButtonSeat},                          //Button座位
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
            betActing == BetActingEnum.Check)
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
            newChips = DataManager.UserUChips + changeValue;
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
            poker = Licensing();
            community.Add(poker);
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
            for (int i = 0; i < 2; i++)
            {
                poker = Licensing();
                handPoker[i] = poker;
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
    private List<GameRoomPlayerData> JudgeWinner(List<GameRoomPlayerData> judgePlayers)
    {
        if (judgePlayers == null ||
            judgePlayers.Count == 0)
        {
            return new List<GameRoomPlayerData>();
        }

        //判斷結果(牌型結果,符合的牌)
        Dictionary<GameRoomPlayerData, (int, List<int>)> shapeDic = new Dictionary<GameRoomPlayerData, (int, List<int>)>();
        //玩家的牌(牌型結果,(公牌+手牌))
        Dictionary<GameRoomPlayerData, (int, List<int>)> clientPokerDic = new Dictionary<GameRoomPlayerData, (int, List<int>)>();

        foreach (var player in judgePlayers)
        {
            List<int> judgePoker = new List<int>();
            judgePoker.Add(player.handPoker[0]);
            judgePoker.Add(player.handPoker[1]);

            if (judgePoker != null &&
                gameRoomData.communityPoker != null)
            {
                judgePoker = judgePoker.Concat(gameRoomData.communityPoker).ToList();

                //判定牌型
                PokerShape.JudgePokerShape(judgePoker, (result, matchPoker) =>
                {
                    shapeDic.Add(player, (result, matchPoker));
                    clientPokerDic.Add(player, (result, judgePoker));
                });
            }
        }

        //最大的牌型结果
        int maxResult = shapeDic.Values.Min(x => x.Item1);

        //最大牌型人數
        int matchCount = shapeDic.Values.Count(x => x.Item1 == maxResult);

        if (matchCount == 1)
        {
            //最大結果1人
            KeyValuePair<GameRoomPlayerData, (int, List<int>)> minItem = shapeDic.FirstOrDefault(x => x.Value.Item1 == maxResult);
            return new List<GameRoomPlayerData>() { minItem.Key };
        }
        else
        {
            //最大結果玩家(符合的牌)
            Dictionary<GameRoomPlayerData, List<int>> pairPlayer = new Dictionary<GameRoomPlayerData, List<int>>();

            //選出相同結果的玩家
            foreach (var shape in shapeDic)
            {
                if (shape.Value.Item1 == maxResult)
                {
                    List<int> numList = shape.Value.Item2.Select(x => x % 13 == 0 ? 14 : x % 13).ToList();
                    numList.Sort(new TexasHoldemUtil.SpecialComparer());
                    pairPlayer.Add(shape.Key, numList);
                }
            }

            //找出符合牌最大結果玩家
            List<GameRoomPlayerData> maxResultPlayersList = new List<GameRoomPlayerData>();
            int maxValue = int.MinValue;
            foreach (var pair in pairPlayer)
            {
                int max = 0;

                //順子判斷
                if (maxResult == 1 || maxResult == 6)
                {
                    //最小順子判斷
                    if ((pair.Value.Contains(14) && pair.Value.Contains(1) &&
                        pair.Value.Contains(2) && pair.Value.Contains(3) && pair.Value.Contains(4)))
                    {
                        max = 4;
                    }
                    else
                    {
                        max = pair.Value.Max();
                    }
                }
                else
                {
                    max = pair.Value.Max();
                }

                if (max > maxValue)
                {
                    maxValue = max;
                    maxResultPlayersList.Clear();
                    maxResultPlayersList.Add(pair.Key);
                }
                else if (max == maxValue)
                {
                    maxResultPlayersList.Add(pair.Key);
                }
            }

            if (maxResultPlayersList.Count() == 1)
            {
                //最大符合結果1人
                return maxResultPlayersList;
            }
            else
            {
                //比較最大手牌玩家
                List<GameRoomPlayerData> handPokerList = new List<GameRoomPlayerData>();

                if (maxResultPlayersList.Count() > 1)
                {
                    //符合最大結果有多人
                    handPokerList = new List<GameRoomPlayerData>(maxResultPlayersList);
                }
                else
                {
                    //所有相同結果的牌型都一樣
                    foreach (var player in pairPlayer)
                    {
                        handPokerList.Add(player.Key);
                    }
                }

                //將最大牌放置手牌1
                foreach (var player in handPokerList)
                {
                    if (player.handPoker[0] % 13 > 0 && player.handPoker[0] % 13 < player.handPoker[1] % 13)
                    {
                        int temp = player.handPoker[0];
                        player.handPoker[0] = player.handPoker[1];
                        player.handPoker[1] = temp;
                    }
                }

                //最大手牌1玩家(不包含符合結果牌)
                GameRoomPlayerData maxHand0PokerPlayer = handPokerList.OrderByDescending(x => (x.handPoker[0] % 13 == 0 ? int.MinValue : x.handPoker[0] % 13) + 1)
                                                          .FirstOrDefault();

                List<GameRoomPlayerData> maxHandPokerClientList = new List<GameRoomPlayerData>();
                if (maxHand0PokerPlayer != null)
                {
                    //符合牌不在手牌1
                    maxHandPokerClientList = handPokerList.Where(x => x.handPoker[0] % 13 == maxHand0PokerPlayer.handPoker[0] % 13).ToList();
                }

                //最大手牌1玩家1人
                if (maxHandPokerClientList.Count() == 1)
                {
                    return maxHandPokerClientList;
                }
                else
                {
                    //比較手牌2(不包含符合結果牌)
                    GameRoomPlayerData maxHand1PokerPlayer = handPokerList.OrderByDescending(x => (x.handPoker[1] % 13 == 0 ? int.MinValue : x.handPoker[1] % 13) + 1)
                                                              .FirstOrDefault();

                    //符合牌都在手牌
                    if (maxHand1PokerPlayer == null)
                    {
                        return handPokerList;
                    }

                    //最大手牌2所有玩家
                    List<GameRoomPlayerData> maxHandPoker1PlayerList = handPokerList.Where(x => x.handPoker[1] % 13 == maxHand1PokerPlayer.handPoker[1] % 13).ToList();
                    return maxHandPoker1PlayerList;
                }
            }
        }
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
