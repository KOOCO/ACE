using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class pokerShapes : MonoBehaviour
{
    public static pokerShapes inst;

    bool isListenered;

    public List<Shape> Shapes;

    private void Awake()
    {
        inst = this;

#if UNITY_EDITOR
        JSBridgeManager.Instance.ReadDataFromFirebase("PokerShapes/-ODL14cFBIY9d4GKtSD_", gameObject.name, nameof(getShapeData));
#endif

#if !UNITY_EDITOR
        if (!isListenered)
        {
            isListenered = true;
            JSBridgeManager.Instance.StartListeningForDataChanges("PokerShapes/-ODL14cFBIY9d4GKtSD_", 
                gameObject.name, 
                nameof(getShapeData));
        }
#endif
    }

    public void getShapeData(string jsonData)
    {
        if (!string.IsNullOrEmpty(jsonData) && jsonData != "null")
        {
            var PS = JsonConvert.DeserializeObject<List<Shape>>(jsonData);
            Shapes = PS;
            //print(jsonData);
            GameView gameView = GameRoomManager.Instance.GameRoomList_Tr.GetComponentInChildren<GameView>();
            if (gameView != null)
                gameView.updateShapeDropList();
        }
    }

    public void addNullShape()
    {
        Shape nullS = new Shape
        {
            shapeName = "New shape",
            Community = new List<Card>
            {
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 }
            },
            Local = new List<Card>
            {
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 }
            },
            Robot = new List<Card>
            {
                new Card { Suit = 0, Num = 0 },
                new Card { Suit = 0, Num = 0 }
            }
        };
        string data = JsonConvert.SerializeObject(nullS);
        Shapes.Add(nullS);
        int index = Shapes.Count - 1;
        JSBridgeManager.Instance.UpdateDataToFirebase($"PokerShapes/-ODL14cFBIY9d4GKtSD_/{index}", data);
    }
    
    public void addNewShape(List<TMP_Dropdown> CP_SuitTogList, List<TMP_Dropdown> CP_NumTogList, List<TMP_Dropdown> robot_SuitTogList, List<TMP_Dropdown> robot_NumTogList)
    {
        List<Card> newCom = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 5).ToList();
        List<Card> newLoc = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 2).ToList();
        List<Card> newRob = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 2).ToList();
        for (int i = 0; i < 5; i++)
        {
            newCom[i].Suit = CP_SuitTogList[i].value;
            newCom[i].Num = CP_NumTogList[i].value;
        }
        newLoc[0].Suit = CP_SuitTogList[5].value;
        newLoc[1].Suit = CP_SuitTogList[11].value;
        newLoc[0].Num = CP_NumTogList[5].value;
        newLoc[1].Num = CP_NumTogList[11].value;
        for (int i = 0; i < 2; i++)
        {
            newRob[i].Suit = robot_SuitTogList[i].value;
            newRob[i].Num = robot_NumTogList[i].value;
        }

        Shape newS = new Shape
        {
            shapeName = "New custom shape",
            Community = newCom,
            Local = newLoc,
            Robot = newRob
        };
        string data = JsonConvert.SerializeObject(newS);
        Shapes.Add(newS);
        int index = Shapes.Count - 1;
        JSBridgeManager.Instance.UpdateDataToFirebase($"PokerShapes/-ODL14cFBIY9d4GKtSD_/{index}", data);
    }
}

[System.Serializable]
public class Shape
{
    public string shapeName;
    public List<Card> Community;
    public List<Card> Local;
    public List<Card> Robot;
}

[System.Serializable]
public class Card
{
    public int Suit;
    public int Num;
}
