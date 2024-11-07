using System.ComponentModel;

    /// <summary>
    /// 表示德州撲克遊戲中玩家籌碼的不同交易類型，例如入桌、下注、全下、結算等操作。這些交易類型用於記錄每次籌碼變化，以便追蹤每個玩家的行為和點數變動。
    /// 
    /// Represents the different types of chip transactions in a Texas Hold'em game, such as buy-in, betting, all-in, and settlement actions. These transaction types are used to record each chip change to track each player's actions and point changes.
    /// </summary>
    public enum ChipTransactionType
    {
        /// <summary>
        /// 玩家進入遊戲時攜帶的籌碼
        /// Buy-in: Chips brought by the player when entering the game.
        /// </summary>
        [Description("Buy-in: Chips brought by the player when entering the game.")]
        BuyIn = 1,

        /// <summary>
        /// 小盲注
        /// Small Blind: Forced bet by the player in the small blind position.
        /// </summary>
        [Description("Small Blind: Forced bet by the player in the small blind position.")]
        SmallBlind = 2,

        /// <summary>
        /// 大盲注
        /// Big Blind: Forced bet by the player in the big blind position.
        /// </summary>
        [Description("Big Blind: Forced bet by the player in the big blind position.")]
        BigBlind = 3,

        /// <summary>
        /// 跟注
        /// Call: Matching the current highest bet in the round.
        /// </summary>
        [Description("Call: Matching the current highest bet in the round.")]
        Call = 4,

        /// <summary>
        /// 加注
        /// Raise: Increasing the current highest bet in the round.
        /// </summary>
        [Description("Raise: Increasing the current highest bet in the round.")]
        Raise = 5,

        /// <summary>
        /// 全下
        /// All-in: Betting all remaining chips.
        /// </summary>
        [Description("All-in: Betting all remaining chips.")]
        AllIn = 6,

        /// <summary>
        /// 棄牌
        /// Fold: Player gives up the hand and forfeits the chips already bet.
        /// </summary>
        [Description("Fold: Player gives up the hand and forfeits the chips already bet.")]
        Fold = 7,

        /// <summary>
        /// 前注
        /// Ante: A small forced bet placed by all players before the hand begins.
        /// </summary>
        [Description("Ante: A small forced bet placed by all players before the hand begins.")]
        Ante = 8,

        /// <summary>
        /// 下注
        /// Bet: Initial wager placed by a player in a betting round.
        /// </summary>
        [Description("Bet: Initial wager placed by a player in a betting round.")]
        Bet = 9,

        /// <summary>
        /// 重新購買籌碼
        /// Rebuy: Purchasing more chips to continue playing.
        /// </summary>
        [Description("Rebuy: Purchasing more chips to continue playing.")]
        ReBuy = 10,

        /// <summary>
        /// 自願放的額外盲注（Straddle）
        /// Straddle: Voluntary blind bet to increase the pot.
        /// </summary>
        [Description("Straddle: Voluntary blind bet to increase the pot.")]
        Straddle = 11,

        /// <summary>
        /// 玩家在結算後的贏取金額
        /// Win: Chips won by the player after a hand.
        /// </summary>
        [Description("Win: Chips won by the player after a hand.")]
        Win = 12,

        /// <summary>
        /// 玩家在結算後的輸掉金額
        /// Loss: Chips lost by the player after a hand.
        /// </summary>
        [Description("Loss: Chips lost by the player after a hand.")]
        Loss = 13,

        /// <summary>
        /// 離開牌桌時的籌碼兌換
        /// Cash Out: Converting chips back to cash when leaving the table.
        /// </summary>
        [Description("Cash Out: Converting chips back to cash when leaving the table.")]
        CashOut = 14,

        /// <summary>
        /// 保險金 (適用於玩家購買保險的情況)
        /// Insurance: Chips spent on purchasing insurance (used in specific game types).
        /// </summary>
        [Description("Insurance: Chips spent on purchasing insurance (used in specific game types).")]
        Insurance = 15,

        /// <summary>
        /// 當莊家All-In時，其他玩家之間的邊池下注
        /// Side Pot: Bets placed in a side pot when the dealer goes all-in.
        /// </summary>
        [Description("Side Pot: Bets placed in a side pot when the dealer goes all-in.")]
        SidePot = 16,

        /// <summary>
        /// 籌碼被沒收（例如：違規行為）
        /// Forfeit: Chips forfeited due to violations or penalties.
        /// </summary>
        [Description("Forfeit: Chips forfeited due to violations or penalties.")]
        Forfeit = 17,

        /// <summary>
        /// 籌碼返還（例如：無效的下注）
        /// Refund: Return of chips due to invalid bets or canceled actions.
        /// </summary>
        [Description("Refund: Return of chips due to invalid bets or canceled actions.")]
        Refund = 18,

        /// <summary>
        /// 超時棄牌被收走的籌碼
        /// Timeout Fold: Chips taken due to folding after a timeout.
        /// </summary>
        [Description("Timeout Fold: Chips taken due to folding after a timeout.")]
        TimeoutFold = 19,

        /// <summary>
        /// 分潤（適用於邊池分配）
        /// Split: Distribution of chips between multiple winners (used for side pot or tied hands).
        /// </summary>
        [Description("Split: Distribution of chips between multiple winners (used for side pot or tied hands).")]
        Split = 20,

        /// <summary>
        /// 桌水（服務費或佣金）：每局從賭池中抽取的一部分作為場地費用或平台佣金。
        /// 
        /// Table Fee (Rake): A portion taken from the pot as a service fee or platform commission for each game.
        /// </summary>
        [Description("Table Fee: A portion taken from the pot as a service fee or platform commission.")]
        TableFee = 21
    }

