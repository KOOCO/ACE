using System;
using System.Collections.Generic;
using Org.BouncyCastle.Tls;
using UnityEditor;

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
public class JoinRoom
{
    public string memberId;
    public string tableId;
    public double amount;
    public int rankPoint;
}
public class LeaveRoom
{
    public string memberId;
    public double amount;
    public string type;
    public int rankPoint;
}
public class GameRoom
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
    public List<PlayerDetails> playerDetails;
    public List<int> communityCards;
}
public class PlayerDetails
{
    public string playerId;
    public string playerName;
    public PlayerHand playerHandData;
}

public class PlayerHand
{
    public List<int> playerHand;
    public int playerCurrHandShape;
    public double potWinChips;
    public double sideWinChips;
    public bool isWinner;
    public int seat;
}



#endregion

#region Noodle Login
public class NoodleUserData
{
    public string memberId;
    public string userName;
    public decimal balance;
    public string accessCode;

    public NoodleUserData(string memberId, string userName, decimal balance, string accessCode)
    {
        this.memberId = memberId;
        this.userName = userName;
        this.balance = balance;
        this.accessCode = accessCode;
    }
}
public class NoodleResponse
{
    public NoodleUserData data;
    public string code;
    public string[] messages;

    public NoodleResponse(NoodleUserData data, string code, string[] messages)
    {
        this.data = data;
        this.code = code;
        this.messages = messages;
    }
}
#endregion

#region Noodle Balance inquiry
public class NoodleBalanceResponse
{
    public NoodleBalance data;
    public string code;
    public List<string> messages;
}

public class NoodleBalance
{
    public double balance;
}
#endregion

#region  Table_Chips_Transaction 
public class Data
{
    public int availableChips;

    public Data(int availableChips)
    {
        this.availableChips = availableChips;
    }
}

public class TableChipsTransactionResponse
{
    public Data data;
    public string code;
    public string[] messages;

    public TableChipsTransactionResponse(Data data, string code, string[] messages)
    {
        this.data = data;
        this.code = code;
        this.messages = messages;
    }
}

public class TableChipsTransaction
{
    public string accessCode;
    public string noodleMemberId;
    public string tableId;
    public string roundId;
    public double amount;
    public int chipTransactionType;
    public string transferId;

    public TableChipsTransaction(string accessCode, string noodleMemberId, string tableId, string roundId, double amount, int chipTransactionType, string transferId)
    {
        this.accessCode = accessCode;
        this.noodleMemberId = noodleMemberId;
        this.tableId = tableId;
        this.roundId = roundId;
        this.amount = amount;
        this.chipTransactionType = chipTransactionType;
        this.transferId = transferId;
    }
}

#endregion

#region Table_Buy_in
public class BuyInData
{
    public int availableChips;

    public BuyInData(int availableChips)
    {
        this.availableChips = availableChips;
    }
}

public class TableBuyInResponse
{
    public BuyInData data;
    public string code;
    public string[] messages;

    public TableBuyInResponse(BuyInData data, string code, string[] messages)
    {
        this.data = data;
        this.code = code;
        this.messages = messages;
    }
}

public class Tablebuyin
{
    public string accessCode;
    public string noodleMemberId;
    public string tableId;
    public double amount;

    public Tablebuyin(string accessCode, string noodleMemberId, string tableId, double amount)
    {
        this.accessCode = accessCode;
        this.noodleMemberId = noodleMemberId;
        this.tableId = tableId;
        this.amount = amount;
    }
}

#endregion

#region Table Cash Out Chips
public class CashOutData
{
    public int cashOutChips;

    public CashOutData(int cashOutChips)
    {
        this.cashOutChips = cashOutChips;
    }
}

public class CashOutChipsResponse
{
    public CashOutData data;
    public string code;
    public string[] messages;

    public CashOutChipsResponse(CashOutData data, string code, string[] messages)
    {
        this.data = data;
        this.code = code;
        this.messages = messages;
    }
}

public class TableCashout
{
    public string accessCode;
    public string noodleMemberId;
    public string tableId;

    public TableCashout(string accessCode, string noodleMemberId, string tableId)
    {
        this.accessCode = accessCode;
        this.noodleMemberId = noodleMemberId;
        this.tableId = tableId;
    }
}

#endregion

#region Auth
public class Register
{
    public string inviteCode;
    public string phoneNumber;
    public string userName;
    public string password;
    public string confirmPassword;
}

public class LoginRequest
{
    public string userNameOrEmailAddress;
    public string password;
    public string ipAddress;
    public string machineCode;
}
public class PasswordLessLogin
{
    public string walletAddress;
    public string ipAddress;
    public string machineCode;
}

/// <summary>
/// 錢包註冊資料
/// </summary>
public class RegisterPasswordLess
{
    public string memberName;
    public string emailAddress;
    public string walletAddress;
}
#endregion