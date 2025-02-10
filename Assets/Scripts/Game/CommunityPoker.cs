using System.Collections.Generic;
using UnityEngine;

public class CommunityPoker : MonoBehaviour
{
    [SerializeField]
    List<Poker> CommunityPokerList = new();


    public void Init()
    {
        foreach (var poker in CommunityPokerList)
        {
            poker.gameObject.SetActive(false);
            poker.SetColor = 1;
        }
    }
    public void Show(int index, bool isShow)
    {
        CommunityPokerList[index].gameObject.SetActive(isShow);
    }
    public void Set(int index,int num)
    {
        CommunityPokerList[index].PokerNum = num;
    }
    public List<Poker> GetList()
    {
        return CommunityPokerList;
    }
    public Poker GetPoker(int index)
    {
        return CommunityPokerList[index];
    }
}
