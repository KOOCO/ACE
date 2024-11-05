using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class HistoryVideoView : MonoBehaviour
{
    [Header("遊戲畫面")]
    [SerializeField]
    Button Close_Btn, Pre_Btn, Play_Btn, Next_Btn, NextVideo_Btn, PreVideo_Btn;
    [SerializeField]
    GamePlayerInfo[] Players;                                                               //所有玩家(0=本地玩家)
    [SerializeField]
    Poker[] CommunityPokser;
    [SerializeField]
    TextMeshProUGUI HandNum_Txt, TotalPot_Txt, WinType_Txt;
    [SerializeField]
    Image PlayBtn_Img;

    [Header("紀錄列表")]
    [SerializeField]
    GameObject HandHistoryViewObj;
    [SerializeField]
    Image Bg_Obj;
    [SerializeField]
    RectTransform HandHistoryViewParent, HistorySample;

    int dataIndex;                                                                          //讀取資料Index
    int currPlayIndex;                                                                      //當前播放Index
    ProcessHistoryData currProcessHistoryData;                                              //當前紀錄資料
    ProcessStepHistoryData currProcessStepHistoryData;                                      //當前行動紀錄資料

    float switchVideoMoveDictance;                                                          //切換影片列表移動距離
    GameView gameView;

    Coroutine playVideoWinCoroutine;                                                        //影片播放獲勝Coroutine

    /// <summary>
    /// 當前播放狀態
    /// </summary>
    PlayState currPlayState;
    enum PlayState
    {
        Pause,          //暫停
        Playing,        //播放中
        Retuen,         //重新播放
    }

    private void Awake()
    {
        gameView = GameObject.Find("GameView").GetComponent<GameView>();
        ListenerEvent();

        //切換影片列表移動距離
        VerticalLayoutGroup verticalLayout = HandHistoryViewParent.GetComponent<VerticalLayoutGroup>();
        float sampleHeight = HistorySample.rect.height;
        switchVideoMoveDictance = verticalLayout.spacing + sampleHeight;
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //關閉
        Close_Btn.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        //下一部紀錄
        NextVideo_Btn.onClick.AddListener(() =>
        {
            dataIndex++;
            SwitchVideo();
        });

        //前一部紀錄
        PreVideo_Btn.onClick.AddListener(() =>
        {
            dataIndex--;
            SwitchVideo();
        });

        //下一段
        Next_Btn.onClick.AddListener(() =>
        {
            PausePlay();
            PlayNext();
            BtnDisplayControl();
        });

        //前一段
        Pre_Btn.onClick.AddListener(() =>
        {
            PausePlay();
            currPlayIndex--;

            if (currPlayIndex < 0)
            {
                SetInit(dataIndex);
            }
            else
            {
                currProcessStepHistoryData = currProcessHistoryData.processStepHistoryDataList[currPlayIndex];
                StartCoroutine(IPlayRecode(currProcessStepHistoryData));
            }

            BtnDisplayControl();
        });

        //播放/暫停/重新撥放
        Play_Btn.onClick.AddListener(() =>
        {
            //當前播放狀態
            switch (currPlayState)
            {
                //暫停
                case PlayState.Pause:
                    currPlayState = PlayState.Playing;
                    StartPlay();
                    break;

                //撥放中
                case PlayState.Playing:
                    currPlayState = PlayState.Pause;
                    PausePlay();
                    break;

                //重新撥放
                case PlayState.Retuen:
                    currPlayState = PlayState.Playing;
                    SetInit(dataIndex);
                    StartPlay();
                    break;
            }

            BtnDisplayControl();
        });
    }

    #region 按鈕與顯示控制

    /// <summary>
    /// 是否顯示手牌紀錄列表
    /// </summary>
    public bool IsShowHandHistoryView
    {
        set
        {
            Bg_Obj.enabled = value;
            HandHistoryViewObj.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// 按鈕顯示控制
    /// </summary>
    private void BtnDisplayControl()
    {
        Pre_Btn.interactable = currPlayIndex >= 0;
        Next_Btn.interactable = currPlayIndex < currProcessHistoryData.processStepHistoryDataList.Count - 1;

        //當前播放狀態
        switch (currPlayState)
        {
            //暫停
            case PlayState.Pause:
                PlayBtn_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.VideoAlbum).album[0];
                break;

            //撥放中
            case PlayState.Playing:
                PlayBtn_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.VideoAlbum).album[1];
                break;

            //重新撥放
            case PlayState.Retuen:
                PlayBtn_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.VideoAlbum).album[2];
                break;
        }
    }

    /// <summary>
    /// 開始播放紀錄
    /// </summary>
    private void StartPlay()
    {
        currPlayState = PlayState.Playing;
        InvokeRepeating(nameof(PlayNext), 1, 1.5f);
    }

    /// <summary>
    /// 暫停撥放
    /// </summary>
    private void PausePlay()
    {
        currPlayState = PlayState.Pause;
        CancelInvoke(nameof(PlayNext));
    }

    /// <summary>
    /// 切換影片
    /// </summary>
    public void SwitchVideo()
    {
        SetInit(dataIndex);
        int allVideoCount = HandHistoryManager.Instance.GetResultDataList().Count;
        float totalHeight = switchVideoMoveDictance * allVideoCount;
        float y = totalHeight - (switchVideoMoveDictance * (dataIndex + 1));
        HandHistoryViewParent.anchoredPosition = new Vector2(0, y);
    }

    /// <summary>
    /// 播放下一段
    /// </summary>
    private void PlayNext()
    {
        currPlayIndex++;

        currProcessStepHistoryData = currProcessHistoryData.processStepHistoryDataList[currPlayIndex];
        StartCoroutine(IPlayRecode(currProcessStepHistoryData));

        //已播放至最後一段
        if (currPlayIndex == currProcessHistoryData.processStepHistoryDataList.Count - 1)
        {
            currPlayState = PlayState.Retuen;
            CancelInvoke(nameof(PlayNext));
        }

        BtnDisplayControl();
    }

    /// <summary>
    /// 設置初始畫面
    /// </summary>
    /// <param name="index"></param>
    public void SetInit(int index)
    {
        WinType_Txt.text = "";
        HandNum_Txt.text = $"{LanguageManager.Instance.GetText("Hand")}{index + 1}";
        int allVideoCount = HandHistoryManager.Instance.GetResultDataList().Count;
        NextVideo_Btn.gameObject.SetActive(index + 1 < allVideoCount);
        PreVideo_Btn.gameObject.SetActive(index > 0);

        dataIndex = index;
        currPlayIndex = -1;
        currPlayState = PlayState.Pause;

        currProcessHistoryData = HandHistoryManager.Instance.GetProcessDataList()[dataIndex];
        BtnDisplayControl();

        for (int i = 1; i < Players.Length; i++)
        {
            Players[i].IsWinnerActive = false;
            Players[i].gameObject.SetActive(false);
        }
        foreach (var common in CommunityPokser)
        {
            common.gameObject.SetActive(false);
        }

        GameInitHistoryData gameInitsHistoryData = HandHistoryManager.Instance.GetGameInitDataList()[index];
        for (int i = 0; i < gameInitsHistoryData.SeatList.Count; i++)
        {
            int seatIndex = gameInitsHistoryData.SeatList[i];
            Players[seatIndex].gameObject.SetActive(true);
            Players[seatIndex].IsOpenInfoMask = false;
            Players[seatIndex].IsWinnerActive = false;

            string nickname = gameInitsHistoryData.NicknameList[i];
            int avatarIndex = gameInitsHistoryData.AvatarList[i];
            if (seatIndex == 0)
            {
                //本地玩家
                nickname = DataManager.UserNickname;
                avatarIndex = DataManager.UserAvatarIndex;
            }

            Players[seatIndex].SetInitPlayerInfo(gameInitsHistoryData.SeatList[i],
                                                 gameInitsHistoryData.UserIdList[i],
                                                 nickname,
                                                 gameInitsHistoryData.InitChipsList[i],
                                                 avatarIndex);

            Players[seatIndex].SetChipsTxt = gameInitsHistoryData.InitChipsList[i];

            Players[seatIndex].GetHandPoker[0].gameObject.SetActive(true);
            Players[seatIndex].GetHandPoker[1].gameObject.SetActive(true);
            Players[seatIndex].GetHandPoker[0].PokerNum = gameInitsHistoryData.HandPoker1List[i];
            Players[seatIndex].GetHandPoker[1].PokerNum = gameInitsHistoryData.HandPoker2List[i];

            //本地玩家
            if (seatIndex == 0)
            {
                List<int> judgePoker = new List<int>()
                {
                    gameInitsHistoryData.HandPoker1List[i],
                    gameInitsHistoryData.HandPoker2List[i],
                };
                //判斷牌型
                PokerShape.JudgePokerShape(judgePoker, (resultIndex, matchPokerList) =>
                {
                    Players[seatIndex].SetPokerShapeStr(resultIndex);
                    List<Poker> pokers = new List<Poker>()
                    {
                        Players[seatIndex].GetHandPoker[0],
                        Players[seatIndex].GetHandPoker[1],
                    };

                    if (resultIndex < 10)
                    {
                        PokerShape.OpenMatchPokerFrame(pokers,
                                                       matchPokerList,
                                                       false);
                    }
                });
            }

            if (seatIndex == gameInitsHistoryData.ButtonSeat)
            {
                Players[seatIndex].SetSeatCharacter(SeatCharacterEnum.Button);

                if (gameInitsHistoryData.SeatList.Count == 2)
                {
                    Players[seatIndex].SetSeatCharacter(SeatCharacterEnum.SB);
                    Players[seatIndex].DisplayBetAction(true,
                                                        gameInitsHistoryData.CurrBetChipsList[i],
                                                        BetActionEnum.Blinds,
                                                        false);
                }
            }
            else if (seatIndex == gameInitsHistoryData.SBSeat)
            {
                Players[seatIndex].SetSeatCharacter(SeatCharacterEnum.SB);
                Players[seatIndex].DisplayBetAction(true,
                                                    gameInitsHistoryData.CurrBetChipsList[i],
                                                    BetActionEnum.Blinds,
                                                    false);
            }
            else if (seatIndex == gameInitsHistoryData.BBSeat)
            {
                Players[seatIndex].SetSeatCharacter(SeatCharacterEnum.BB);
                Players[seatIndex].DisplayBetAction(true,
                                                    gameInitsHistoryData.CurrBetChipsList[i],
                                                    BetActionEnum.Blinds,
                                                    false);
            }
        }

        TotalPot_Txt.text = StringUtils.SetChipsUnit(gameInitsHistoryData.TotalPotChips);
    }

    #endregion

    #region 影片內容

    /// <summary>
    /// 播放紀錄
    /// </summary>
    /// <param name="processStepHistoryData"></param>
    private IEnumerator IPlayRecode(ProcessStepHistoryData processStepHistoryData)
    {
        TotalPot_Txt.text = StringUtils.SetChipsUnit(processStepHistoryData.TotalPot);
        WinType_Txt.text = "";

        //初始化
        foreach (var player in Players)
        {
            player.gameObject.SetActive(false);
            player.SetBackChips = 0;
        }

        //設置玩家訊息
        for (int i = 0; i < processStepHistoryData.SeatList.Count; i++)
        {
            int seatIndex = processStepHistoryData.SeatList[i];

            Players[seatIndex].gameObject.SetActive(true);

            Players[seatIndex].DisplayBetAction(false);
            Players[seatIndex].IsWinnerActive = false;

            Players[seatIndex].GetHandPoker[0].PokerNum = processStepHistoryData.HandPoker1[i];
            Players[seatIndex].GetHandPoker[1].PokerNum = processStepHistoryData.HandPoker2[i];

            //棄牌開啟遮罩
            Players[seatIndex].IsOpenInfoMask = processStepHistoryData.BetActionEnumIndex[i] == 2;
            Players[seatIndex].IsFold = processStepHistoryData.BetActionEnumIndex[i] == 2;
            Players[seatIndex].IsAllIn = processStepHistoryData.BetActionEnumIndex[i] == 6;

            //玩家行動
            if (processStepHistoryData.BetActionEnumIndex[i] == 0)
            {
                Players[seatIndex].DisplayBetAction(false);
            }
            else
            {
                Players[seatIndex].DisplayBetAction(true,
                                                    processStepHistoryData.BetChipsList[i],
                                                    ((BetActionEnum)processStepHistoryData.BetActionEnumIndex[i]),
                                                    false);
            }
        }
        Debug.Log("Show Chips");
        yield return gameView.IConcentrateBetChips();
        if (processStepHistoryData.CommunityPoker == null ||
            processStepHistoryData.CommunityPoker.Count == 0)
        {
            //未有公共牌
            foreach (var common in CommunityPokser)
            {
                common.PokerNum = -1;
                common.gameObject.SetActive(false);
            }
        }
        else
        {
            //已顯示的公共牌數量
            int showCommonCount = CommunityPokser.Where(x => x.gameObject.activeSelf).Count();
            if (showCommonCount > processStepHistoryData.CommunityPoker.Count)
            {
                for (int i = CommunityPokser.Length - 1; i >= 0; i--)
                {
                    if (CommunityPokser[i].gameObject.activeSelf)
                    {
                        CommunityPokser[i].PokerNum = -1;
                        CommunityPokser[i].gameObject.SetActive(false);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < processStepHistoryData.CommunityPoker.Count; i++)
                {
                    if (CommunityPokser[i].gameObject.activeSelf == false)
                    {
                        CommunityPokser[i].gameObject.SetActive(true);
                        CommunityPokser[i].PokerNum = processStepHistoryData.CommunityPoker[i];
                        StartCoroutine(CommunityPokser[i].IHorizontalFlopEffect(processStepHistoryData.CommunityPoker[i]));
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        //籌碼更新
        for (int i = 0; i < processStepHistoryData.SeatList.Count; i++)
        {
            int seatIndex = processStepHistoryData.SeatList[i];
            Players[seatIndex].SetChipsTxt = processStepHistoryData.ChipsList[i];
        }

        //關閉所有撲克效果
        CloseAllPokerEffect();

        //判斷牌行
        for (int i = 0; i < Players.Length; i++)
        {
            JudgePokerShape(i, false);
        }

        //主池贏家
        if (playVideoWinCoroutine != null)
        {
            StopCoroutine(playVideoWinCoroutine);
            playVideoWinCoroutine = null;
        }
        playVideoWinCoroutine = StartCoroutine(OnWinner(processStepHistoryData.PotWinnerSeatList,
                                                        processStepHistoryData.PotWinChips,
                                                        $"{LanguageManager.Instance.GetText("Pot")}"));
        yield return playVideoWinCoroutine;
        //邊池贏家
        if (playVideoWinCoroutine != null)
        {
            StopCoroutine(playVideoWinCoroutine);
            playVideoWinCoroutine = null;
        }
        playVideoWinCoroutine = StartCoroutine(OnWinner(processStepHistoryData.SildWinnerSeatList,
                                                        processStepHistoryData.SildWinChips,
                                                        $"{LanguageManager.Instance.GetText("Side")}"));
        //退回籌碼
        if (processStepHistoryData.BackChipsDic != null)
        {
            foreach (var item in processStepHistoryData.BackChipsDic)
            {
                int seat = item.Key;
                double backChipsValue = item.Value;

                if (backChipsValue > 0)
                {
                    Players[seat].SetBackChips = backChipsValue;
                }
            }
        }

        yield return playVideoWinCoroutine;

        ///贏家(資料, 贏得籌碼, 主池/邊池文字)
        IEnumerator OnWinner(List<int> dataList, double winChips, string typeStr)
        {
            if (dataList != null &&
                dataList.Count > 0)
            {
                TotalPot_Txt.text = StringUtils.SetChipsUnit(winChips);

                //開啟所有玩家遮罩
                foreach (var player in Players)
                {
                    player.IsOpenInfoMask = true;
                    player.DisplayBetAction(false);
                }

                //贏得類型顯示
                WinType_Txt.text = typeStr;

                //贏家顯示
                foreach (var seat in dataList)
                {
                    //關閉所有撲克效果
                    CloseAllPokerEffect();

                    Players[seat].IsOpenInfoMask = false;
                    Players[seat].IsWinnerActive = true;

                    JudgePokerShape(seat, true);

                    yield return new WaitForSeconds(2);
                }
            }
        }

        //關閉所有撲克效果
        void CloseAllPokerEffect()
        {
            List<Poker> pokers = new List<Poker>();
            foreach (var item in Players)
            {
                pokers.Add(item.GetHandPoker[0]);
                pokers.Add(item.GetHandPoker[1]);
            }
            List<Poker> allPokerList = CommunityPokser.ToList().Concat(pokers).ToList();
            foreach (var poker in allPokerList)
            {
                poker.PokerEffectEnable = false;
            }
        }

        //判斷牌型(座位, 是否顯示贏家效果)
        void JudgePokerShape(int seat, bool isShowWinEffect)
        {
            if (Players[seat].GetHandPoker[0].PokerNum < 0)
            {
                //Players[seat].SetPokerShapeTxtStr = "";
                Players[seat].SetPokerShapeImage = null;
                return;
            }

            List<int> judgePoker = new List<int>();
            judgePoker.Add(Players[seat].GetHandPoker[0].PokerNum);
            judgePoker.Add(Players[seat].GetHandPoker[1].PokerNum);
            foreach (var common in CommunityPokser)
            {
                if (common.PokerNum >= 0)
                {
                    judgePoker.Add(common.PokerNum);
                }
            }

            PokerShape.JudgePokerShape(judgePoker, (resultIndex, matchPokerList) =>
            {
                Players[seat].SetPokerShapeStr(resultIndex);

                if (resultIndex < 10)
                {
                    List<Poker> pokers = new List<Poker>()
                        {
                            Players[seat].GetHandPoker[0],
                            Players[seat].GetHandPoker[1],
                        };
                    foreach (var common in CommunityPokser)
                    {
                        if (common.PokerNum >= 0)
                        {
                            pokers.Add(common);
                        }
                    }
                    PokerShape.OpenMatchPokerFrame(pokers,
                                                   matchPokerList,
                                                   isShowWinEffect);
                }
            });
        }
    }

    #endregion
}
