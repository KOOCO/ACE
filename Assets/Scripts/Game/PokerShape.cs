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
    public static readonly Dictionary<string, int> HandRanks = new Dictionary<string, int>
    {
        { "Fold", 0 },
        { "Royal Flush", 1 },
        { "Straight Flush", 2 },
        { "Four of a Kind", 3 },
        { "Full House", 4 },
        { "Flush", 5 },
        { "Straight", 6 },
        { "Three of a Kind", 7 },
        { "Two Pair", 8 },
        { "One Pair", 9 },
        { "High Card", 10 }
    };

    public static void JudgePokerShape(List<int> cards, UnityAction<int, List<int>> callback)
    {
        // Map card indices to ranks (1 to 13) and suits (0 to 3)
        var rankCounts = cards.GroupBy(card => (card % 13 == 0 ? 13 : card % 13))
                              .ToDictionary(g => g.Key, g => g.Count());
        var suitGroups = cards.GroupBy(card => card / 13)
                              .ToDictionary(g => g.Key, g => g.ToList());

        var uniqueRanks = rankCounts.Keys.OrderByDescending(r => r).ToList();

        // Helper to get rank of a card
        int GetRank(int card) => card % 13 == 0 ? 13 : card % 13;

        // Helper to get flush cards of a specific suit
        List<int> GetFlushCards(int suit) =>
            suitGroups[suit].OrderByDescending(GetRank).ToList();

        // Check for Royal Flush and Straight Flush
        foreach (var suit in suitGroups.Keys)
        {
            var flushCards = GetFlushCards(suit);
            if (flushCards.Count >= 5)
            {
                var flushRanks = flushCards.Select(GetRank).Distinct().OrderByDescending(r => r).ToList();

                // Check for Straight Flush
                for (int i = 0; i <= flushRanks.Count - 5; i++)
                {
                    if (flushRanks[i] - flushRanks[i + 4] == 4)
                    {
                        // Royal Flush Check
                        if (flushRanks.Take(5).SequenceEqual(new List<int> { 13, 12, 11, 10, 9 }))
                        {
                            callback(HandRanks["Royal Flush"], flushCards.Take(5).ToList());
                            return;
                        }

                        callback(HandRanks["Straight Flush"], flushCards.Take(5).ToList());
                        return;
                    }
                }
            }
        }

        // Remaining logic for Four of a Kind, Full House, Flush, Straight, etc.
        // Check for Four of a Kind
        if (rankCounts.Any(rc => rc.Value == 4))
        {
            var quadRank = rankCounts.First(rc => rc.Value == 4).Key;
            var quadCards = cards.Where(card => GetRank(card) == quadRank).ToList();
            var kicker = cards.Where(card => GetRank(card) != quadRank)
                              .OrderByDescending(GetRank)
                              .First();

            callback(HandRanks["Four of a Kind"], quadCards.Concat(new List<int> { kicker }).ToList());
            return;
        }

        // Check for Full House
        if (rankCounts.Any(rc => rc.Value == 3) && rankCounts.Count(rc => rc.Value >= 2) >= 2)
        {
            var tripletRank = rankCounts.Where(rc => rc.Value == 3).OrderByDescending(rc => rc.Key).First().Key;
            var pairRank = rankCounts.Where(rc => rc.Value >= 2 && rc.Key != tripletRank).OrderByDescending(rc => rc.Key).First().Key;

            var tripletCards = cards.Where(card => GetRank(card) == tripletRank).Take(3).ToList();
            var pairCards = cards.Where(card => GetRank(card) == pairRank).Take(2).ToList();

            callback(HandRanks["Full House"], tripletCards.Concat(pairCards).ToList());
            return;
        }

        // Check for Flush
        foreach (var suit in suitGroups.Keys)
        {
            var flushCards = GetFlushCards(suit);
            if (flushCards.Count >= 5)
            {
                callback(HandRanks["Flush"], flushCards.Take(5).ToList());
                return;
            }
        }

        // Check for Straight
        for (int i = 0; i <= uniqueRanks.Count - 5; i++)
        {
            if (uniqueRanks[i] - uniqueRanks[i + 4] == 4)
            {
                var straightCards = cards.Where(card => uniqueRanks.Contains(GetRank(card)))
                                         .OrderByDescending(GetRank)
                                         .Take(5)
                                         .ToList();
                callback(HandRanks["Straight"], straightCards);
                return;
            }
        }

        // Other hand types (Three of a Kind, Two Pair, One Pair, High Card)
        // Check for Three of a Kind
        if (rankCounts.Any(rc => rc.Value == 3))
        {
            var tripletRank = rankCounts.First(rc => rc.Value == 3).Key;
            var tripletCards = cards.Where(card => GetRank(card) == tripletRank).Take(3).ToList();
            var kickers = cards.Where(card => GetRank(card) != tripletRank)
                               .OrderByDescending(GetRank)
                               .Take(2)
                               .ToList();

            callback(HandRanks["Three of a Kind"], tripletCards.Concat(kickers).ToList());
            return;
        }

        // Check for Two Pair
        if (rankCounts.Count(rc => rc.Value >= 2) >= 2)
        {
            var pairRanks = rankCounts.Where(rc => rc.Value >= 2).OrderByDescending(rc => rc.Key).Take(2).Select(rc => rc.Key).ToList();
            var pairCards = cards.Where(card => pairRanks.Contains(GetRank(card))).Take(4).ToList();
            var kicker = cards.Where(card => !pairRanks.Contains(GetRank(card)))
                              .OrderByDescending(GetRank)
                              .First();

            callback(HandRanks["Two Pair"], pairCards.Concat(new List<int> { kicker }).ToList());
            return;
        }

        // Check for One Pair
        if (rankCounts.Any(rc => rc.Value == 2))
        {
            var pairRank = rankCounts.First(rc => rc.Value == 2).Key;
            var pairCards = cards.Where(card => GetRank(card) == pairRank).Take(2).ToList();
            var kickers = cards.Where(card => GetRank(card) != pairRank)
                               .OrderByDescending(GetRank)
                               .Take(3)
                               .ToList();

            callback(HandRanks["One Pair"], pairCards.Concat(kickers).ToList());
            return;
        }

        // High Card
        var highCards = cards.OrderByDescending(GetRank).Take(5).ToList();
        callback(HandRanks["High Card"], highCards);
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