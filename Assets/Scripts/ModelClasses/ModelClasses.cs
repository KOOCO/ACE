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