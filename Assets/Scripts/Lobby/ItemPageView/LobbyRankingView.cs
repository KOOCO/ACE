using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LobbyRankingView : MonoBehaviour
{
    [SerializeField]
    Toggle Integral_Tog, Cash_Tog, Golden_Tog;
    [SerializeField]
    Button ChangeSeason_Btn, RefreshRank_Btn;
    [SerializeField]
    GameObject RankSampleObj;
    [SerializeField]
    Transform RankContent;
    [SerializeField]
    RankSample[] TopThreeData;
    [SerializeField]
    TextMeshProUGUI IntegralTog_Txt, CashTog_Txt, GoldenTog_Txt,
                    Season_Txt, ChangeSeasonBtn_Txt,
                    TimeLestTitle_Txt, TimeLest_Txt,PlayerTag_Txt,PointTag_Txt,AwardTag_Txt;

    [Header("本地玩家")]
    [SerializeField]
    TextMeshProUGUI LocalUserRank_Txt, LocalUserPoint_Txt, LocalUserAward_Txt;

    [Header("排行規則")]
    [SerializeField]
    GameObject Rule_Obj;
    [SerializeField]
    TextMeshProUGUI RuleTitle_Txt, RuleContent_Txt;
    [SerializeField]
    Button RuleMask_Btn, Rule_Btn, RuleClose_Btn;

    ObjPool objPool;

    /// <summary>
    /// 當前排名類型
    /// </summary>
    RankType currRankType;
    enum RankType
    {
        Integral,
        Cash,
        Golden,
    }

    /// <summary>
    /// 當前賽季類型
    /// </summary>
    SeasonType currSeasonType;
    enum SeasonType
    {
        Current,
        Previous,
    }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        IntegralTog_Txt.text = LanguageManager.Instance.GetText("INTEGRAL");
        CashTog_Txt.text = LanguageManager.Instance.GetText("CASH");
        GoldenTog_Txt.text = LanguageManager.Instance.GetText("GOLDEN");
        TimeLestTitle_Txt.text = $"{LanguageManager.Instance.GetText("Time Lest")}:";
        ChangeSeasonBtn_Txt.text = currSeasonType == SeasonType.Current ?
                                   LanguageManager.Instance.GetText("Previous") :
                                   LanguageManager.Instance.GetText("Current");
        PlayerTag_Txt.text = $"{LanguageManager.Instance.GetText("Player")}";
        PointTag_Txt.text = $"{LanguageManager.Instance.GetText("Rank")}";
        AwardTag_Txt.text = $"{LanguageManager.Instance.GetText("Award")}";
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        objPool = new ObjPool(transform, 50);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //顯示積分排行
        Integral_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                currRankType = RankType.Integral;
                SetRank();
            }
        });

        //顯示Cash排行
        Cash_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                currRankType = RankType.Cash;
                SetRank();
            }
        });

        //顯示Golden排行
        Golden_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                currRankType = RankType.Golden;
                SetRank();
            }
        });

        //更換賽季
        ChangeSeason_Btn.onClick.AddListener(() =>
        {
            if (DataManager.CurrRankSeason - 1 <= 0)
            {
                return;
            }

            currSeasonType = currSeasonType == SeasonType.Current ?
                             SeasonType.Previous :
                             SeasonType.Current;

            ChangeSeasonBtn_Txt.text = currSeasonType == SeasonType.Current ?
                                       LanguageManager.Instance.GetText("Previous") :
                                       LanguageManager.Instance.GetText("Current");

            Season_Txt.text = currSeasonType == SeasonType.Current ?
                              $"{LanguageManager.Instance.GetText("SEASON")} {DataManager.CurrRankSeason}" :
                              $"{LanguageManager.Instance.GetText("SEASON")} {DataManager.CurrRankSeason - 1}";

            SetRank();
        });

        //刷新排行
        RefreshRank_Btn.onClick.AddListener(() =>
        {
            DataManager.ReciveRankData();
            Debug.Log("Ranl Refresh!");
        });

        #region 排行規則

        //規則說明
        Rule_Btn.onClick.AddListener(() =>
        {
            Rule_Obj.SetActive(true);
            switch (currRankType)
            {
                case RankType.Integral:
                    RuleTitle_Txt.text = LanguageManager.Instance.GetText("INTEGRAL RULE");
                    RuleContent_Txt.text = LanguageManager.Instance.GetText("Rule");
                    break;

                case RankType.Cash:
                    RuleTitle_Txt.text = LanguageManager.Instance.GetText("INTEGRAL RULE");
                    RuleContent_Txt.text = LanguageManager.Instance.GetText("Rule");
                    break;

                case RankType.Golden:
                    RuleTitle_Txt.text = LanguageManager.Instance.GetText("INTEGRAL RULE");
                    RuleContent_Txt.text = LanguageManager.Instance.GetText("Rule");
                    break;
            }
        });

        //關閉規則
        RuleClose_Btn.onClick.AddListener(() =>
        {
            Rule_Obj.SetActive(false);
        });

        //規則遮罩按鈕
        RuleMask_Btn.onClick.AddListener(() =>
        {
            Rule_Obj.gameObject.SetActive(false);
        });

        #endregion
    }

    private void OnEnable()
    {
        Rule_Obj.gameObject.SetActive(false);
        Season_Txt.text = $"{LanguageManager.Instance.GetText("SEASON")} {DataManager.CurrRankSeason}";
        RankSampleObj.SetActive(false);
        currSeasonType = SeasonType.Current;
        currRankType = RankType.Integral;

        SetRank();
    }

    private void Update()
    {
        //賽季時間倒數
        if ((DataManager.RandEndDate - DateTime.Now).TotalSeconds > 0)
        {
            TimeSpan timeLest = DataManager.RandEndDate - DateTime.Now;
            TimeLest_Txt.text = $"{timeLest.Days}{LanguageManager.Instance.GetText("D")} : " +
                                $"{timeLest.Hours}{LanguageManager.Instance.GetText("H")} : " +
                                $"{timeLest.Minutes}{LanguageManager.Instance.GetText("M")}";
        }
    }

    /// <summary>
    /// 設置排名
    /// </summary>
    public void SetRank()
    {
        List<RankData> rankDatas = null;
        string pointStr = "";

        RankData localUserData = new RankData();

        switch (currRankType)
        {
            case RankType.Integral:
                pointStr = LanguageManager.Instance.GetText("Points");
                rankDatas = currSeasonType == SeasonType.Current ?
                            DataManager.CurrSeasonIntegralRankList :
                            DataManager.PreSeasonIntegralRankList;
                localUserData = currSeasonType == SeasonType.Current ?
                                DataManager.LocalUserRankData[0] :
                                DataManager.LocalUserRankData[3];
                break;

            case RankType.Cash:
                pointStr = LanguageManager.Instance.GetText("Cash");
                rankDatas = currSeasonType == SeasonType.Current ?
                            DataManager.CurrSeasonCashRankList :
                            DataManager.PreSeasonCashRankList;
                localUserData = currSeasonType == SeasonType.Current ?
                                DataManager.LocalUserRankData[1] :
                                DataManager.LocalUserRankData[4];
                break;

            case RankType.Golden:
                pointStr = LanguageManager.Instance.GetText("Golden");
                rankDatas = currSeasonType == SeasonType.Current ?
                            DataManager.CurrSeasonGoldenRankList :
                            DataManager.PreSeasonGoldenRankList;
                localUserData = currSeasonType == SeasonType.Current ?
                                DataManager.LocalUserRankData[2] :
                                DataManager.LocalUserRankData[5];
                break;
        }

        if (rankDatas == null || rankDatas.Count == 0)
        {
            return;
        }

        //關閉舊排名
        for (int i = 1; i < RankContent.childCount; i++)
        {
            RankContent.GetChild(i).gameObject.SetActive(false);
        }

        //前三名設置
        for (int i = 0; i < TopThreeData.Length; i++)
        {
            TopThreeData[i].SetRankData(rankDatas[i], i + 1, pointStr);
        }

        //排名4名以後設置
        for (int i = TopThreeData.Length; i < rankDatas.Count; i++)
        {
            RankSample ranlSample = objPool.CreateObj<RankSample>(RankSampleObj, RankContent);
            ranlSample.SetRankData(rankDatas[i], i + 1, pointStr);
        }

        #region 本地玩家排名資料

        LocalUserRank_Txt.text = localUserData.rank > 999 ?
                                 $"{LanguageManager.Instance.GetText("My Rank")}   #999+" :
                                 $"{LanguageManager.Instance.GetText("My Rank")}   #{localUserData.rank}";
        LocalUserPoint_Txt.text = $"{localUserData.point}";
        LocalUserAward_Txt.text = $"{localUserData.award}";

        #endregion
    }
}
