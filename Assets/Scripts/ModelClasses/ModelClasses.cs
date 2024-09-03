using System;

#region Shop
[Serializable]
public class Item
{
    public string id;
    //public DateTime creationTime;
    public string creatorId;
    //public DateTime lastModificationTime;
    public string lastModifierId;
    public string name;
    public float price;
    public string imageUrl;
    public string blobFileName;
}

[Serializable]
public class ItemList
{
    public Item[] items;
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