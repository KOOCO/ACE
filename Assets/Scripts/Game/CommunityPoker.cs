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
    /// <summary>
    /// 顯示公牌
    /// </summary>
    public void Show(int index, bool isShow)
    {
        CommunityPokerList[index].gameObject.SetActive(isShow);
    }
    /// <summary>
    /// 設定公牌數值
    /// </summary>
    public void Set(int index,int num)
    {
        CommunityPokerList[index].PokerNum = num;
    }
    /// <summary>
    /// 獲取公牌List
    /// </summary>
    public List<Poker> GetList()
    {
        return CommunityPokerList;
    }
    /// <summary>
    /// 獲取公牌數值
    /// </summary>
    public Poker GetPoker(int index)
    {
        return CommunityPokerList[index];
    }
}
