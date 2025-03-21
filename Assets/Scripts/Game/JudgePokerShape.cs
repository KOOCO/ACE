using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JudgePokerShape : MonoBehaviour
{
    public Dictionary<List<int>, List<int>> CalculateRank(List<int> cards, out List<int> targetList, bool isStraight = false, bool isFlush = false)
    {
        var result = new Dictionary<List<int>, List<int>>();

        // Convert cards to ranks and suits
        var cardRanks = cards.Select(card => card % 13 + 2).ToList(); // Convert to ranks (2 to 14)
        string res = string.Join(", ", cardRanks);
        print("牌號: " + res);
        var cardSuits = cards.GroupBy(card => card / 13); // Group by suit

        // Handle Straight Flush
        if (isStraight && isFlush)
        {
            print("同花順");
            foreach (var suitGroup in cardSuits)
            {
                if (suitGroup.Count() >= 5)
                {
                    var suitedRanks = suitGroup
                        .Select(card => card % 13 + 2)
                        .Distinct()
                        .OrderByDescending(rank => rank)
                        .ToList();

                    // Handle Ace-low straight
                    if (HasLowStraight(suitedRanks))
                        suitedRanks = suitedRanks.Select(rank => rank == 14 ? 1 : rank).OrderBy(rank => rank).ToList();

                    var highestStraightFlush = PokerShape.FindHighestConsecutiveSequence(suitedRanks);
                    if (highestStraightFlush.Count == 5)
                    {
                        highestStraightFlush = highestStraightFlush.Select(rank => rank == 1 ? 14 : rank).ToList();

                        var suitedCards = suitGroup
                            .Where(card => highestStraightFlush.Contains(card % 13 + 2))
                            .OrderByDescending(card => card % 13 + 2)
                            .ToList();

                        targetList = suitedCards;
                        result[highestStraightFlush] = suitedCards;
                        return result;
                    }
                }
            }
        }

        // Handle Flush
        if (isFlush)
        {
            print("同花");
            var flushGroup = cardSuits.FirstOrDefault(group => group.Count() >= 5);
            if (flushGroup != null)
            {
                var flushCards = flushGroup
                    .OrderByDescending(card => card % 13 + 2)
                    .Take(5)
                    .ToList();

                var flushRanks = flushCards.Select(card => card % 13 + 2).ToList();
                targetList = flushCards;
                result[flushRanks] = flushCards;
                return result;
            }
        }

        // Handle Straight
        if (isStraight)
        {
            print("順子");
            bool hasLowStraight = false;
            var distinctRanks = cardRanks.Distinct().OrderByDescending(rank => rank).ToList();
            if (HasLowStraight(distinctRanks))
            {
                hasLowStraight = true;
                distinctRanks = distinctRanks.Select(rank => rank == 14 ? 1 : rank).OrderBy(rank => rank).ToList();
            }
            var highestStraight = PokerShape.FindHighestConsecutiveSequence(distinctRanks);
            if (highestStraight.Count == 5)
            {
                if (hasLowStraight)
                {
                    highestStraight = highestStraight.Select(rank => rank == 1 ? 14 : rank).OrderBy(rank => rank).ToList();
                    hasLowStraight = false;
                }
                // 建立一個副本，逐一移除已匹配的數值
                var remainingRanks = new HashSet<int>(highestStraight);
                print(string.Join(", ", remainingRanks));

                // 使用 `remainingRanks` 避免重複
                var straightCards = cards
                    .Where(card =>
                    {
                        int rank = card % 13 + 2;
                        if (remainingRanks.Contains(rank))
                        {
                            remainingRanks.Remove(rank); // 使用後從集合中移除
                            return true; // 包含該牌
                        }
                        return false; // 排除重複牌
                    })
                    .OrderByDescending(card => card % 13 + 2)
                    .ToList();

                targetList = straightCards;
                result[highestStraight] = straightCards;
                return result;
            }
        }
        var groupedRanks = cards
            .GroupBy(card => card % 13) // Group by rank
            .Select(group => new
            {
                Rank = group.Key + 2, // Convert to ranks (2 to 14)
                Count = group.Count(),
                Cards = group.ToList()
            })
            .OrderByDescending(group => group.Count) // Sort by group size (pairs/trips/quads first)
            .ThenByDescending(group => group.Rank) // Then by rank
            .ToList();

        // Combine groups into a sorted list of cards
        var sortedCards = groupedRanks
            .SelectMany(group => group.Cards) // Flatten groups into a single list
            .ToList();

        var sortedRanks = groupedRanks
            .SelectMany(group => Enumerable.Repeat(group.Rank, group.Cards.Count)) // Maintain rank grouping
            .ToList();

        var fourOfAKindGroup = groupedRanks.FirstOrDefault(group => group.Count == 4);
        if (fourOfAKindGroup != null)
        {
            print("四條");
            // Get the kicker
            var kicker = groupedRanks
                .Where(group => group.Rank != fourOfAKindGroup.Rank)
                .OrderByDescending(group => group.Rank)
                .FirstOrDefault();

            var fourOfAKindCards = fourOfAKindGroup.Cards;
            targetList = new List<int>(fourOfAKindCards);
            if (kicker != null)
            {
                fourOfAKindCards.Add(kicker.Cards.First());
            }

            // Take the first 5 cards for the final hand
            sortedCards = fourOfAKindCards.Take(5).ToList();
            sortedRanks = sortedCards.Select(card => card % 13 + 2).ToList();

            result[sortedRanks] = sortedCards;
            return result;
        }

        var threeOfAKindGroup = groupedRanks.FirstOrDefault(group => group.Count == 3);
        if (threeOfAKindGroup != null)
        {
            print("三條");

            var pairGroup = groupedRanks.FirstOrDefault(group => group.Count == 2 && group.Rank != threeOfAKindGroup.Rank);
            if (pairGroup != null)
            {
                print("找到葫蘆");

                // 將三條和對子組合形成葫蘆
                var fullHouseCards = threeOfAKindGroup.Cards.ToList(); // 複製三條的牌
                fullHouseCards.AddRange(pairGroup.Cards); // 加入對子的牌

                // 取得前 5 張卡片（這裡一定是五張）
                sortedCards = fullHouseCards.Take(5).ToList();
                sortedRanks = sortedCards.Select(card => card % 13 + 2).ToList();

                targetList = sortedCards;
                result[sortedRanks] = sortedCards;
                return result; // 返回結果
            }

            // Get the top two kickers
            var kickers = groupedRanks
                .Where(group => group.Rank != threeOfAKindGroup.Rank)
                .OrderByDescending(group => group.Rank)
                .Take(2)
                .SelectMany(group => group.Cards)
                .Take(2)
                .ToList();

            var threeOfAKindCards = threeOfAKindGroup.Cards;
            targetList = new List<int>(threeOfAKindCards);
            // Combine three-of-a-kind cards with the kickers
            threeOfAKindCards.AddRange(kickers);

            // Take the first 5 cards for the final hand
            sortedCards = threeOfAKindCards.Take(5).ToList();
            sortedRanks = sortedCards.Select(card => card % 13 + 2).ToList();

            result[sortedRanks] = sortedCards;

            return result;
        }
        var pairGroups = groupedRanks.Where(group => group.Count == 2).OrderByDescending(group => group.Rank).Take(2).ToList();
        if (pairGroups.Count == 2)
        {
            print("兩對");
            // Get the kickers (remaining cards not in the two pairs)
            var kicker = groupedRanks
                .Where(group => !pairGroups.Any(pairGroup => pairGroup.Rank == group.Rank))
                .OrderByDescending(group => group.Rank)
                .FirstOrDefault();

            var twoPairCards = pairGroups.SelectMany(group => group.Cards).ToList();
            targetList = new List<int>(twoPairCards);

            // Add the kicker to complete the hand
            if (kicker != null)
            {
                twoPairCards.Add(kicker.Cards.First());
            }

            // Take the first 5 cards for the final hand
            sortedCards = twoPairCards.Take(5).ToList();
            sortedRanks = sortedCards.Select(card => card % 13 + 2).ToList();

            result[sortedRanks] = sortedCards;
            return result;
        }
        // Keep only the top 5 cards
        sortedRanks = sortedRanks.Take(5).ToList();
        sortedCards = sortedCards.Take(5).ToList();

        targetList = new List<int>(takeOnePair(sortedCards));
        result[sortedRanks] = sortedCards;
        return result;
    }

    private bool HasLowStraight(List<int> ranks)
    {
        return ranks.Contains(14) && ranks.Contains(2) && ranks.Contains(3) && ranks.Contains(4) && ranks.Contains(5);
    }

    List<int> takeOnePair(List<int> sortedCards)
    {
        var sortedRanks = sortedCards.Select(card => card % 13 + 2).ToList();

        var duplicateRanks = sortedRanks.GroupBy(rank => rank)
                                .Where(group => group.Count() == 2)
                                .Select(group => group.Key) 
                                .ToList();

        var pairedCards = sortedCards.Where(card => duplicateRanks.Contains(card % 13 + 2)).ToList();
        if (pairedCards.Count != 0)
            return pairedCards;
        else
            return new List<int>();
    }

    /// <summary>
    /// 開啟符合撲克外框
    /// </summary>
    /// <param name="pokerList">撲克</param>
    /// <param name="matchNumList">符合撲克數字</param>
    public void OpenMatchPokerFrame(List<Poker> pokerList, List<int> matchNumList)
    {
        print($"開啟牌型外框 牌號: {string.Join(", ", matchNumList)}");

        foreach (var poker in pokerList)
        {
            poker.setFrameActive = false;
        }

        // Highlight matched cards
        if (matchNumList.Count != 0)
        {
            foreach (var matchNum in matchNumList)
            {
                foreach (var poker in pokerList)
                {
                    if (poker.PokerNum == matchNum)
                    {
                        Debug.Log("開啟牌框");
                        poker.setFrameActive = true;
                    }
                }
            }
        }
    }
}
