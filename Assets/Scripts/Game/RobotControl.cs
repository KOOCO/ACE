using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class RobotControl : MonoBehaviour
{
    [SerializeField]
    GameView gameView;
    [SerializeField]
    GameControl gameControl;
    [SerializeField]
    bool isTest;      //如果需要測試多機器人的話可以打開這個讓他們全部棄牌，加快輪轉時間

    /// <summary>
    /// 機器人下注
    /// </summary>
    /// <param name="gameRoomData">房間資料</param>
    public void RobotBet(GameRoomData gameRoomData, string backendCallback)
    {
        //機器人資料
        GameRoomPlayerData robotData = gameRoomData.playerDataDic.Where(x => x.Key == gameRoomData.currActionerId)
                                                                  .FirstOrDefault()
                                                                  .Value;

        //接收回傳資料
        var resData = JsonConvert.DeserializeObject<robotResponse>(backendCallback);
        var actionData = resData.data;

        /////// 以下AI邏輯 ///////////////


        BetActingEnum action = BetActingEnum.None;
        double betValue = 0;
        #region old Editor
        //int foldRate = isTest ? 0 : new System.Random().Next(0, 100);
        int foldRate = new System.Random().Next(0, 100);

        float randomWait = UnityEngine.Random.Range(3, 6);

        if (isTest)
        {
            //是否只能All In
            bool isJustAllIn = robotData.carryChips <= gameRoomData.currCallValue;
            //首位加注玩家
            bool isFirst = gameRoomData.actionPlayerCount == 0;

            if (foldRate > 0)
            {
                action = BetActingEnum.Call;

                if (isJustAllIn)
                {
                    action = BetActingEnum.AllIn;
                    betValue = robotData.carryChips;
                }
                else
                {
                    if (isFirst == true)
                    {
                        if (robotData.currAllBetChips == gameRoomData.currCallValue)
                        {
                            action = BetActingEnum.Check;
                        }
                        else
                        {
                            betValue = gameRoomData.currCallValue;
                        }
                    }
                    else
                    {
                        if (robotData.currAllBetChips == gameRoomData.currCallValue)
                        {
                            action = BetActingEnum.Check;
                        }
                        else
                        {
                            betValue = gameRoomData.currCallValue;
                        }
                    }
                }
            }
        }
        #endregion

        if (!isTest)
        {
            action = BetActingEnum.Call;
            bool isFirst = gameRoomData.actionPlayerCount == 0;
            bool isJustAllIn = robotData.carryChips <= gameRoomData.currCallValue;

            //跟舊版雷同，防止機器人是小盲時做出非法操作(強迫跟注)
            if (isJustAllIn && actionData.action != 5)
            {
                action = BetActingEnum.AllIn;
                betValue = robotData.carryChips;
            }
            else
            {
                if (isFirst == true)
                {
                    if (robotData.currAllBetChips == gameRoomData.currCallValue)
                    {
                        action = judgeAction(actionData.action);
                        if (actionData.raisePercentage != 0)
                        {
                            action = BetActingEnum.Raise;
                            double bb = gameRoomData.smallBlind * 2;
                            betValue = Mathf.Floor((float)(gameView.gameData.thisData.CurrRaiseValue + (bb * (1 + (double)actionData.raisePercentage / 100))));
                        }
                        else if (action == BetActingEnum.AllIn)
                            betValue = robotData.carryChips;
                    }
                    else
                    {
                        if (actionData.action != 5)
                            betValue = gameRoomData.currCallValue;
                        else
                            action = BetActingEnum.Fold;
                    }
                }
                else
                {
                    if (robotData.currAllBetChips == gameRoomData.currCallValue)
                    {
                        action = judgeAction(actionData.action);
                        if (actionData.raisePercentage != 0)
                        {
                            action = BetActingEnum.Raise;
                            double bb = gameRoomData.smallBlind * 2;
                            betValue = Mathf.Floor((float)(gameView.gameData.thisData.CurrRaiseValue + (bb * (1 + (double)actionData.raisePercentage / 100))));
                        }
                        else if (action == BetActingEnum.AllIn)
                            betValue = robotData.carryChips;
                    }
                    else
                    {
                        if (actionData.action != 5)
                            betValue = gameRoomData.currCallValue;
                        else
                            action = BetActingEnum.Fold;
                    }
                }
            }

            print("實際下注: " + action + " 金額: " + betValue);
        }
        //更新資料(機器人下注行為)
        gameControl.UpdateBetAction(gameRoomData.currActionerId,
                                    action,
                                    betValue);
    }

    public string handConvert(List<int> handPoker)
    {
        List<int> converted = handPoker.Select(card => card % 13 + 2).ToList();
        var cardSuits = handPoker.GroupBy(card => card / 13); // Group by suit
        List<string> tranS = new();
        bool suited = false;

        foreach (var suitGroup in cardSuits)
        {
            if (suitGroup.Count() > 1)
                suited = true;
            else
                suited = false;
        }

        converted.Sort((x, y) => -x.CompareTo(y));

        for (int i = 0; i < converted.Count; i++)
        {
            int index = i;
            if (converted[index] < 10)
                tranS.Add(converted[index].ToString());
            else
                tranS.Add(((pokerTrans)converted[index]).ToString());
        }

        if (suited)
            return string.Join("", tranS) + "s";
        else
            return string.Join("", tranS) + "o";
    }

    public BetActingEnum judgeAction(int i)
    {
        BetActingEnum action = BetActingEnum.None;

        switch (i)
        {
            case 1:
                action = BetActingEnum.AllIn;
                break;
            case 2:
                action = BetActingEnum.Bet;
                break;
            case 3:
                action = BetActingEnum.Call;
                break;
            case 4:
                action = BetActingEnum.Check;
                break;
            case 5:
                action = BetActingEnum.Fold;
                break;
        }
        return action;
    }
}
