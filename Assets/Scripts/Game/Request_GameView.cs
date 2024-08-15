using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RequestBuf;

public class Request_GameView : BaseRequest
{
    [SerializeField]
    GameView thisView;

    [Header("Demo")]
    [SerializeField]
    DemoControl demoControl;

    private bool isStartReceiveRequest { get; set; }     //是否開始接收協議

    public override void Awake()
    {
        requestDic = new List<ActionCode>()
        {
            ActionCode.Request_UpdateRoomInfo,
            ActionCode.Request_InsufficientChips,
            ActionCode.Request_BuyChips,
        };

        roomBroadcastDic = new List<ActionCode>()
        {
            ActionCode.Request_PlayerInOutRoom,
            ActionCode.BroadCastRequest_GameStage,
            ActionCode.BroadCastRequest_PlayerActingRound,
            ActionCode.BroadCastRequest_ActingCD,
            ActionCode.BroadCastRequest_ShowActing,
            ActionCode.BroadCast_Request_SideReault,
            ActionCode.Request_ShowFoldPoker,
            ActionCode.BroadCastRequest_BattleResult,
        };

        base.Awake();
    }

    private void OnEnable()
    {
        isStartReceiveRequest = false;       
    }

    public override void HandleRequest(MainPack pack)
    {
        if (thisView.gameObject.activeSelf == false) return;

        switch (pack.ActionCode)
        {
            //更新房間訊息
            case ActionCode.Request_UpdateRoomInfo:
                isStartReceiveRequest = true;
               // thisView.UpdateGameRoomInfo(pack);
                break;

            //本地玩家行動回合
            case ActionCode.BroadCastRequest_PlayerActingRound:
                //thisView.LocalPlayerRound(pack.PlayerActingRoundPack);
                break;

            //籌碼不足
            case ActionCode.Request_InsufficientChips:
               // thisView.OnInsufficientChips(pack);
                break;

            //購買籌碼
            case ActionCode.Request_BuyChips:
                //thisView.BuyChipsGoBack(pack);
                break;
        }
    }

    public override void HandleRoomBroadcast(MainPack pack)
    {
       /* if (isStartReceiveRequest == false || thisView.gameObject.activeSelf == false) return;

        switch (pack.ActionCode)
        {
            //聊天
            case ActionCode.BroadCastRequest_Chat:
                thisView.ReciveChat(pack);
                break;

            //玩家進出房間
            case ActionCode.Request_PlayerInOutRoom:
                if (pack.PlayerInOutRoomPack.IsInRoom)
                {
                    //新玩家進入
                    //thisView.AddPlayer(pack.PlayerInOutRoomPack.PlayerInfoPack);
                }
                else
                {
                    //玩家退出
                    //thisView.PlayerExitRoom(pack.PlayerInOutRoomPack.PlayerInfoPack.UserID);
                }
                break;

            //遊戲階段
            case ActionCode.BroadCastRequest_GameStage:
                demoControl.IsShowDemoControl(false);
                thisView.AutoActionState = GameView.AutoActingEnum.None;
                StartCoroutine(thisView.IGameStage(pack));
                break;

            //玩家行動倒數
            case ActionCode.BroadCastRequest_ActingCD:
                demoControl.IsShowDemoControl(pack.ActingCDPack.ActingPlayerId != Entry.TestInfoData.LocalUserId);
                //行動框
                GamePlayerInfo player = thisView.GetPlayer(pack.ActingCDPack.ActingPlayerId);
                player.ActionFrame = true;
                player.StartCountDown(pack.ActingCDPack.TotalCDTime,
                                      pack.ActingCDPack.CD);
                break;

            //演示玩家行動
            case ActionCode.BroadCastRequest_ShowActing:
                thisView.SetTotalPot = pack.GameRoomInfoPack.TotalPot;
                thisView.GetPlayerAction(pack.PlayerActedPack);
                break;

            //邊池結果
            case ActionCode.BroadCast_Request_SideReault:
                StartCoroutine(thisView.SideResult(pack));
                break;

            //顯示手牌
            case ActionCode.Request_ShowFoldPoker:
                thisView.GetShowFoldPoker(pack);
                break;

            //積分結果
            case ActionCode.BroadCastRequest_BattleResult:
                thisView.SetBattleResult(pack.BattleResultPack.FailPlayerId != Entry.TestInfoData.LocalUserId);
                break;
        }*/
    }

    /// <summary>
    /// 發送聊天訊息
    /// </summary>
    /// <param name="chatContent"></param>
    public void SendRequestRequest_Chat(string chatContent)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastRequest_Chat;

        ChatPack chatPack = new ChatPack();
        chatPack.Id = DataManager.UserId;
        chatPack.Nickname = DataManager.UserNickname;
        chatPack.Avatar = DataManager.UserAvatarIndex;
        chatPack.Content = chatContent;

        pack.ChatPack = chatPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 發送更新房間訊息
    /// </summary>
    public void SendRequest_UpdateRoomInfo()
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_UpdateRoomInfo;

        SendRequest(pack);
    }

    /// <summary>
    /// 發送玩家採取行動
    /// </summary>
    /// <param name="id">玩家ID</param>
    /// <param name="acting">採取行動</param>
    /// <param name="betValue">下注值</param>
    public void SendRequest_PlayerActed(string id, BetActingEnum acting, double betValue)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_PlayerActed;

        PlayerActedPack playerActedPack = new PlayerActedPack();
        playerActedPack.ActPlayerId = id;
        playerActedPack.ActingEnum = acting;
        playerActedPack.BetValue = betValue;

        pack.PlayerActedPack = playerActedPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 發送顯示棄牌手牌
    /// </summary>
    /// <param name="index"></param>
    public void SendShowFoldPoker(int index)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_ShowFoldPoker;

        ShowFoldPokerPack showFoldPokerPack = new ShowFoldPokerPack();
        showFoldPokerPack.HandPokerIndex = index;
        showFoldPokerPack.UserID = Entry.TestInfoData.LocalUserId;

        pack.ShowFoldPokerPack = showFoldPokerPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 發送購買籌碼
    /// </summary>
    /// <param name="id">ID</param>
    /// <param name="buyChipsValue">購買籌碼數量</param>
    public void SendRequest_BuyChips(string id, double buyChipsValue)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_BuyChips;

        BuyChipsPack buyChipsPack = new BuyChipsPack();
        buyChipsPack.UserId = id;
        buyChipsPack.BuyChipsValue = buyChipsValue;

        pack.BuyChipsPack = buyChipsPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 發送離/回座
    /// </summary>
    /// <param name="isSitOut">true=離座</param>
    public void SendRequest_SitOut(bool isSitOut)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.BroadCastSitOut;

        SitOutPack sitOutPack = new SitOutPack();
        sitOutPack.UserId = DataManager.UserId;
        sitOutPack.IsSitOut = isSitOut;

        pack.SitOutPack = sitOutPack;
        SendRequest(pack);
    }

    /// <summary>
    /// 離開房間
    /// </summary>
    /// <param name="id"></param>
    public void SendRequest_ExitRoom(string id)
    {
        MainPack pack = new MainPack();
        pack.ActionCode = ActionCode.Request_PlayerInOutRoom;

        PlayerInfoPack playerInfoPack = new PlayerInfoPack();
        playerInfoPack.UserID = id;

        PlayerInOutRoomPack playerInOutRoomPack = new PlayerInOutRoomPack();
        playerInOutRoomPack.IsInRoom = false;
        playerInOutRoomPack.PlayerInfoPack = playerInfoPack;

        pack.PlayerInOutRoomPack = playerInOutRoomPack;
        SendRequest(pack);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
