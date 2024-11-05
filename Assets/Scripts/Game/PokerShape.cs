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
    "RoyalFlush",           // 皇家同花順
    "StraightFlush",        // 同花順
    "FourOfKind",           // 四條
    "FullHouse",            // 葫蘆
    "Flush",                // 同花
    "Straight",             // 順子
    "ThreeOfAKind",         // 三條
    "TwoPairs",             // 兩對
    "OnePair",              // 一對
    "HighCard",             // 高牌
    };

    /// <summary>
    /// 判斷牌型 (Evaluate Poker Hand)
    /// </summary>
    /// <param name="judgePokerList">List of poker cards to evaluate</param>
    /// <param name="callBack">Callback to return the result and matching cards</param>
    public static void JudgePokerShape(List<int> judgePokerList, UnityAction<int, List<int>> callBack)
    {
        // Group cards by rank
        Dictionary<int, List<int>> groupPoker = GroupPokerByRank(judgePokerList);

        // Check each hand from the highest ranked to lowest
        List<int> handResult;

        //Debug.Log("judgePokerList ::: " + judgePokerList + "\ngroupPoker ::: " + groupPoker);

        handResult = CheckRoyalFlush(groupPoker);
        if (handResult.Count == 5) { callBack(0, handResult); return; }

        handResult = CheckStraightFlush(groupPoker);
        if (handResult.Count == 5) { callBack(1, handResult); return; }

        handResult = CheckFourOfAKind(groupPoker);
        if (handResult.Count == 5) { callBack(2, handResult); return; }

        handResult = CheckFullHouse(groupPoker);
        if (handResult.Count == 5) { callBack(3, handResult); return; }

        handResult = CheckFlush(groupPoker);
        if (handResult.Count == 5) { callBack(4, handResult); return; }

        handResult = CheckStraight(groupPoker);
        if (handResult.Count == 5) { callBack(5, handResult); return; }

        handResult = CheckThreeOfAKind(groupPoker);
        if (handResult.Count == 5) { callBack(6, handResult); return; }

        handResult = CheckTwoPair(groupPoker);
        if (handResult.Count == 5) { callBack(7, handResult); return; }

        handResult = CheckPair(groupPoker);
        if (handResult.Count == 5 || handResult.Count == 2) { callBack(8, handResult); return; }

        handResult = CheckHighCard(groupPoker);
        callBack(9, handResult); // High Card
    }

    /// <summary>
    /// Group cards by rank
    /// </summary>
    private static Dictionary<int, List<int>> GroupPokerByRank(List<int> pokerList)
    {
        Dictionary<int, List<int>> groupedByRank = new Dictionary<int, List<int>>();

        foreach (var poker in pokerList)
        {
            int suit = poker / 13;
            int rank = poker % 13 + 1; // Rank from 1 (Ace) to 13 (King)

            if (!groupedByRank.ContainsKey(rank))
            {
                groupedByRank[rank] = new List<int>();
            }
            groupedByRank[rank].Add(suit);
        }

        return groupedByRank;
    }

    /// <summary>
    /// Check for Royal Flush
    /// </summary>
    private static List<int> CheckRoyalFlush(Dictionary<int, List<int>> groupPoker)
    {
        var straightFlush = CheckStraightFlush(groupPoker);
        if (straightFlush.Count == 5)
        {
            // Check if the straight flush is 10-J-Q-K-A
            var ranks = straightFlush.Select(card => (card % 13) + 1).OrderBy(x => x).ToList();
            if (ranks.SequenceEqual(new List<int> { 1, 10, 11, 12, 13 })) // Ace is 1
            {
                return straightFlush;
            }
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for Straight Flush
    /// </summary>
    private static List<int> CheckStraightFlush(Dictionary<int, List<int>> groupPoker)
    {
        for (int suit = 0; suit < 4; suit++)
        {
            // Get all cards of the same suit
            var suitedCards = groupPoker
                .Where(kv => kv.Value.Contains(suit))
                .Select(kv => kv.Key)
                .OrderByDescending(x => x)
                .ToList();

            if (suitedCards.Count >= 5)
            {
                // Check for straight within the suited cards
                var straightFlush = CheckStraightInRanks(suitedCards, suit, groupPoker);
                if (straightFlush.Count == 5)
                {
                    return straightFlush;
                }
            }
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for Four of a Kind
    /// </summary>
    private static List<int> CheckFourOfAKind(Dictionary<int, List<int>> groupPoker)
    {
        var quads = groupPoker.Where(kv => kv.Value.Count == 4).OrderByDescending(kv => kv.Key).FirstOrDefault();
        if (quads.Value != null)
        {
            var quadCards = quads.Value.Select(suit => (quads.Key - 1) + (13 * suit)).ToList();
            var kicker = GetHighestRemainingCards(groupPoker, quadCards, 1);
            quadCards.AddRange(kicker);
            return quadCards;
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for Full House
    /// </summary>
    private static List<int> CheckFullHouse(Dictionary<int, List<int>> groupPoker)
    {
        var threeOfAKinds = groupPoker.Where(kv => kv.Value.Count >= 3).OrderByDescending(kv => kv.Key).ToList();
        if (threeOfAKinds.Count >= 1)
        {
            var bestTrip = threeOfAKinds[0];
            var tripCards = bestTrip.Value.Take(3).Select(suit => (bestTrip.Key - 1) + (13 * suit)).ToList();

            // Remove the trip rank to look for pairs
            var remainingRanks = groupPoker.Where(kv => kv.Key != bestTrip.Key && kv.Value.Count >= 2)
                .OrderByDescending(kv => kv.Key)
                .ToList();

            if (remainingRanks.Count >= 1)
            {
                var bestPair = remainingRanks[0];
                var pairCards = bestPair.Value.Take(2).Select(suit => (bestPair.Key - 1) + (13 * suit)).ToList();
                tripCards.AddRange(pairCards);
                return tripCards;
            }
            else if (threeOfAKinds.Count >= 2)
            {
                // Use the second three of a kind as the pair
                var secondTrip = threeOfAKinds[1];
                var pairCards = secondTrip.Value.Take(2).Select(suit => (secondTrip.Key - 1) + (13 * suit)).ToList();
                tripCards.AddRange(pairCards);
                return tripCards;
            }
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for Flush
    /// </summary>
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

    /// <summary>
    /// Check for Straight
    /// </summary>
    private static List<int> CheckStraight(Dictionary<int, List<int>> groupPoker)
    {
        var ranks = groupPoker.Keys.OrderByDescending(x => x).ToList();
        var straight = CheckStraightInRanks(ranks, -1, groupPoker); // -1 indicates any suit
        return straight;
    }

    /// <summary>
    /// Check for Three of a Kind
    /// </summary>
    private static List<int> CheckThreeOfAKind(Dictionary<int, List<int>> groupPoker)
    {
        var trips = groupPoker.Where(kv => kv.Value.Count == 3).OrderByDescending(kv => kv.Key).FirstOrDefault();
        if (trips.Value != null)
        {
            var tripCards = trips.Value.Select(suit => (trips.Key - 1) + (13 * suit)).ToList();
            var kickers = GetHighestRemainingCards(groupPoker, tripCards, 2);
            tripCards.AddRange(kickers);
            return tripCards;
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for Two Pairs
    /// </summary>
    private static List<int> CheckTwoPair(Dictionary<int, List<int>> groupPoker)
    {
        var pairs = groupPoker.Where(kv => kv.Value.Count >= 2)
                              .OrderByDescending(kv => kv.Key)
                              .ToList();

        if (pairs.Count >= 2)
        {
            var firstPair = pairs[0];
            var secondPair = pairs[1];

            var pairCards = firstPair.Value.Take(2)
                .Select(suit => (firstPair.Key - 1) + (13 * suit)).ToList();

            pairCards.AddRange(secondPair.Value.Take(2)
                .Select(suit => (secondPair.Key - 1) + (13 * suit)).ToList());

            var usedCards = pairCards.ToList();
            var kicker = GetHighestRemainingCards(groupPoker, usedCards, 1);
            pairCards.AddRange(kicker);
            return pairCards;
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for One Pair
    /// </summary>
    private static List<int> CheckPair(Dictionary<int, List<int>> groupPoker)
    {
        var pair = groupPoker.Where(kv => kv.Value.Count == 2)
                             .OrderByDescending(kv => kv.Key)
                             .FirstOrDefault();

        if (pair.Value != null)
        {
            var pairCards = pair.Value.Select(suit => (pair.Key - 1) + (13 * suit)).ToList();
            var kickers = GetHighestRemainingCards(groupPoker, pairCards, 3);
            pairCards.AddRange(kickers);
            return pairCards;
        }
        return new List<int>();
    }

    /// <summary>
    /// Check for High Card
    /// </summary>
    private static List<int> CheckHighCard(Dictionary<int, List<int>> groupPoker)
    {
        var highCards = groupPoker
            .OrderByDescending(kv => kv.Key)
            .SelectMany(kv => kv.Value.Select(suit => (kv.Key - 1) + (13 * suit)))
            .Take(5) // Ensure only 5 cards are returned
            .ToList();

        return highCards;
    }

    /// <summary>
    /// Check for a straight within given ranks and suit
    /// </summary>
    private static List<int> CheckStraightInRanks(List<int> ranks, int suit, Dictionary<int, List<int>> groupPoker)
    {
        // Handle Ace as both high and low
        var extendedRanks = new List<int>(ranks);
        if (ranks.Contains(1)) // Ace as 1
        {
            extendedRanks.Add(14); // Ace as 14
        }
        extendedRanks = extendedRanks.OrderByDescending(x => x).ToList();

        for (int i = 0; i < extendedRanks.Count - 4; i++)
        {
            if (extendedRanks[i] - 1 == extendedRanks[i + 1] &&
                extendedRanks[i + 1] - 1 == extendedRanks[i + 2] &&
                extendedRanks[i + 2] - 1 == extendedRanks[i + 3] &&
                extendedRanks[i + 3] - 1 == extendedRanks[i + 4])
            {
                var straightRanks = new List<int> {
                extendedRanks[i],
                extendedRanks[i + 1],
                extendedRanks[i + 2],
                extendedRanks[i + 3],
                extendedRanks[i + 4]
            };

                // Convert rank 14 back to Ace (1)
                straightRanks = straightRanks.Select(r => r == 14 ? 1 : r).ToList();

                var straightCards = new List<int>();
                foreach (var rank in straightRanks)
                {
                    if (suit == -1)
                    {
                        // Any suit
                        int cardSuit = groupPoker[rank][0]; // Use any available suit
                        straightCards.Add((rank - 1) + (13 * cardSuit));
                    }
                    else
                    {
                        // Specific suit
                        if (groupPoker[rank].Contains(suit))
                        {
                            straightCards.Add((rank - 1) + (13 * suit));
                        }
                        else
                        {
                            break; // This rank does not have the required suit
                        }
                    }
                }

                if (straightCards.Count == 5)
                {
                    return straightCards;
                }
            }
        }
        return new List<int>();
    }

    /// <summary>
    /// Get the highest remaining cards that are not already in use
    /// </summary>
    private static List<int> GetHighestRemainingCards(Dictionary<int, List<int>> groupPoker, List<int> usedCards, int count)
    {
        var usedCardSet = new HashSet<int>(usedCards);
        var remainingCards = groupPoker
            .SelectMany(kv => kv.Value.Select(suit => (kv.Key - 1) + (13 * suit)))
            .Where(card => !usedCardSet.Contains(card))
            .OrderByDescending(card => (card % 13) + 1 == 1 ? 14 : (card % 13) + 1)
            .Take(count)
            .ToList();

        return remainingCards;
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
            poker.SetColor = isWinEffect ? 0.5f : 1;
        }

        // Highlight matched cards
        foreach (var matchNum in matchNumList)
        {
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