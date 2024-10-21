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
        // Group cards by rank
        Dictionary<int, List<int>> groupPoker = GroupPokerByRank(judgePokerList);

        // Check for each poker hand, starting from the highest-ranked hand
        List<int> handResult = CheckRoyalFlush(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(0, handResult); // Royal Flush
            return;
        }

        handResult = CheckStraightFlush(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(1, handResult); // Straight Flush
            return;
        }

        handResult = CheckQuads(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(2, handResult); // Four of a Kind
            return;
        }

        handResult = CheckFullHouse(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(3, handResult); // Full House
            return;
        }

        handResult = CheckFlush(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(4, handResult); // Flush
            return;
        }

        handResult = CheckStraight(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(6, handResult); // Straight
            return;
        }

        handResult = CheckThreeOfAKind(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(7, handResult); // Three of a Kind
            return;
        }

        handResult = CheckTwoPair(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(8, handResult); // Two Pair
            return;
        }

        handResult = CheckPair(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(9, handResult); // One Pair
            return;
        }

        handResult = CheckHighCard(groupPoker);
        callBack(10, handResult); // High Card
    }

    // Helper Methods

    private static Dictionary<int, List<int>> GroupPokerByRank(List<int> judgePokerList)
    {
        Dictionary<int, List<int>> groupPoker = new Dictionary<int, List<int>>();

        foreach (var poker in judgePokerList)
        {
            int suit = poker / 13;
            int rank = poker % 13 + 1;

            if (!groupPoker.ContainsKey(rank))
            {
                groupPoker[rank] = new List<int>();
            }

            groupPoker[rank].Add(suit);
        }

        return groupPoker;
    }

    private static List<int> CheckRoyalFlush(Dictionary<int, List<int>> groupPoker)
    {
        return CheckStraightFlush(groupPoker, true);
    }

    private static List<int> CheckStraightFlush(Dictionary<int, List<int>> groupPoker, bool checkRoyal = false)
    {
        List<int> flushList = CheckFlush(groupPoker);
        if (flushList.Count >= 5)
        {
            List<int> straightFlushList = CheckStraight(groupPoker, flushList);
            if (straightFlushList.Count == 5 && (!checkRoyal || straightFlushList.Contains(0)))
            {
                return straightFlushList;
            }
        }
        return new List<int>();
    }

    private static List<int> CheckQuads(Dictionary<int, List<int>> groupPoker)
    {
        var quads = JudgePairs(groupPoker, 4);
        if (quads.Count == 4)
        {
            // Add highest kicker
            quads.Add(GetHighestRemainingCard(groupPoker, quads));
        }
        return quads;
    }

    private static List<int> CheckFullHouse(Dictionary<int, List<int>> groupPoker)
    {
        var triples = JudgePairs(groupPoker, 3);
        var pairs = JudgePairs(groupPoker, 2);

        if (triples.Count >= 3 && pairs.Count >= 2)
        {
            return triples.Take(3).Concat(pairs.Take(2)).ToList();
        }
        return new List<int>();
    }

    private static List<int> CheckFlush(Dictionary<int, List<int>> groupPoker)
    {
        List<int> flushList = new List<int>();

        for (int suit = 0; suit < 4; suit++)
        {
            var suitedCards = groupPoker.Where(kv => kv.Value.Contains(suit)).OrderByDescending(kv => kv.Key).Select(kv => (kv.Key - 1) + (13 * suit)).ToList();
            if (suitedCards.Count >= 5)
            {
                return suitedCards.Take(5).ToList();
            }
        }
        return flushList;
    }

    private static List<int> CheckStraight(Dictionary<int, List<int>> groupPoker, List<int> flushList = null)
    {
        List<int> straightList = new List<int>();
        IEnumerable<int> ranks = flushList == null
            ? groupPoker.Keys.OrderByDescending(x => x)
            : flushList.Select(card => (card % 13) + 1).Distinct();

        for (int i = 13; i >= 5; i--)
        {
            if (ranks.Contains(i) &&
                ranks.Contains(i - 1) &&
                ranks.Contains(i - 2) &&
                ranks.Contains(i - 3) &&
                ranks.Contains(i - 4))
            {
                for (int j = i; j >= i - 4; j--)
                {
                    int cardNum = (j - 1) + (13 * groupPoker[j][0]);
                    straightList.Add(cardNum);
                }
                break;
            }
        }
        return straightList;
    }

    private static List<int> CheckThreeOfAKind(Dictionary<int, List<int>> groupPoker)
    {
        var trips = JudgePairs(groupPoker, 3);
        if (trips.Count == 3)
        {
            // Add top 2 kickers
            var kickers = GetHighestRemainingCards(groupPoker, trips, 2);
            trips.AddRange(kickers);
        }
        return trips;
    }

    private static List<int> CheckTwoPair(Dictionary<int, List<int>> groupPoker)
    {
        var pairs = JudgePairs(groupPoker, 2);
        if (pairs.Count >= 4)
        {
            // Add kicker
            pairs.Add(GetHighestRemainingCard(groupPoker, pairs));
        }
        return pairs;
    }

    private static List<int> CheckPair(Dictionary<int, List<int>> groupPoker)
    {
        var pair = JudgePairs(groupPoker, 2);
        if (pair.Count == 2)
        {
            // Add top 3 kickers
            var kickers = GetHighestRemainingCards(groupPoker, pair, 3);
            pair.AddRange(kickers);
        }
        return pair;
    }

    private static List<int> CheckHighCard(Dictionary<int, List<int>> groupPoker)
    {
        List<int> highCardList = new List<int>();

        if (groupPoker.ContainsKey(1))
        {
            highCardList.Add(0 + (13 * groupPoker[1][0])); // Ace as high card
        }
        else
        {
            int max = groupPoker.Max(kv => kv.Key);
            highCardList.Add((max - 1) + (13 * groupPoker[max][0]));
        }

        // Add the next highest cards to complete the hand
        var remaining = GetHighestRemainingCards(groupPoker, highCardList, 4);
        highCardList.AddRange(remaining);

        return highCardList;
    }

    // Utility to judge pairs, triples, quads
    private static List<int> JudgePairs(Dictionary<int, List<int>> groupPoker, int pairSize)
    {
        List<int> pokerList = new List<int>();
        var matchingRanks = groupPoker.Where(kv => kv.Value.Count == pairSize).Select(kv => kv.Key).OrderByDescending(x => x).ToList();

        foreach (var rank in matchingRanks)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                if (groupPoker[rank].Contains(suit))
                {
                    pokerList.Add((rank - 1) + (13 * suit));
                }
            }
        }
        return pokerList;
    }

    // Utility to get remaining high cards
    private static int GetHighestRemainingCard(Dictionary<int, List<int>> groupPoker, List<int> excludedCards)
    {
        return GetHighestRemainingCards(groupPoker, excludedCards, 1).FirstOrDefault();
    }

    private static List<int> GetHighestRemainingCards(Dictionary<int, List<int>> groupPoker, List<int> excludedCards, int count)
    {
        var allCards = groupPoker.SelectMany(kv => kv.Value.Select(suit => (kv.Key - 1) + (13 * suit))).ToList();
        var remainingCards = allCards.Except(excludedCards).OrderByDescending(n => (n % 13 == 0) ? int.MaxValue : (n % 13 + 1)).ToList();
        return remainingCards.Take(count).ToList();
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
