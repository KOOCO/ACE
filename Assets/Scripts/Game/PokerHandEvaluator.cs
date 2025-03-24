using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PokerWinRateCalculator
{
    //模擬次數
    private const int simulationCount = 10000;
    //玩家手牌
    private List<int> playerHand;
    //公共牌
    private List<int> communityCards;
    //剩餘牌
    private List<int> deck;

    public PokerWinRateCalculator(List<int> playerHand, List<int> communityCards)
    {
        this.playerHand = playerHand;
        this.communityCards = communityCards;
        this.deck = InitializeDeck(playerHand, communityCards);
    }

    private List<int> InitializeDeck(List<int> playerHand, List<int> communityCards)
    {
        var deck = new List<int>();
        for (int i = 0; i < 52; i++)
        {
            if (!playerHand.Contains(i) && !communityCards.Contains(i))
            {
                deck.Add(i);
            }
        }
        return deck;
    }

    public void CalculateWinRate(UnityAction<float> callBack)
    {
        int wins = 0;
        int ties = 0;

        for (int i = 0; i < simulationCount; i++)
        {
            List<int> remainingDeck = new List<int>(deck);
            List<int> simulatedCommunityCards = new List<int>(communityCards);

            // 随机补全公共牌
            while (simulatedCommunityCards.Count < 5)
            {
                int card = remainingDeck[UnityEngine.Random.Range(0, remainingDeck.Count)];
                simulatedCommunityCards.Add(card);
                remainingDeck.Remove(card);
            }

            // 随机生成对手手牌
            List<int> opponentHand = new List<int>();
            while (opponentHand.Count < 2)
            {
                int card = remainingDeck[UnityEngine.Random.Range(0, remainingDeck.Count)];
                opponentHand.Add(card);
                remainingDeck.Remove(card);
            }

            // 判断玩家和对手的牌型
            List<int> playerCards = new List<int>(playerHand);
            playerCards.AddRange(simulatedCommunityCards);

            List<int> opponentCards = new List<int>(opponentHand);
            opponentCards.AddRange(simulatedCommunityCards);

            int playerScore = 0;
            int opponentScore = 0;
            List<int> playerBestHand = new List<int>();
            List<int> opponentBestHand = new List<int>();

            PokerShape.JudgePokerShape(playerCards, (score, bestHand) =>
            {
                playerScore = score;
                playerBestHand = bestHand;
            });

            PokerShape.JudgePokerShape(opponentCards, (score, bestHand) =>
            {
                opponentScore = score;
                opponentBestHand = bestHand;
            });

            if (playerScore < opponentScore)
            {
                wins++;
            }
            else if (playerScore == opponentScore)
            {
                if (CompareHands(playerBestHand, opponentBestHand) > 0)
                {
                    wins++;
                }
                else if (CompareHands(playerBestHand, opponentBestHand) == 0)
                {
                    ties++;
                }
            }
        }

        float winRate = (float)wins / simulationCount * 100;
        callBack(winRate);
    }

    private int CompareHands(List<int> hand1, List<int> hand2)
    {
        hand1 = hand1.OrderByDescending(x => x).ToList();
        hand2 = hand2.OrderByDescending(x => x).ToList();

        for (int i = 0; i < hand1.Count; i++)
        {
            if (hand1[i] > hand2[i])
            {
                return 1;
            }
            else if (hand1[i] < hand2[i])
            {
                return -1;
            }
        }
        return 0;
    }
}
