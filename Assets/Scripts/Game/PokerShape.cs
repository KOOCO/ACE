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
        // Check for an empty list
        if (judgePokerList == null || judgePokerList.Count == 0)
        {
            callBack(10, new List<int>()); // High card (empty)
            return;
        }

        // Grouping cards by rank and suit
        Dictionary<int, List<int>> groupedPoker = new Dictionary<int, List<int>>();
        foreach (var poker in judgePokerList)
        {
            int suit = poker / 13; // Flower suit
            int rank = poker % 13 + 1; // Rank (1-13)

            if (!groupedPoker.ContainsKey(rank))
            {
                groupedPoker[rank] = new List<int>();
            }
            groupedPoker[rank].Add(suit);
        }

        // Check for Royal Straight and Royal Flush
        List<int> royalStraightList = new List<int>();
        bool isRoyalStraight = groupedPoker.ContainsKey(10) && groupedPoker.ContainsKey(11) &&
                               groupedPoker.ContainsKey(12) && groupedPoker.ContainsKey(13) &&
                               groupedPoker.ContainsKey(1);

        if (isRoyalStraight)
        {
            royalStraightList.Add(0 + (13 * groupedPoker[1][0]));
            for (int i = 12; i >= 9; i--)
            {
                royalStraightList.Add(i + (13 * groupedPoker[10][0]));
            }
        }

        List<int> royalFlushList = new List<int>();
        if (isRoyalStraight)
        {
            foreach (var comparisonSuit in groupedPoker[10])
            {
                royalFlushList.Clear();
                royalFlushList.Add((10 - 1) + (13 * comparisonSuit));

                bool isFound = true;
                for (int j = 11; j <= 13; j++)
                {
                    if (!groupedPoker[j].Contains(comparisonSuit))
                    {
                        isFound = false;
                        break;
                    }
                    royalFlushList.Add((j - 1) + (13 * comparisonSuit));
                }

                if (isFound && groupedPoker[1].Contains(comparisonSuit))
                {
                    royalFlushList.Add(0 + (13 * comparisonSuit));
                    break;
                }
                else
                {
                    royalFlushList.Clear();
                }
            }
        }

        // Check for Flush and Straight Flush
        List<int> flushList = new List<int>();
        List<int> straightFlushList = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            if (groupedPoker.Values.Any(suits => suits.Contains(i)) &&
                groupedPoker.Count(kv => kv.Value.Contains(i)) >= 5)
            {
                for (int rank = 13; rank >= 1; rank--)
                {
                    if (groupedPoker.ContainsKey(rank) && groupedPoker[rank].Contains(i))
                    {
                        flushList.Add((rank - 1) + (13 * i));
                    }
                }

                // Check for Straight Flush
                for (int n = 13; n >= 5; n--)
                {
                    if (flushList.Contains((n - 1) + (13 * i)) &&
                        flushList.Contains((n - 1) + (13 * i) - 1) &&
                        flushList.Contains((n - 1) + (13 * i) - 2) &&
                        flushList.Contains((n - 1) + (13 * i) - 3) &&
                        flushList.Contains((n - 1) + (13 * i) - 4))
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

        // Sort flush cards by rank
        flushList = flushList.OrderByDescending(n => (n % 13 == 0) ? int.MaxValue : (n % 13 + 1)).ToList();

        // Check for Straight
        List<int> straightList = new List<int>();
        for (int i = 13; i >= 5; i--)
        {
            if (groupedPoker.ContainsKey(i) && groupedPoker.ContainsKey(i - 1) &&
                groupedPoker.ContainsKey(i - 2) && groupedPoker.ContainsKey(i - 3) &&
                groupedPoker.ContainsKey(i - 4))
            {
                for (int j = i; j >= i - 4; j--)
                {
                    int num = (j - 1) + (13 * groupedPoker[j][0]);
                    straightList.Add(num);
                }
                break;
            }
        }

        // Check for Quads, Triples, and Pairs
        List<int> quadsList = JudgePairs(4, groupedPoker, judgePokerList);
        List<int> triplesList = JudgePairs(3, groupedPoker, judgePokerList);
        List<int> pairsList = JudgePairs(2, groupedPoker, judgePokerList);

        // High Card
        List<int> highCardList = new List<int>();
        if (groupedPoker.ContainsKey(1))
        {
            highCardList.Add(0 + (13 * groupedPoker[1][0]));
        }
        else
        {
            int max = groupedPoker.Max(kv => kv.Key);
            highCardList.Add((max - 1) + (13 * groupedPoker[max][0]));
        }

        // Return Results
        if (royalFlushList.Count == 5)
        {
            callBack(0, royalFlushList); // Royal Flush
        }
        else if (straightFlushList.Count == 5)
        {
            callBack(1, straightFlushList); // Straight Flush
        }
        else if (quadsList.Count == 4)
        {
            callBack(2, quadsList); // Four of a Kind
        }
        else if (triplesList.Count >= 3 && pairsList.Count >= 2)
        {
            List<int> fullHouseList = triplesList.Take(3).Concat(pairsList.Take(2)).ToList();
            callBack(3, fullHouseList); // Full House
        }
        else if (flushList.Count >= 5)
        {
            callBack(4, flushList.Take(5).ToList()); // Flush
        }
        else if (isRoyalStraight)
        {
            callBack(5, royalStraightList); // Royal Straight
        }
        else if (straightList.Count == 5)
        {
            callBack(6, straightList); // Straight
        }
        else if (triplesList.Count >= 3)
        {
            callBack(7, triplesList.Take(3).ToList()); // Three of a Kind
        }
        else if (pairsList.Count >= 4)
        {
            callBack(8, pairsList.Take(4).ToList()); // Two Pairs
        }
        else if (pairsList.Count == 2)
        {
            callBack(9, pairsList.Take(2).ToList()); // One Pair
        }
        else
        {
            callBack(10, highCardList); // High Card
        }

        // Method to judge pairs
        List<int> JudgePairs(int pairs, Dictionary<int, List<int>> groupedPoker, List<int> judgePokerList)
        {
            List<int> pokerList = new List<int>();
            var ranks = groupedPoker.Where(kv => kv.Value.Count == pairs)
                                    .Select(kv => kv.Key)
                                    .OrderByDescending(x => x)
                                    .ToList();

            // Move Ace to the front if it's a pair
            if (ranks.Count >= 2 && ranks.Last() == 1)
            {
                var temp = new List<int>(ranks);
                ranks.Clear();
                ranks.Add(1);
                ranks.AddRange(temp);
            }

            foreach (var rank in ranks)
            {
                for (int j = 0; j < 4; j++)
                {
                    int judgeNum = (rank - 1) + (13 * j);
                    if (judgePokerList.Contains(judgeNum))
                    {
                        pokerList.Add(judgeNum);
                    }
                }
            }

            return pokerList;
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
