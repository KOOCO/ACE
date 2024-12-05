using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class pokerShapes : MonoBehaviour
{
    public static pokerShapes inst;

    private void Awake()
    {
        inst = this;

        JSBridgeManager.Instance.ReadDataFromFirebase("PokerShapes/-ODL14cFBIY9d4GKtSD_", gameObject.name, nameof(getShapeData));
    }

    public List<Shape> Shapes;

    public void getShapeData(string jsonData)
    {
        if (!string.IsNullOrEmpty(jsonData) && jsonData != "null")
        {
            var PS = JsonConvert.DeserializeObject<List<Shape>>(jsonData);
            Shapes = PS;
            print(jsonData);
        }
    }

    public void test()
    {
        Shape nullS = new Shape
        {
            shapeName = string.Empty,
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
