using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// 紀錄結果資料
/// </summary>
public class ResultHistoryData
{
    public string RoomType;                         //房間類型
    public double SmallBlind;                       //小盲注
    public string NickName;                         //獲勝玩家暱稱
    public int Avatar;                              //獲勝玩家頭像
    public int[] HandPokers;                        //獲勝玩家手牌
    public List<int> CommunityPoker;                //公共牌
    public double WinChips;                         //贏得籌碼
}

/// <summary>
/// 遊戲初始紀錄資料
/// </summary>
public class GameInitHistoryData
{
    public int ButtonSeat;                              //Button座位
    public int SBSeat;                                  //小盲座位
    public int BBSeat;                                  //大盲座位
    public List<int> SeatList;                          //座位
    public List<string> UserIdList;                     //用戶ID
    public List<string> NicknameList;                   //暱稱
    public List<int> AvatarList;                        //頭像
    public List<int> HandPoker1List;                    //手牌1
    public List<int> HandPoker2List;                    //手牌2
    public List<double> InitChipsList;                  //初始籌碼
    public List<double> CurrBetChipsList;               //當前下注籌碼
    public double TotalPotChips;                        //底池籌碼
}

/// <summary>
/// 遊戲過程紀錄資料
/// </summary>
public class ProcessHistoryData
{
    public List<ProcessStepHistoryData> processStepHistoryDataList;
}
/// <summary>
/// 遊戲過程每次行動紀錄資料
/// </summary>
public class ProcessStepHistoryData
{
    public List<int> SeatList;                          //座位
    public List<double> ChipsList;                      //本次行動各玩家擁有籌碼
    public List<double> BetChipsList;                   //本次行動各玩家下注籌碼
    public int ActionPlayerIndex;                       //行動玩家編號
    public int ActionIndex;                             //ActingEnum Index
    public List<int> CommunityPoker;                    //公共牌
    public double TotalPot;                             //底池
    public List<int> HandPoker1;                        //手牌1
    public List<int> HandPoker2;                        //手牌2
    public List<int> BetActionEnumIndex;                //下注行為編號(BetActionEnum)

    public List<int> PotWinnerSeatList;                 //底池贏家位置
    public double PotWinChips;                          //底池獲勝籌碼
    public List<int> SildWinnerSeatList;                //邊池贏家位置
    public double SildWinChips;                         //邊池獲勝籌碼
    public Dictionary<int, double> BackChipsDic;        //退回籌碼(座位,退回籌碼值)

    public List<int> ExitPlayerSeatList;                //退出玩家座位
}

public class HandHistoryManager : UnitySingleton<HandHistoryManager>
{
    [Header("影片播放介面")]
    [SerializeField]
    GameObject HistoryVideoViewObj;

    string ResultHistoryPlayerPrefsKey;                     //遊戲結果紀錄Key
    string GameInitHistoryPlayerPrefsKey;                   //遊戲初始資料Key
    string ProcessHistoryPlayerPrefsKey;                    //遊戲過程資料Key

    List<ResultHistoryData> resultDataList;                 //遊戲結果紀錄
    List<GameInitHistoryData> gameInitHistoryDataList;      //遊戲初始資料紀錄
    List<ProcessHistoryData> processHistoryDataList;        //遊戲過程資料

    HistoryVideoView historyVideoView;

    public override void Awake()
    {
        ResultHistoryPlayerPrefsKey = $"AsiaPoker_ResultHistoryDataList_{DataManager.UserId}";
        GameInitHistoryPlayerPrefsKey = $"AsiaPoker_GameInifHistoryDataList_{DataManager.UserId}";
        ProcessHistoryPlayerPrefsKey = $"AsiaPoker_ProcessHistoryDataList_{DataManager.UserId}";

        base.Awake();
    }

