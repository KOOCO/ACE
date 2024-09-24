using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;
using Proyecto26;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class JoinRoomView : MonoBehaviour
{
    [SerializeField]
    Request_JoinRoom baseRequest;
    [SerializeField]
    Image BlindACoin_Img, BlindUCoin_Img,
          MinBuyACoin_Img, MinBuyUCoin_Img,
          MaxBuyACoin_Img, MaxBuyUCoin_Img;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Close_Btn, Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, BlindsTitle_Txt,
                    Blind_Txt, PreBuyChips_Txt,
                    MinBuyChips_Txt, MaxBuyChips_Txt,
                    CancelBtn_Txt, BuyBtn_Txt;
    [SerializeField]
    SliderClickDetection sliderClickDetection;

    LobbyView lobbyView;
    string dataRoomName;                 //查詢資料的房間名稱
    double smallBlind;                   //小盲值
    TableTypeEnum tableType;             //房間類型
    double newCarryChipsValue;           //更新後的購買籌碼

    bool isClassic;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        BlindsTitle_Txt.text = LanguageManager.Instance.GetText("Blind Bet");
        CancelBtn_Txt.text = LanguageManager.Instance.GetText("CANCEL");
        BuyBtn_Txt.text = LanguageManager.Instance.GetText("CONFIRM");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    public void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        lobbyView = GameObject.FindAnyObjectByType<LobbyView>();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //關閉
        Close_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            gameObject.SetActive(false);
        });

        //取消
        Cancel_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = false;
            gameObject.SetActive(false);
        });

        //購買
        Buy_Btn.onClick.AddListener(() =>
        {
            //籌碼不足
            if (tableType == TableTypeEnum.Cash &&
                newCarryChipsValue > DataManager.UserUChips)
            {
                ViewManager.Instance.OpenTipMsgView(lobbyView.transform, LanguageManager.Instance.GetText("Not enough chips."));
                gameObject.SetActive(false);
                return;
            }
            else if (tableType == TableTypeEnum.VCTable &&
                     newCarryChipsValue > DataManager.UserAChips)
            {
                ViewManager.Instance.OpenTipMsgView(lobbyView.transform, LanguageManager.Instance.GetText("Not enough chips."));
                gameObject.SetActive(false);
                return;
            }

            ViewManager.Instance.OpenWaitingView(transform);
            lobbyView.audioSource.Stop();
            JoinRound newRound = new JoinRound
            {
                memberId = DataManager.UserId,
                tableId = DataManager.TableId,
                amount = newCarryChipsValue
            };
            Debug.Log($"MemberId {newRound.memberId} :: TableId {newRound.tableId} :: Amount {newRound.amount}");

            //ViewManager.Instance.OpenWaitingView(transform);
            SwaggerAPIManager.Instance.SendPostAPI<JoinRound>($"api/app/rounds/join-round?memberId={newRound.memberId}&tableId={newRound.tableId}&amount={newRound.amount}", newRound, (data) =>
            {
                Debug.Log("Join Round Response :: " + data);
                GameRound gameRound = JsonConvert.DeserializeObject<GameRound>(data);
                var _currencyType = DataManager.CurrencyType;
                Debug.Log("Currency Type :: " + _currencyType);
                switch (_currencyType)
                {
                    case CurrencyType.Gold:
                        Debug.Log(_currencyType);
                        DataManager.UserGold -= newCarryChipsValue;
                        break;
                    case CurrencyType.ACoin:
                        Debug.Log(_currencyType);
                        DataManager.UserAChips -= newCarryChipsValue;
                        break;
                    case CurrencyType.UCoin:
                        Debug.Log(_currencyType);
                        DataManager.UserUChips -= newCarryChipsValue;
                        break;
                }
                DataManager.DataUpdated = true;
            }, null, true, true);

#if UNITY_EDITOR

            dataRoomName = "EditorRoom";
            //創新房間資料
            var dataDic = new Dictionary<string, object>()
            {
                { FirebaseManager.SMALL_BLIND, smallBlind},                         //小盲值
                { FirebaseManager.ROOM_HOST_ID, DataManager.UserId},                //房主ID
                { FirebaseManager.POT_CHIPS, 0},                                    //底池總籌碼
                { FirebaseManager.COMMUNITY_POKER, new List<int>()},                //公共牌
                { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>()},           //當前公共牌
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}/{dataRoomName}",
                dataDic,
                gameObject.name,
                nameof(CreateNewRoomCallback));
            return;
#endif

            JSBridgeManager.Instance.JoinRoomQueryData($"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}",
                                                        $"{DataManager.MaxPlayerCount}",
                                                        $"{DataManager.UserId}",
                                                        gameObject.name,
                                                        nameof(JoinRoomQueryCallback));

        });

        //購買Slider單位設定
        BuyChips_Sli.onValueChanged.AddListener((value) =>
        {
            newCarryChipsValue = TexasHoldemUtil.SliderValueChange(BuyChips_Sli,
                                                        value,
                                                        smallBlind * 2,
                                                        BuyChips_Sli.minValue,
                                                        BuyChips_Sli.maxValue,
                                                        sliderClickDetection);
            PreBuyChips_Txt.text = StringUtils.SetChipsUnit(newCarryChipsValue);
        });

        //購買+按鈕
        BuyPlus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newCarryChipsValue + smallBlind * 2);
        });

        //購買-按鈕
        BuyMinus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value = (float)(newCarryChipsValue - smallBlind * 2);
        });
    }

    /// <summary>
    /// 設定創建房間介面
    /// </summary>
    /// <param name="tableType">遊戲桌類型</param>
    /// <param name="smallBlind">小盲值</param>
    public void SetCreatRoomViewInfo(TableTypeEnum tableType, double smallBlind)
    {
        this.smallBlind = smallBlind;
        this.tableType = tableType;

        string titleStr = "";
        switch (tableType)
        {
            //現金桌
            case TableTypeEnum.Cash:
                titleStr = "High Roller Battleground";
                isClassic = false;
                BlindACoin_Img.gameObject.SetActive(true);
                BlindUCoin_Img.gameObject.SetActive(false);
                MinBuyACoin_Img.gameObject.SetActive(true);
                MinBuyUCoin_Img.gameObject.SetActive(false);
                MaxBuyACoin_Img.gameObject.SetActive(true);
                MaxBuyUCoin_Img.gameObject.SetActive(false);
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "Classic Battle";
                isClassic = true;
                BlindACoin_Img.gameObject.SetActive(false);
                BlindUCoin_Img.gameObject.SetActive(true);
                MinBuyACoin_Img.gameObject.SetActive(false);
                MinBuyUCoin_Img.gameObject.SetActive(true);
                MaxBuyACoin_Img.gameObject.SetActive(false);
                MaxBuyUCoin_Img.gameObject.SetActive(true);
                break;
        }
        Title_Txt.text = LanguageManager.Instance.GetText(titleStr);

        Blind_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)} / " +
                         $"{StringUtils.SetChipsUnit(smallBlind * 2)}";

        if (isClassic)
            TexasHoldemUtil.SetBuySlider(this.smallBlind * 2, DataManager.UserAChips < ((this.smallBlind * 2) * DataManager.MaxMagnification) ? DataManager.UserAChips : (this.smallBlind * 2) * DataManager.MaxMagnification, BuyChips_Sli, tableType);
        else
            TexasHoldemUtil.SetBuySlider(this.smallBlind * 2, DataManager.UserUChips < ((this.smallBlind * 2) * DataManager.MaxMagnification) ? DataManager.UserUChips : (this.smallBlind * 2) * DataManager.MaxMagnification, BuyChips_Sli, tableType);

        MinBuyChips_Txt.text = $"{StringUtils.SetChipsUnit((this.smallBlind * 2) * DataManager.MinMagnification)}";
        MaxBuyChips_Txt.text = $"{StringUtils.SetChipsUnit((this.smallBlind * 2) * DataManager.MaxMagnification)}"; ;
    }

    /// <summary>
    /// 加入房間查詢回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void JoinRoomQueryCallback(string jsonData)
    {
        QueryRoom queryRoom = FirebaseManager.Instance.OnFirebaseDataRead<QueryRoom>(jsonData);

        //錯誤
        if (!string.IsNullOrEmpty(queryRoom.error))
        {
            Debug.LogError(queryRoom.error);
            return;
        }

        if (queryRoom.getRoomName == "false")
        {
            //沒有找到房間
            Debug.Log($"沒有找到房間:{queryRoom.roomCount}");
            string roomToken = StringUtils.GenerateRandomString(DataManager.RoomTokenLength);
            dataRoomName = $"{FirebaseManager.ROOM_NAME}{queryRoom.roomCount + 1}_{roomToken}";

            //創新房間資料
            var dataDic = new Dictionary<string, object>()
            {
                { FirebaseManager.SMALL_BLIND, smallBlind},                         //小盲值
                { FirebaseManager.ROOM_HOST_ID, DataManager.UserId},                //房主ID
                { FirebaseManager.POT_CHIPS, 0},                                    //底池總籌碼
                { FirebaseManager.COMMUNITY_POKER, new List<int>()},                //公共牌
                { FirebaseManager.CURR_COMMUNITY_POKER, new List<int>()},           //當前公共牌
            };
            JSBridgeManager.Instance.WriteDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}/{dataRoomName}",
                dataDic,
                gameObject.name,
                nameof(CreateNewRoomCallback));
        }
        else
        {
            //有房間
            dataRoomName = queryRoom.getRoomName;
            JSBridgeManager.Instance.ReadDataFromFirebase(
                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}/{dataRoomName}",
                gameObject.name,
                nameof(JoinRoomCallback));
        }
    }

    /// <summary>
    /// 創建新房間回傳
    /// </summary>
    /// <param name="isSuccess">創建/加入房間回傳結果</param>
    public void CreateNewRoomCallback(string isSuccess)
    {
        //錯誤
        if (isSuccess == "false")
        {
            ViewManager.Instance.CloseWaitingView(transform);
            Debug.LogError("Create Room Error!!!");
            return;
        }

        StartCoroutine(IYieldInCreateRoom());
    }

    /// <summary>
    /// 延遲創建房間
    /// </summary>
    /// <returns></returns>
    private IEnumerator IYieldInCreateRoom()
    {
        yield return new WaitForSeconds(0.2f);

        GameRoomManager.Instance.CreateGameRoom(tableType,
                                                smallBlind,
                                                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}/{dataRoomName}",
                                                true,
                                                newCarryChipsValue,
                                                0);

        ViewManager.Instance.CloseWaitingView(transform);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 加入房間回傳
    /// </summary>
    /// <param name="jsonData">房間資料</param>
    public void JoinRoomCallback(string jsonData)
    {
        var gameRoomData = FirebaseManager.Instance.OnFirebaseDataRead<GameRoomData>(jsonData);
        int seat = TexasHoldemUtil.SetGameSeat(gameRoomData);

        //本地創建房間
        GameRoomManager.Instance.CreateGameRoom(tableType,
                                                smallBlind,
                                                $"{Entry.Instance.releaseType}/{FirebaseManager.ROOM_DATA_PATH}{tableType}/{smallBlind}/{dataRoomName}",
                                                false,
                                                newCarryChipsValue,
                                                seat);

        ViewManager.Instance.CloseWaitingView(transform);
        gameObject.SetActive(false);
    }
}
