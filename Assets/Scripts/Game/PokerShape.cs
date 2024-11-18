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
        { "Fold" , 0 },
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
        // Step 1: Count occurrences of each card rank (1 - 13)
        var rankCounts = cards.GroupBy(card => (card % 13 == 0 ? 13 : card % 13))
                              .ToDictionary(g => g.Key, g => g.Count());

        // Step 2: Check if Flush (same suit)
        bool isFlush = cards.GroupBy(card => card / 13) // Group by suit (0 = Spades, 1 = Hearts, etc.)
                            .Any(g => g.Count() >= 5);

        // Step 3: Check if Straight (consecutive ranks)
        var uniqueRanks = rankCounts.Keys.OrderByDescending(r => r).ToList();
        bool isStraight = false;
        int highestStraightCard = 0;

        // Checking for normal straight
        for (int i = 0; i <= uniqueRanks.Count - 5; i++)
        {
            if (uniqueRanks[i] - uniqueRanks[i + 4] == 4)
            {
                isStraight = true;
                highestStraightCard = uniqueRanks[i];
                break;
            }
        }

        // Special case: Ace-low straight (A-2-3-4-5)
        if (!isStraight && uniqueRanks.Contains(13) &&
            uniqueRanks.Contains(2) && uniqueRanks.Contains(3) &&
            uniqueRanks.Contains(4) && uniqueRanks.Contains(5))
        {
            isStraight = true;
            highestStraightCard = 5; // Low-end straight (A-2-3-4-5)
        }

        // Step 4: Determine hand type
        if (isFlush && isStraight)
        {
            // Check for Royal Flush or Straight Flush
            int flushSuit = cards.GroupBy(card => card / 13)
                                 .OrderByDescending(g => g.Count())
                                 .First().Key;

            var flushCards = cards.Where(card => card / 13 == flushSuit)
                                  .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                                  .ToList();

            // Check if it's a Royal Flush (A, K, Q, J, 10 of same suit)
            if (flushCards.Any(card => card % 13 == 12) && // Ace in the flush
                flushCards.Any(card => card % 13 == 11) && // King in the flush
                flushCards.Any(card => card % 13 == 10) && // Queen in the flush
                flushCards.Any(card => card % 13 == 9))  // Jack in the flush
            {
                callback(HandRanks["Royal Flush"], flushCards);
                return;
            }
            else
            {
                callback(HandRanks["Straight Flush"], flushCards.Take(5).ToList());
                return;
            }
        }

        // Check for Four of a Kind
        if (rankCounts.Any(rc => rc.Value == 4))
        {
            int quadRank = rankCounts.First(rc => rc.Value == 4).Key;
            var quadCards = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) == quadRank).ToList();

            int kickerRank = rankCounts.Keys.Where(r => r != quadRank).Max();
            var kickerCard = cards.First(card => (card % 13 == 0 ? 13 : card % 13) == kickerRank);

            callback(HandRanks["Four of a Kind"], quadCards.Concat(new List<int> { kickerCard }).ToList());
            return;
        }

        // Check for Full House
        if (rankCounts.Any(rc => rc.Value == 3) &&
            rankCounts.Any(rc => rc.Value >= 2 && rc.Key != rankCounts.First(rc => rc.Value == 3).Key))
        {
            int tripletRank = rankCounts.Where(rc => rc.Value == 3).OrderByDescending(rc => rc.Key).First().Key;
            var tripletCards = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) == tripletRank).Take(3).ToList();

            int pairRank = rankCounts.Where(rc => rc.Value >= 2 && rc.Key != tripletRank).OrderByDescending(rc => rc.Key).First().Key;
            var pairCards = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) == pairRank).Take(2).ToList();

            callback(HandRanks["Full House"], tripletCards.Concat(pairCards).ToList());
            return;
        }

        // Check for Flush (same suit)
        if (isFlush)
        {
            int flushSuit = cards.GroupBy(card => card / 13)
                                 .OrderByDescending(g => g.Count())
                                 .First().Key;

            var flushCards = cards.Where(card => card / 13 == flushSuit)
                                  .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                                  .Take(5)
                                  .ToList();

            callback(HandRanks["Flush"], flushCards);
            return;
        }

        // Check for Straight (consecutive ranks)
        if (isStraight)
        {
            var straightCards = cards.Where(card => uniqueRanks.Contains(card % 13 == 0 ? 13 : card % 13))
                                     .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                                     .Take(5)
                                     .ToList();

            callback(HandRanks["Straight"], straightCards);
            return;
        }

        // Check for Three of a Kind
        if (rankCounts.Any(rc => rc.Value == 3))
        {
            int tripletRank = rankCounts.Where(rc => rc.Value == 3).OrderByDescending(rc => rc.Key).First().Key;
            var tripletCards = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) == tripletRank).Take(3).ToList();

            var kickers = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) != tripletRank)
                               .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                               .Take(2)
                               .ToList();

            callback(HandRanks["Three of a Kind"], tripletCards.Concat(kickers).ToList());
            return;
        }

        // Check for Two Pair
        if (rankCounts.Count(rc => rc.Value >= 2) >= 2)
        {
            var pairs = rankCounts.Where(rc => rc.Value >= 2)
                                  .OrderByDescending(rc => rc.Key)
                                  .Take(2)
                                  .Select(rc => rc.Key)
                                  .ToList();

            var pairCards = cards.Where(card => pairs.Contains(card % 13 == 0 ? 13 : card % 13))
                                 .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                                 .Take(4)
                                 .ToList();

            var kicker = cards.Where(card => !pairs.Contains(card % 13 == 0 ? 13 : card % 13))
                              .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                              .First();

            callback(HandRanks["Two Pair"], pairCards.Concat(new List<int> { kicker }).ToList());
            return;
        }

        // Check for One Pair
        if (rankCounts.Any(rc => rc.Value == 2))
        {
            int pairRank = rankCounts.Where(rc => rc.Value == 2).OrderByDescending(rc => rc.Key).First().Key;
            var pairCards = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) == pairRank).Take(2).ToList();

            var kickers = cards.Where(card => (card % 13 == 0 ? 13 : card % 13) != pairRank)
                               .OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                               .Take(3)
                               .ToList();

            callback(HandRanks["One Pair"], pairCards.Concat(kickers).ToList());
            return;
        }

        // High Card
        var highCards = cards.OrderByDescending(card => card % 13 == 0 ? 13 : card % 13)
                             .Take(5)
                             .ToList();

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