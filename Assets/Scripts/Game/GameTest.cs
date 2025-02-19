using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameServer;

public class GameTest : MonoBehaviour
{
    [Header("遊戲測試用")]
    [SerializeField]
    GameView gameView;
    [SerializeField]
    GameObject GameTest_Obj;
    [SerializeField]
    List<GameObject> PlayerTestObjList;
    [SerializeField]
    Button GameTestStart_Btn, Pause_Btn, addNull_Btn, addNew_Btn;
    [SerializeField]
    public List<TMP_Dropdown> CP_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> CP_NumTogList;
    [SerializeField]
    public List<TMP_Dropdown> PH0_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> PH1_SuitTogList;
    [SerializeField]
    public List<TMP_Dropdown> PN0_NumTogList;
    [SerializeField]
    public List<TMP_Dropdown> PN1_NumTogList;
    [SerializeField]
    public TMP_Dropdown pokerShapeDrop;
    [SerializeField]
    public List<TMP_Dropdown> robot_SuitTogList;
    public List<TMP_Dropdown> robot_NumTogList;
    public bool IsStartGameTest;                                //是否開始遊戲測試

    public GameControl gameControl;

    private void Awake()
    {
        EventListener();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            gameControl.CreateRobot(true);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //EditorReadRoomData();
            Debug.Log($"玩家是否為房主: {gameControl.gameRoomData.hostId == DataManager.UserId}");
        }

#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.X))
        {
            gameControl.RemoveRobot();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            string id = gameControl.gameRoomData.currActionerId;
            gameControl.UpdateBetAction(id,
                            BetActingEnum.Call,
                            gameControl.gameRoomData.currCallValue);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            string id = gameControl.gameRoomData.currActionerId;
            gameControl.UpdateBetAction(id,
                            BetActingEnum.Check,
                            0);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            string id = gameControl.gameRoomData.currActionerId;
            gameControl.UpdateBetAction(id,
                            BetActingEnum.Raise,
                            gameControl.gameRoomData.currCallValue + gameControl.gameRoomData.smallBlind);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            string id = gameControl.gameRoomData.currActionerId;
            gameControl.UpdateBetAction(id,
                            BetActingEnum.Fold,
                            0);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            string id = gameControl.gameRoomData.currActionerId;
            GameRoomPlayerData p = gameControl.gameRoomData.playerDataDic.Where(x => x.Value.userId == id)
                                                             .FirstOrDefault()
                                                             .Value;
            gameControl.UpdateBetAction(id,
                            BetActingEnum.AllIn,
                            p.carryChips);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //更新房主
            var dataDic = new Dictionary<string, object>()
                {
                     { FirebaseManager.ROOM_HOST_ID, "robot1"},
                };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{gameControl.QueryRoomPath}",
                                                            dataDic);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameControl.startRepeatEditorRead();
        }

