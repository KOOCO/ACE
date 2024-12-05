using System.Collections.Generic;
using UnityEngine;

public class pokerShapes : MonoBehaviour
{
    public static pokerShapes inst;

    private void Awake()
    {
        inst = this;
    }

    public List<Shape> Shapes;
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
