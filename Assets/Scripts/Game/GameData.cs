using NBitcoin;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    //底池倍率
    public readonly float[] PotPercentRate = new float[]
    {
        33, 50, 80, 100,
    };

    //加註大盲倍率
    public readonly float[] PotBbRate = new float[]
    {
        2f, 3f, 4.0f, 4.0f,
    };

    public float PageMoveTime = 0.25f;                           //滑動頁面移動時間

    public int MaxChatCount = 50;                                //保留聊天最大訊息數
    public Vector2 InitPotPointPos { get; set; }                 //初始底池位置
    public List<GamePlayerInfo> gamePlayerInfoList = new();            //玩家資料
    public bool isOnFold { get; set; }                           //是否棄牌

    public bool isDealed { get; set; }                           //是否播放過發牌動畫
    public TableTypeEnum RoomType { get; set; }                  //房間類型

    public Dictionary<string, double> playerWinValueList = new Dictionary<string, double>();
    public ThisData thisData;

    public List<int> exitPlayerSeatList = new List<int>();              //玩家離開座位
    public GameInitHistoryData gameInitHistoryData;                     //遊戲初始資料紀錄
    public ProcessHistoryData processHistoryData;                       //遊戲過程資料紀錄
    public ResultHistoryData saveResultData;                            //遊戲結果資料紀錄

    public StrData strData;

    public Dictionary<int, string> betStringsE = new Dictionary<int, string>
    {
        {0, "Call"},
        {1, "Check"},
        {2, "Fold"},
        {3, "Fold / Check"},
        {4, "All In"},
        {5, ""}
    };
    public Dictionary<int, string> betStringsC = new Dictionary<int, string>
    {
        {0, "跟注"},
        {1, "過牌"},
        {2, "棄牌"},
        {3, "棄牌 / 過牌"},
        {4, "All In"},
        {5, ""}
    };
}


public class ThisData
{
    public GamePlayerInfo LocalGamePlayerInfo;         //本地玩家
    public int LocalPlayerSeat;                        //本地玩家座位
    public double LocalPlayerChips;                    //本地玩家籌碼
    public double LocalPlayerCurrBetValue;             //本地玩家當前下注值
    public double TotalPot;                            //當前底池
    public double CallDifference;                      //當前跟注差額
    public double CurrCallValue;                       //當前跟注值
    public double CurrRaiseValue;                      //當前加注值
    public double MinRaiseValue;                       //最小加注值
    public double SmallBlindValue;                     //小盲值
    public bool IsFirstRaisePlayer;                    //首位加注玩家
    public bool IsUnableRaise;                         //無法加注
    public bool IsPlaying;                             //有參與遊戲
    public bool IsSitOut;                              //是否離開座位
    public bool isLocalPlayerTurn;                     //本地玩家回合
    public bool isFold;                                //是否已棄牌
    public bool isCanCall;                             //是否可以跟注
    public List<int> CurrCommunityPoker;               //當前公共牌
    public List<string> PotWinnerList;                 //主池贏家
    public double PowWinChips;                         //主池贏得籌碼
    public List<string> SideWinnerList;                //邊池贏家
    public double SideWinChips;                        //邊池贏得籌碼
    public Dictionary<int, double> BackChipsDic;       //退回籌碼(座位,退回籌碼值)
}

public class StrData
{
    public string FoldStr { get; set; }
    public string CallStr { get; set; }
    public string CallValueStr { get; set; }
    public string RaiseStr { get; set; }
    public string RaiseValueStr { get; set; }
}