    /// <summary>
    /// 讀取手牌紀錄資料
    /// </summary>
    public void LoadHandHistoryData()
    {
        LoadResultData();
        LoadGameInitData();
        LoadProcessData();

        Debug.Log("Loaded Hand History Data!");
    }

    /// <summary>
    /// 移除牌局歷史紀錄
    /// </summary>
    public void OnDeleteHistoryData()
    {
        PlayerPrefs.DeleteKey(ResultHistoryPlayerPrefsKey);
        PlayerPrefs.DeleteKey(GameInitHistoryPlayerPrefsKey);
        PlayerPrefs.DeleteKey(ProcessHistoryPlayerPrefsKey);
        PlayerPrefs.Save();
        resultDataList.Clear();
        gameInitHistoryDataList.Clear();
        processHistoryDataList.Clear();

        //更新存檔資料
        HandHistoryView handHistoryView = GameObject.FindAnyObjectByType<HandHistoryView>();
        handHistoryView?.UpdateHitoryDate();

        Debug.Log("Deleted Hand History Data!!!");
    }


    #region 遊戲結果存檔

    /// <summary>
    /// 獲取遊戲結果存檔
    /// </summary>
    /// <returns></returns>
    public List<ResultHistoryData> GetResultDataList()
    {
        return resultDataList;
    }

    /// <summary>
    /// 讀取結果存檔
    /// </summary>
    private void LoadResultData()
    {
        string json = PlayerPrefs.GetString(ResultHistoryPlayerPrefsKey, "[]");
        resultDataList = JsonConvert.DeserializeObject<List<ResultHistoryData>>(json) ?? new List<ResultHistoryData>();
    }

