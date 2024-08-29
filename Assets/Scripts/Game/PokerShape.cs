using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public static class PokerShape
{
    /// <summary>
    /// 牌型名稱
    /// </summary>
    public static string[] shapeStr = new string[]
    {
        "RoyalFlush",           //皇家同花順
        "StraightFlush",        //同花順
        "FourOfKind",           //四條
        "FullHouse",            //葫蘆
        "Flush",                //同花
        "Straight",             //皇家大順
        "Straight",             //順子
        "ThreeOfAKind",         //三條
        "TwoPairs",             //兩對
        "OnePair",              //一對
        "HightCard",            //高牌
    };

    /// <summary>
    /// 判斷牌型
    /// </summary>
    /// <param name="judgePokerList"></param>
    /// <param name="callBack">回傳(結果，符合的牌)</param>
    public static void JudgePokerShape(List<int> judgePokerList, UnityAction<int, List<int>> callBack)
    {
        //照花色分類
        Dictionary<int, List<int>> groupPoker = new Dictionary<int, List<int>>();
        foreach (var poker in judgePokerList)
        {
            //花色
            int suit = poker / 13;
            //數字
            int rank = poker % 13 + 1;

            if (!groupPoker.ContainsKey(rank))
            {
                groupPoker[rank] = new List<int>();
            }

            groupPoker[rank].Add(suit);
        }

        //檢查皇家大順
        List<int> royalStraightList = new List<int>();
        bool royalStraight = groupPoker.ContainsKey(10) &&
                             groupPoker.ContainsKey(11) &&
                             groupPoker.ContainsKey(12) &&
                             groupPoker.ContainsKey(13) &&
                             groupPoker.ContainsKey(1);
        if (royalStraight)
        {
            royalStraightList.Add(0 + (13 * groupPoker[1][0]));
            for (int i = 12; i >= 9; i--)
            {
                royalStraightList.Add(i + (13 * groupPoker[10][0]));
            }            
        }

        //檢查皇家同花順
        List<int> royalFlushList = new List<int>();
        if (royalStraight)
        {
            for (int i = 0; i < groupPoker[10].Count(); i++)
            {
                bool isFind = true;
                int comparisonSuit = groupPoker[10][i];
                royalFlushList.Clear();
                royalFlushList.Add((10 - 1) + (13 * comparisonSuit));

                for (int j = 11; j <= 13; j++)
                {
                    if (!groupPoker[j].Contains(comparisonSuit))
                    {
                        isFind = false;
                        break;
                    }

                    royalFlushList.Add((j - 1) + (13 * comparisonSuit));
                }
                if (!groupPoker[1].Contains(comparisonSuit))
                {
                    isFind = false;
                    royalFlushList.Clear();
                }
                else
                {
                    royalFlushList.Add(0 + (13 * comparisonSuit));
                }

                if (isFind) break;
            }
        }

        //檢查同花
        List<int> flushList = new List<int>();
        List<int> straightFlushList = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (groupPoker.Where(kv => kv.Value.Contains(i)).Count() >= 5)
            {
                for (int j = 13; j >= 1; j--)
                {
                    if (groupPoker.ContainsKey(j) && groupPoker[j].Contains(i))
                    {
                        flushList.Add((j - 1) + (13 * i));
                    }
                }

                //檢查同花順
                for (int n = 13; n >= 5; n--)
                {
                    if (flushList.Contains(((n - 1) + (13 * i))) &&
                        flushList.Contains(((n - 1) + (13 * i)) - 1) &&
                        flushList.Contains(((n - 1) + (13 * i)) - 2) &&
                        flushList.Contains(((n - 1) + (13 * i)) - 3) &&
                        flushList.Contains(((n - 1) + (13 * i)) - 4))
                    {
                        for (int sf = 0; sf < 5; sf++)
                        {
                            straightFlushList.Add((n - 1) + (13 * i) - sf);
                        }
                        break;
                    }
                }

                break;
            }
        }
        //排列同花大小
        flushList = flushList.OrderByDescending(n => (n % 13 == 0) ? int.MaxValue : (n % 13 + 1)).ToList();

        //檢查順子
        List<int> straightList = new List<int>();
        for (int i = 13; i >= 5; i--)
        {
            if (groupPoker.ContainsKey(i) &&
                groupPoker.ContainsKey(i - 1) &&
                groupPoker.ContainsKey(i - 2) &&
                groupPoker.ContainsKey(i - 3) &&
                groupPoker.ContainsKey(i - 4))
            {
                for (int j = i; j >= i - 4; j--)
                {
                    int num = (j - 1) + (13 * groupPoker[j][0]);
                    straightList.Add(num);
                }
                break;
            }
        }

        //檢查四條
        List<int> quadsList = JudgePairs(4);
        //檢查三條
        List<int> triplesList = JudgePairs(3);
        //檢查對子
        List<int> pairsList = JudgePairs(2);

        //檢查高牌
        List<int> hightCardList = new List<int>();
        if (groupPoker.ContainsKey(1))
        {
            int max = 0 + (13 * groupPoker[1][0]);
            hightCardList.Add(max);
        }
        else
        {
            int max = groupPoker.Max(kv => kv.Key);
            max = (max - 1) + (13 * groupPoker[max][0]);
            hightCardList.Add(max);
        }

        //回傳結果
        if (royalFlushList.Count == 5)
        {
            //皇家同花順
            callBack(0, royalFlushList);
        }
        else if (straightFlushList.Count == 5)
        {
            //同花順
            callBack(1, straightFlushList);
        }
        else if (quadsList.Count == 4)
        {
            //四條
            callBack(2, quadsList);
        }
        else if (triplesList.Count >= 3 && pairsList.Count >= 2)
        {
            //葫蘆
            List<int> triples = triplesList.Take(3).ToList();
            List<int> pairs = pairsList.Take(2).ToList();
            List<int> fullHouseList = triples.Concat(pairs).ToList();
            callBack(3, fullHouseList);
        }
        else if (flushList.Count >= 5)
        {
            //同花
            flushList = flushList.Take(5).ToList();
            callBack(4, flushList);
        }
        else if (royalStraightList.Count == 5)
        {
            //皇家大順
            callBack(5, royalStraightList);
        }
        else if (straightList.Count == 5)
        {
            //順子
            callBack(6, straightList);
        }
        else if (triplesList.Count >= 3)
        {
            //三條
            List<int> threeOfAKindList = triplesList.Take(3).ToList();
            callBack(7, threeOfAKindList);
        }
        else if (pairsList.Count >= 4)
        {
            //兩對
            List<int> TowPairsList = pairsList.Take(4).ToList();
            callBack(8, TowPairsList);
        }
        else if (pairsList.Count == 2)
        {
            //一對
            List<int> onePairsList = pairsList.Take(2).ToList();
            callBack(9, onePairsList);
        }
        else
        {
            //高牌
            callBack(10, hightCardList);
        }


        // 檢查對子、三條、四條
        List<int> JudgePairs(int pairs)
        {
            List<int> pokerLiset = new List<int>();
            List<int> list = groupPoker.Where(kv => kv.Value.Count == pairs).Select(kv => kv.Key).ToList();
            if (list.Count > 0)
            {
                list = list.OrderByDescending(x => x).ToList();
                if (list.Count >= 2 && list[list.Count() - 1] == 1)
                {
                    //A最大，排列至前頭
                    List<int> temp = new List<int>(list);
                    list.Clear();
                    list.Add(1);
                    for (int i = 0; i < temp.Count; i++)
                    {
                        list.Add(temp[i]);
                    }
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int judgeNum = (list[i] - 1) + (13 * j);
                    if (judgePokerList.Contains(judgeNum))
                    {
                        pokerLiset.Add(judgeNum);
                    }
                }
            }

            return pokerLiset;
        }
    }

    /// <summary>
    /// 開啟符合撲克外框
    /// </summary>
    /// <param name="pokerList">撲克</param>
    /// <param name="matchNumList">符合撲克數字</param>
    /// <param name="isWinEffect">贏家效果</param>
    public static void OpenMatchPokerFrame(List<Poker> pokerList, List<int> matchNumList, bool isWinEffect)
    {
        foreach (var poker in pokerList)
        {
            poker.PokerEffectEnable = false;
            poker.SetColor = isWinEffect == true ?
                             0.5f :
                             1;
        }

        //符合牌開啟外框
        string s = "";
        foreach (var matchNum in matchNumList)
        {
            s += matchNum + ",";
            foreach (var poker in pokerList)
            {
                if (poker.PokerNum == matchNum)
                {
                    poker.PokerEffectEnable = true;

                    if (isWinEffect)
                    {
                        poker.StartWinEffect();
                        poker.SetColor = 1;
                    }
                }                
            }
        }
    }
}