#endif
    }
    private void EventListener()
    {
        //測試開始
        GameTestStart_Btn.onClick.AddListener(() =>
        {
            IsStartGameTest = true;
            gameView.gameControl.preUpdateGameFlow = GameFlowEnum.None;
            gameView.gameControl.preLocalGameFlow = GameFlowEnum.None;
            StartCoroutine(gameView.gameControl.IStartGameFlow(GameFlowEnum.Licensing));
            IsOpenGameTestObj = false;
        });

        Pause_Btn.onClick.AddListener(() =>
        {
            if (Time.timeScale != 0)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        });

        addNull_Btn.onClick.AddListener(pokerShapes.inst.addNullShape);

        addNew_Btn.onClick.AddListener(() =>
        {
            pokerShapes.inst.addNewShape(CP_SuitTogList, CP_NumTogList, robot_SuitTogList, robot_NumTogList);
        });

        pokerShapeDrop.onValueChanged.AddListener((value) =>
        {
            oneKeySet(pokerShapeDrop.options[value].text);
        });
    }
    /// <summary>
    /// 是否開啟遊戲測試操作
    /// </summary>
    public bool IsOpenGameTestObj
    {
        set
        {
            GameTest_Obj.SetActive(value);

            foreach (var item in PlayerTestObjList)
            {
                item.SetActive(false);
            }

            if (gameView.gameData.gameRoomData != null &&
                gameView.gameData.gameRoomData.playerDataDic != null)
            {
                foreach (var item in gameView.gameData.gameRoomData.playerDataDic)
                {
                    PlayerTestObjList[item.Value.gameSeat].SetActive(true);
                }

                for (int i = 1; i < PlayerTestObjList.Count; i++)
                {
                    int index = i;
                    if (PlayerTestObjList[index].activeSelf)
                    {
                        robot_SuitTogList.Add(PlayerTestObjList[index].transform.GetChild(0).GetChild(0).GetComponent<TMP_Dropdown>());
                        robot_SuitTogList.Add(PlayerTestObjList[index].transform.GetChild(1).GetChild(0).GetComponent<TMP_Dropdown>());
                        robot_NumTogList.Add(PlayerTestObjList[index].transform.GetChild(0).GetChild(1).GetComponent<TMP_Dropdown>());
                        robot_NumTogList.Add(PlayerTestObjList[index].transform.GetChild(1).GetChild(1).GetComponent<TMP_Dropdown>());
                        Debug.Log($"Adding Dropdown from {PlayerTestObjList[i].name}");
                        break;
                    }
                }

                updateShapeDropList();
            }
        }
    }
    ///<summary>
    ///遊戲測試工具設定
    /// </summary>
    public void testToolSetUp()
    {
        IsOpenGameTestObj = false;
        CP_SuitTogList.AddRange(PH0_SuitTogList);
        CP_SuitTogList.AddRange(PH1_SuitTogList);
        List<string> suitName = new()
        {
            "C",//梅花
            "D",//方塊
            "H",//紅心
            "S",//黑桃
        };
        for (int i = 0; i < CP_SuitTogList.Count; i++)
        {
            Utils.SetOptionsToDropdown(CP_SuitTogList[i], suitName);
        }

        List<string> numName = new()
        {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "J",
            "Q",
            "K",
            "A",
        };
        CP_NumTogList.AddRange(PN0_NumTogList);
        CP_NumTogList.AddRange(PN1_NumTogList);
        for (int i = 0; i < CP_NumTogList.Count; i++)
        {
            Utils.SetOptionsToDropdown(CP_NumTogList[i], numName);
        }
    }


    ///<summary>
    ///更新牌型Dropdown列表
    /// </summary>
    public void updateShapeDropList()
    {
        List<string> shapeName = new List<string>();
        foreach (var item in pokerShapes.inst.Shapes)
        {
            shapeName.Add(item.shapeName);
        }
        for (int i = 0; i < pokerShapes.inst.Shapes.Count; i++)
        {
            Utils.SetOptionsToDropdown(pokerShapeDrop, shapeName);
        }
    }

    ///<summary>
    ///設置一鍵設定小工具參數
    /// </summary>
    public void oneKeySet(string pokerShape)
    {
        int shapeIndex = -1;
        #region Old

        //switch (pokerShape)
        //{
        //    case "皇家同花順":
        //        List<int> shape = new List<int> { 1, 8, 9, 10, 12 };
        //        for (int i = 0; i < 5; i++)
        //        {
        //            CP_SuitTogList[i].value = 3;
        //            CP_NumTogList[i].value = shape[i];
        //        }
        //        CP_SuitTogList[5].value = 0;
        //        CP_SuitTogList[11].value = 1;
        //        CP_NumTogList[5].value = 12;
        //        CP_NumTogList[11].value = 11;
        //        robot_SuitTogList[0].value = 3;
        //        robot_SuitTogList[1].value = 2;
        //        robot_NumTogList[0].value = 11;
        //        robot_NumTogList[1].value = 9;
        //        break;
        //}

        #endregion
        for (int i = 0; i < pokerShapes.inst.Shapes.Count; i++)
        {
            int index = i;
            if (pokerShapes.inst.Shapes[index].shapeName == pokerShape)
            {
                shapeIndex = index;
                break;
            }
        }

        for (int i = 0; i < 5; i++)
        {
            CP_SuitTogList[i].value = pokerShapes.inst.Shapes[shapeIndex].Community[i].Suit;
            CP_NumTogList[i].value = pokerShapes.inst.Shapes[shapeIndex].Community[i].Num;
        }
        CP_SuitTogList[5].value = pokerShapes.inst.Shapes[shapeIndex].Local[0].Suit;
        CP_SuitTogList[11].value = pokerShapes.inst.Shapes[shapeIndex].Local[1].Suit;
        CP_NumTogList[5].value = pokerShapes.inst.Shapes[shapeIndex].Local[0].Num;
        CP_NumTogList[11].value = pokerShapes.inst.Shapes[shapeIndex].Local[1].Num;
        robot_SuitTogList[0].value = pokerShapes.inst.Shapes[shapeIndex].Robot[0].Suit;
        robot_SuitTogList[1].value = pokerShapes.inst.Shapes[shapeIndex].Robot[1].Suit;
        robot_NumTogList[0].value = pokerShapes.inst.Shapes[shapeIndex].Robot[0].Num;
        robot_NumTogList[1].value = pokerShapes.inst.Shapes[shapeIndex].Robot[1].Num;
    }

    //Test initGame
    public void initGameTest()
    {
        int poker;
        //52張撲克
        List<int> pokerList = new List<int>();
        for (int i = 0; i < 52; i++)
        {
            pokerList.Add(i);
        }

        foreach (var player in gameControl.gameRoomData.playerDataDic)
        {
            int[] handPoker = new int[2];

            for (int i = 0; i < 2; i++)
            {
                poker = Licensing();
                handPoker[i] = poker;
            }

            //更新玩家資料
            Dictionary<string, object> dataDic = new Dictionary<string, object>()
            {
                { FirebaseManager.HAND_POKER, handPoker.ToList()},              //手牌
                { FirebaseManager.CURR_ALL_BET_CHIPS, 0},                       //該回合總下注籌碼
                { FirebaseManager.ALL_BET_CHIPS, 0},                            //該局總下注籌碼
                { FirebaseManager.GAME_STATE, PlayerStateEnum.Playing},         //遊戲狀態(等待/遊戲中/棄牌/All In)
            };
            JSBridgeManager.Instance.UpdateDataFromFirebase($"{gameControl.QueryRoomPath}/{FirebaseManager.PLAYER_DATA_LIST}/{player.Key}",
                                                            dataDic);
        }

        //發牌
        int Licensing()
        {
            int index = new System.Random().Next(0, pokerList.Count);
            int poker = pokerList[index];
            pokerList.RemoveAt(index);
            return poker;
        }
    }
}
