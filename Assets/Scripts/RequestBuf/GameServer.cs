using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

using RequestBuf;

public class GameServer : MonoBehaviour
{
    Coroutine cdCoroutine;

    public BaseRequest targetBaseRequest;   //對應的協議接收物件


    public List<Client> clientList;        //所有客戶端
    List<Client> playingList;              //當前遊戲玩家
    Dictionary<string, double> allInDic;   //AllIn玩家

    int countDownTime = 20;                     //倒數時間
    int autoActionTime = 2;                    //自動行動時間
    //int accumulationPlayer;                     //房間累積人數
    public int maxRoomPeople;                   //房間最大人數
    Client currTestClient;

    //撲克名稱
    string[] pokerName = new string[]
    {
        "Club", "Diamond","Heart","Spade"
    };

    public double SmallBlind { get; set; }
    public TableTypeEnum RoomType { get; set; }
    public string QueryRoomPath { get; set; }       //查詢房間資料路徑

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        gameRoomData = new GameRoomData();
        clientList = new List<Client>();
        playingList = new List<Client>();
        allInDic = new Dictionary<string, double>();
    }

    /// <summary>
    /// Server開始
    /// </summary>
    /// <param name="roomType">房間類型</param>
    public void ServerStart(TableTypeEnum roomType)
    {
        //StartCoroutine(ITest(roomType));
    }

    /*private IEnumerator ITest(TableTypeEnum roomType)
    {
        maxRoomPeople = RoomType == TableTypeEnum.IntegralTable ? 2 : 6;

        //測試用
        int initPlayerCount = RoomType == TableTypeEnum.IntegralTable ? 1 : UnityEngine.Random.Range(2, 6);
        gameRoomData.ButtonSeat = UnityEngine.Random.Range(0, initPlayerCount);
        for (int i = 0; i < initPlayerCount; i++)
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.Request_PlayerInOutRoom;

            PlayerInfoPack playerInfoPack = new PlayerInfoPack();
            playerInfoPack.UserID = $"Player{accumulationPlayer}";
            playerInfoPack.NickName = RoomType == TableTypeEnum.IntegralTable ? $"Battle{i}" : $"Player{accumulationPlayer}";
            playerInfoPack.Chips = RoomType == TableTypeEnum.IntegralTable ? 10000 : ((SmallBlind * 2) * 40) + 1000 + (i * 4000);
            playerInfoPack.Avatar = UnityEngine.Random.Range(1, 3);

             if (i == 0)
             {
                 playerInfoPack.Chips = 1200;
             }
             else if (i == 1)
             {
                 playerInfoPack.Chips = 1000;
             }
             else
             {
                 playerInfoPack.Chips = 500;
             }

            PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
            playerInOutRoomPack.IsInRoom = true;
            playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

            pack.PlayerInOutRoomPack = playerInOutRoomPack;
            Request_PlayerInOutRoom(pack);
        }

        if (roomType == TableTypeEnum.Cash || roomType == TableTypeEnum.VCTable)
        {
            SetPoker();
            gameRoomData.CurrFlow = FlowEnum.PotResult;
            gameRoomData.CurrCommunityPoker = new List<int>(); //(List<int>)gameRoomData.CommunityPoker.Take(5).ToList();
            gameRoomData.TotalPot = 0;
            gameRoomData.CurrCallValue = SmallBlind;
        }
        else if (roomType == TableTypeEnum.IntegralTable)
        {
            gameRoomData.CurrFlow = FlowEnum.PotResult;
            gameRoomData.TotalPot = 0;
            gameRoomData.CurrCommunityPoker = new List<int>();
        }

        yield return new WaitForSeconds(1);

        yield return SendRequest_NextGameStage();
    }*/

    /// <summary>
    /// 測試玩家加操作
    /// </summary>
    public void TextPlayerAction(BetActingEnum acting, bool isExit = false)
    {
        if (gameRoomData.CurrActingPlayer == null ||
            gameRoomData.CurrActingPlayer.UserId == Entry.TestInfoData.LocalUserId ||
            currTestClient == gameRoomData.CurrActingPlayer ||
            cdCoroutine == null)
        {
            return;
        }

        //離開
        if (isExit == true)
        {
            bool hasPlayer = clientList.Any(x => x == gameRoomData.CurrActingPlayer);
            if (hasPlayer)
            {
                MainPack exitPack = new MainPack();
                exitPack.ActionCode = ActionCode.Request_PlayerInOutRoom;

                PlayerInfoPack playerInfoPack = new PlayerInfoPack();
                playerInfoPack.UserID = gameRoomData.CurrActingPlayer.UserId;

                PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
                playerInOutRoomPack.IsInRoom = false;
                playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

                exitPack.PlayerInOutRoomPack = playerInOutRoomPack;
                Request_PlayerInOutRoom(exitPack);
            }
            return;
        }

        bool isJustAllIn = gameRoomData.CurrActingPlayer.CurrRoomChips <= gameRoomData.CurrCallValue;

        double betValue = 0;
        if (acting == BetActingEnum.Raise)
        {
            if (isJustAllIn)
            {
                acting = BetActingEnum.AllIn;
                betValue = gameRoomData.CurrActingPlayer.CurrRoomChips;
            }
            else
            {
                if (gameRoomData.CurrActingPlayer.CurrRoomChips <= gameRoomData.CurrCallValue * 2)
                {
                    acting = BetActingEnum.AllIn;
                    betValue = gameRoomData.CurrActingPlayer.CurrRoomChips;
                }
                else
                {
                    betValue = gameRoomData.CurrCallValue * 2;
                }
            }
        }
        else if (acting == BetActingEnum.Call)
        {
            if (isJustAllIn)
            {
                acting = BetActingEnum.AllIn;
                betValue = gameRoomData.CurrActingPlayer.CurrRoomChips;
            }
            else
            {
                if (gameRoomData.IsFirstRaisePlayer)
                {
                    if (gameRoomData.CurrCallValue == SmallBlind)
                    {
                        acting = BetActingEnum.Check;
                    }
                    else
                    {
                        betValue = gameRoomData.CurrCallValue;
                    }
                }
                else
                {
                    if (gameRoomData.CurrActingPlayer.CurrBetValue == gameRoomData.CurrCallValue)
                    {
                        acting = BetActingEnum.Check;
                    }
                    else
                    {
                        betValue = gameRoomData.CurrCallValue;
                    }
                }
            }
        }
        else if (acting == BetActingEnum.AllIn)
        {
            betValue = gameRoomData.CurrActingPlayer.CurrRoomChips;
        }

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_PlayerActed;

        PlayerActedPack playerActedPack = new PlayerActedPack();
        playerActedPack.ActPlayerId = gameRoomData.CurrActingPlayer.UserId;
        playerActedPack.ActingEnum = acting;
        playerActedPack.BetValue = betValue;

        pack.PlayerActedPack = playerActedPack;
        StartCoroutine(Request_PlayerActed(pack));
    }


    /// <summary>
    /// 用戶
    /// </summary>
    public class Client
    {
        public int Seat;            //座位
        public string UserId;       //ID
        public string NickName;     //暱稱
        public int Avatar;          //頭像
        public double CurrRoomChips;    //房間內持有籌碼
        public int HandPoker0;      //手牌0
        public int HandPoker1;      //手牌1

        public PlayerStateEnum State;           //玩家狀態
        public BetActingEnum PreActingEnum;        //上回行動
        public double CurrBetValue;             //當前下注值
        public double AllBetValue;              //總下注直
        public bool IsActionTaken;              //該回合是否已行動過
        public bool IsSitOut;                   //是否離座
    }

    /// <summary>
    /// 遊戲資料
    /// </summary>
    private GameRoomData gameRoomData;
    public class GameRoomData
    {
        public List<int> CommunityPoker;                        //公共牌
        public Dictionary<string, (int, int)> HandPokerDic;     //玩家手牌(id, (手牌0, 手牌1))
        public List<int> CurrCommunityPoker;                    //當前公共牌

        public Client CurrActingPlayer;            //當前行動玩家
        public GameFlowEnum CurrFlow;                  //當前遊戲階段
        public bool IsFirstRaisePlayer;            //首位加注玩家
        public int ButtonSeat;                     //button座位玩家
        public double TotalPot;                    //底池籌碼
        public double CurrCallValue;               //當前跟注值
        public List<Client> PotWineerList;         //主池贏家
    }

    /// <summary>
    /// 發送協議
    /// </summary>
    /// <param name="pack"></param>
    private void SendRequest(MainPack pack)
    {
        bool isLocalUser = clientList.Where(x => x.UserId == Entry.TestInfoData.LocalUserId).Count() > 0;
        if (isLocalUser == true)
        {
            //RequestManager.Instance.HandleRequest(pack);
            targetBaseRequest.HandleRequest(pack);
        }
    }

    /// <summary>
    /// 發送廣播
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="isExcludeId">排除本地玩家</param>
    private void SendRoomBroadCast(MainPack pack, bool isExcludeId = false)
    {
        pack.SendModeCode = SendModeCode.RoomBroadcast;
        bool isLocalUser = clientList.Where(x => x.UserId == Entry.TestInfoData.LocalUserId).Count() > 0;
        if (targetBaseRequest != null && isExcludeId == false && isLocalUser == true)
        {
            //RequestManager.Instance.HandleRequest(pack);
            targetBaseRequest.HandleRoomBroadcast(pack);
        }
    }

    /// <summary>
    /// 設置Botton座位
    /// </summary>
    private void SetButtonSeat()
    {
        gameRoomData.ButtonSeat = (gameRoomData.ButtonSeat + 1) % maxRoomPeople;
        bool isHave = playingList.Any(x => x.Seat == gameRoomData.ButtonSeat);
        if (isHave == false)
        {
            SetButtonSeat();
        }
    }

    /// <summary>
    /// 獲取Button座位玩家
    /// </summary>
    /// <returns></returns>
    private Client GetButtonPlayer()
    {
        return playingList.Where(x => x.Seat == gameRoomData.ButtonSeat).FirstOrDefault();
    }

    /// <summary>
    /// 獲取玩家
    /// </summary>
    /// <param name="matchState">符合狀態</param>
    /// <returns></returns>
    private List<Client> GetPlayingUser(List<PlayerStateEnum> matchState)
    {
        List<Client> playing = new List<Client>();
        foreach (var player in playingList)
        {
            for (int i = 0; i < matchState.Count; i++)
            {
                if (player.State == matchState[i])
                {
                    playing.Add(player);
                    break;
                }
            }
        }

        return playing;
    }

    /// <summary>
    /// 聊天測試
    /// </summary>
    public void TextChat()
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_Chat;

        int clientIndex = UnityEngine.Random.Range(0, clientList.Count);
        while (clientList[clientIndex].UserId == DataManager.UserId)
        {
            clientIndex = UnityEngine.Random.Range(0, clientList.Count);
        }


        string[] contents = new string[]
        {
            "Hurry up, hurry up.",
            "The weather is sunny today, suitable for All In, don’t hesitate!!! Hurry up!",
            "I have a pair of AA, fold it everyone~",
        };

        ChatPack chatPack = new ChatPack();
        chatPack.Id = clientList[clientIndex].UserId;
        chatPack.Nickname = clientList[clientIndex].NickName;
        chatPack.Avatar = clientList[clientIndex].Avatar;
        chatPack.Content = contents[UnityEngine.Random.Range(0, contents.Length)];

        pack.ChatPack = chatPack;
        targetBaseRequest.HandleRoomBroadcast(pack);
    }

    /// <summary>
    /// 請求_聊天
    /// </summary>
    /// <param name="pack"></param>
    public void BroadCastRequest_Chat(MainPack pack)
    {
        SendRoomBroadCast(pack, true);
    }

    /// <summary>
    /// 請求_玩家進出房間
    /// </summary>
    /// <param name="pack"></param>
    public void Request_PlayerInOutRoom(MainPack pack)
    {
        if (pack.PlayerInOutRoomPack.IsInRoom == true)
        {
            //進入房間
            AddNewPlayer(pack.PlayerInOutRoomPack.PlayerInfoPack);
        }
        else
        {
            StartCoroutine(IExitRoom(pack));
        }
    }

    /// <summary>
    /// 離開房間
    /// </summary>
    /// <returns></returns>
    private IEnumerator IExitRoom(MainPack pack)
    {
        //退出房間
        Client client = clientList.Where(x => x.UserId == pack.PlayerInOutRoomPack.PlayerInfoPack.UserID).FirstOrDefault();

        if (client != null)
        {
            if (gameRoomData.CurrActingPlayer == client)
            {
                //當前回合是該玩家
                pack = new MainPack();
                pack.ActionCode = ActionCode.Request_PlayerActed;

                PlayerActedPack playerActedPack = new PlayerActedPack();
                playerActedPack.ActPlayerId = client.UserId;
                playerActedPack.ActingEnum = BetActingEnum.Fold;
                playerActedPack.BetValue = 0;

                pack.PlayerActedPack = playerActedPack;

                yield return Request_PlayerActed(pack);

                yield return new WaitForSeconds(1);
            }

            clientList.Remove(client);
            playingList.Remove(client);

            //Debug.Log($"{client.UserId}:Exit Room");

            pack = new MainPack();
            pack.ActionCode = ActionCode.Request_PlayerInOutRoom;
            pack.SendModeCode = SendModeCode.RoomBroadcast;

            PlayerInfoPack playerInfoPack = new PlayerInfoPack();
            playerInfoPack.UserID = client.UserId;

            PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
            playerInOutRoomPack.IsInRoom = false;
            playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

            pack.PlayerInOutRoomPack = playerInOutRoomPack;
            SendRoomBroadCast(pack);
        }
    }

    /// <summary>
    /// 添加新玩家
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="nickName"></param>
    /// <param name="initchips"></param>
    public void AddNewPlayer(PlayerInfoPack playerInfoPack)
    {
        Client client = new Client()
        {
            UserId = playerInfoPack.UserID,
            NickName = playerInfoPack.NickName,
            CurrRoomChips = playerInfoPack.Chips,
            State = PlayerStateEnum.Waiting,
            Avatar = playerInfoPack.Avatar,
        };
        clientList.Add(client);


        //座位
        int seat = 0;
        int currMaxSeat = clientList.OrderByDescending(x => x.Seat).FirstOrDefault().Seat;
        int temp = (currMaxSeat + 1) % maxRoomPeople;
        for (int i = 0; i < maxRoomPeople; i++)
        {
            bool seated = clientList.Any(x => x.Seat == temp);
            if (seated == false)
            {
                seat = temp;
                break;
            }

            temp = (temp + 1) % maxRoomPeople;
        }
        client.Seat = seat;

        /*//監測連線狀態
        JSBridgeManager.Instance.StartListenerConnectState($"{QueryRoomPath}",
                                                           client.UserId);*/
        /*MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_PlayerInOutRoom;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        playerInfoPack.Seat = client.Seat;

        PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
        playerInOutRoomPack.IsInRoom = true;
        playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

        pack.PlayerInOutRoomPack = playerInOutRoomPack;
        bool isExclude = client.UserId == Entry.TestInfoData.LocalUserId;
        SendRoomBroadCast(pack, isExclude);*/
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        playingList = new List<Client>();
        allInDic.Clear();
        gameRoomData.CommunityPoker = new List<int>();
        gameRoomData.CurrCommunityPoker = new List<int>();
        gameRoomData.HandPokerDic = new Dictionary<string, (int, int)>();
        gameRoomData.PotWineerList = new List<Client>();
    }

    /// <summary>
    /// 判斷玩家擁有籌碼
    /// </summary>
    private bool JudgeClientChips()
    {
        //籌碼不足玩家
        List<Client> NotEnoughChipsPlayerList = new List<Client>();

        foreach (var client in clientList)
        {
            //籌碼 < 小盲注
            if (client.CurrRoomChips <= SmallBlind)
            {
                NotEnoughChipsPlayerList.Add(client);
            }
        }

        //發送籌碼不足玩家
        if (RoomType != TableTypeEnum.IntegralTable)
        {
            foreach (var client in NotEnoughChipsPlayerList)
            {
                client.State = PlayerStateEnum.Waiting;
                if (client.UserId == Entry.TestInfoData.LocalUserId)
                {
                    SendRequest_NotEnoughChips(client);
                }
                else
                {
                    client.CurrRoomChips = 7000;
                    /*MainPack exitPack = new MainPack();
                    exitPack.ActionCode = ActionCode.Request_PlayerInOutRoom;

                    PlayerInfoPack playerInfoPack = new PlayerInfoPack();
                    playerInfoPack.UserID = client.UserId;

                    PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
                    playerInOutRoomPack.IsInRoom = false;
                    playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

                    exitPack.PlayerInOutRoomPack = playerInOutRoomPack;
                    Request_PlayerInOutRoom(exitPack);*/
                }
            }

            if (clientList.Count < 3)
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.Request_PlayerInOutRoom;

                PlayerInfoPack playerInfoPack = new PlayerInfoPack();
                playerInfoPack.UserID = $"00000{Entry.TestInfoData.newPlayerId}";
                playerInfoPack.NickName = $"Player{Entry.TestInfoData.newPlayerId}";
                playerInfoPack.Chips = 6000;

                PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
                playerInOutRoomPack.IsInRoom = true;
                playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

                pack.PlayerInOutRoomPack = playerInOutRoomPack;
                Request_PlayerInOutRoom(pack);

                Entry.TestInfoData.newPlayerId++;
            }
        }
        else
        {
            //積分房
            if (NotEnoughChipsPlayerList.Count > 0)
            {
                SendRequest_BattleResult(NotEnoughChipsPlayerList[0]);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 設定玩家手牌與公共牌
    /// </summary>
    public void SetPoker()
    {
        Init();

        int poker;
        //52張撲克
        List<int> pokerList = new List<int>();
        for (int i = 0; i < 52; i++)
        {
            pokerList.Add(i);
        }
        
        string str = "";
        //牌面結果
        for (int i = 0; i < 5; i++)
        {
            poker = Licensing();
            gameRoomData.CommunityPoker.Add(poker);
            str += $"{pokerName[poker / 13]}{poker % 13 + 1} ,";
        }
        //Debug.Log($"CommunityPoker: {str}");

        //玩家手牌
        foreach (var client in clientList)
        {
            //籌碼 < 小盲注 || 離座
            if (client.CurrRoomChips <= SmallBlind ||
                client.IsSitOut == true)
            {
                continue;
            }

            int[] handPoker = new int[2];
            for (int i = 0; i < 2; i++)
            {
                poker = Licensing();
                handPoker[i] = poker;
            }

            client.HandPoker0 = handPoker[0];
            client.HandPoker1 = handPoker[1];
            client.State = PlayerStateEnum.Playing;
            client.CurrBetValue = 0;
            client.AllBetValue = 0;
            playingList.Add(client);
            gameRoomData.HandPokerDic.Add(client.UserId, (handPoker[0], handPoker[1]));

            //Debug.Log($"{client.UserId} HandPoker: {pokerName[handPoker[0] / 13]}{handPoker[0] % 13 + 1} , {pokerName[handPoker[1] / 13]}{handPoker[1] % 13 + 1}");
        }

        playingList = (List<Client>)playingList.OrderBy(x => x.Seat).ToList();
        
        
        /*
        //測試用
        gameRoomData.CommunityPoker = new List<int>() {8,19,11,27,37};
        clientList[0].HandPoker0 = 0;
        clientList[0].HandPoker1 = 13;
        clientList[0].State = PlayerStateEnum.Playing;
        gameRoomData.HandPokerDic.Add(clientList[0].UserId, (clientList[0].HandPoker0, clientList[0].HandPoker1));
        clientList[0].CurrBetValue = 0;
        playingList.Add(clientList[0]);
        
        clientList[1].HandPoker0 = 26;
        clientList[1].HandPoker1 = 39;
        clientList[1].State = PlayerStateEnum.Playing;
        gameRoomData.HandPokerDic.Add(clientList[1].UserId, (clientList[1].HandPoker0, clientList[1].HandPoker1));
        clientList[1].CurrBetValue = 0;
        playingList.Add(clientList[1]);
        /*
        clientList[2].HandPoker0 = 8;
        clientList[2].HandPoker1 = 10;
        clientList[2].State = PlayerStateEnum.Playing;
        gameRoomData.HandPokerDic.Add(clientList[2].UserId, (clientList[2].HandPoker0, clientList[2].HandPoker1));
        clientList[2].CurrBetValue = 0;
        playingList.Add(clientList[2]);
        
        clientList[3].HandPoker0 = 39;
        clientList[3].HandPoker1 = 50;
        clientList[3].State = PlayerStateEnum.Playing;
        gameRoomData.HandPokerDic.Add(clientList[3].UserId, (clientList[3].HandPoker0, clientList[3].HandPoker1));
        clientList[3].CurrBetValue = 0;
        playingList.Add(clientList[3]);
        
        if (clientList.Count == 3)
        {
            int index = clientList.Count - 1;
            clientList[index].HandPoker0 = 12;
            clientList[index].HandPoker1 = 7;
            clientList[index].State = PlayerStateEnum.Playing;
            gameRoomData.HandPokerDic.Add(clientList[index].UserId, (clientList[index].HandPoker0, clientList[index].HandPoker1));
            clientList[index].CurrBetValue = 0;
            playingList.Add(clientList[index]);
        }*/
 
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
    /// 玩家離/回座
    /// </summary>
    /// <param name="pack"></param>
    public void BroadCastSitOut(MainPack pack)
    {
        string id = pack.SitOutPack.UserId;
        bool isSitOut = pack.SitOutPack.IsSitOut;

        Client client = clientList.Where(x => x.UserId == id).FirstOrDefault();
        client.IsSitOut = isSitOut;
    }

    /// <summary>
    /// 發送積分結果
    /// </summary>
    /// <param name="losePlayer"></param>
    private void SendRequest_BattleResult(Client losePlayer)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_BattleResult;

        BattleResultPack battleResultPack = new BattleResultPack();
        battleResultPack.FailPlayerId = losePlayer.UserId;

        pack.BattleResultPack = battleResultPack;
        SendRoomBroadCast(pack);
    }

    /// <summary>
    /// 發送籌碼不足
    /// </summary>
    /// <param name="client"></param>
    private void SendRequest_NotEnoughChips(Client client)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_InsufficientChips;

        InsufficientChipsPack insufficientChipsPack = new InsufficientChipsPack();
        insufficientChipsPack.SmallBlind = SmallBlind;

        pack.InsufficientChipsPack = insufficientChipsPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 設定下位行動玩家
    /// </summary>
    /// <returns></returns>
    private void SetNextPlayer()
    {
        int curr = playingList.Select((c, i) => (c, i))
                              .Where(x => x.c == gameRoomData.CurrActingPlayer)
                              .FirstOrDefault()
                              .i;
        curr = (curr + 1) % playingList.Count;
        while (playingList[curr].State != PlayerStateEnum.Playing)
        {
            curr = curr + 1 >= playingList.Count ? 0 : curr + 1;
        }

        gameRoomData.CurrActingPlayer = playingList[curr];
    }

    /// <summary>
    /// 獲取所有遊戲玩家籌碼
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, double> GetAllPlayerChips()
    {
        Dictionary<string, double> allPlayerChipsDic = new Dictionary<string, double>();
        foreach (var client in clientList)
        {
            allPlayerChipsDic.Add(client.UserId, client.CurrRoomChips);
        }

        return allPlayerChipsDic;
    }

    /// <summary>
    /// 取消倒數
    /// </summary>
    public void CancelCountDown()
    {
        if (cdCoroutine != null)
        {
            StopCoroutine(cdCoroutine);
            cdCoroutine = null;
        }
    }

    /// <summary>
    /// 獲取邊池籌碼值
    /// </summary>
    /// <returns></returns>
    private double GetSideChipsValue()
    {
        double sideValue = 0;

        if (allInDic.Count() >= 2)
        {
            double min = allInDic.Min(x => x.Value);

            foreach (var allIn in allInDic)
            {
                sideValue += allIn.Value - min;
            }
        }

        return sideValue;
    }

    /// <summary>
    /// 是否進入下階段
    /// </summary>
    /// <returns></returns>
    private bool IsNextStage()
    {
        List<Client> judgePlayingPlayerList = new List<Client>();
        foreach (var player in playingList)
        {
            if (player.State == PlayerStateEnum.Playing)
            {
                judgePlayingPlayerList.Add(player);
            }
        }

        // bool isAllPlayerActionTaken = judgePlayerList.All(x => x.IsActionTaken == true);
        bool isBetSame = judgePlayingPlayerList.All(x => x.CurrBetValue == judgePlayingPlayerList[0].CurrBetValue);

        List<Client> judgePlayerList = new List<Client>();
        foreach (var player in playingList)
        {
            if (player.State == PlayerStateEnum.Playing || player.State == PlayerStateEnum.AllIn)
            {
                judgePlayerList.Add(player);
            }
        }

        bool isAllPlayerActionTaken = judgePlayerList.All(x => x.IsActionTaken == true);
        double minAllBetValue = judgePlayerList.Min(x => x.AllBetValue);
        bool isAllBetValueSmae = judgePlayerList.All(x => x.AllBetValue >= minAllBetValue && (x.AllBetValue >= gameRoomData.CurrCallValue || x.CurrRoomChips == 0));
        return isAllPlayerActionTaken && isAllBetValueSmae && isBetSame;
    }

    /// <summary>
    /// 獲取下個遊戲階段
    /// </summary>
    /// <returns></returns>
    private GameFlowEnum GetNextFlow()
    {
        int curr = (int)gameRoomData.CurrFlow;
        int next = (curr + 1) % Enum.GetValues(typeof(GameFlowEnum)).Length;

        return (GameFlowEnum)next;
    }

    /// <summary>
    /// 獲取遊戲房間訊息包
    /// </summary>
    /// <returns></returns>
    private GameRoomInfoPack GetGameRoomInfoPack()
    {
        GameRoomInfoPack gameRoomInfoPack = new GameRoomInfoPack();
        gameRoomInfoPack.TotalPot = gameRoomData.TotalPot;
        gameRoomInfoPack.AllPlayerChips = GetAllPlayerChips();

        return gameRoomInfoPack;
    }

    /// <summary>
    /// 獲取遊戲階段包
    /// </summary>
    /// <returns></returns>
    private GameStagePack GetGameStagePack()
    {
        GameStagePack gameStagePack = new GameStagePack();
        gameStagePack.flowEnum = gameRoomData.CurrFlow;
        gameStagePack.SmallBlind = SmallBlind;

        return gameStagePack;
    }

    /// <summary>
    /// 請求_更新房間訊息
    /// </summary>
    public IEnumerator Request_UpdateRoomInfo(MainPack pack)
    {
        List<PlayerInfoPack> playerInfoPacks = new List<PlayerInfoPack>();
        pack.PlayerInfoPackList = new List<PlayerInfoPack>();
        foreach (var player in clientList)
        {
            PlayerInfoPack playerInfoPack = new PlayerInfoPack();
            playerInfoPack.Seat = player.Seat;
            playerInfoPack.UserID = player.UserId;
            playerInfoPack.NickName = player.NickName;
            playerInfoPack.Chips = player.CurrRoomChips;
            playerInfoPack.Avatar = player.Avatar;
            playerInfoPack.CurrBetValue = player.CurrBetValue;
            
            playerInfoPacks.Add(playerInfoPack);
        }

        UpdateRoomInfoPack updateRoomInfoPack = new UpdateRoomInfoPack();
        updateRoomInfoPack.flowEnum = gameRoomData.CurrFlow;
        updateRoomInfoPack.TotalPot = gameRoomData.TotalPot;
        updateRoomInfoPack.playingIdList = playingList.Select(x => x.UserId).ToList(); ;

        CommunityPokerPack currCommunityPokerPack = new CommunityPokerPack();
        currCommunityPokerPack.CurrCommunityPoker = gameRoomData.CurrCommunityPoker;

        pack.CommunityPokerPack = currCommunityPokerPack;
        pack.UpdateRoomInfoPack = updateRoomInfoPack;
        pack.PlayerInfoPackList = playerInfoPacks;
        SendRequest(pack);

        yield return null;
    }

    /// <summary>
    /// 發送下個遊戲階段
    /// </summary>
    private IEnumerator SendRequest_NextGameStage()
    {
        foreach (var player in playingList)
        {
            if (player.State == PlayerStateEnum.Playing)
            {
                player.CurrBetValue = 0;
                player.IsActionTaken = false;
            }
        }

        gameRoomData.CurrFlow = GetNextFlow();

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_GameStage;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        GameStagePack gameStagePack = GetGameStagePack();

        switch (gameRoomData.CurrFlow)
        {
            //發牌
            case GameFlowEnum.Licensing:
                yield return SendRequest_GameStage_Licensing(pack, gameStagePack);
                break;

            //大小盲
            case GameFlowEnum.SetBlind:
                yield return SendRequest_GameStage_SetBlind(pack, gameStagePack);
                break;
                
            //翻牌
            case GameFlowEnum.Flop:
                yield return SendRequest_TurnStage(pack, gameStagePack, 3);
                break;

            //轉牌
            case GameFlowEnum.Turn:
                yield return SendRequest_TurnStage(pack, gameStagePack, 4);
                break;

            //河牌
            case GameFlowEnum.River:
                yield return SendRequest_TurnStage(pack, gameStagePack, 5);
                break;

            //遊戲結果
            case GameFlowEnum.PotResult:
                yield return SendRequest_GameResult(pack, gameStagePack);
                break;
        }
    }

    /// <summary>
    /// 發送遊戲階段_(翻牌/轉牌/河牌)
    /// </summary>
    private IEnumerator SendRequest_TurnStage(MainPack pack, GameStagePack gameStagePack, int communityCount)
    {
        gameRoomData.CurrCommunityPoker = (List<int>)gameRoomData.CommunityPoker.Take(communityCount).ToList();
        gameRoomData.CurrCallValue = SmallBlind;

        CommunityPokerPack currCommunityPokerPack = new CommunityPokerPack();
        currCommunityPokerPack.CurrCommunityPoker = gameRoomData.CurrCommunityPoker;

        pack.CommunityPokerPack = currCommunityPokerPack;
        pack.GameRoomInfoPack = GetGameRoomInfoPack();
        pack.GameStagePack = gameStagePack;
        SendRoomBroadCast(pack);

        yield return new WaitForSeconds(2.1f);

        gameRoomData.IsFirstRaisePlayer = true;
        gameRoomData.CurrActingPlayer = GetButtonPlayer();
        yield return SendRequest_NextPlayerActingRound();
    }

    /// <summary>
    /// 發送遊戲階段_發牌
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="gameStagePack"></param>
    private IEnumerator SendRequest_GameStage_Licensing(MainPack pack, GameStagePack gameStagePack)
    {
        if (JudgeClientChips() == true)
        {
            SetPoker();
            SetButtonSeat();

            LicensingStagePack licensingStagePack = new LicensingStagePack();
            licensingStagePack.HandPokerDic = gameRoomData.HandPokerDic;
            licensingStagePack.ButtonSeatId = GetButtonPlayer().UserId;

            pack.LicensingStagePack = licensingStagePack;
            pack.GameStagePack = gameStagePack;
            SendRoomBroadCast(pack);

            yield return new WaitForSeconds(1);
            yield return SendRequest_NextGameStage();
        }        
    }

    /// <summary>
    /// 發送遊戲階段_設置大小盲
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="gameStagePack"></param>
    private IEnumerator SendRequest_GameStage_SetBlind(MainPack pack, GameStagePack gameStagePack)
    {
        int currButtonSeatIndex = playingList.Select((c, i) => (c, i))
                                             .Where(x => x.c.Seat == gameRoomData.ButtonSeat)
                                             .FirstOrDefault()
                                             .i;

        int sbPlayerSeat = (currButtonSeatIndex + 1) % playingList.Count;
        int bbPlayerSeat = (sbPlayerSeat + 1) % playingList.Count;      

        playingList[sbPlayerSeat].CurrRoomChips -= SmallBlind;
        playingList[bbPlayerSeat].CurrRoomChips -= SmallBlind * 2;

        playingList[sbPlayerSeat].CurrBetValue = SmallBlind;
        playingList[bbPlayerSeat].CurrBetValue = SmallBlind * 2;

        playingList[sbPlayerSeat].AllBetValue = SmallBlind;
        playingList[bbPlayerSeat].AllBetValue = SmallBlind * 2;

        gameRoomData.CurrActingPlayer = playingList[bbPlayerSeat];
        gameRoomData.IsFirstRaisePlayer = false;
        gameRoomData.TotalPot = (SmallBlind + SmallBlind * 2);
        gameRoomData.CurrCallValue = SmallBlind * 2;

        gameStagePack.flowEnum = GameFlowEnum.SetBlind;
        gameStagePack.SmallBlind = SmallBlind;

        BlindStagePack blindStagePack = new BlindStagePack();
        blindStagePack.SBPlayerId = playingList[sbPlayerSeat].UserId;
        blindStagePack.BBPlayerId = playingList[bbPlayerSeat].UserId;
        blindStagePack.SBPlayerChips = playingList[sbPlayerSeat].CurrRoomChips;
        blindStagePack.BBPlayerChips = playingList[bbPlayerSeat].CurrRoomChips;

        pack.BlindStagePack = blindStagePack;
        pack.GameRoomInfoPack = GetGameRoomInfoPack();
        pack.GameStagePack = gameStagePack;
        SendRoomBroadCast(pack);

        yield return new WaitForSeconds(1);
        yield return SendRequest_NextPlayerActingRound();
    }

    /// <summary>
    /// 發送下位玩家行動回合
    /// </summary>
    private IEnumerator SendRequest_NextPlayerActingRound()
    {
        //前位玩家
        Client preClient = gameRoomData.CurrActingPlayer;

        SetNextPlayer();
        if (currTestClient == gameRoomData.CurrActingPlayer || gameRoomData.CurrActingPlayer.UserId == Entry.TestInfoData.LocalUserId)
        {
            currTestClient = gameRoomData.CurrActingPlayer;
        }
        
        if (gameRoomData.CurrActingPlayer.UserId == Entry.TestInfoData.LocalUserId)
        {
            Client client = playingList.Where(x => x.UserId == Entry.TestInfoData.LocalUserId).FirstOrDefault();

            bool IsUnableRaise = false;
            if (allInDic.Count() > 0)
            {
                double allInMin = allInDic.Min(x => x.Value);
                IsUnableRaise = allInDic.Count() == playingList.Count - 1 ||
                                (gameRoomData.CurrCallValue == allInMin && preClient.PreActingEnum == BetActingEnum.Call);
            }

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.BroadCastRequest_PlayerActingRound;
            pack.SendModeCode = SendModeCode.RoomBroadcast;

            PlayerActingRoundPack playerActingRoundPack = new PlayerActingRoundPack();
            playerActingRoundPack.IsFirstRaisePlayer = gameRoomData.IsFirstRaisePlayer;
            playerActingRoundPack.CurrCallValue = gameRoomData.CurrCallValue;
            playerActingRoundPack.CallDifference = gameRoomData.CurrCallValue - gameRoomData.CurrActingPlayer.CurrBetValue;
            playerActingRoundPack.PlayerChips = gameRoomData.CurrActingPlayer.CurrRoomChips;
            playerActingRoundPack.PlayerCurrBryValue = gameRoomData.CurrActingPlayer.CurrBetValue;
            playerActingRoundPack.IsUnableRaise = IsUnableRaise;
            playerActingRoundPack.TotalPot = gameRoomData.TotalPot;

            pack.PlayerActingRoundPack = playerActingRoundPack;

            SendRequest(pack);
        }

        yield return null;
        SendRequest_GameStage_ActingCD();
    }

    /// <summary>
    /// 發送遊戲階段_行動倒數
    /// </summary>
    private void SendRequest_GameStage_ActingCD()
    {
        cdCoroutine = StartCoroutine(ICountDown());
    }

    /// <summary>
    /// 倒數
    /// </summary>
    /// <returns></returns>
    private IEnumerator ICountDown()
    {
        int cdTime = countDownTime;//行動倒數時間
        CancelCountDown();
        yield return null;

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_ActingCD;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        ActingCDPack actingCDPack = new ActingCDPack();
        actingCDPack.TotalCDTime = cdTime;
        actingCDPack.ActingPlayerId = gameRoomData.CurrActingPlayer.UserId;

        for (int i = cdTime; i >= 0; i--)
        {
            actingCDPack.CD = i;
            pack.ActingCDPack = actingCDPack;
            SendRoomBroadCast(pack);

            yield return new WaitForSeconds(1);

            //AI自動
            if (i == autoActionTime && gameRoomData.CurrActingPlayer.UserId != Entry.TestInfoData.LocalUserId)
            {
                BetActingEnum aiActing = BetActingEnum.Fold;
                double aiBetValue = 0;
                int foldRate = new System.Random().Next(0, 100);
      
                bool isJustAllIn = gameRoomData.CurrActingPlayer.CurrRoomChips <= gameRoomData.CurrCallValue;
                if (foldRate > 10)
                {
                    aiActing = BetActingEnum.Call;

                    if (isJustAllIn)
                    {
                        aiActing = BetActingEnum.AllIn;
                        aiBetValue = gameRoomData.CurrActingPlayer.CurrRoomChips;
                    }
                    else
                    {
                        if (gameRoomData.IsFirstRaisePlayer)
                        {
                            if (gameRoomData.CurrCallValue == SmallBlind)
                            {
                                aiActing = BetActingEnum.Check;
                            }
                            else
                            {
                                aiBetValue = gameRoomData.CurrCallValue;
                            }
                        }
                        else
                        {
                            if (gameRoomData.CurrActingPlayer.CurrBetValue == gameRoomData.CurrCallValue)
                            {
                                aiActing = BetActingEnum.Check;
                            }
                            else
                            {
                                aiBetValue = gameRoomData.CurrCallValue;
                            }
                        }
                    }
                }

                MainPack aiPack = new MainPack();
                aiPack.ActionCode = ActionCode.Request_PlayerActed;

                PlayerActedPack aiPlayerActedPack = new PlayerActedPack();
                aiPlayerActedPack.ActPlayerId = gameRoomData.CurrActingPlayer.UserId;
                aiPlayerActedPack.ActingEnum = aiActing;
                aiPlayerActedPack.BetValue = aiBetValue;

                aiPack.PlayerActedPack = aiPlayerActedPack;
                StartCoroutine(Request_PlayerActed(aiPack));

                CancelCountDown();
            }
        }

        //倒數結束
        BetActingEnum acting = BetActingEnum.Fold;

        //大小盲回合 && 所有玩家Call輪到大盲玩家
        if (gameRoomData.CurrFlow == GameFlowEnum.SetBlind)
        {
            List<Client> judgePlayerList = new List<Client>();
            foreach (var player in playingList)
            {
                if (player.State == PlayerStateEnum.Playing)
                {
                    judgePlayerList.Add(player);
                }
            }
            bool isBetSame = judgePlayerList.All(x => x.CurrBetValue == judgePlayerList[0].CurrBetValue);
            if (isBetSame)
            {
                acting = BetActingEnum.Check;
            }
        }

        pack = new MainPack();
        pack.ActionCode = ActionCode.Request_PlayerActed;

        PlayerActedPack playerActedPack = new PlayerActedPack();
        playerActedPack.ActPlayerId = gameRoomData.CurrActingPlayer.UserId;
        playerActedPack.ActingEnum = acting;
        playerActedPack.BetValue = 0;

        pack.PlayerActedPack = playerActedPack;
        yield return Request_PlayerActed(pack);
    }

    /// <summary>
    /// 請求_玩家採取動作
    /// </summary>
    /// <param name="acting"></param>
    public IEnumerator Request_PlayerActed(MainPack pack)
    {
        CancelCountDown();
        gameRoomData.CurrActingPlayer.PreActingEnum = pack.PlayerActedPack.ActingEnum;

        yield return null;

        if (pack.PlayerActedPack.ActingEnum == BetActingEnum.AllIn)
        {
            yield return AllInJudge(pack);
        }
        else
        {
            //更新跟注值
            if (pack.PlayerActedPack.ActingEnum == BetActingEnum.Raise)
            {
                gameRoomData.CurrCallValue = pack.PlayerActedPack.BetValue;
            }

            //下注籌碼
            double betChips = 0;
            if (pack.PlayerActedPack.ActingEnum == BetActingEnum.Call ||
                pack.PlayerActedPack.ActingEnum == BetActingEnum.Raise)
            {
                gameRoomData.IsFirstRaisePlayer = false;
                //差額
                betChips = pack.PlayerActedPack.BetValue - gameRoomData.CurrActingPlayer.CurrBetValue;
            }

            //更新玩家訊息
            if (pack.PlayerActedPack.ActingEnum == BetActingEnum.Fold)
            {
                gameRoomData.CurrActingPlayer.State = PlayerStateEnum.Fold;
            }
            gameRoomData.CurrActingPlayer.IsActionTaken = true;
            gameRoomData.CurrActingPlayer.CurrRoomChips -= betChips;
            if (pack.PlayerActedPack.ActingEnum == BetActingEnum.Raise ||
                pack.PlayerActedPack.ActingEnum == BetActingEnum.Call)
            {
                gameRoomData.CurrActingPlayer.CurrBetValue = pack.PlayerActedPack.BetValue;
                gameRoomData.CurrActingPlayer.AllBetValue += pack.PlayerActedPack.BetValue;
            }

            //更新底池
            gameRoomData.TotalPot += betChips;

            pack.GameRoomInfoPack = GetGameRoomInfoPack();
            pack.ActionCode = ActionCode.BroadCastRequest_ShowActing;
            pack.SendModeCode = SendModeCode.RoomBroadcast;

            pack.PlayerActedPack.PlayerChips = gameRoomData.CurrActingPlayer.CurrRoomChips;

            SendRoomBroadCast(pack);

            currTestClient = null;

            yield return new WaitForSeconds(1);

            int foldCount = playingList.Where(x => x.State == PlayerStateEnum.Fold).Count();
            Client survivor = playingList.Where(x => x.State == PlayerStateEnum.Playing || x.State == PlayerStateEnum.AllIn).FirstOrDefault();

            if (foldCount == playingList.Count - 1)
            {
                //剩下一位玩家未棄牌
                yield return SendRequest_SurvivorWinner(survivor);
            }
            else
            {
                //是否進入下階段
                if (IsNextStage())
                {
                    int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing).Count();                  

                    if (playingCount <= 1)
                    {
                        if (playingCount == 1)
                        {
                            int allInCount = playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count();
                            Client c = playingList.Where(x => x.State == PlayerStateEnum.Playing).FirstOrDefault();
                            
                            if (c.CurrBetValue >= gameRoomData.CurrCallValue || allInCount > 0)
                            {
                                double allInMin = playingList.Where(x => x.State == PlayerStateEnum.AllIn)
                                                             .Min(x => x.CurrBetValue);

                                if (c.AllBetValue >= allInMin)
                                {
                                    yield return SendRequest_ShowGameResult();
                                }
                                else
                                {
                                    yield return SendRequest_NextPlayerActingRound();
                                }
                            }
                            else
                            {
                                yield return SendRequest_NextPlayerActingRound();
                            }
                        }
                        else
                        {
                            yield return SendRequest_ShowGameResult();
                        }
                    }
                    else
                    {
                        yield return SendRequest_NextGameStage();
                    }                    
                }
                else
                {
                    int allInCount = playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count();
                    int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing).Count();

                    if (playingCount == 1 && allInCount > 0)
                    {
                        Client c = playingList.Where(x => x.State == PlayerStateEnum.Playing).FirstOrDefault();

                        double allInMin = playingList.Where(x => x.State == PlayerStateEnum.AllIn)
                                                     .Min(x => x.CurrBetValue);

                        if (c.AllBetValue >= allInMin)
                        {
                            yield return SendRequest_ShowGameResult();
                        }
                        else
                        {
                            yield return SendRequest_NextPlayerActingRound();
                        }
                    }
                    else
                    {
                        yield return SendRequest_NextPlayerActingRound();
                    }
                    /*
                    if (IsPlayingBetBiggerAllIn() == true)
                    {
                        yield return SendRequest_ShowGameResult();
                    }
                    else
                    {
                        int allInCount = playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count();
                        int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing).Count();

                        if (playingCount == 1 && allInCount > 0)
                        {
                            Client c = playingList.Where(x => x.State == PlayerStateEnum.Playing).FirstOrDefault();

                            double allInMin = playingList.Where(x => x.State == PlayerStateEnum.AllIn)
                                                         .Min(x => x.CurrBetValue);

                            if (c.AllBetValue >= allInMin)
                            {
                                yield return SendRequest_ShowGameResult();
                            }
                            else
                            {
                                yield return SendRequest_NextPlayerActingRound();
                            }
                        }
                        else
                        {
                            yield return SendRequest_NextPlayerActingRound();
                        }                        
                    }*/
    }
}
        }
    }

    /// <summary>
    /// 玩家All In判斷
    /// </summary>
    /// <returns></returns>
    private IEnumerator AllInJudge(MainPack pack)
    {       
        double allInValue = gameRoomData.CurrActingPlayer.CurrRoomChips + gameRoomData.CurrActingPlayer.CurrBetValue;
        allInDic.Add(gameRoomData.CurrActingPlayer.UserId, allInValue);

        //更新跟注值
        gameRoomData.IsFirstRaisePlayer = false;
        gameRoomData.CurrActingPlayer.State = PlayerStateEnum.AllIn;
        if (allInValue > gameRoomData.CurrCallValue)
        {
            gameRoomData.CurrCallValue = allInValue;
        }

        //下注籌碼
        double betChips = pack.PlayerActedPack.BetValue;

        //更新玩家訊息
        gameRoomData.CurrActingPlayer.State = PlayerStateEnum.AllIn;
        gameRoomData.CurrActingPlayer.IsActionTaken = true;
        gameRoomData.CurrActingPlayer.CurrBetValue = allInValue;
        gameRoomData.CurrActingPlayer.AllBetValue = allInValue;
        gameRoomData.CurrActingPlayer.CurrRoomChips = 0;      

        //更新底池
        gameRoomData.TotalPot += betChips;

        pack.GameRoomInfoPack = GetGameRoomInfoPack();
        pack.ActionCode = ActionCode.BroadCastRequest_ShowActing;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        pack.PlayerActedPack.BetValue = gameRoomData.CurrActingPlayer.CurrBetValue;
        pack.PlayerActedPack.PlayerChips = gameRoomData.CurrActingPlayer.CurrRoomChips;

        SendRoomBroadCast(pack);

        currTestClient = null;

        yield return new WaitForSeconds(1);

        //是否直接顯示結果
        int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing).Count();
        if (playingCount == 0)
        {
            yield return SendRequest_ShowGameResult();
        }
        else
        {
            if (IsNextStage())
            {
                Client c = playingList.Where(x => x.State == PlayerStateEnum.Playing).FirstOrDefault();
                double allInMax = playingList.Where(x => x.State == PlayerStateEnum.AllIn)
                                             .Max(x => x.CurrBetValue);
                if (playingCount == 1 && c.AllBetValue >= allInMax)
                {
                    yield return SendRequest_ShowGameResult();
                }
                else
                {
                    yield return SendRequest_NextGameStage();
                }
                
            }
            else
            {
                yield return SendRequest_NextPlayerActingRound();
            }
            
            /*if (IsPlayingBetBiggerAllIn())
            {
                yield return SendRequest_ShowGameResult();
            }
            else
            {
                yield return SendRequest_NextPlayerActingRound();
            }   */         
        }
    }

    /// <summary>
    /// 剩餘玩家下注籌碼>=AllIn
    /// </summary>
    /// <returns></returns>
    private bool IsPlayingBetBiggerAllIn()
    {
        if (playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count() == 0)
        {
            return false;
        }

        //未AllIn玩家最小下注值
        double playingMin = playingList.Where(x => x.State == PlayerStateEnum.Playing)
                                       .Min(x => x.CurrBetValue);

        double allInMin = playingList.Where(x => x.State == PlayerStateEnum.AllIn)
                                     .Min(x => x.CurrBetValue);

        return playingMin >= allInMin;
    }

    /// <summary>
    /// 發送遊戲階段_判斷遊戲結果
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="gameStagePack"></param>
    /// <param name="survivorWinner">棄牌結果獲勝玩家</param>
    /// <returns></returns>
    private IEnumerator SendRequest_GameResult(MainPack pack, GameStagePack gameStagePack)
    {
        Dictionary<string, (int, int)> handPokerDic = new Dictionary<string, (int, int)>();
        foreach (var player in playingList)
        {
            if (player.State != PlayerStateEnum.Waiting &&
                player.State != PlayerStateEnum.Fold)
            {
                handPokerDic.Add(player.UserId,
                                (gameRoomData.HandPokerDic[player.UserId].Item1, gameRoomData.HandPokerDic[player.UserId].Item2));
            }
        }       

        List<Client> judgePlayers = GetPlayingUser(new List<PlayerStateEnum>() { PlayerStateEnum.Playing, PlayerStateEnum.AllIn});
        List<Client> winners = JudgeWinner(judgePlayers);

        List<Client> judgeSidePlayers = GetPlayingUser(new List<PlayerStateEnum>() { PlayerStateEnum.AllIn });
        List<Client> sidewinnerList = JudgeWinner(judgeSidePlayers);
        bool isHaveSide = true;    //是否有邊池
        for (int i = 0; i < winners.Count; i++)
        {
            if (sidewinnerList.Contains(winners[i]))
            {
                isHaveSide = false;
                break;
            }
        }

        double winChips = isHaveSide ? gameRoomData.TotalPot - GetSideChipsValue() : gameRoomData.TotalPot;
        winChips = winChips / winners.Count();
        WinnerPack winnerPack = new WinnerPack();
        winnerPack.WinnerDic = new Dictionary<string, double>();
        foreach (var winnerPlayer in winners)
        {
            winnerPlayer.CurrRoomChips += winChips;
            winnerPack.WinnerDic.Add(winnerPlayer.UserId, winnerPlayer.CurrRoomChips);
            gameRoomData.PotWineerList.Add(winnerPlayer);
        }
        winnerPack.WinChips = winChips;

        LicensingStagePack licensingStagePack = new LicensingStagePack();
        licensingStagePack.HandPokerDic = handPokerDic;

        CommunityPokerPack currCommunityPokerPack = new CommunityPokerPack();
        currCommunityPokerPack.CurrCommunityPoker = gameRoomData.CurrCommunityPoker;

        pack.CommunityPokerPack = currCommunityPokerPack;
        pack.LicensingStagePack = licensingStagePack;
        pack.GameStagePack = gameStagePack;
        pack.WinnerPack = winnerPack;

        SendRoomBroadCast(pack);

        float winnerTime = winners.Count() * 2;

        int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing || x.State == PlayerStateEnum.AllIn).Count();

        //判斷邊池
        if (allInDic.Count() >= 2 && playingCount > 2)
        {
            yield return new WaitForSeconds(5 + winnerTime);
            yield return SendRequest_SideResult();
        }
        else
        {
            int allInCount = playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count();
            float allinTime = allInCount > 0 ? 2 : 0;

            yield return new WaitForSeconds(6 + winnerTime + allinTime);
            yield return SendRequest_NextGameStage();
        }
    }

    /// <summary>
    /// 發送遊戲階段_存活玩家遊戲結果
    /// </summary>
    /// <param name="survivorWinner">存活玩家</param>
    /// <returns></returns>
    private IEnumerator SendRequest_SurvivorWinner(Client winner)
    {
        gameRoomData.CurrFlow = GameFlowEnum.PotResult;

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_GameStage;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        winner.CurrRoomChips += gameRoomData.TotalPot;
        WinnerPack winnerPack = new WinnerPack();
        winnerPack.WinnerDic = new Dictionary<string, double>() { { winner.UserId, winner.CurrRoomChips } };
        winnerPack.WinChips = gameRoomData.TotalPot;

        Dictionary<string, (int, int)> handPokerDic = new Dictionary<string, (int, int)>();
        handPokerDic.Add(winner.UserId, (winner.HandPoker0, winner.HandPoker1));

        LicensingStagePack licensingStagePack = new LicensingStagePack();
        licensingStagePack.HandPokerDic = handPokerDic;

        CommunityPokerPack currCommunityPokerPack = new CommunityPokerPack();
        currCommunityPokerPack.CurrCommunityPoker = gameRoomData.CurrCommunityPoker;

        pack.CommunityPokerPack = currCommunityPokerPack;
        pack.LicensingStagePack = licensingStagePack;
        pack.GameStagePack = GetGameStagePack();
        pack.WinnerPack = winnerPack;

        SendRoomBroadCast(pack);

        yield return new WaitForSeconds(6);
        yield return SendRequest_NextGameStage();
    }

    /// <summary>
    /// 發送_直接顯示遊戲結果
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendRequest_ShowGameResult()
    {
        gameRoomData.CurrFlow = GameFlowEnum.PotResult;

        Dictionary<string, (int, int)> handPokerDic = new Dictionary<string, (int, int)>();
        foreach (var player in playingList)
        {
            if (player.State != PlayerStateEnum.Waiting &&
                player.State != PlayerStateEnum.Fold)
            {
                handPokerDic.Add(player.UserId,
                                (gameRoomData.HandPokerDic[player.UserId].Item1, gameRoomData.HandPokerDic[player.UserId].Item2));
            }
        }

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_GameStage;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        List<Client> judgePlayers = GetPlayingUser(new List<PlayerStateEnum>() { PlayerStateEnum.Playing, PlayerStateEnum.AllIn });
        List<Client> winners = JudgeWinner(judgePlayers);

        List<Client> judgeSidePlayers = GetPlayingUser(new List<PlayerStateEnum>() { PlayerStateEnum.AllIn });
        List<Client> sidewinnerList = JudgeWinner(judgeSidePlayers);
        bool isHaveSide = true;    //是否有邊池
        for (int i = 0; i < winners.Count; i++)
        {
            if (sidewinnerList.Contains(winners[i]))
            {
                isHaveSide = false;
                break;
            }
        }

        int allInCount = playingList.Where(x => x.State == PlayerStateEnum.AllIn).Count();

        double winChips = isHaveSide ? gameRoomData.TotalPot - GetSideChipsValue() : gameRoomData.TotalPot;
        winChips = winChips / winners.Count();
        WinnerPack winnerPack = new WinnerPack();
        winnerPack.WinnerDic = new Dictionary<string, double>();
        foreach (var winnerPlayer in winners)
        {
            winnerPlayer.CurrRoomChips += winChips;
            winnerPack.WinnerDic.Add(winnerPlayer.UserId, winnerPlayer.CurrRoomChips);
            gameRoomData.PotWineerList.Add(winnerPlayer);
        }
        winnerPack.WinChips = winChips;

        LicensingStagePack licensingStagePack = new LicensingStagePack();
        licensingStagePack.HandPokerDic = handPokerDic;

        CommunityPokerPack currCommunityPokerPack = new CommunityPokerPack();
        currCommunityPokerPack.CurrCommunityPoker = allInCount >= 1 ?
                                                    gameRoomData.CommunityPoker :
                                                    gameRoomData.CurrCommunityPoker;

        pack.CommunityPokerPack = currCommunityPokerPack;
        pack.LicensingStagePack = licensingStagePack;
        pack.GameStagePack = GetGameStagePack();
        pack.WinnerPack = winnerPack;

        SendRoomBroadCast(pack);

        float winnerTime = winners.Count() * 2;

        //判斷邊池
        int playingCount = playingList.Where(x => x.State == PlayerStateEnum.Playing || x.State == PlayerStateEnum.AllIn).Count();
        if (allInDic.Count() >= 2 && playingCount > 2)
        {
            if (playingList.Count == 2)
            {
                yield return new WaitForSeconds(6 + winnerTime);
                yield return SendRequest_NextGameStage();
            }
            else
            {
                if (playingCount > 2)
                {
                    yield return new WaitForSeconds(5 + winnerTime);
                    yield return SendRequest_SideResult();
                }
                else
                {
                    yield return new WaitForSeconds(6 + winnerTime);
                    yield return SendRequest_NextGameStage();
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(6 + winnerTime);
            yield return SendRequest_NextGameStage();
        }
    }

    /// <summary>
    /// 發送_邊池結果
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendRequest_SideResult()
    {
        List<Client> judgePlayers = GetPlayingUser(new List<PlayerStateEnum>() { PlayerStateEnum.AllIn });
        List<Client> sidewinnerList = JudgeWinner(judgePlayers);

        //最小allIn
        //double min = allInDic.Min(x => x.Value);
        double min = sidewinnerList.Min(x => x.AllBetValue);

        double sideValue = GetSideChipsValue();

        //退回籌碼
        Dictionary<string, double> backDic = new Dictionary<string, double>();
        foreach (var allInPlayer in allInDic)
        {
            double backChips = Mathf.Max(0, (float)(allInPlayer.Value - min)); ;
            backDic.Add(allInPlayer.Key, backChips);

            //非主池贏家
            if (sidewinnerList.All(x => x.UserId != allInPlayer.Key))
            {
                Client c = playingList.Where(x => x.UserId == allInPlayer.Key).FirstOrDefault();
                c.CurrRoomChips += backChips;

                sideValue = Mathf.Max(0, (float)(sideValue - backChips));
            }
            else
            {
                sideValue = 0;
            }
        }

        //贏得籌碼
        Dictionary<string, double> sideWinnerDic = new Dictionary<string, double>();
        foreach (var winner in sidewinnerList)
        {
            double winValue = sideValue / sidewinnerList.Count;
            sideWinnerDic.Add(winner.UserId, winValue);

            Client c = playingList.Where(x => x.UserId == winner.UserId).FirstOrDefault();
            c.CurrRoomChips += winValue;
        }
       

        /*//贏得籌碼
        Dictionary<string, double> sideWinnerDic = new Dictionary<string, double>();
        foreach (var allInPlayer in allInDic)
        {
            Client player = playingList.Where(x => x.UserId == allInPlayer.Key).FirstOrDefault();
            bool isWinner = sidewinnerList.Contains(player);
            if (isWinner)
            {
                //非主池贏家
                if (!gameRoomData.PotWineerList.Contains(player))
                {
                    sideWinnerDic.Add(player.UserId, 0);
                    foreach (var judgePlayer in allInDic)
                    {
                        //不是自己且不是贏家
                        if (judgePlayer.Key != player.UserId && 
                            !sideWinnerDic.ContainsKey(judgePlayer.Key))
                        {
                            double getChips = judgePlayer.Value - min;
                            double diff = judgePlayer.Value - allInDic[player.UserId];
                            if (diff > 0)
                            {
                                getChips -= diff;
                            }
                            backDic[judgePlayer.Key] -= getChips;
                            sideWinnerDic[player.UserId] += getChips;
                        }
                    }
                }                
            }
        }

        foreach (var win in sideWinnerDic)
        {
            Client player = playingList.Where(x => x.UserId == win.Key).FirstOrDefault();
            player.RoomChips += win.Value;
        }*/

        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCast_Request_SideReault;
        pack.SendModeCode = SendModeCode.RoomBroadcast;

        SidePack sidePack = new SidePack();
        sidePack.AllPlayerChips = GetAllPlayerChips();
        sidePack.BackChips = backDic;
        sidePack.SideWinnerDic = sideWinnerDic;
        sidePack.TotalSideChips = GetSideChipsValue();

        pack.SidePack = sidePack;
        SendRoomBroadCast(pack);

        float winnerTime = sidewinnerList.Count() * 1.5f;

        yield return new WaitForSeconds(3 + winnerTime);
        yield return SendRequest_NextGameStage();
    }

    /// <summary>
    /// 測試玩家顯示棄牌手牌
    /// </summary>
    /// <param name="index">手牌編號(0/1)</param>
    /// <param testIndex="index">測試玩家編號</param>
    public void TestShowFoldPoker(int index, int testIndex)
    {
        if (playingList[testIndex].State == PlayerStateEnum.Fold && gameRoomData.CurrFlow == GameFlowEnum.PotResult)
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.Request_ShowFoldPoker;

            ShowFoldPokerPack showFoldPokerPack = new ShowFoldPokerPack();
            showFoldPokerPack.HandPokerIndex = index;
            showFoldPokerPack.UserID = testIndex == 0 ? "Player0" : "Player1";

            pack.ShowFoldPokerPack = showFoldPokerPack;
            Request_ShowFoldPoker(pack);            
        }
        else
        {
            Debug.LogError("Test Player Not Flod");
        }
    }

    /// <summary>
    /// 請求_顯示棄牌撲克
    /// </summary>
    /// <param name="pack"></param>
    public void Request_ShowFoldPoker(MainPack pack)
    {
        string id = pack.ShowFoldPokerPack.UserID;
        int showPokerIndex = pack.ShowFoldPokerPack.HandPokerIndex;

        Client client = clientList.Where(x => x.UserId == id).FirstOrDefault();

        pack.ShowFoldPokerPack.PokerNum = showPokerIndex == 0 ? client.HandPoker0 : client.HandPoker1;
        SendRoomBroadCast(pack);

        int pokerNum = pack.ShowFoldPokerPack.PokerNum;
        Debug.Log($"{client.UserId}:Show Fold Poker {pokerName[pokerNum / 13]} {pokerNum % 13 + 1}");
    }

    /// <summary>
    /// 判斷結果
    /// </summary>
    /// <param name="judgePlayerList">判斷玩家</param>
    /// <returns></returns>
    private List<Client> JudgeWinner(List<Client> judgePlayerList)
    {
        if(judgePlayerList.Count == 0)
        {
            return new List<Client>();
        }

        //判斷結果(牌型結果,符合的牌)
        Dictionary<Client, (int, List<int>)> shapeDic = new Dictionary<Client, (int, List<int>)>();
        //玩家的牌(牌型結果,(公牌+手牌))
        Dictionary<Client, (int, List<int>)> clientPokerDic = new Dictionary<Client, (int, List<int>)>();

        foreach (var player in judgePlayerList)
        {
            List<int> judgePoker = new List<int>();
            judgePoker.Add(player.HandPoker0);
            judgePoker.Add(player.HandPoker1);
            judgePoker = judgePoker.Concat(gameRoomData.CommunityPoker).ToList();

            //判定牌型
            PokerShape.JudgePokerShape(judgePoker, (result, matchPoker) =>
            {
                shapeDic.Add(player, (result, matchPoker));
                clientPokerDic.Add(player, (result, judgePoker));
            });
        }

        //最大的牌型结果
        int maxResult = shapeDic.Values.Min(x => x.Item1);
        //最大牌型人數
        int matchCount = shapeDic.Values.Count(x => x.Item1 == maxResult);

        if (matchCount == 1)
        {
            //最大結果1人
            KeyValuePair<Client, (int, List<int>)> minItem = shapeDic.FirstOrDefault(x => x.Value.Item1 == maxResult);
            return new List<Client>() { minItem.Key };
        }
        else
        {
            //最大結果玩家(符合的牌)
            Dictionary<Client, List<int>> pairPlayer = new Dictionary<Client, List<int>>();

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
            List<Client> maxResultPlayersList = new List<Client>();
            int maxValue = int.MinValue;
            foreach (var pair in pairPlayer)
            {
                int max = pair.Value.Max();
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
                List<Client> handPokerList = new List<Client>();

                if (maxResultPlayersList.Count() > 1)
                {
                    //符合最大結果有多人
                    handPokerList = new List<Client>(maxResultPlayersList);
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
                foreach (var client in handPokerList)
                {
                    if (client.HandPoker0 % 13 > 0 && client.HandPoker0 % 13 < client.HandPoker1 % 13)
                    {
                        int temp = client.HandPoker0;
                        client.HandPoker0 = client.HandPoker1;
                        client.HandPoker1 = temp;
                    }
                }

                //最大手牌1玩家(不包含符合結果牌)
                Client maxHandPoker0Client = handPokerList.OrderByDescending(x => (x.HandPoker0 % 13 == 0 ? int.MinValue : x.HandPoker0 % 13) + 1)
                                                          .FirstOrDefault();

                List<Client> maxHandPokerClientList = new List<Client>();
                if (maxHandPoker0Client != null)
                {
                    //符合牌不在手牌1
                    maxHandPokerClientList = handPokerList.Where(x => x.HandPoker0 % 13 == maxHandPoker0Client.HandPoker0 % 13).ToList();
                }

                //最大手牌1玩家1人
                if (maxHandPokerClientList.Count() == 1)
                {
                    return maxHandPokerClientList;
                }
                else
                {
                    //比較手牌2(不包含符合結果牌)
                    Client maxHandPoker1Client = handPokerList.OrderByDescending(x => (x.HandPoker1 % 13 == 0 ? int.MinValue : x.HandPoker1 % 13) + 1)
                                                              .FirstOrDefault();

                    //符合牌都在手牌
                    if (maxHandPoker1Client == null)
                    {
                        return handPokerList;
                    }
                    
                    //最大手牌2所有玩家
                    List<Client> maxHandPoker1ClientList = handPokerList.Where(x => x.HandPoker1 % 13 == maxHandPoker1Client.HandPoker1 % 13).ToList();
                    return maxHandPoker1ClientList;
                }
            }
        }
    }

    /// <summary>
    /// 請求_購買籌碼
    /// </summary>
    /// <param name="pack"></param>
    public void Request_BuyChips(MainPack pack)
    {
        string id = pack.BuyChipsPack.UserId;
        double buyChips = pack.BuyChipsPack.BuyChipsValue;

        Client c = clientList.Where(x => x.UserId == id).FirstOrDefault();
        c.CurrRoomChips += buyChips;

        pack.BuyChipsPack.BuyChipsValue = c.CurrRoomChips;
        SendRequest(pack);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
