using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class FirebaseManager : UnitySingleton<FirebaseManager>
{
    [Header("資料路徑名稱")]
    public const string USER_DATA_PATH = "users/";                                           //Database用戶資料路徑
    public const string ROOM_DATA_PATH = "room/";                                           //房間資料路徑

    [Header("用戶資料內容路徑名稱")]
    public const string USER_ID = "userId";                                                 //用戶ID
    public const string ONLINE = "online";                                                 //用戶ID
    public const string PHONE_NUMBER = "phoneNumber";                                       //登入手機號
    public const string PASSWORD = "password";                                              //登入密碼
    public const string NICKNAME = "nickname";                                              //暱稱
    public const string AVATAR_INDEX = "avatarIndex";                                       //頭像編號
    public const string INVITATION_CODE = "invitationCode";                                 //邀請碼
    public const string BOUND_INVITER_ID = "boundInviterId";                                //綁定的邀請者Id
    public const string LINE_TOKEN = "lineToken";                                           //Line Token
    public const string U_CHIPS = "UChips";                                                 //U幣
    public const string A_CHIPS = "AChips";                                                 //A幣
    public const string GOLD = "gold";                                                      //黃金

    [Header("遊戲房間資料內容路徑名稱")]
    public const string ROBOT_ID = "robot";                                                 //機器人ID
    public const string PLAYER_DATA_LIST = "playerDataDic";                                 //房間內所有玩家列表路徑
    public const string PLAYING_PLAYER_ID = "playingPlayersIdList";                         //遊戲中玩家ID列表路徑
    public const string BET_ACTION_DATA = "betActionDataDic";                               //下注行為資料路徑
    public const string POT_WIN_DATA = "potWinData";                                        //底池獲勝資料路徑
    public const string SIDE_WIN_DATA = "sideWinData";                                      //邊池獲勝資料路徑
    public const string BACK_CHIPS_DATA = "backChipsData";                                  //退回籌碼資料路徑
    public const string CHAT_DATA = "chatData";                                             //聊天資料路徑
    public const string SMALL_BLIND = "smallBlind";                                         //小盲值
    public const string ROBOT_INDEX = "robotIndex";                                         //機器人編號
    public const string ROOM_HOST_ID = "hostId";                                            //房主ID
    public const string POT_CHIPS = "potChips";                                             //底池總籌碼
    public const string CURR_COMMUNITY_POKER = "currCommunityPoker";                        //當前顯示公共牌
    public const string COMMUNITY_POKER = "communityPoker";                                 //公共牌
    public const string BUTTON_SEAT = "buttonSeat";                                         //Button座位
    public const string ACTION_CD = "actionCD";                                             //行動倒數時間
    public const string CURR_ACTIONER_ID = "currActionerId";                                //當前行動玩家ID
    public const string CURR_ACTIONER_SEAT = "currActionerSeat";                            //當前行動座位
    public const string CURR_CALL_VALUE = "currCallValue";                                  //當前跟注值
    public const string ACTIONP_PLAYER_COUNT = "actionPlayerCount";                         //當前流程行動玩家次數
    public const string GAME_START_TIME = "gameStartTime";                                  //遊戲開始時間
    public const string GAME_END_TIME = "gameEndTime";                                      //遊戲結束時間

    [Header("遊戲玩家資料路徑名稱")]
    public const string ROOM_NAME = "room_";                                                //房間名
    public const string CURR_GAME_FLOW = "currGameFlow";                                    //(GameFlowEnum)當前遊戲流程(發牌/盲注/翻牌/轉牌/河牌/遊戲結果(主池/邊池))
    public const string CARRY_CHIPS = "carryChips";                                         //攜帶籌碼
    public const string SEAT_CHARACTER = "seatCharacter";                                   //(SeatCharacterEnum)座位角色(Button/SB/BB)
    public const string GAME_SEAT = "gameSeat";                                             //遊戲座位
    public const string GAME_STATE = "gameState";                                           //遊戲狀態(等待/遊戲中/棄牌/All In)
    public const string HAND_POKER = "handPoker";                                           //手牌
    public const string CURR_ALL_BET_CHIPS = "currAllBetChips";                             //該回合總下注籌碼
    public const string ALL_BET_CHIPS = "allBetChips";                                      //該局總下注籌碼
    public const string IS_BET = "isBet";                                                   //該流程是否已下注
    public const string IS_SIT_OUT = "isSitOut";                                            //是否保留座位離開
    public const string SHOW_HAND_POKER = "showHandPoker";                                  //棄牌後顯示手牌

    [Header("下注行為")]
    public const string BET_ACTIONER_ID = "betActionerId";                                  //下注玩家ID
    public const string BET_ACTION = "betAction";                                           //(BetActingEnum)下注行為
    public const string BET_ACTION_VALUE = "betActionValue";                                //下注籌碼值
    public const string UPDATE_CARRY_CHIPS = "updateCarryChips";                            //更新後的攜帶籌碼

    [Header("底池獲勝資料")]
    public const string POT_WIN_CHIPS = "potWinChips";                                      //底池獲得籌碼
    public const string POT_WINNERS_ID = "potWinnersId";                                    //底池贏家ID
    public const string IS_HAVE_SIDE = "isHaveSide";                                        //是否有邊池

    [Header("邊池獲勝資料")]
    public const string SIDE_WIN_CHIPS = "sideWinChips";                                    //邊池獲得籌碼
    public const string SIDE_WINNERS_ID = "sideWinnersId";                                  //邊池贏家ID
    public const string BACK_USER_ID = "backUserId";                                        //退回籌碼用戶ID
    public const string BACK_CHIPS_VALUE = "backChipsValue";                                //退回籌碼值

    [Header("積分房")]
    public const string INTEGRAL_ROOM = "integralRoom";                                     //積分房房間路徑
    public const string INTEGRAL_WAIT_DATA = "integralWaitData";                            //積分房等待資料路徑
    public const string PAIR_ROOM_NAME = "pairRoomName";                                    //配對成功房間名稱
    public const string PAIRED = "paired";                                                  //是否已被選上配對

    [Header("聊天")]
    public const string CHAT_MSG = "chatMsg";                                               //聊天訊息

    public override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// 讀取資料回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public T OnFirebaseDataRead<T>(string jsonData) where T : class
    {
        var data = JsonConvert.DeserializeObject<T>(jsonData);

        if (data == null)
        {
            return default;
        }
        else
        {
            return data;
        }
    }

    /// <summary>
    /// 移除資料
    /// </summary>
    [Serializable]
    public class RemoveData
    {
        public bool success;
        public string error;
    }
    /// <summary>
    /// 移除資料回傳
    /// </summary>
    /// <param name="jsonData"></param>
    public void OnRemoveDataCallback(string jsonData)
    {
        var data = JsonUtility.FromJson<RemoveData>(jsonData);
    }
}

