using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

using RequestBuf;

public class GameRoomManager : UnitySingleton<GameRoomManager>
{
    [SerializeField]
    GameObject gameServerObj;

    [Header("Canvas")]
    [SerializeField]
    Canvas GameRoomCanvas, SwtichBtnCanvas;

    [Header("房間")]
    [SerializeField]
    RectTransform GameRoomList_Tr;
    [SerializeField]
    ScrollRect GameRoomList_Sr;

    [Header("房間按鈕")]
    [SerializeField]
    RectTransform SwitchBtnSample, SwitchBtnParent, GoLobbyBtn_Tr;
    [SerializeField]
    Button GoLobby_Btn;

    [Header("背景遮罩")]
    [SerializeField]
    GameObject BgMask_Obj;

    [Header("遊戲房間物件")]
    [SerializeField]
    GameObject GameViewObj;

    public readonly int maxRoomCount = 2;
    public readonly float moveTargetDictance = 108;     //移動房間所需移動距離

    public bool IsShow;                                 //是否顯示遊戲房間

    private ThisData thisData;
    public class ThisData
    {
        /// <summary>
        /// (房間名, (房間View, 切換按鈕))
        /// </summary>
        public Dictionary<string, (RectTransform, SwitchRoomBtn)> RoomDic;

        public int CurrRoomIndex { get; set; }      //當前顯示房間編號
        public int RoomNameIndex;                   //當前房間編號(已開啟房間數量)
        public float AddSwitchBtnParnetWidth;       //切換按鈕父物件每單位寬度
        public bool IsRoomMoving;                   //是否房間正在移動
        public Vector2 MouseStartPos;               //滑鼠按下起始位置
        public List<int> SwitchBtnIndexList;        //切換按鈕房間編號
        public List<SwitchRoomBtn> SwitchBtnList;   //切換房間按鈕
    }

    public override void Awake()
    {
        base.Awake();

        Init();
        ListenerEnent();
    }

