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
        "RoyalFlush",       // 皇家同花順
        "StraightFlush",    // 同花順
        "FourOfKind",       // 四條
        "FullHouse",        // 葫蘆
        "Flush",            // 同花
        "Straight",         // 順子
        "ThreeOfAKind",     // 三條
        "TwoPairs",         // 兩對
        "OnePair",          // 一對
        "HighCard"          // 高牌
        };

    /// <summary>
    /// 判斷牌型
    /// </summary>
    /// <param name="judgePokerList"></param>
    /// <param name="callBack">回傳(結果，符合的牌)</param>
    public static void JudgePokerShape(List<int> judgePokerList, UnityAction<int, List<int>> callBack)
    {
        // Group cards by rank (1-13) and store suits
        Dictionary<int, List<int>> groupPoker = new Dictionary<int, List<int>>();
        foreach (var poker in judgePokerList)
        {
            int suit = poker / 13;
            int rank = poker % 13 + 1; // rank 1 is Ace, 13 is King

            if (!groupPoker.ContainsKey(rank))
            {
                groupPoker[rank] = new List<int>();
            }

            groupPoker[rank].Add(suit);
        }

        // Check for various poker hands
        List<int> royalFlushList = CheckRoyalFlush(groupPoker);
        List<int> straightFlushList = CheckStraightFlush(groupPoker);
        List<int> quadsList = JudgePairs(4, groupPoker);
        List<int> fullHouseList = CheckFullHouse(groupPoker);
        List<int> flushList = CheckFlush(groupPoker);
        List<int> straightList = CheckStraight(groupPoker);
        List<int> triplesList = JudgePairs(3, groupPoker);
        List<int> twoPairsList = CheckTwoPairs(groupPoker);
        List<int> onePairList = JudgePairs(2, groupPoker);
        List<int> highCardList = CheckHighCard(groupPoker);

        // Return the result with callback
        if (royalFlushList.Count == 5)
        {
            callBack(0, royalFlushList);
        }
        else if (straightFlushList.Count == 5)
        {
            callBack(1, straightFlushList);
        }
        else if (quadsList.Count == 4)
        {
            callBack(2, quadsList);
        }
        else if (fullHouseList.Count == 5)
        {
            callBack(3, fullHouseList);
        }
        else if (flushList.Count == 5)
        {
            callBack(4, flushList);
        }
        else if (straightList.Count == 5)
        {
            callBack(5, straightList);
        }
        else if (triplesList.Count == 3)
        {
            callBack(6, triplesList);
        }
        else if (twoPairsList.Count == 4)
        {
            callBack(7, twoPairsList);
        }
        else if (onePairList.Count == 2)
        {
            callBack(8, onePairList);
        }
        else
        {
            callBack(9, highCardList);
        }
    }

    // Check for Royal Flush
    private static List<int> CheckRoyalFlush(Dictionary<int, List<int>> groupPoker)
    {
        List<int> royalFlushList = new List<int>();
        if (groupPoker.ContainsKey(10) && groupPoker.ContainsKey(11) &&
            groupPoker.ContainsKey(12) && groupPoker.ContainsKey(13) &&
            groupPoker.ContainsKey(1))
        {
            // Check each suit
            foreach (var suit in groupPoker[10])
            {
                bool isRoyalFlush = true;
                royalFlushList.Clear();
                royalFlushList.Add(9 + (13 * suit)); // 10

                for (int rank = 11; rank <= 13; rank++) // J, Q, K
                {
                    if (!groupPoker.ContainsKey(rank) || !groupPoker[rank].Contains(suit))
                    {
                        isRoyalFlush = false;
                        break;
                    }
                    royalFlushList.Add((rank - 1) + (13 * suit));
                }

                // Add Ace
                if (isRoyalFlush && groupPoker[1].Contains(suit))
                {
                    royalFlushList.Add(0 + (13 * suit)); // A
                }

                if (isRoyalFlush && royalFlushList.Count == 5)
                    break; // Found a Royal Flush
            }
        }
        return royalFlushList;
    }

    // Check for Straight Flush
    private static List<int> CheckStraightFlush(Dictionary<int, List<int>> groupPoker)
    {
        List<int> straightFlushList = new List<int>();
        for (int suit = 0; suit < 4; suit++)
        {
            List<int> flushCards = new List<int>();

            // Collect cards of the same suit
            for (int rank = 1; rank <= 13; rank++)
            {
                if (groupPoker.ContainsKey(rank) && groupPoker[rank].Contains(suit))
                {
                    flushCards.Add(rank);
                }
            }

            // Check for straight in the flush
            for (int i = 0; i <= flushCards.Count - 5; i++)
            {
                if (flushCards[i] + 1 == flushCards[i + 1] &&
                    flushCards[i] + 2 == flushCards[i + 2] &&
                    flushCards[i] + 3 == flushCards[i + 3] &&
                    flushCards[i] + 4 == flushCards[i + 4])
                {
                    for (int j = 0; j < 5; j++)
                    {
                        straightFlushList.Add((flushCards[i + j] - 1) + (13 * suit));
                    }
                    break; // Found a Straight Flush
                }
            }
        }
        return straightFlushList;
    }

    // Check for Flush
    private static List<int> CheckFlush(Dictionary<int, List<int>> groupPoker)
    {
        List<int> flushList = new List<int>();
        for (int suit = 0; suit < 4; suit++)
        {
            // Collect cards of the same suit
            List<int> flushCards = new List<int>();
            for (int rank = 1; rank <= 13; rank++)
            {
                if (groupPoker.ContainsKey(rank) && groupPoker[rank].Contains(suit))
                {
                    flushCards.Add(rank);
                }
            }

            // If there are at least 5 cards of the same suit
            if (flushCards.Count >= 5)
            {
                // Take the top 5 highest cards
                flushCards = flushCards.OrderByDescending(x => x).Take(5).ToList();
                foreach (var rank in flushCards)
                {
                    flushList.Add((rank - 1) + (13 * suit));
                }
                break; // Found a Flush
            }
        }
        return flushList;
    }

    // Check for Straight
    private static List<int> CheckStraight(Dictionary<int, List<int>> groupPoker)
    {
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
                break; // Found a Straight
            }
        }
        return straightList;
    }

    // Check for Full House
    private static List<int> CheckFullHouse(Dictionary<int, List<int>> groupPoker)
    {
        List<int> fullHouseList = new List<int>();
        List<int> triples = JudgePairs(3, groupPoker);
        List<int> pairs = JudgePairs(2, groupPoker);

        if (triples.Count >= 3 && pairs.Count >= 2)
        {
            fullHouseList.AddRange(triples.Take(3));
            fullHouseList.AddRange(pairs.Take(2));
        }

        // Ensure we return 5 cards
        if (fullHouseList.Count < 5)
        {
            fullHouseList.AddRange(CheckHighCard(groupPoker).Take(5 - fullHouseList.Count));
        }

        return fullHouseList;
    }

    // Check for Two Pairs
    private static List<int> CheckTwoPairs(Dictionary<int, List<int>> groupPoker)
    {
        List<int> twoPairsList = new List<int>();
        List<int> pairs = JudgePairs(2, groupPoker);

        if (pairs.Count >= 4)
        {
            twoPairsList.AddRange(pairs.Take(4)); // Take the top 4
        }

        // Ensure we return 5 cards
        if (twoPairsList.Count < 5)
        {
            twoPairsList.AddRange(CheckHighCard(groupPoker).Take(5 - twoPairsList.Count));
        }

        return twoPairsList;
    }

    // Check for High Card
    private static List<int> CheckHighCard(Dictionary<int, List<int>> groupPoker)
    {
        List<int> highCardList = new List<int>();
        if (groupPoker.ContainsKey(1))
        {
            // Ace is the highest card
            highCardList.Add(0 + (13 * groupPoker[1][0])); // Ace
        }

        // Add remaining high cards
        for (int rank = 13; rank >= 2; rank--)
        {
            if (groupPoker.ContainsKey(rank))
            {
                highCardList.Add((rank - 1) + (13 * groupPoker[rank][0])); // Add the highest remaining cards
            }
        }

        return highCardList;
    }

    // Judge pairs for specific counts
    private static List<int> JudgePairs(int pairs, Dictionary<int, List<int>> groupPoker)
    {
        List<int> pairsList = new List<int>();
        foreach (var kvp in groupPoker)
        {
            if (kvp.Value.Count == pairs)
            {
                for (int i = 0; i < pairs; i++)
                {
                    pairsList.Add((kvp.Key - 1) + (13 * kvp.Value[i])); // Add card with suit
                }
            }
        }
        return pairsList;
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

