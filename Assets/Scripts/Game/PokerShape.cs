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
    public static Dictionary<int, string> HandRanks = new Dictionary<int, string>
    {
        {0, "FOLD"},
        {1, "ROYALFLUSH"},
        {2, "STRAIGHTFLUSH"},
        {3, "FOUROFAKIND"},
        {4, "FULLHOUSE"},
        {5, "FLUSH"},
        {6, "STRAIGHT"},
        {7, "THREEOFAKIND"},
        {8, "TWOPAIR"},
        {9, "ONEPAIR"},
        {10, "HIGHCARD"}
    };

    public static void JudgePokerShape(List<int> judgePokerList, UnityAction<int, List<int>> callBack)
    {
        if (judgePokerList == null || judgePokerList.Count == 0)
        {
            callBack?.Invoke(0, new List<int>()); // Return Fold if no cards provided
            return;
        }

        // Convert card numbers (0-51) to rank and suit
        List<int> ranks = judgePokerList.Select(card => card % 13 + 2).ToList(); // Rank 2-14
        List<int> suits = judgePokerList.Select(card => card / 13).ToList();    // Suits 0-3

        // Sort cards by rank for easier evaluation
        var sortedCards = judgePokerList.OrderBy(card => card % 13).ToList();
        ranks = sortedCards.Select(card => card % 13 + 2).ToList();

        // Create helper dictionaries
        var rankCounts = ranks.GroupBy(rank => rank).ToDictionary(g => g.Key, g => g.Count());
        var suitGroups = sortedCards.GroupBy(card => card / 13).ToDictionary(g => g.Key, g => g.ToList());

        // Helper to check for straight
        List<int> GetStraight(List<int> cardList)
        {
            var sortedRanks = cardList.Select(card => card % 13 + 2).Distinct().OrderBy(rank => rank).ToList();
            if (sortedRanks.Count < 5) return null;

            // Handle Ace-low straight
            if (sortedRanks.Contains(14) && sortedRanks.Take(4).SequenceEqual(new List<int> { 2, 3, 4, 5 }))
            {
                return cardList.Where(card => card % 13 + 2 == 14 || card % 13 + 2 <= 5).ToList();
            }

            for (int i = 0; i <= sortedRanks.Count - 5; i++)
            {
                if (sortedRanks[i + 4] - sortedRanks[i] == 4)
                {
                    var straightRanks = sortedRanks.Skip(i).Take(5).ToHashSet();
                    return cardList.Where(card => straightRanks.Contains(card % 13 + 2)).ToList();
                }
            }

            return null;
        }

        // Check for Royal Flush or Straight Flush
        foreach (var suit in suitGroups.Keys)
        {
            if (suitGroups[suit].Count >= 5)
            {
                var flushCards = suitGroups[suit].OrderBy(card => card % 13).ToList();
                var straightFlush = GetStraight(flushCards);
                if (straightFlush != null)
                {
                    if (straightFlush.All(card => card % 13 + 2 >= 10))
                    {
                        callBack?.Invoke(1, judgePokerList); // Royal Flush
                        return;
                    }

                    callBack?.Invoke(2, judgePokerList); // Straight Flush
                    return;
                }
            }
        }

        // Check for Four of a Kind
        if (rankCounts.Values.Contains(4))
        {
            callBack?.Invoke(3, judgePokerList); // Four of a Kind
            return;
        }

        // Check for Full House
        if (rankCounts.Values.Contains(3) && rankCounts.Values.Contains(2))
        {
            callBack?.Invoke(4, judgePokerList); // Full House
            return;
        }

        // Check for Flush
        foreach (var suit in suitGroups.Keys)
        {
            if (suitGroups[suit].Count >= 5)
            {
                // Flush found, return the cards of this suit
                var flushCards = suitGroups[suit];
                callBack?.Invoke(5, flushCards); // Flush
                return;
            }
        }

        // Check for Straight
        var straight = GetStraight(sortedCards);
        if (straight != null)
        {
            callBack?.Invoke(6, judgePokerList); // Straight
            return;
        }

        // Check for Three of a Kind
        if (rankCounts.Values.Contains(3))
        {
            callBack?.Invoke(7, judgePokerList); // Three of a Kind
            return;
        }

        // Check for Two Pair
        if (rankCounts.Values.Count(v => v == 2) >= 2)
        {
            callBack?.Invoke(8, judgePokerList); // Two Pair
            return;
        }

        // Check for One Pair
        if (rankCounts.Values.Contains(2))
        {
            callBack?.Invoke(9, judgePokerList); // One Pair
            return;
        }

        // High Card
        callBack?.Invoke(10, judgePokerList); // High Card
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