    private void Update()
    {
        if (GetRoomCount > 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                thisData.MouseStartPos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0) && 
                !thisData.IsRoomMoving && 
                GameRoomList_Sr.enabled == true &&
                BgMask_Obj.activeSelf)
            {
                //房間右移
                if (Input.mousePosition.x < thisData.MouseStartPos.x)
                {
                    if (GameRoomList_Tr.anchoredPosition.x < (-moveTargetDictance * (thisData.CurrRoomIndex)) &&
                        GetRoomCount > thisData.CurrRoomIndex + 1)
                    {
                        thisData.CurrRoomIndex++;                        
                    }

                    ChangeRoom(thisData.CurrRoomIndex);
                }

                //房間左移
                if (Input.mousePosition.x > thisData.MouseStartPos.x)
                {
                    if (GameRoomList_Tr.anchoredPosition.x < (-moveTargetDictance * (thisData.CurrRoomIndex)) &&
                        thisData.CurrRoomIndex > 0)
                    {
                        thisData.CurrRoomIndex--;
                        ChangeRoom(thisData.CurrRoomIndex);
                    }
                    else if (thisData.CurrRoomIndex == 0 && GameRoomList_Tr.anchoredPosition.x > moveTargetDictance)
                    {
                        //CurrRoomIndex = -1;
                        //OnGoLobby();
                    }
                    else
                    {
                        ChangeRoom(thisData.CurrRoomIndex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 獲取遊戲房間Canvas
    /// </summary>
    /// <returns></returns>
    public Canvas GetGameRoomCanvas()
    {
        return GameRoomCanvas;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        thisData = new ThisData();
        thisData.RoomDic = new Dictionary<string, (RectTransform, SwitchRoomBtn)>();
        thisData.AddSwitchBtnParnetWidth = SwitchBtnSample.rect.width + (SwitchBtnParent.GetComponent<HorizontalLayoutGroup>().spacing * 2);
        thisData.SwitchBtnIndexList = new List<int>();
        thisData.SwitchBtnList = new List<SwitchRoomBtn>();

        BgMask_Obj.SetActive(false);

        SwitchBtnSample.gameObject.SetActive(false);
        StartCoroutine(IJudgeShowSwitchBtn());
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEnent()
    {
        //新增房間按鈕(返回大廳)
        GoLobby_Btn.onClick.AddListener(() =>
        {
            OnGoLobby();
        });
    }

    /// <summary>
    /// 獲取房間數量
    /// </summary>
    public int GetRoomCount
    {
        get
        {
            return GameRoomList_Tr.childCount;
        }
    }

    /// <summary>
    /// 是否可以移動切換房間
    /// </summary>
    public bool IsCanMoveSwitch
    {
        set
        {            
            GameRoomList_Sr.enabled = value;
            SwtichBtnCanvas.sortingOrder = value == true ?
                                           100 :
                                           -1;
            
        }
    }

    /// <summary>
    /// 移動到大廳
    /// </summary>
    private void OnGoLobby()
    {
        BgMask_Obj.SetActive(false);
        IsShowGameRoom = false;
        CloseAllBtnFrame();
    }

    /// <summary>
    /// 顯示遊戲房間
    /// </summary>
    public bool IsShowGameRoom
    {
        get
        {
            return IsShow;
        }
        set
        {
            IsShow = value;

            if (IsShow)
            {
                JudgeShowGoLobbyBtn();
            }
            else
            {
                GoLobby_Btn.gameObject.SetActive(false);
            }
            
            GoLobby_Btn.interactable = value;

            GameRoomCanvas.sortingOrder = value == true ?
                                         50 :
                                         -1;
        }
    }

    /// <summary>
    /// 判斷是否可以創建房間
    /// </summary>
    /// <returns></returns>
    public bool JudgeIsCanBeCreateRoom()
    {
        return GetRoomCount < maxRoomCount;
    }

    /// <summary>
    /// 關閉所有切換按鈕選擇框
    /// </summary>
    public void CloseAllBtnFrame()
    {
        foreach (var room in thisData.RoomDic.Values)
        {
            room.Item2.SetSelectFrameActive = false;
        }
    }

    /// <summary>
    /// 獲取房間名稱
    /// </summary>
    /// <param name="roomType"></param>
    /// <returns></returns>
    private string GetRoomName(TableTypeEnum roomType)
    {
        string str = "";

        switch (roomType)
        {
            //積分房
            case TableTypeEnum.IntegralTable:
                str = "INTEGRAL";
                break;

            //現金桌
            case TableTypeEnum.Cash:
                str = "U";
                break;

            //虛擬桌
            case TableTypeEnum.VCTable:
                str = "A";
                break;
        }

        return str;
    }

    /// <summary>
    /// 創建遊戲房間
    /// </summary>
    /// <param name="pack"></param>
    /// <param name="roomType">房間類型</param>
    /// <param name="smallBlind">小盲值</param>
    /// <param name="queryRoomPath">查詢房間資料路徑</param>
    /// <param name="isNewRoom">是否是新房間</param>
    /// <param name="carryChips">攜帶籌碼</param>
    /// <param name="seatIndex">座位</param>
    /// <param name="pairPlayerId">積分被配對上的玩家ID</param>
    /// <param name="integralRoomName">積分房間名稱</param>
    public void CreateGameRoom(TableTypeEnum roomType, double smallBlind, string queryRoomPath, bool isNewRoom, double carryChips, int seatIndex, 
        string pairPlayerId = null, string integralRoomName = null)
    {
        IsShowGameRoom = true;
        thisData.RoomNameIndex++;

        if (GetRoomCount > maxRoomCount)
        {
            Debug.LogError("Room Count Max!!!");
            return;
        }

        //創建房間介面
        RectTransform room = Instantiate(GameViewObj).GetComponent<RectTransform>();
        room.gameObject.SetActive(true);
        room.SetParent(GameRoomList_Tr);
        string roomName = $"{roomType}{thisData.RoomNameIndex}";
        room.anchorMax = new Vector2(0, 1);
        room.anchorMin = new Vector2(0, 0);
        room.offsetMax = Vector2.zero;
        room.offsetMin = Vector2.zero;
        room.sizeDelta = new Vector2(Entry.Instance.resolution.x, 0);
        room.anchoredPosition = Vector2.zero;
        room.localScale = Vector3.one;
        room.eulerAngles = Vector3.zero;
        room.name = roomName;
        room.anchoredPosition = new Vector2(Entry.Instance.resolution.x * (GetRoomCount - 1), 0);

        //房間腳本
        GameView gameView = room.GetComponent<GameView>();
        gameView.RoomType = roomType;

        //遊戲控制
        GameControl gameControl = room.GetComponent<GameControl>();
        gameControl.QueryRoomPath = queryRoomPath;
        gameControl.SmallBlind = smallBlind;
        gameControl.RoomType = roomType;
        gameControl.MaxRoomPeople = roomType == TableTypeEnum.IntegralTable ?
                                    2 :
                                    DataManager.MaxPlayerCount;
        if (isNewRoom)
        {
            gameControl.CreateFirstPlayer(Math.Floor(carryChips),
                                          seatIndex,
                                          pairPlayerId,
                                          integralRoomName);

        }
        else
        {
            gameControl.NewPlayerInRoom(Math.Floor(carryChips), 
                                        seatIndex);
        }

        //關閉其他切換房間按鈕框
        CloseAllBtnFrame();

        //創建切換房間按鈕
        RectTransform switchBtnObj = Instantiate(SwitchBtnSample.gameObject).GetComponent<RectTransform>();
        switchBtnObj.gameObject.SetActive(true);
        switchBtnObj.SetParent(SwitchBtnParent);
        switchBtnObj.localScale = Vector3.one;
        SwitchRoomBtn switchRoomBtn = switchBtnObj.GetComponent<SwitchRoomBtn>();
        switchRoomBtn.SetSwitchBtnInfo(GetRoomName(roomType), thisData.RoomNameIndex);
        switchRoomBtn.SetSelectFrameActive = true;

        thisData.SwitchBtnList.Add(switchRoomBtn);
        thisData.SwitchBtnIndexList.Add(thisData.RoomNameIndex);
        thisData.CurrRoomIndex = GetRoomCount;
        thisData.RoomNameIndex++;
        thisData.RoomDic.Add(roomName, (room, switchRoomBtn));

        GoLobbyBtn_Tr.SetSiblingIndex(GetRoomCount + 1);

        BgMask_Obj.SetActive(true);
        IsCanMoveSwitch = true;

        StartCoroutine(IJudgeShowSwitchBtn());
    }

    /// <summary>
    /// 移除房間
    /// </summary>
    /// <param name="roomName">房間名</param>
    public void RemoveGameRoom(string roomName)
    {
        if (thisData.RoomDic.ContainsKey(roomName))
        {
            thisData.SwitchBtnIndexList.Remove(thisData.RoomDic[roomName].Item2.BtnIndex);
            thisData.SwitchBtnList.Remove(thisData.RoomDic[roomName].Item2);

            Destroy(thisData.RoomDic[roomName].Item1.gameObject);
            Destroy(thisData.RoomDic[roomName].Item2.gameObject);


            thisData.RoomDic.Remove(roomName); 
        }
        else
        {
            Debug.LogError($"{roomName}:移除房間出錯");
            return;
        }

        foreach (var room in thisData.RoomDic)
        {
            room.Value.Item1.anchoredPosition = new Vector2(Mathf.Max(0, room.Value.Item1.anchoredPosition.x - Entry.Instance.resolution.x),
                                                            room.Value.Item1.anchoredPosition.y);
        }

        StartCoroutine(IJudgeShowSwitchBtn());
    }

    /// <summary>
    /// 判斷顯示切換房間按鈕
    /// </summary>
    private IEnumerator IJudgeShowSwitchBtn()
    {
        yield return null;
        
        SwtichBtnCanvas.sortingOrder = GetRoomCount > 0 ?
                                       100 :
                                       -1;
        
        BgMask_Obj.SetActive(GetRoomCount > 0);
        IsCanMoveSwitch = GetRoomCount > 0;
        GameRoomManager.Instance.IsShowGameRoom = GetRoomCount > 0;

        //切換按鈕空間大小
        float sizeX = (GetRoomCount + 1) * thisData.AddSwitchBtnParnetWidth;
        SwitchBtnParent.sizeDelta = new Vector2(sizeX, SwitchBtnParent.sizeDelta.y);

        //房間列表
        GameRoomList_Tr.sizeDelta = new Vector2(Entry.Instance.resolution.x * GetRoomCount, 0);
        ChangeRoom(GetRoomCount - 1);

        JudgeShowGoLobbyBtn();
    }

    /// <summary>
    /// 判斷返回大廳按鈕顯示
    /// </summary>
    private void JudgeShowGoLobbyBtn()
    {
        GoLobby_Btn.gameObject.SetActive(GetRoomCount < maxRoomCount);
    }

    /// <summary>
    /// 更換房間按鈕點擊
    /// </summary>
    /// <param name="roomIndex"></param>
    public void SwitchBtnClick(int roomIndex)
    {
        int index = -1;

        for (int i = 0; i < thisData.SwitchBtnIndexList.Count; i++)
        {
            if (thisData.SwitchBtnIndexList[i] == roomIndex)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            Debug.LogError($"Switch Room Error");
            return;
        }
        else
        {
            ChangeRoom(index);
        }
    }

    /// <summary>
    /// 更換遊戲房間
    /// </summary>
    /// <param name="roomIndex"></param>
    public void ChangeRoom(int roomIndex)
    {
        if (roomIndex < 0 || thisData.RoomDic == null || thisData.RoomDic.Count == 0) return;

        thisData.CurrRoomIndex = roomIndex;
        CloseAllBtnFrame();
        IsShowGameRoom = true;
        BgMask_Obj.SetActive(true);

        StartCoroutine(IRoomMove());
    }

    /// <summary>
    /// 房間移動
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRoomMove()
    {
        thisData.IsRoomMoving = true;

        float moveTime = 0.1f;
        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < moveTime)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / moveTime;
            float x = Mathf.Lerp(GameRoomList_Tr.anchoredPosition.x, -Entry.Instance.resolution.x * thisData.CurrRoomIndex, progress);
            GameRoomList_Tr.anchoredPosition = new Vector2(x, GameRoomList_Tr.anchoredPosition.y);
            yield return null;
        }

        GameRoomList_Tr.anchoredPosition = new Vector2(-Entry.Instance.resolution.x * thisData.CurrRoomIndex, GameRoomList_Tr.anchoredPosition.y);
        thisData.SwitchBtnList[thisData.CurrRoomIndex].SetSelectFrameActive = true;
        thisData.IsRoomMoving = false;
    }

    /// <summary>
    /// 遊戲刷新
    /// </summary>
    public void OnUpdateGame()
    {
        foreach (var room in thisData.RoomDic)
        {
            room.Value.Item1.GetComponent<GameControl>().UpdateGameRoom();
        }
    }
}