#region 積分

/// <summary>
/// 積分等待資料
/// </summary>
[SerializeField]
public class IntegralTable
{
    public Dictionary<string, IntefralWaitUserData> integralWaitData;           //積分等待資料
}

/// <summary>
/// 積分等待用戶資料
/// </summary>
[SerializeField]
public class IntefralWaitUserData
{
    public string userId;                        //等待玩家ID
    public string pairRoomName;                  //配對成功房間名稱
    public bool paired;                          //是否已被選上配對
}

#endregion

#region 用戶

/// <summary>
/// 用戶資料查詢
/// </summary>
[Serializable]
public class CheckUserData
{
    public string exists;                   //差尋結果(true/false)
    public string phoneNumber;              //資料符合的用戶手機號
    public string error;                    //錯誤訊息
}

/// <summary>
/// 用戶資料
/// </summary>
[Serializable]
public class AccountData
{
    public bool online;                     //在線狀態
    public long last_changed;               //最後更新時間
    public string userId;                   //用戶ID
    public string phoneNumber;              //登入手機號
    public string password;                 //登入密碼
    public string nickname;                 //暱稱
    public int avatarIndex;                 //頭像編號
    public string invitationCode;           //邀請碼
    public string boundInviterId;           //綁定的邀請者Id
    public string lineToken;                //Line Token
    public double UChips;                   //U幣
    public double AChips;                   //A幣
    public double gold;                     //黃金
}

#endregion

#region 遊戲回傳資料

/// <summary>
/// 加入房間查詢資料
/// </summary>
[SerializeField]
public class QueryRoom
{
    public string error;
    public string getRoomName;               //查詢到的房間(沒有=false)
    public int roomCount;                    //該房間類型的房間數量
}

