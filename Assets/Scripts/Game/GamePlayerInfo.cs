using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

using RequestBuf;

public class GamePlayerInfo : MonoBehaviour
{
    [Header("用戶訊息")]
    [SerializeField]
    GameObject ActionFrame_Obj, Winner_Obj, InfoMask_Obj;
    [SerializeField]
    Image CDMask_Img, Avatar_Img, ButtonCharacter_Img;
    [SerializeField]
    TextMeshProUGUI Nickname_Txt, Chips_Txt, BackChips_Txt, PokerShape_Txt, BlindCharacter_Txt, Winner_Txt;

    [Header("手牌")]
    [SerializeField]
    RectTransform HandPoker_Tr, ShowHandPoker_Tr;
    [SerializeField]
    Poker[] HandPokers;
    [SerializeField]
    Poker[] ShowHandPokers;

    [Header("下注籌碼")]
    [SerializeField]
    RectTransform BetChips_Tr;

    [Header("行動")]
    [SerializeField]
    Image Action_Img;
    [SerializeField]
    TextMeshProUGUI Action_Txt;
    [SerializeField]
    Color foldColor, callColor, checkColor, raiseColor, allInColor, blindColor;

    [Header("條天訊息")]
    [SerializeField]
    GameObject Chat_Obj;
    [SerializeField]
    TextMeshProUGUI Chat_Txt;

    Coroutine cdCoroutine;              //倒數協程
    Coroutine chatCoroutine;            //聊天協程

    int pokerShapeIndex;                //牌型編號

    Vector2 betChipsr_TrInitPos;         //下注籌碼物件初始位置

    /// <summary>
    /// 是否有在進行遊戲
    /// </summary>
    public bool IsPlaying { get; set; }

    /// <summary>
    /// 是否已棄牌
    /// </summary>
    public bool IsFold { get; set; }

    /// <summary>
    /// 是否已All In
    /// </summary>
    public bool IsAllIn { get; set; }

    /// <summary>
    /// 當前下注行為
    /// </summary>
    public BetActionEnum CurrBetAction { get; set; }

    /// <summary>
    /// 座位編號
    /// </summary>
    public int SeatIndex { get; set; }

    /// <summary>
    /// 用戶ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 暱稱
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    /// 頭像
    /// </summary>
    public int Avatar { get; set; }

    /// <summary>
    /// 座位上的腳色
    /// </summary>
    public SeatCharacterEnum SeatCharacter { get; set; }

    /// <summary>
    /// 當前擁有籌碼
    /// </summary>
    public double CurrRoomChips { get; set; }

