using System;
using System.Collections.Generic;

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
