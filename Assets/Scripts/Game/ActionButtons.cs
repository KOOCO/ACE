using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButtons : MonoBehaviour
{
    [Header("操作按鈕")]
    [SerializeField]
    GameObject actionButtonsMainObject;
    [SerializeField]
    Button Raise_Btn, Call_Btn, Fold_Btn, RaiseClose_Btn;
    [SerializeField]
    RectTransform AutoActionFrame_Tr;
    [SerializeField]
    Button[] ShowPokerBtnList;
    [SerializeField]
    Button BackToSit_Btn;
    [SerializeField]
    TextMeshProUGUI RaiseBtn_Txt, CallBtn_Txt;
    [SerializeField]
    GameObject coinIconObj;
    [SerializeField]
    public Image CallBtn_Img, FoldBtn_Img;
    Vector2 originCallPos;

    [Header("加注操作")]
    [SerializeField]
    List<Button> PotPercentRaiseBtnList;
    [SerializeField]
    Slider Raise_Sli;
    [SerializeField]
    RectTransform Raise_Tr;
    [SerializeField]
    SliderClickDetection SliderClickDetection;
    [SerializeField]
    Button AllIn_Btn, MinRaise_Btn;
    private bool isAllinSelected = false; //判斷下注拉條是否為all
    [SerializeField]
    List<TextMeshProUGUI> PotPercentRaiseTxtList;
    [SerializeField]
    TextMeshProUGUI RaiseSliHandle_Txt, CurrRaise_Txt, MinRaiseBtn_Txt, AllinBtn_Txt;
    [SerializeField]
    List<Sprite> ActionBtn_Images;

    private GameData gameData;

    public event Action UpdateActionBtn;
    public event Action<bool> SetMenuBtn;
    public event Action<BetActingEnum, double> UpdateBetAction;
    public event Action<Dictionary<string, object>> UpdataPlayerData;
    public event Action<int> SetShowFoldPoker;

    private void Awake()
    {
        //初始跟注圖片位置
        originCallPos = CallBtn_Img.transform.localPosition;
        EventListener();
    }
    private void OnEnable()
    {
        BackToSit_Btn.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (!gameData.isOnFold)
        {
            Call_Btn.gameObject.SetActive(!(CallBtn_Img.sprite.name == "blank" || !CallBtn_Img.gameObject.activeSelf));
            Fold_Btn.gameObject.SetActive(!(FoldBtn_Img.sprite.name == "blank" || !FoldBtn_Img.gameObject.activeSelf));
        }
        else
            Call_Btn.gameObject.SetActive(false);
    }
    public void Init(string roomName)
    {
        gameData = GameRoomManager.Instance.GetGameData(roomName);
        gameData.strData.FoldStr = "Fold";
        gameData.strData.CallStr = "Check";
        gameData.strData.CallValueStr = "";
        CallBtn_Txt.text = "";
        gameData.strData.RaiseStr = "CallAny";
        gameData.strData.RaiseValueStr = "";
        RaiseBtn_Txt.text = LanguageManager.Instance.GetText(gameData.strData.RaiseStr) + gameData.strData.RaiseValueStr;
        coinIconObj.SetActive(false);
        SetSitOutDisplay();

        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            int keyF = gameData.betStringsE.FirstOrDefault(x => x.Value == gameData.strData.FoldStr).Key;
            int keyC = gameData.betStringsE.FirstOrDefault(x => x.Value == gameData.strData.CallStr).Key;
            SetCallFoldBetStr("Call", keyC);
            SetCallFoldBetStr("Fold", keyF);
        }
        else
        {
            int keyF = gameData.betStringsC.FirstOrDefault(x => x.Value == LanguageManager.Instance.GetText(gameData.strData.FoldStr)).Key;
            int keyC = gameData.betStringsC.FirstOrDefault(x => x.Value == LanguageManager.Instance.GetText(gameData.strData.CallStr)).Key;
            SetCallFoldBetStr("Call", keyC);
            SetCallFoldBetStr("Fold", keyF);
        }

        ShowRaise = false;
        foreach (var show in ShowPokerBtnList)
        {
            show.gameObject.SetActive(false);
        }
        SetActionButton = false;
        gameData.isOnFold = false;
        AutoActionState = AutoActingEnum.None;
        actionButtonsMainObject.SetActive(false);
        Fold_Btn.gameObject.SetActive(true);
        Call_Btn.gameObject.SetActive(true);
        Raise_Btn.gameObject.SetActive(true);
    }
    private void EventListener()
    {
        #region 操作按鈕

        //回到座位
        BackToSit_Btn.onClick.AddListener(() =>
        {
            gameData.thisData.IsSitOut = false;
            SetSitOutDisplay();
            var data = new Dictionary<string, object>()
            {
                { FirebaseManager.IS_SIT_OUT, gameData.thisData.IsSitOut},         //是否保留座位離開
            };
            UpdataPlayerData.Invoke(data);
        });

        //棄牌顯示手牌按鈕
        for (int i = 0; i < ShowPokerBtnList.Count(); i++)
        {
            int index = i;
            ShowPokerBtnList[i].onClick.AddListener(delegate { setShowFoldPoker(index); });
        }

        //加注滑條
        Raise_Sli.onValueChanged.AddListener((value) =>
        {
            double newRaiseValue = TexasHoldemUtil.SliderValueChange(Raise_Sli,
                                                                    value,
                                                                    gameData.thisData.SmallBlindValue * 2,
                                                                    gameData.thisData.MinRaiseValue,
                                                                    gameData.thisData.LocalPlayerChips,
                                                                    SliderClickDetection);

            gameData.strData.RaiseStr = Math.Floor(newRaiseValue) >= gameData.thisData.LocalPlayerChips ?
                               "AllIn" :
                               "RaiseTo";

            if (gameData.strData.RaiseStr == "RaiseTo" &&
                gameData.gameRoomData.actionPlayerCount == 0)
            {
                gameData.strData.RaiseStr = "BetTo";
            }

            gameData.strData.RaiseValueStr = newRaiseValue >= gameData.thisData.LocalPlayerChips ?
                                    $"\n${StringUtils.SetChipsUnit(gameData.thisData.LocalPlayerChips)}" :
                                    $"\n${StringUtils.SetChipsUnit(newRaiseValue)}";
            RaiseBtn_Txt.text = LanguageManager.Instance.GetText(gameData.strData.RaiseStr) + gameData.strData.RaiseValueStr;
            coinIconObj.SetActive(true);

            SetRaiseToText = newRaiseValue;
            gameData.thisData.CurrRaiseValue = Math.Floor(newRaiseValue);
        });

        //最小加注
        MinRaise_Btn.onClick.AddListener(() =>
        {
            Raise_Sli.value = (float)gameData.thisData.MinRaiseValue;
        });

        //All In
        AllIn_Btn.onClick.AddListener(() =>
        {
            Raise_Sli.value = (float)gameData.thisData.LocalPlayerChips;
        });

        //底池百分比加註
        for (int i = 0; i < PotPercentRaiseBtnList.Count; i++)
        {
            int index = i;
            PotPercentRaiseBtnList[i].onClick.AddListener(delegate { PotRaisePercent(index); });
        }

        //棄牌
        Fold_Btn.onClick.AddListener(() =>
        {
            if (gameData.thisData.isLocalPlayerTurn)
            {
                OnFold();
            }
            else
            {
                print("執行預選");
                AutoActionState = AutoActionState == AutoActingEnum.CheckAndFold ?
                                  AutoActingEnum.None :
                                  AutoActingEnum.CheckAndFold;
            }
            ShowRaise = false;
            GameRoomManager.Instance.EnanbleBtn(false);
            SetMenuBtn.Invoke(true);

            SetActionButton = false;
        });

        //跟注/過牌
        Call_Btn.onClick.AddListener(() =>
        {
            if (gameData.thisData.isLocalPlayerTurn)
            {
                print("執行跟注/過牌");
                OnCallAndCheck();
            }
            else
            {
                AutoActionState = AutoActionState == AutoActingEnum.Check ?
                             AutoActingEnum.None :
                             AutoActingEnum.Check;
            }
            ShowRaise = false;
            GameRoomManager.Instance.EnanbleBtn(false);
            SetMenuBtn.Invoke(true);
            SetActionButton = false;

            if (CallBtn_Img.sprite != AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteEnglish).album[1] || CallBtn_Img.sprite != AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteChinese).album[1])
            {
                print(LanguageManager.Instance.GetText("Call"));
            }
            else
                print("Player Action: " + LanguageManager.Instance.GetText("Check"));
        });

        //加注/All In
        Raise_Btn.onClick.AddListener(() =>
        {
            if (gameData.thisData.isLocalPlayerTurn)
            {
                bool isAllIn = gameData.thisData.LocalPlayerChips < gameData.thisData.MinRaiseValue ||
                               gameData.thisData.CurrRaiseValue == gameData.thisData.LocalPlayerChips;
                gameData.strData.RaiseStr = isAllIn ?
                               "AllIn" :
                               "RaiseTo";

                BetActingEnum acting = isAllIn == true ?
                                    BetActingEnum.AllIn :
                                    BetActingEnum.Raise;

                //加注
                if (acting == BetActingEnum.Raise &&
                    gameData.gameRoomData.actionPlayerCount == 0)
                {
                    acting = BetActingEnum.Bet;
                }

                if (Raise_Tr.gameObject.activeSelf || isAllIn == true)
                {

                    double betValue = isAllIn == true ?
                                  gameData.thisData.LocalPlayerChips :
                                  gameData.thisData.CurrRaiseValue;

                    UpdateBetAction.Invoke(acting, betValue);

                    ShowRaise = false;
                    GameRoomManager.Instance.EnanbleBtn(false);
                    SetMenuBtn.Invoke(true);
                    SetActionButton = false;
                }
                else
                {
                    ShowRaise = true;
                    GameRoomManager.Instance.EnanbleBtn(true);
                    SetMenuBtn.Invoke(false); //開啟下注拉條時menu不能按
                    gameData.strData.RaiseStr = acting == BetActingEnum.Bet ?
                                       "BetTo" :
                                       "RaiseTo";
                    gameData.strData.RaiseValueStr = $"\n${StringUtils.SetChipsUnit(gameData.thisData.CurrRaiseValue)}";
                }
            }
            else
            {
                AutoActionState = AutoActionState == AutoActingEnum.CallAny ?
                                  AutoActingEnum.None :
                                  AutoActingEnum.CallAny;
            }
        });

        RaiseClose_Btn.onClick.AddListener(() =>
        {
            ShowRaise = false;
            GameRoomManager.Instance.EnanbleBtn(false);
            SetMenuBtn.Invoke(true);
        });

        #endregion
    }

    /// <summary>
    /// 設定顯示棄牌手牌
    /// </summary>
    /// <param name="index"></param>
    private void setShowFoldPoker(int index)
    {
        ShowPokerBtnList[index].gameObject.SetActive(false);
        SetShowFoldPoker.Invoke(index);
    }

    /// <summary>
    /// 底池百分比加注
    /// </summary>
    /// <param name="btnIndex"></param>
    private void PotRaisePercent(int btnIndex)
    {
        float raiseValue = 0f;

        // Check if the current game flow is in the Preflop stage
        bool isPreflop = (GameFlowEnum)gameData.gameRoomData.currGameFlow == GameFlowEnum.SetBlind || (GameFlowEnum)gameData.gameRoomData.currGameFlow == GameFlowEnum.Licensing;

        if (isPreflop)
        {
            Debug.Log("IsPreflop :: PotRaisePercent" + btnIndex);
            // Preflop: Calculate raise based on BB values
            switch (btnIndex)
            {
                case 0:
                    // 2BB Raise
                    raiseValue = (float)((gameData.gameRoomData.smallBlind * 2) * 2);
                    break;
                case 1:
                    // 3BB Raise
                    raiseValue = (float)((gameData.gameRoomData.smallBlind * 2) * 3);
                    break;
                case 2:
                    // 4BB Raise
                    raiseValue = (float)((gameData.gameRoomData.smallBlind * 2) * 4);
                    break;
                case 3:
                    // POT Raise (All-in)
                    raiseValue = (float)gameData.gameRoomData.potChips;
                    break;
                default:
                    raiseValue = 0f;
                    break;
            }
        }
        else
        {
            Debug.Log("!IsPreflop :: PotRaisePercent" + btnIndex);

            // Postflop: Calculate raise based on percentage of the pot
            raiseValue = (float)gameData.gameRoomData.potChips * (gameData.PotPercentRate[btnIndex] / 100);

            // Handle All-In case for btnIndex == 3
            if (btnIndex == 3)
            {
                raiseValue = (float)gameData.gameRoomData.potChips;
            }
        }

        // Update the Raise Slider with the calculated raise value
        Raise_Sli.value = (int)raiseValue;
    }
    /// <summary>
    /// 自動操作狀態
    /// </summary>
    public AutoActingEnum AutoActionState
    {
        get
        {
            return autoActingEnum;
        }
        set
        {
            autoActingEnum = value;
            switch (value)
            {
                case AutoActingEnum.None:
                    SetAutoAction(false);
                    break;
                case AutoActingEnum.CallAny:
                    SetAutoAction(true, Raise_Btn.transform, ActionBtn_Images[0]);
                    break;
                case AutoActingEnum.Check:
                    SetAutoAction(true, Call_Btn.transform, ActionBtn_Images[1]);
                    break;
                case AutoActingEnum.CheckAndFold:
                    SetAutoAction(true, Fold_Btn.transform, ActionBtn_Images[2]);
                    break;
            }
            print($"自動操作預選框: {value}, 是否開啟: {AutoActionFrame_Tr.gameObject.activeSelf}");
        }
    }
    /// <summary>
    /// 自動操作設定
    /// </summary>
    /// <param name="isActive"></param>
    /// <param name="parent"></param>
    public void SetAutoAction(bool isActive, Transform parent = null, Sprite _selectImg = null)
    {
        AutoActionFrame_Tr.gameObject.SetActive(isActive);
        AutoActionFrame_Tr.gameObject.GetComponent<Image>().sprite = _selectImg;
        if (parent != null)
        {
            AutoActionFrame_Tr.SetParent(parent);
            AutoActionFrame_Tr.anchoredPosition = Vector2.zero;
            AutoActionFrame_Tr.offsetMax = Vector2.zero;
            AutoActionFrame_Tr.offsetMin = Vector2.zero;
        }
    }
    /// <summary>
    /// 設置行動按鈕文字(是否為玩家回合)
    /// </summary>
    public bool SetActionButton
    {
        set
        {
            Debug.Log($"SetActionButton called with value: {value}");
            //SetActingButtonEnable = value;
            gameData.thisData.isLocalPlayerTurn = value;

            UpdateActionBtn.Invoke();

            if (!value)
            {
                //HandleInactivePlayerTurn();
                coinIconObj.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 跟注/過牌
    /// </summary>
    private void OnCallAndCheck()
    {
        double betValue = 0;
        BetActingEnum acting = BetActingEnum.Call;
        if (gameData.thisData.IsFirstRaisePlayer)
        {
            if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
            {
                acting = BetActingEnum.Check;
            }
            else if (gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue)
            {
                acting = BetActingEnum.AllIn;
                betValue = gameData.thisData.LocalPlayerChips;
            }
            else
            {
                betValue = gameData.thisData.CurrCallValue;
            }
        }
        else
        {
            if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
            {
                acting = BetActingEnum.Check;
            }
            else if (gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue)
            {
                acting = BetActingEnum.AllIn;
                betValue = gameData.thisData.LocalPlayerChips;
            }
            else
            {
                betValue = gameData.thisData.CurrCallValue;
            }
        }

        AutoActionState = AutoActingEnum.None;

        print("更新下注動作");
        UpdateBetAction.Invoke(acting, betValue);
    }
    /// <summary>
    /// 行動按鈕激活
    /// </summary>
    public bool SetActingButtonEnable
    {
        get
        {
            print("禁用加注按鈕");
            //return Raise_Btn.interactable;
            return Raise_Btn.gameObject.activeSelf;
        }
        set
        {
            //Raise_Btn.interactable = value;
            Raise_Btn.gameObject.SetActive(value);
            //Call_Btn.interactable = value;
            Call_Btn.gameObject.SetActive(value);
            //Fold_Btn.interactable = value;
            Fold_Btn.gameObject.SetActive(value);
        }
    }

    /// <summary>
    /// 設定跟注按鈕
    /// </summary>
    /// <param name="value"></param>
    public Sprite SetCallBetImage
    {
        set
        {
            if (CallBtn_Img != null)
            {
                CallBtn_Img.gameObject.SetActive(true);

                if (value != null)
                {
                    CallBtn_Img.sprite = value;
                    CallBtn_Img.preserveAspect = true;
                }
                else
                    CallBtn_Img.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 設定棄牌按鈕
    /// </summary>
    private Sprite foldBetImage
    {
        set
        {
            if (FoldBtn_Img != null)
            {
                FoldBtn_Img.gameObject.SetActive(true);

                if (value != null)
                {
                    FoldBtn_Img.sprite = value;
                    FoldBtn_Img.preserveAspect = true;
                }
                else
                    FoldBtn_Img.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 棄牌
    /// </summary>
    public void OnFold()
    {
        UpdateBetAction.Invoke(BetActingEnum.Fold, 0);
        gameData.isOnFold = true;
    }
    /// <summary>
    /// 取得當前棄牌按鈕狀態
    /// </summary>
    public Sprite FoldBtnImage()
    {
        return FoldBtn_Img.sprite;
    }
    /// <summary>
    /// 加注條顯示
    /// </summary>
    public bool ShowRaise
    {
        set
        {
            Raise_Tr.gameObject.SetActive(value);
        }
    }
    /// <summary>
    /// 設定加注至文字
    /// </summary>
    public double SetRaiseToText
    {
        set
        {
            if (value >= gameData.thisData.LocalPlayerChips)
            {
                //All In
                CurrRaise_Txt.text = LanguageManager.Instance.GetText("AllIn");
                RaiseSliHandle_Txt.text = LanguageManager.Instance.GetText("AllIn");
                AllIn_Btn.Select(); //allin按鈕亮起
                isAllinSelected = true;
            }
            else
            {
                CurrRaise_Txt.text = StringUtils.SetChipsUnit(value);
                RaiseSliHandle_Txt.text = "$" + StringUtils.SetChipsUnit(value);
                if (isAllinSelected) EventSystem.current.SetSelectedGameObject(null); //取消allin按鈕亮起
                isAllinSelected = false;
            }
        }
    }
    /// <summary>
    /// 設定跟注按鈕文字
    /// </summary>
    public string CallBtnText
    {
        get
        {
            return CallBtn_Txt.text;
        }
        set
        {
            CallBtn_Txt.text = setCallStr(value);
        }
    }
    /// <summary>
    /// 設定加注按鈕文字
    /// </summary>
    public string RaiseBtnText
    {
        set
        {
            RaiseBtn_Txt.text = value;
        }
    }
    /// <summary>
    /// 設置離/回座顯示
    /// </summary>
    public void SetSitOutDisplay()
    {
        if (!gameData.thisData.IsPlaying)
        {
            BackToSit_Btn.gameObject.SetActive(gameData.thisData.IsSitOut);
        }
    }
    /// <summary>
    /// 顯示行動按鈕
    /// </summary>
    public bool ShowActionBtns
    {
        set
        {
            actionButtonsMainObject.SetActive(value);
        }
    }
    /// <summary>
    /// 顯示手牌按鈕
    /// </summary>
    public void ShowPokerList()
    {
        foreach (var show in ShowPokerBtnList)
        {
            show.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 設置下注按鈕文字
    /// </summary>
    public void SetCallFoldBetStr(string btnName, int shapeIndex)
    {
        if (btnName == "Call" && CallBtn_Img == null)
            return;
        if (btnName == "Fold" && FoldBtn_Img == null)
            return;

        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            if (btnName == "Call")
            {
                SetCallBetImage = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteEnglish).album[shapeIndex];
                CallBtn_Img.transform.localPosition = shapeIndex == 0 ? new Vector2(CallBtn_Img.transform.localPosition.x, 5.95f) : originCallPos;
            }
            else
                foldBetImage = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteEnglish).album[shapeIndex];
        }
        else
        {
            if (btnName == "Call")
            {
                SetCallBetImage = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteChinese).album[shapeIndex];
                CallBtn_Img.transform.localPosition = shapeIndex == 0 ? new Vector2(CallBtn_Img.transform.localPosition.x, 5.95f) : originCallPos;
            }
            else
                foldBetImage = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.betSpriteChinese).album[shapeIndex];
        }
    }

    /// <summary>
    /// 自動操作判斷
    /// </summary>
    public IEnumerator JudgeAutoAction()
    {
        yield return new WaitForSeconds(0.2f);

        switch (AutoActionState)
        {
            //任何跟注
            case AutoActingEnum.CallAny:
                OnCallAndCheck();
                SetActionButton = false;
                break;

            //過牌
            case AutoActingEnum.Check:
                if (gameData.thisData.IsFirstRaisePlayer == true)
                {
                    OnCallAndCheck();
                    SetActionButton = false;
                }
                else
                {
                    if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
                    {
                        OnCallAndCheck();
                        SetActionButton = false;
                    }
                    else if (gameData.thisData.LocalPlayerCurrBetValue < gameData.thisData.SmallBlindValue * 2 &&
                             gameData.gameRoomData.currCallValue <= gameData.thisData.SmallBlindValue * 2 &&
                             gameData.gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind)
                    {
                        OnCallAndCheck();
                        SetActionButton = false;
                    }
                    else
                    {
                        ShowBetArea();
                        SetActionButton = gameData.gameRoomData.currActionerId == DataManager.UserId;
                    }
                }

                break;

            //過牌或棄牌
            case AutoActingEnum.CheckAndFold:
                if (gameData.thisData.IsFirstRaisePlayer == true)
                {
                    if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
                    {
                        OnCallAndCheck();
                    }
                    else
                    {
                        print("首次加注後棄牌");
                        OnFold();
                    }
                    SetActionButton = false;
                }
                else
                {
                    if (gameData.gameRoomData.currGameFlow == (int)GameFlowEnum.SetBlind)
                    {
                        print("未加注棄牌");
                        var gameControl = gameObject.transform.parent.GetComponent<GameControl>();
                        var localPlayer = gameControl.GetLocalPlayer();
                        bool isBigBlind = localPlayer.seatCharacter == (int)SeatCharacterEnum.BB;
                        if (isBigBlind)
                        {
                            if (gameData.thisData.CurrCallValue > gameData.thisData.SmallBlindValue * 2)
                                OnFold();
                            else
                                OnCallAndCheck();
                        }
                        else
                            OnFold();

                        SetActionButton = false;
                    }
                    else if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
                    {
                        print("當前下注金額等於當前跟注");
                        OnCallAndCheck();
                        SetActionButton = false;
                    }
                    else if (gameData.thisData.CurrCallValue <= gameData.thisData.SmallBlindValue * 2)
                    {
                        print("當前跟注金額小於大盲");
                        OnCallAndCheck();
                        SetActionButton = false;
                    }
                    else
                    {
                        print("未加注棄牌else");
                        OnFold();
                        SetActionButton = false;
                    }
                }
                break;
        }


        yield return null;

        AutoActionState = AutoActingEnum.None;
    }
    /// <summary>
    /// 顯示下注區塊
    /// </summary>
    public void ShowBetArea()
    {
        //是否無法在加注
        bool IsUnableRaise = gameData.thisData.IsUnableRaise;
        bool isJustAllIn = gameData.thisData.LocalPlayerChips <= gameData.thisData.CurrCallValue;
        bool isCanCall = gameData.thisData.isCanCall;

        //棄牌
        Fold_Btn.gameObject.SetActive(true);

        //加注&All In
        gameData.strData.RaiseStr = isJustAllIn == true ?
                           "AllIn" :
                           "Raise";
        gameData.strData.RaiseValueStr = isJustAllIn == true ?
                                $"\n${StringUtils.SetChipsUnit(gameData.thisData.LocalPlayerChips)}" :
                                "";
        RaiseBtn_Txt.text = LanguageManager.Instance.GetText(gameData.strData.RaiseStr);
        coinIconObj.SetActive(true);

        if (IsUnableRaise == true && isJustAllIn == false)
        {
            print("無法在加注");
            //Raise_Btn.gameObject.SetActive(false);
        }
        else
        {
            Raise_Btn.gameObject.SetActive(true);
        }

        //跟注&過牌
        gameData.strData.CallStr = "Call";
        gameData.strData.CallValueStr = $" {StringUtils.SetChipsUnit(gameData.thisData.CurrCallValue - gameData.thisData.CallDifference)}";
        //Debug.Log($"{nameof(ShowBetArea)} {gameData.thisData.CurrCallValue} currentCall :: {gameData.thisData.CurrRaiseValue} currentLocalRaise :: {gameData.thisData.LocalPlayerCurrBetValue} :: currentGlobalRaise {gameData.thisData.CurrRaiseValue} :: isCallOrRaise {gameData.thisData.isCanCall} :: Total Pot {gameData.thisData.TotalPot}");
        if (gameData.thisData.IsFirstRaisePlayer == true)
        {
            if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
            {
                //Debug.Log($"{nameof(ShowBetArea)} :: check");
                gameData.strData.CallStr = "Check";
                gameData.strData.CallValueStr = "";
            }
            else
            {
                //Debug.Log($"{nameof(ShowBetArea)} :: call");
                gameData.strData.CallStr = "Call";
                gameData.strData.CallValueStr = $" {StringUtils.SetChipsUnit(gameData.thisData.CallDifference)}";
            }
        }
        else
        {
            if (gameData.thisData.LocalPlayerCurrBetValue == gameData.thisData.CurrCallValue)
            {
                print("CallBtn: 是否等於當前跟注金額");
                //Debug.Log($"{nameof(ShowBetArea)} :: else check");
                gameData.strData.CallStr = "Check";
                gameData.strData.CallValueStr = "";
            }
            else
            {
                print("CallBtn: 是否不等於當前跟注金額");
                //Debug.Log($"{nameof(ShowBetArea)} :: else call");
                gameData.strData.CallStr = "Call";
                gameData.strData.CallValueStr = $" {StringUtils.SetChipsUnit(gameData.thisData.CallDifference)}";
            }
        }

        int keyC = 0;
        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            keyC = gameData.betStringsE.FirstOrDefault(x => x.Value == gameData.strData.CallStr).Key;
            SetCallFoldBetStr("Call", keyC);
            //print("跟注按鈕文字: " + betStringsE[keyC] + " " + gameData.strData.CallStr);
        }
        else
        {
            keyC = gameData.betStringsC.FirstOrDefault(x => x.Value == LanguageManager.Instance.GetText(gameData.strData.CallStr)).Key;
            SetCallFoldBetStr("Call", keyC);
            //print("跟注按鈕文字: " + betStringsE[keyC] + " " + gameData.strData.CallStr);
        }

        if (gameData.strData.CallValueStr != "" && int.Parse(gameData.strData.CallValueStr) > 0 && keyC == 0)
        //CallBtn_Txt.text = "$" + gameData.strData.CallValueStr;
        {
            print(keyC);
            CallBtn_Txt.text = setCallStr("$" + gameData.strData.CallValueStr);
        }
        else
            CallBtn_Txt.text = "";

        if (IsUnableRaise == true && isJustAllIn == false && isCanCall == true)
        {
            Call_Btn.gameObject.SetActive(true);
        }
        else
        {
            Call_Btn.gameObject.SetActive(isJustAllIn == false);
        }

        //加注區域物件
        ShowRaise = false;
        GameRoomManager.Instance.EnanbleBtn(false);
        SetMenuBtn.Invoke(true);
        if (isJustAllIn == false)
        {
            //倍數
            float multiple = (int)Mathf.Ceil((float)gameData.thisData.LocalPlayerChips / (float)(gameData.thisData.SmallBlindValue * 2));
            Raise_Sli.maxValue = (float)(gameData.thisData.SmallBlindValue * 2) * multiple;
            Raise_Sli.minValue = 0;
            Raise_Sli.value = 0;
            Raise_Sli.value = (float)gameData.thisData.MinRaiseValue;

            //加注值
            SetRaiseToText = gameData.thisData.MinRaiseValue;

            //最小加注值
            MinRaiseBtn_Txt.text = gameData.thisData.MinRaiseValue.ToString();

            AllinBtn_Txt.text = $"${gameData.thisData.LocalPlayerChips}";

            // Always display BB values and Pot for the raise options
            // Check if the current game flow is in the Preflop stage
            bool isPreflop = (GameFlowEnum)gameData.gameRoomData.currGameFlow == GameFlowEnum.Licensing || (GameFlowEnum)gameData.gameRoomData.currGameFlow == GameFlowEnum.SetBlind;

            for (int i = 0; i < 4; i++)
            {
                if (isPreflop)
                {
                    // Preflop: Display BB-based values (2BB, 3BB, 4BB, and Pot)
                    switch (i)
                    {
                        case 0:
                            PotPercentRaiseTxtList[i].text = "2BB";
                            break;
                        case 1:
                            PotPercentRaiseTxtList[i].text = "3BB";
                            break;
                        case 2:
                            PotPercentRaiseTxtList[i].text = "4BB";
                            break;
                        case 3:
                            PotPercentRaiseTxtList[i].text = LanguageManager.Instance.GetText("Pot");
                            break;
                    }
                }
                else
                {
                    // Postflop: Display percentage-based values
                    PotPercentRaiseTxtList[i].text = $"{gameData.PotPercentRate[i]}%";
                }
            }
        }
    }
    private string setCallStr(string str)
    {
        string s = "";
        List<string> strs = new List<string>();
        if (str != "" && str != " ")
        {
            for (int i = 0; i < str.Length; i++)
            {
                int index = i;
                if (index == 1)
                    continue;
                else
                    s = $"<sprite name=\"{str.Substring(index, 1)}\">";
                strs.Add(s);
            }
            strs.Insert(1, " ");
        }

        return string.Join("", strs);
    }

    private AutoActingEnum autoActingEnum;
}

/// <summary>
/// 自動操作
/// </summary>
public enum AutoActingEnum
{
    None,
    CallAny,
    Check,
    CheckAndFold,
}
