using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class LobbyRankingView : MonoBehaviour
{
    [SerializeField]
    GameObject RankSampleObj;
    [SerializeField]
    RankSample SelfRankSample;
    [SerializeField]
    Transform RankContent;
    [SerializeField]
    Toggle daily_Btn, weekly_Btn;
    [SerializeField]
    ScrollRect Rank_Sv;
    ObjPool objPool;
    bool isDaily = true;

    private void Awake()
    {
        ListenerEvent();

        objPool = new ObjPool(transform, 50);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        daily_Btn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                if (!isDaily)
                {
                    Rank_Sv.verticalNormalizedPosition = 1;
                    AppApi.GetTopPlayers("daily", SetRank);
                    isDaily = true;
                }
            }
        });
        weekly_Btn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                if (isDaily)
                {
                    Rank_Sv.verticalNormalizedPosition = 1;
                    AppApi.GetTopPlayers("weekly", SetRank);
                    isDaily = false;
                }
            }
        });
    }

    private void OnEnable()
    {
        RankSampleObj.SetActive(false);

        AppApi.GetTopPlayers("daily", SetRank);
    }
    /// <summary>
    /// 設置排名
    /// </summary>
    public void SetRank(string data)
    {
        foreach(Transform item in RankContent.GetComponentInChildren<Transform>())
        {
            item.gameObject.SetActive(false);
        }
        SelfRankSample.SetRankData(new RankData()
        {
            avatar = DataManager.UserAvatarIndex,
            nickname = DataManager.UserNickname,
            point = 0,
            status = DataManager.UserStatus,
            rank = "#"
        }, "#");
        List<Ranking> ranking = JsonConvert.DeserializeObject<List<Ranking>>(data) ?? new List<Ranking>();
        int rankNum = 1;
        foreach(Ranking rank in ranking)
        {
            if (!rank.isCurrentPlayer)
            {
                RankSample ranlSample = objPool.CreateObj<RankSample>(RankSampleObj, RankContent);
                RankData rankData = new RankData();
                rankData.avatar = Random.Range(0, 8);
                rankData.status = false;
                rankData.rank = rankNum.ToString();
                rankData.nickname = rank.playerId;
                rankData.point = rank.totalWinnings;

                ranlSample.SetRankData(rankData, rankNum.ToString());
                rankNum++;
            }
            if(rank.isCurrentPlayer)
            {
                SelfRankSample.SetRankData(new RankData()
                {
                    avatar = DataManager.UserAvatarIndex,
                    nickname = DataManager.UserNickname,
                    point = (int)rank.totalWinnings,
                    status = DataManager.UserStatus,
                    rank = rank.rank
                }, rank.rank);
            }
        }
    }
}

public class Ranking
{
    public string playerId;
    public double totalWinnings;
    public string status;
    public bool isCurrentPlayer;
    public string rank;
}