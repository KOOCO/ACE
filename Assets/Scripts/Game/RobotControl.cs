using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RobotControl : MonoBehaviour
{
    [SerializeField]
    GameControl gameControl;

    /// <summary>
    /// 機器人下注
    /// </summary>
    /// <param name="gameRoomData">房間資料</param>
    public void RobotBet(GameRoomData gameRoomData)
    {
        //機器人資料
        GameRoomPlayerData robotData = gameRoomData.playerDataDic.Where(x => x.Key == gameRoomData.currActionerId)
                                                                  .FirstOrDefault()
                                                                  .Value;
        
        /////// 以下AI邏輯 ///////////////


        BetActingEnum action = BetActingEnum.Fold;
        double betValue = 0;
        int foldRate = new System.Random().Next(0, 100);

        //是否只能All In
        bool isJustAllIn = robotData.carryChips <= gameRoomData.currCallValue;
        //首位加注玩家
        bool isFirst = gameRoomData.actionPlayerCount == 0;

        if (foldRate >= 0)
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
                    action = BetActingEnum.Check;
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

        //更新資料(機器人下注行為)
        gameControl.UpdateBetAction(gameRoomData.currActionerId,
                                    action,
                                    betValue);
    }
}
