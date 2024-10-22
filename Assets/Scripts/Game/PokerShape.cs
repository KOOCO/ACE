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
    "RoyalFlush",           // 皇家同花順 (Strongest hand)
    "StraightFlush",        // 同花順
    "FourOfAKind",          // 四條
    "FullHouse",            // 葫蘆
    "Flush",                // 同花
    "Straight",             // 順子 (Regular straight)
    "ThreeOfAKind",         // 三條
    "TwoPairs",             // 兩對
    "OnePair",              // 一對
    "HighCard",             // 高牌 (Weakest hand)
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
        List<int> handResult;

        handResult = CheckRoyalFlush(groupPoker);
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
            callBack(5, handResult); // Straight
            return;
        }

        handResult = CheckThreeOfAKind(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(6, handResult); // Three of a Kind
            return;
        }

        handResult = CheckTwoPair(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(7, handResult); // Two Pair
            return;
        }

        handResult = CheckPair(groupPoker);
        if (handResult.Count == 5)
        {
            callBack(8, handResult); // One Pair
            return;
        }

        handResult = CheckHighCard(groupPoker);
        callBack(9, handResult); // High Card
    }

    // Helper Methods

    private static Dictionary<int, List<int>> GroupPokerByRank(List<int> judgePokerList)
    {
        Dictionary<int, List<int>> groupPoker = new Dictionary<int, List<int>>();

        foreach (var poker in judgePokerList)
        {
            int suit = poker / 13; // Assuming 0-3 for suits
            int rank = poker % 13 + 1; // Assuming ranks 1-13

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
        var royalFlushCards = new List<int> { 10, 11, 12, 13, 1 }; // 10, J, Q, K, A
        var flushSuit = groupPoker.Values.SelectMany(suits => suits).Distinct().ToList();

        foreach (var suit in flushSuit)
        {
            if (royalFlushCards.All(rank => groupPoker.ContainsKey(rank) && groupPoker[rank].Contains(suit)))
            {
                return royalFlushCards.Select(rank => (rank - 1) + (13 * suit)).ToList();
            }
        }

        return new List<int>();
    }

    private static List<int> CheckStraightFlush(Dictionary<int, List<int>> groupPoker)
    {
        var flushCards = CheckFlush(groupPoker);
        return CheckStraight(groupPoker, flushCards);
    }

    private static List<int> CheckQuads(Dictionary<int, List<int>> groupPoker)
    {
        var quads = JudgePairs(groupPoker, 4);
        if (quads.Count == 4)
        {
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
        for (int suit = 0; suit < 4; suit++)
        {
            var suitedCards = groupPoker
                .Where(kv => kv.Value.Contains(suit))
                .OrderByDescending(kv => kv.Key)
                .Select(kv => (kv.Key - 1) + (13 * suit))
                .ToList();

            if (suitedCards.Count >= 5)
            {
                return suitedCards.Take(5).ToList();
            }
        }
        return new List<int>();
    }

    private static List<int> CheckStraight(Dictionary<int, List<int>> groupPoker, List<int> flushList = null)
    {
        List<int> straightList = new List<int>();
        var ranks = flushList == null
            ? groupPoker.Keys.OrderByDescending(x => x)
            : flushList.Select(card => (card % 13) + 1).Distinct();

        // Check for a regular straight
        for (int i = 13; i >= 5; i--)
        {
            if (ranks.Contains(i) && ranks.Contains(i - 1) && ranks.Contains(i - 2) && ranks.Contains(i - 3) && ranks.Contains(i - 4))
            {
                straightList.AddRange(new List<int>
            {
                (i - 1) + (13 * groupPoker[i][0]),
                (i - 2) + (13 * groupPoker[i - 1][0]),
                (i - 3) + (13 * groupPoker[i - 2][0]),
                (i - 4) + (13 * groupPoker[i - 3][0]),
                (i - 5) + (13 * groupPoker[i - 4][0])
            });
                break;
            }
        }

        // Check for a low straight (A, 2, 3, 4, 5)
        if (ranks.Contains(1) && ranks.Contains(2) && ranks.Contains(3) && ranks.Contains(4) && ranks.Contains(5))
        {
            straightList.AddRange(new List<int>
        {
            (1 - 1) + (13 * groupPoker[1][0]), // Ace
            (2 - 1) + (13 * groupPoker[2][0]),
            (3 - 1) + (13 * groupPoker[3][0]),
            (4 - 1) + (13 * groupPoker[4][0]),
            (5 - 1) + (13 * groupPoker[5][0])
        });
        }

        return straightList;
    }

    private static List<int> CheckThreeOfAKind(Dictionary<int, List<int>> groupPoker)
    {
        var trips = JudgePairs(groupPoker, 3);
        if (trips.Count == 3)
        {
            trips.AddRange(GetHighestRemainingCards(groupPoker, trips, 2));
        }
        return trips;
    }

    private static List<int> CheckTwoPair(Dictionary<int, List<int>> groupPoker)
    {
        var pairs = JudgePairs(groupPoker, 2);
        if (pairs.Count >= 4)
        {
            pairs.Add(GetHighestRemainingCard(groupPoker, pairs));
        }
        return pairs;
    }

    private static List<int> CheckPair(Dictionary<int, List<int>> groupPoker)
    {
        var pair = JudgePairs(groupPoker, 2);
        if (pair.Count == 2)
        {
            pair.AddRange(GetHighestRemainingCards(groupPoker, pair, 3));
        }
        return pair;
    }

    private static List<int> CheckHighCard(Dictionary<int, List<int>> groupPoker)
    {
        List<int> highCardList = new List<int>();

        // Get the highest card available
        int highestCardRank = groupPoker.Keys.Max();
        highCardList.Add((highestCardRank - 1) + (13 * groupPoker[highestCardRank][0])); // Highest card as high card

        // Add the next three highest cards
        var remaining = GetHighestRemainingCards(groupPoker, highCardList, 4);
        highCardList.AddRange(remaining);

        // Return at index 9 to represent High Card in shapeStr array
        return highCardList.Count >= 5 ? highCardList.Take(5).ToList() : new List<int>();
    }

    // Utility to judge pairs, triples, quads
    private static List<int> JudgePairs(Dictionary<int, List<int>> groupPoker, int pairSize)
    {
        List<int> pokerList = new List<int>();
        var matchingRanks = groupPoker
            .Where(kv => kv.Value.Count == pairSize)
            .Select(kv => kv.Key)
            .OrderByDescending(x => x)
            .ToList();

        foreach (var rank in matchingRanks)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                if (groupPoker[rank].Contains(suit))
                {
                    pokerList.Add((rank - 1) + (13 * suit));
                    if (pokerList.Count >= pairSize) break; // Ensure only pairSize number of cards are added
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