/// <summary>
/// 遊戲房間資料
/// </summary>
[SerializeField]
public class GameRoomData
{
    public Dictionary<string, GameRoomPlayerData> playerDataDic;        //房間內玩家資料
    public List<string> playingPlayersIdList;                           //遊戲中玩家ID列表
    public BetActionData betActionDataDic;                              //下注行為資料
    public PotWinData potWinData;                                       //底池獲勝資料
    public SideWinData sideWinData;                                     //邊池獲勝資料
    public ChatData chatData;                                           //聊天資料
    public int currGameFlow;                                            //(GameFlowEnum)當前遊戲流程(發牌/盲注/翻牌/轉牌/河牌/遊戲結果(主池/邊池))
    public double smallBlind;                                           //小盲值
    public string hostId;                                               //房主ID
    public double potChips;                                             //底池總籌碼
    public List<int> communityPoker;                                    //公共牌
    public List<int> currCommunityPoker;                                //當前顯示公共牌
    public int buttonSeat;                                              //Button座位
    public int robotIndex;                                              //機器人編號
    public int actionCD;                                                //行動倒數時間
    public string currActionerId;                                       //當前行動玩家Id
    public int currActionerSeat;                                        //前行動座位
    public double currCallValue;                                        //當前跟注值
    public int actionPlayerCount;                                       //當前流程行動玩家次數
    public string gameStartTime;                                        //遊戲開始時間
    public string gameEndTime;                                          //遊戲結束時間
}

/// <summary>
/// 遊戲玩家資料
/// </summary>
[SerializeField]
public class GameRoomPlayerData
{
    public bool online;                             //在線狀態
    public long last_changed;                       //最後更新時間
    public string userId;                           //用戶ID
    public string nickname;                         //暱稱
    public int avatarIndex;                         //頭像編號
    public double carryChips;                       //攜帶籌碼
    public int gameSeat;                            //遊戲座位
    public int seatCharacter;                       //(SeatCharacterEnum)座位角色(Button/SB/BB)
    public int gameState;                           //(PlayerStateEnum)遊戲狀態(等待/遊戲中/棄牌/All In)
    public List<int> handPoker;                     //手牌
    public double currAllBetChips;                  //當前流程總下注籌碼
    public double allBetChips;                      //該局總下注籌碼
    public bool isBet;                              //該流程是否已下注
    public bool isSitOut;                           //是否保留座位離開
    public List<int> showHandPoker;                 //棄牌後顯示手牌
}

/// <summary>
/// 下注行為資料
/// </summary>
[SerializeField]
public class BetActionData
{
    public string betActionerId;       //行動玩家ID
    public int betAction;              //(BetActingEnum)下注行為
    public double betActionValue;      //下注籌碼值
    public double updateCarryChips;    //更新後的攜帶籌碼
}

/// <summary>
/// 底池獲勝資料
/// </summary>
[SerializeField]
public class PotWinData
{
    public double potWinChips;                                          //底池獲得籌碼
    public List<string> potWinnersId;                                   //底池贏家ID
    public bool isHaveSide;                                             //是否有邊池
}

/// <summary>
/// 邊池獲勝資料
/// </summary>
[SerializeField]
public class SideWinData
{
    public double sideWinChips;                                         //邊池獲得籌碼
    public List<string> sideWinnersId;                                  //邊池贏家ID
    public Dictionary<string, BackChipsData> backChipsData;             //退回籌碼
}

/// <summary>
/// 退回籌碼資料
/// </summary>
public class BackChipsData
{
    public string backUserId;                                           //用戶ID
    public double backChipsValue;                                       //退回籌碼值
}

/// <summary>
/// 聊天資料
/// </summary>
public class ChatData
{
    public string userId;                       //用戶ID
    public string nickname;                     //暱稱
    public string chatMsg;                      //聊天訊息
    public int avatarIndex;                     //頭像編號
}

#endregion

#region 遊戲資料

/// <summary>
/// 遊戲座位角色
/// </summary>
public enum SeatCharacterEnum
{
    None,
    Button,
    SB,
    BB,
}

/// <summary>
/// 玩家遊戲狀態
/// </summary>
public enum PlayerStateEnum
{
    Waiting,        //等待下局
    Playing,        //遊戲中
    AllIn,          //All In狀態
    Fold,           //棄牌
}

/// <summary>
/// 遊戲流程
/// </summary>
public enum GameFlowEnum
{
    None,
    Licensing,                  //發牌
    SetBlind,                   //大小盲
    Flop,                       //翻牌
    Turn,                       //轉牌
    River,                      //河牌
    PotResult,                  //底池結果
    SideResult,                 //邊池結果
    OnePlayerLeftResult,        //剩餘1名玩家結果
}

/// <summary>
/// 玩家下注行為
/// </summary>
public enum BetActingEnum
{
    None,
    Blind,      //大小盲
    Fold,       //棄牌
    Check,      //過牌
    Raise,      //加注
    Bet,        //下注
    Call,       //跟注
    AllIn,      //All In
}

#endregion
