using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Proyecto26;
using System.Threading.Tasks;

public class JoinRoomView : MonoBehaviour
{
    [SerializeField]
    Request_JoinRoom baseRequest;
    [SerializeField]
    Image SB_Img, BB_Img;
    [SerializeField]
    Slider BuyChips_Sli;
    [SerializeField]
    Button Cancel_Btn, Buy_Btn, BuyPlus_Btn, BuyMinus_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt, BlindsTitle_Txt,
                    SB_Txt, BB_Txt, PreBuyChips_Txt,
                    MinBuyChips_Txt, MaxBuyChips_Txt,
                    CancelBtn_Txt, BuyBtn_Txt;
    [SerializeField]
    SliderClickDetection sliderClickDetection;

    string dataRoomName;                 //查詢資料的房間名稱
    double smallBlind;                   //小盲值
    TableTypeEnum tableType;             //房間類型

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        BlindsTitle_Txt.text = LanguageManager.Instance.GetText("Blinds");
        CancelBtn_Txt.text = LanguageManager.Instance.GetText("Cancel");
        BuyBtn_Txt.text = LanguageManager.Instance.GetText("Buy");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    public void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //取消
        Cancel_Btn.onClick.AddListener(() =>
        {
            GameRoomManager.Instance.IsCanMoveSwitch = true;
            gameObject.SetActive(false);
        });

        //購買
        Buy_Btn.onClick.AddListener(() =>
        {
            ViewManager.Instance.OpenWaitingView(transform);

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
            double newRaiseValue = TexasHoldemUtil.SliderValueChange(BuyChips_Sli,
                                                                    value,
                                                                    smallBlind * 2,
                                                                    BuyChips_Sli.minValue,
                                                                    BuyChips_Sli.maxValue,
                                                                    sliderClickDetection);
            PreBuyChips_Txt.text = StringUtils.SetChipsUnit(newRaiseValue);
        });

        //購買+按鈕
        BuyPlus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value += (float)smallBlind * 2;
        });

        //購買-按鈕
        BuyMinus_Btn.onClick.AddListener(() =>
        {
            BuyChips_Sli.value -= (float)smallBlind * 2;
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
        string maxBuyChipsStr = "";
        switch (tableType)
        {
            //現金桌
            case TableTypeEnum.Cash:
                titleStr = "Classic Battle";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserUChips)}";
                SB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[0];
                BB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[0];
                break;

            //虛擬貨幣桌
            case TableTypeEnum.VCTable:
                titleStr = "High Roller Battleground";
                maxBuyChipsStr = $"{StringUtils.SetChipsUnit(DataManager.UserAChips)}";
                SB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[1];
                BB_Img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[1];
                break;
        }
        Title_Txt.text = LanguageManager.Instance.GetText(titleStr);

        SB_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind)}";
        BB_Txt.text = $"{StringUtils.SetChipsUnit(smallBlind * 2)}";

        TexasHoldemUtil.SetBuySlider(this.smallBlind, BuyChips_Sli, tableType);
        MinBuyChips_Txt.text = $"{StringUtils.SetChipsUnit(this.smallBlind * DataManager.MinMagnification)}";
        MaxBuyChips_Txt.text = maxBuyChipsStr;
    }

    /// <summary>
    /// 加入房間查詢回傳
    /// </summary>
    /// <param name="jsonData">回傳資料</param>
    public void JoinRoomQueryCallback(string jsonData)
    {
        QueryRoom queryRoom = FirebaseManager.Instance.OnFirebaseDataRead<QueryRoom>(jsonData);

        //錯誤
        if (!string.IsNullOrEmpty(queryRoom.error) )
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
                                                BuyChips_Sli.value,
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
                                                BuyChips_Sli.value,
                                                seat);

        ViewManager.Instance.CloseWaitingView(transform);
        gameObject.SetActive(false);
    }
}
