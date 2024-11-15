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
    private static readonly Dictionary<string, int> HandRanks = new Dictionary<string, int>
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
        // Step 1: Count occurrences of each card rank
        var rankCounts = cards.GroupBy(card => card % 13 == 0 ? 13 : card % 13)
                              .ToDictionary(g => g.Key, g => g.Count());

        // Step 2: Determine if Flush
        bool isFlush = cards.GroupBy(card => card / 13) // Assuming card representation: suit * 13 + rank
                            .Any(g => g.Count() >= 5);

        // Step 3: Determine if Straight
        var uniqueRanks = rankCounts.Keys.OrderByDescending(r => r).Distinct().ToList();
        bool isStraight = false;
        int highestStraightCard = 0;

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
            highestStraightCard = 5;
        }

        // Step 4: Determine hand type
        if (isFlush && isStraight)
        {
            if (highestStraightCard == 13)
            {
                // Royal Flush
                callback(HandRanks["Royal Flush"], new List<int> { 14 });
                return;
            }
            else
            {
                // Straight Flush
                callback(HandRanks["Straight Flush"], uniqueRanks.SkipWhile(r => r > highestStraightCard).Take(5).ToList());
                return;
            }
        }

        // Check for Four of a Kind
        if (rankCounts.Any(rc => rc.Value == 4))
        {
            int quadRank = rankCounts.First(rc => rc.Value == 4).Key;
            int kicker = rankCounts.Keys.Where(r => r != quadRank).Max();
            callback(HandRanks["Four of a Kind"], new List<int> { quadRank, quadRank, quadRank, quadRank, kicker });
            return;
        }

        // Check for Full House
        if (rankCounts.Any(rc => rc.Value == 3) &&
            rankCounts.Any(rc => rc.Value >= 2 && rc.Key != rankCounts.First(rc => rc.Value == 3).Key))
        {
            int tripletRank = rankCounts.Where(rc => rc.Value == 3).OrderByDescending(rc => rc.Key).First().Key;
            int pairRank = rankCounts.Where(rc => rc.Value >= 2 && rc.Key != tripletRank).OrderByDescending(rc => rc.Key).First().Key;
            callback(HandRanks["Full House"], new List<int> { tripletRank, tripletRank, tripletRank, pairRank, pairRank });
            return;
        }

        // Check for Flush
        if (isFlush)
        {
            var flushCards = cards.Where(card => (card / 13) == cards.GroupBy(c => c / 13)
                                                              .OrderByDescending(g => g.Count())
                                                              .First().Key)
                                  .Select(card => card % 13 == 0 ? 13 : card % 13)
                                  .OrderByDescending(r => r)
                                  .Take(5)
                                  .ToList();
            callback(HandRanks["Flush"], flushCards);
            return;
        }

        // Check for Straight
        if (isStraight)
        {
            callback(HandRanks["Straight"], uniqueRanks.SkipWhile(r => r > highestStraightCard).Take(5).ToList());
            return;
        }

        // Check for Three of a Kind
        if (rankCounts.Any(rc => rc.Value == 3))
        {
            var triplet = rankCounts.Where(rc => rc.Value == 3).OrderByDescending(rc => rc.Key).First().Key;
            var kickers = rankCounts.Keys.Where(r => r != triplet).OrderByDescending(r => r).Take(2).ToList();
            callback(HandRanks["Three of a Kind"], new List<int> { triplet, triplet, triplet }.Concat(kickers).Take(5).ToList());
            return;
        }

        // Check for Two Pair
        if (rankCounts.Count(rc => rc.Value >= 2) >= 2)
        {
            var highPair = rankCounts.Where(rc => rc.Value >= 2).OrderByDescending(rc => rc.Key).First().Key;
            var lowPair = rankCounts.Where(rc => rc.Value >= 2 && rc.Key != highPair).OrderByDescending(rc => rc.Key).First().Key;
            var kicker = rankCounts.Keys.Where(r => r != highPair && r != lowPair).OrderByDescending(r => r).First();
            callback(HandRanks["Two Pair"], new List<int> { highPair, highPair, lowPair, lowPair, kicker });
            return;
        }

        // Check for One Pair
        if (rankCounts.Any(rc => rc.Value == 2))
        {
            var pairRank = rankCounts.Where(rc => rc.Value == 2).OrderByDescending(rc => rc.Key).First().Key;
            var kickers = rankCounts.Keys.Where(r => r != pairRank).OrderByDescending(r => r).Take(3).ToList();
            callback(HandRanks["One Pair"], new List<int> { pairRank, pairRank }.Concat(kickers).Take(5).ToList());
            return;
        }

        // High Card
        var highCards = rankCounts.Keys.OrderByDescending(r => r).Take(5).ToList();
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