    /// <summary>
    /// 遊戲結果存檔
    /// </summary>
    /// <param name="newData"></param>
    public void SaveResult(ResultHistoryData newData)
    {
        if (resultDataList.Count >= DataManager.MaxVideoSaveCount)
        {
            //移除第一筆數據
            resultDataList.RemoveAt(0);
        }

        resultDataList.Add(newData);
        string json = JsonConvert.SerializeObject(resultDataList);
        PlayerPrefs.SetString(ResultHistoryPlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    #endregion

    #region 遊戲初始資料存檔

    /// <summary>
    /// 獲取遊戲初始資料存檔
    /// </summary>
    /// <returns></returns>
    public List<GameInitHistoryData> GetGameInitDataList()
    {
        return gameInitHistoryDataList;
    }

    /// <summary>
    /// 設置初始遊戲資料
    /// </summary>
    /// <param name="gamePlayerInfos"></param>
    /// <param name="totalPot"></param>
    /// <returns></returns>
    public GameInitHistoryData SetGameInitData(List<GamePlayerInfo> gamePlayerInfos, double totalPot)
    {
        GameInitHistoryData gameInitHistoryData = new GameInitHistoryData();
        gameInitHistoryData.SeatList = new List<int>();
        gameInitHistoryData.UserIdList = new List<string>();
        gameInitHistoryData.NicknameList = new List<string>();
        gameInitHistoryData.AvatarList = new List<int>();
        gameInitHistoryData.HandPoker1List = new List<int>();
        gameInitHistoryData.HandPoker2List = new List<int>();
        gameInitHistoryData.InitChipsList = new List<double>();
        gameInitHistoryData.CurrBetChipsList = new List<double>();
        foreach (var player in gamePlayerInfos)
        {
            if (player.SeatCharacter == SeatCharacterEnum.Button)
            {
                gameInitHistoryData.ButtonSeat = player.SeatIndex;
            }
            else if (player.SeatCharacter == SeatCharacterEnum.SB)
            {
                gameInitHistoryData.SBSeat = player.SeatIndex;
            }
            else if (player.SeatCharacter == SeatCharacterEnum.BB)
            {
                gameInitHistoryData.BBSeat = player.SeatIndex;
            }

            gameInitHistoryData.SeatList.Add(player.SeatIndex);
            gameInitHistoryData.UserIdList.Add(player.UserId);
            gameInitHistoryData.NicknameList.Add(player.Nickname);
            gameInitHistoryData.AvatarList.Add(player.Avatar);
            gameInitHistoryData.HandPoker1List.Add(player.GetHandPoker[0].PokerNum);
            gameInitHistoryData.HandPoker2List.Add(player.GetHandPoker[1].PokerNum);
            gameInitHistoryData.InitChipsList.Add(player.CurrRoomChips);
            gameInitHistoryData.CurrBetChipsList.Add(player.CurrBetValue);
        }

        gameInitHistoryData.TotalPotChips = totalPot;

        return gameInitHistoryData;
    }

    /// <summary>
    /// 讀取遊戲初始資料存檔
    /// </summary>
    private void LoadGameInitData()
    {
        string json = PlayerPrefs.GetString(GameInitHistoryPlayerPrefsKey, "[]");
        gameInitHistoryDataList = JsonConvert.DeserializeObject<List<GameInitHistoryData>>(json) ?? new List<GameInitHistoryData>();
    }

    /// <summary>
    /// 遊戲初始資料存檔
    /// </summary>
    /// <param name="newData"></param>
    public void SaveGameInit(GameInitHistoryData newData)
    {
        if (gameInitHistoryDataList.Count >= DataManager.MaxVideoSaveCount)
        {
            //移除第一筆數據
            gameInitHistoryDataList.RemoveAt(0);
        }

        gameInitHistoryDataList.Add(newData);
        string json = JsonConvert.SerializeObject(gameInitHistoryDataList);
        PlayerPrefs.SetString(GameInitHistoryPlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    #endregion

    #region 遊戲過程存檔

    /// <summary>
    /// 獲取遊戲過程存檔
    /// </summary>
    /// <returns></returns>
    public List<ProcessHistoryData> GetProcessDataList()
    {
        return processHistoryDataList;
    }

    /// <summary>
    /// 讀取遊戲過程存檔
    /// </summary>
    private void LoadProcessData()
    {
        string json = PlayerPrefs.GetString(ProcessHistoryPlayerPrefsKey, "[]");
        processHistoryDataList = JsonConvert.DeserializeObject<List<ProcessHistoryData>>(json) ?? new List<ProcessHistoryData>();
    }

    /// <summary>
    /// 遊戲過程存檔
    /// </summary>
    /// <param name="newData"></param>
    public void SaveProcess(ProcessHistoryData newData)
    {
        if (processHistoryDataList.Count >= DataManager.MaxVideoSaveCount)
        {
            //移除第一筆數據
            processHistoryDataList.RemoveAt(0);
        }

        processHistoryDataList.Add(newData);
        string json = JsonConvert.SerializeObject(processHistoryDataList);
        PlayerPrefs.SetString(ProcessHistoryPlayerPrefsKey, json);
        PlayerPrefs.Save();

        Debug.Log("Video Data Saved!!!");
    }

    #endregion

    #region 影片播放控制

    /// <summary>
    /// 播放紀錄影片
    /// </summary>
    /// <param name="index"></param>
    public void PlayVideo(int index)
    {
        bool isInLobby = !GameRoomManager.Instance.IsShowGameRoom;
        Transform parent = isInLobby ?
                           ViewManager.Instance.GetCanvas().transform :
                           GameRoomManager.Instance.GetGameRoomCanvas().transform;

        if (historyVideoView != null)
        {
            Destroy(historyVideoView.gameObject);
            historyVideoView = null;
        }

        historyVideoView = Instantiate(HistoryVideoViewObj, parent).GetComponent<HistoryVideoView>();
        historyVideoView.transform.SetSiblingIndex(parent.childCount + 1);
        historyVideoView.IsShowHandHistoryView = isInLobby;
        historyVideoView.SetInit(index);
        historyVideoView.SwitchVideo();
    }

    #endregion
}