    /// <summary>
    /// 當前下注值
    /// </summary>
    public double CurrBetValue { get; set; }

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        SetPokerShapeTxtStr = LanguageManager.Instance.GetText(PokerShape.shapeStr[pokerShapeIndex]);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        InitCountDown();
        Chat_Obj.SetActive(false);
        IsOpenInfoMask = true;
        PokerShape_Txt.gameObject.SetActive(false);

        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        IsFold = false;
        IsAllIn = false;
        CurrBetAction = BetActionEnum.None;
        CurrBetValue = 0;
        ButtonCharacter_Img.gameObject.SetActive(false);
        BlindCharacter_Txt.text = "";
        Action_Img.gameObject.SetActive(false);
        betChipsr_TrInitPos = BetChips_Tr.anchoredPosition;
        BetChips_Tr.gameObject.SetActive(false);
        ActionFrame = false;
        HandPokers[0].PokerNum = -1;
        HandPokers[0].gameObject.SetActive(false);
        HandPokers[1].PokerNum = -1;
        HandPokers[1].gameObject.SetActive(false);        
        Winner_Obj.SetActive(false);
        SetBackChips = 0;
        SetPokerShapeTxtStr = "";
        ShowHandPoker_Tr.gameObject.SetActive(false);
    }

    /// <summary>
    /// 設置牌型文字元件文字
    /// </summary>
    public string SetPokerShapeTxtStr
    {
        set
        {
            PokerShape_Txt.gameObject.SetActive(true);
            PokerShape_Txt.text = value;
        }
    }

    /// <summary>
    /// 玩家房間籌碼值
    /// </summary>
    public double PlayerRoomChips
    {
        get
        {
            return CurrRoomChips;
        }
        set
        {
            CurrRoomChips = value;
            StringUtils.ChipsChangeEffect(Chips_Txt, CurrRoomChips);
        }
    }

    /// <summary>
    /// 設置退回籌碼
    /// </summary>
    public double SetBackChips
    {
        set
        {
            BackChips_Txt.text = value > 0 ?
                                 $"+{StringUtils.SetChipsUnit(value)}" :
                                 "";
        }
    }

    /// <summary>
    /// 行動框
    /// </summary>
    public bool ActionFrame
    {
        set
        {
            ActionFrame_Obj.SetActive(value);
        }
    }

    /// <summary>
    /// 設定贏家顯示
    /// </summary>
    public bool IsWinnerActive
    {
        set
        {
            Winner_Obj.SetActive(value);
            Winner_Txt.text = LanguageManager.Instance.GetText("Winner");
        }
    }

    /// <summary>
    /// 設定本地玩家手牌位置
    /// </summary>
    public RectTransform SetLocalHandPokerPosition
    {
        set
        {
            HandPoker_Tr.SetParent(value);
            HandPoker_Tr.anchoredPosition = Vector2.zero;
            HandPoker_Tr.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    /// <summary>
    /// 獲取手牌
    /// </summary>
    public Poker[] GetHandPoker
    {
        get
        {
            return HandPokers;
        }
    }

    /// <summary>
    /// 獲取下注物件激活狀態
    /// </summary>
    public bool GetBetChipsActive
    {
        get
        {
            return BetChips_Tr.gameObject.activeSelf;
        }
    }

    /// <summary>
    /// 開訊息遮罩
    /// </summary>
    public bool IsOpenInfoMask
    {
        set
        {
            InfoMask_Obj.SetActive(value);
        }
    }

    /// <summary>
    /// 設置擁有籌碼文字
    /// </summary>
    public double SetChipsTxt
    {
        set
        {
            Chips_Txt.text = StringUtils.SetChipsUnit(value);
        }
    }

    /// <summary>
    /// 設定初始玩家訊息
    /// </summary>
    /// <param name="seatIndex">座位編號</param>
    /// <param name="userId">userId</param>
    /// <param name="nickName">暱稱</param>
    /// <param name="initChips">攜帶籌碼</param>
    /// <param name="avatar">頭像</param>
    public void SetInitPlayerInfo(int seatIndex, string userId, string nickName, double initChips, int avatar)
    {
        //Init();

        UserId = userId;
        Nickname = nickName;
        Avatar = avatar;
        SeatIndex = seatIndex;

        CurrRoomChips = initChips;
        Avatar_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.AvatarAlbum).album[avatar];
        Nickname_Txt.text = $"@{nickName}";
        Chips_Txt.text = $"{StringUtils.SetChipsUnit(initChips)}";
    }

    /// <summary>
    /// 顯示聊天訊息
    /// </summary>
    /// <param name="chatContent"></param>
    public void ShowChatInfo(string chatContent)
    {
        Chat_Obj.SetActive(true);
        StringUtils.StrExceedSize(chatContent, Chat_Txt);

        if (chatCoroutine != null)
        {
            StopCoroutine(chatCoroutine);
        }
        chatCoroutine = StartCoroutine(IYieldCloseChatInfo());
    }

    /// <summary>
    /// 關閉聊訊息
    /// </summary>
    public void CloseChatInfo()
    {
        if (chatCoroutine != null)
        {
            StopCoroutine(chatCoroutine);
        }
        Chat_Obj.SetActive(false);
    }

    /// <summary>
    /// 延遲關閉聊訊息
    /// </summary>
    /// <returns></returns>
    private IEnumerator IYieldCloseChatInfo()
    {
        yield return new WaitForSeconds(1);
        Chat_Obj.SetActive(false);
    }

    /// <summary>
    /// 設定手牌
    /// </summary>
    /// <param name="handPoker0"></param>
    /// <param name="handPoker1"></param>
    public void SetHandPoker(int hand0, int hand1)
    {
        InitCountDown();

        IsPlaying = true;

        foreach (var poker in HandPokers)
        {
            poker.gameObject.SetActive(true);
            poker.SetColor = 1;
        }

        HandPokers[0].PokerNum = hand0;
        HandPokers[1].PokerNum = hand1;

        IsOpenInfoMask = false;
    }

    /// <summary>
    /// 設定棄牌顯示手牌
    /// </summary>
    /// <param name="isShow"></param>
    /// <param name="pokerNum"></param>
    public void SetShowHandPoker(bool isShow, List<int> pokerNum = null)
    {
        ShowHandPoker_Tr.gameObject.SetActive(isShow);

        if (pokerNum != null)
        {
            Debug.Log($"設定棄牌顯示手牌:{pokerNum[0]}/{pokerNum[1]}");

            ShowHandPokers[0].gameObject.SetActive(isShow);
            ShowHandPokers[1].gameObject.SetActive(isShow);
            ShowHandPokers[0].PokerNum = pokerNum[0];
            ShowHandPokers[1].PokerNum = pokerNum[1];
        }
    }

    /// <summary>
    /// 顯示棄牌顯示手牌
    /// </summary>
    /// <param name="isShow"></param>
    public void SwitchShoHandPoker(List<int> showPoker)
    {
        ShowHandPoker_Tr.gameObject.SetActive(showPoker.Any(x => x >= 0));
        ShowHandPokers[0].gameObject.SetActive(showPoker[0] >= 0);
        ShowHandPokers[1].gameObject.SetActive(showPoker[1] >= 0);
        ShowHandPokers[0].PokerNum = showPoker[0];
        ShowHandPokers[1].PokerNum = showPoker[1];
    }

    /// <summary>
    /// 開啟本地棄牌顯示手牌
    /// </summary>
    /// <param name="index"></param>
    /// <param name="num"></param>
    public void OpenLocalShowHandPoker(int index, int num)
    {
        ShowHandPoker_Tr.gameObject.SetActive(true);
        ShowHandPokers[index].gameObject.SetActive(true);
        ShowHandPokers[index].PokerNum = num;
    }

    /// <summary>
    /// 設置座位的腳色
    /// </summary>
    /// <param name="seatCharacter"></param>
    public void SetSeatCharacter(SeatCharacterEnum seatCharacter)
    {
        SeatCharacter = seatCharacter;

        switch (seatCharacter)
        {
            case SeatCharacterEnum.None:
                ButtonCharacter_Img.gameObject.SetActive(false);
                BlindCharacter_Txt.text = "";
                break;

            case SeatCharacterEnum.Button:
                ButtonCharacter_Img.gameObject.SetActive(true);
                break;

            case SeatCharacterEnum.SB:
                BlindCharacter_Txt.text = "SB";
                break;

            case SeatCharacterEnum.BB:
                BlindCharacter_Txt.text = "BB";
                break;
        }
    }

    /// <summary>
    /// 重製倒數
    /// </summary>
    public void InitCountDown()
    {
        if(cdCoroutine != null) StopCoroutine(cdCoroutine);
        CDMask_Img.fillAmount = 0;
    }

    /// <summary>
    /// 倒數
    /// </summary>
    /// <param name="cdTime">倒數總時間</param>
    /// <param name="cd">倒數</param>
    public void CountDown(int cdTime, int cd)
    {
        cdCoroutine = StartCoroutine(ICountDown(cdTime, cd));
    }

    /// <summary>
    /// 倒數效果
    /// </summary>
    /// <param name="cdTime">倒數總時間</param>
    /// <param name="cd">倒數</param>
    private IEnumerator ICountDown(int cdTime, int cd)
    {
        float target = ((float)cdTime - (cd - 1)) / (float)cdTime;
        float curr = ((float)cdTime - cd) / (float)cdTime;

        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < 1)
        {
            float process = Mathf.Clamp01((float)(DateTime.Now - startTime).TotalSeconds / 1);
            float value = Mathf.Lerp(curr, target, process);

            CDMask_Img.fillAmount = value;
            yield return null;
        }

        CDMask_Img.fillAmount = target;
    }

    /// <summary>
    /// 每輪回合開始初始
    /// </summary>
    public void RountInit()
    {
        if (CurrBetAction != BetActionEnum.AllIn)
        {
            CurrBetValue = 0;
        }
    }

    /// <summary>
    /// 玩家下注
    /// </summary>
    /// <param name="betValue">下注值</param>
    /// <param name="chips">玩家籌碼</param>
    public void PlayerBet(double betValue, double chips)
    {
        Debug.Log($"Player Bet:{betValue}");
        Debug.Log($"Player Carry Chips:{chips}");

        BetChips_Tr.gameObject.SetActive(true);
        PlayerRoomChips = chips;
        CurrBetValue = betValue;
    }

    /// <summary>
    /// 下注籌碼集中效果
    /// </summary>
    /// <param name="potPointPos">底池位置</param>
    public void ConcentrateBetChips(Vector2 potPointPos)
    {
        float during = 0.5f;//效果時間

        ObjMoveUtils.ObjMoveToTarget(BetChips_Tr, potPointPos, during,
                                    () =>
                                    {
                                        if(BetChips_Tr != null)
                                        {
                                            BetChips_Tr.anchoredPosition = betChipsr_TrInitPos;
                                            BetChips_Tr.gameObject.SetActive(false);
                                        }                                        
                                    });
    }

    /// <summary>
    /// 顯示行動
    /// </summary>
    /// <param name="isShow"></param>
    /// <param name="betValue">下注值</param>
    /// <param name="betActionEnum">行動文字</param>
    /// <param name="isEffect">是否使用籌碼變化效果</param>
    public void DisplayBetAction(bool isShow, double betValue = 0, BetActionEnum betActionEnum = BetActionEnum.None, bool isEffect = true)
    {
        Action_Img.gameObject.SetActive(isShow);

        Debug.Log($"顯示行動:{isShow}/{betActionEnum}");

        switch (betActionEnum)
        {
            case BetActionEnum.Fold:
                Action_Img.color = foldColor;
                break;

            case BetActionEnum.Check:
                Action_Img.color = checkColor;
                break;

            case BetActionEnum.Raise:
                Action_Img.color = raiseColor;
                break;

            case BetActionEnum.Bet:
                Action_Img.color = raiseColor;
                break;

            case BetActionEnum.Call:
                Action_Img.color = callColor;
                break;

            case BetActionEnum.AllIn:
                Action_Img.color = allInColor;
                break;

            case BetActionEnum.Blinds:
                Action_Img.color = blindColor;
                break;
        }

        if (betActionEnum == BetActionEnum.Blinds ||
            betActionEnum == BetActionEnum.Call ||
            betActionEnum == BetActionEnum.Raise ||
            betActionEnum == BetActionEnum.Bet ||
            betActionEnum == BetActionEnum.AllIn)
        {
            if (isEffect)
            {
                StringUtils.ChipsChangeEffect(Action_Txt,
                                              betValue,
                                              $"{LanguageManager.Instance.GetText($"{betActionEnum}")}\n");
            }
            else
            {
                Action_Txt.text = $"{LanguageManager.Instance.GetText($"{betActionEnum}")}\n{StringUtils.SetChipsUnit(betValue)}";
            }
        }
        else
        {
            Action_Txt.text = betActionEnum != BetActionEnum.None ?
                              $"{LanguageManager.Instance.GetText($"{betActionEnum}")}" :
                              "";
        }

        float txtWidth = Action_Txt.preferredHeight;
        Action_Img.rectTransform.sizeDelta = new Vector2(Action_Img.rectTransform.rect.width,
                                                         txtWidth + 5);
    }

    /// <summary>
    /// 玩家行動
    /// </summary>
    /// <param name="actionEnum">行動</param>
    /// <param name="betValue">下注值</param>
    /// <param name="chips">玩家籌碼</param>
    public void PlayerAction(BetActingEnum actionEnum, double betValue, double chips)
    {
        if (!gameObject.activeSelf) return;

        //StopCountDown();
        ActionFrame = false;

        CurrBetAction = (BetActionEnum)Convert.ToInt32(actionEnum);

        DisplayBetAction(true,
                         betValue,
                         (BetActionEnum)(int)actionEnum);

        switch (actionEnum)
        {
            //大小盲
            case BetActingEnum.Blind:
                PlayerBet(betValue, chips);
                break;

            //棄牌
            case BetActingEnum.Fold:
                IsFold = true;
                foreach (var poker in HandPokers)
                {
                    poker.SetColor = 0.5f;
                }

                IsOpenInfoMask = true;
                break;

            //過牌
            case BetActingEnum.Check:
                break;

            //加注
            case BetActingEnum.Raise:
                PlayerBet(betValue, chips);
                break;

            //下注
            case BetActingEnum.Bet:
                PlayerBet(betValue, chips);
                break;

            //跟注
            case BetActingEnum.Call:
                PlayerBet(betValue, chips);
                break;

            //All In
            case BetActingEnum.AllIn:
                IsAllIn = true;
                PlayerBet(betValue, chips);
                break;
        }
    }

    /// <summary>
    /// 設置牌型文字
    /// </summary>
    /// <param name="shapeIndex"></param>
    public void SetPokerShapeStr(int shapeIndex)
    {

        pokerShapeIndex = shapeIndex;
        SetPokerShapeTxtStr = LanguageManager.Instance.GetText(PokerShape.shapeStr[pokerShapeIndex]);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
        StopAllCoroutines();
    }
}
