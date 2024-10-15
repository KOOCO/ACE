using System;
using System.Collections.Generic;
using Org.BouncyCastle.Tls;

#region Shop
[Serializable]
public class ShopItem
{
    public string name;
    public double price;
    public int currency;
    public int category;
    public int targetItemQuantity;
    public string imageUrl;
    public string blobFileName;
    //public DateTime? lastModificationTime;
    public string lastModifierId;
    //public DateTime creationTime;
    public string creatorId;
    public string id;
}

[Serializable]
public class ItemList
{
    public List<ShopItem> items;
    public int totalCount;
}

public class PurchaseItem
{
    public string itemId;
    public string playerId;

}
#endregion

#region User Data

[Serializable]
public class Player
{
    public string accessToken;
    public string memberId;
    public string memberStatus;
    public int promotionCoin;
    public int gold;
    public int timer;
    public int currentEnergy;
    public int maxEnergy;
    public float walletAmount;
    public int rankPoint;
    public string inviteCode;
}
#endregion

#region Lobby
[System.Serializable]
public class TableItem
{
    public string id;
    public string creationTime;
    public string creatorId;
    public string lastModificationTime;
    public string lastModifierId;
    public bool isDeleted;
    public string deleterId;
    public string deletionTime;
    public string tenantId;
    public int mode;
    public double rebateSetting;
    public double stake;
    public double smallStake;
    public double minBuyIn;
    public double maxBuyIn;
    public bool isEnable;
}

[Serializable]
public class TableItemList
{
    public List<TableItem> items;
    public int totalCount;
}
#endregion

#region Join Round
public class JoinRound
{
    public string memberId;
    public string tableId;
    public double amount;
    public int rankPoint;
}
public class LeaveRound
{
    public string memberId;
    public double amount;
    public string type;
    public int rankPoint;
}
public class GameRound
{
    public string id;
    public string roomId;
    public string tableId;
    public string tableType;
    public bool isGameEnd;
    //public DateTime gameEndTime;
    public double tableCommission;
    public string name;
    public string tenantId;
    public string roundId;
    //public DateTime creationTime;
    public Table table;
    public List<RoundMember> roundMembers;
    public List<BoardCard> boardCards;
    public List<Winner> winners;
    public List<Loser> losers;
}

public class Table
{
    public string id;
    //public DateTime creationTime;
    public string creatorId;
    //public DateTime lastModificationTime;
    public string lastModifierId;
    public bool isDeleted;
    public string deleterId;
    //public DateTime deletionTime;
    public string tenantId;
    public int mode;
    public double rebateSetting;
    public double stake;
    public double smallStake;
    public double minBuyIn;
    public double maxBuyIn;
    public bool isEnable;
}

public class RoundMember
{
    public string id;
    public string memberId;
    public string roundId;
    public bool isBot;
    public bool isDeleted;
    public Member member;
    public List<MemberCard> memberCards;
}

public class Member
{
    public string id;
    //public DateTime creationTime;
    public string creatorId;
    //public DateTime lastModificationTime;
    public string lastModifierId;
    public int memberId;
    public string companyCode;
    public string systemCode;
    public string siteCode;
    public string memberName;
    public string memberAccount;
    public bool status;
    public string currency;
    public double suspensionAmount;
    public double currentAmount;
    public double singleWallet;
    public double openTable;
    public bool online;
    public string ip;
    public string inviteCode;
    //public DateTime lastLoginTime;
    //public DateTime firstLoginTime;
    public string tenantId;
    public string userId;
    public int rankPoint;
    public string phone;
}

public class MemberCard
{
    public string id;
    public string code;
    public string roundMemberId;
    public bool isDeleted;
}

public class BoardCard
{
    public string id;
    public string code;
    public string roundId;
    public bool isDeleted;
}

public class Winner
{
    public string id;
    public string roundId;
    public string memberId;
    public double gainRankPoint;
    public double winAmount;
    public bool isDeleted;
    public Member member;
}

public class Loser
{
    public string id;
    public string roundId;
    public string memberId;
    public double loseRankPoint;
    public double lostAmount;
    public bool isDeleted;
    public Member member;
}

public class RoundEndResult
{
    public DateTime dateTime;
    public string tableId;
    public string roomId;
    public int roundId;
    public List<PlayerHand> playerHands;
    public List<int> communityCards;
}
public class PlayerHand
{
    public List<int> playerHand;
    public double potWinAmount;
    public double sideWinAmount;
}



#endregion