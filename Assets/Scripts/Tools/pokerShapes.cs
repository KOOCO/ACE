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
                gameView.GameTest.updateShapeDropList();
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
        //string r1 = string.Join(", ", CP_SuitTogList.Select(obj => obj.value));
        //string r2 = string.Join(", ", CP_NumTogList.Select(obj => obj.value));
        //print(r1);
        //print(r2);

        List<Card> newCom = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 5).ToList();
        List<Card> newLoc = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 2).ToList();
        List<Card> newRob = Enumerable.Repeat(new Card { Suit = 0, Num = 0 }, 2).ToList();
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            newCom[index] = new Card();
            newCom[index].Suit = CP_SuitTogList[index].value;
            newCom[index].Num = CP_NumTogList[index].value;
            print($"newCom[{index}].Suit = {newCom[index].Suit}, newCom[{index}].Num = {newCom[index].Num}");
        }
        newLoc[0] = new Card();
        newLoc[1] = new Card();
        newLoc[0].Suit = CP_SuitTogList[5].value;
        newLoc[1].Suit = CP_SuitTogList[11].value;
        newLoc[0].Num = CP_NumTogList[5].value;
        newLoc[1].Num = CP_NumTogList[11].value;
        for (int i = 0; i < 2; i++)
        {
            int index = i;
            newRob[index] = new Card();
            newRob[index].Suit = robot_SuitTogList[index].value;
            newRob[index].Num = robot_NumTogList[index].value;
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
        int Index = Shapes.Count - 1;
        JSBridgeManager.Instance.UpdateDataToFirebase($"PokerShapes/-ODL14cFBIY9d4GKtSD_/{Index}", data);
    }

    [EButton]
    public void addNewShape()
    {
        for (int i = 0; i < Shapes.Count; i++)
        {
            int index = i;
            string data = JsonConvert.SerializeObject(Shapes[index]);
            JSBridgeManager.Instance.UpdateDataToFirebase($"PokerShapes/-ODL14cFBIY9d4GKtSD_/{index}", data);
        }
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
