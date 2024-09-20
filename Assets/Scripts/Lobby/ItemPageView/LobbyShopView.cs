using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.Linq;

public class LobbyShopView : MonoBehaviour
{
    [SerializeField]
    Toggle All_Tog, Energy_Tog, Gold_Tog, Tools_Tog, Timer_Tog, ACoin_Tog;

    [SerializeField]
    GameObject All_Area, Energy_Area, Gold_Area, Timer_Area, Tools_Area, ACoin_Area;
    [SerializeField]
    TextMeshProUGUI ALLTog_Text, EnergyTog_Text, GoldTog_Text, TimerTog_Text, ToolsTog_Text, ACoinTog_Text;
    [SerializeField]
    TextMeshProUGUI EnergyTitle_Text, GoldTitle_Text, TimerTitle_Text, ToolsTitle_Text, ACoinTitle_Text;

    [Header("全部商品欄位")]
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject[] All_ShopItem_Sample;
    [SerializeField]
    GameObject Shop_Item;
    [SerializeField]
    GameObject[] ShopItemView;

    [Header("耐力商品欄位")]
    // [SerializeField]
    // GameObject Stamina_Sample;
    [SerializeField]
    GameObject Energy_Parent;
    [SerializeField]
    TextMeshProUGUI EnergyTitle;

    [Header("金幣商品欄位")]
    // [SerializeField]
    // GameObject Gold_Sample;
    [SerializeField]
    GameObject Gold_Parent;
    [SerializeField]
    TextMeshProUGUI GoldTitle;

    [Header("加時商品欄位")]
    // [SerializeField]
    // GameObject ExtraTime_Sample;
    [SerializeField]
    GameObject Timer_Parent;
    [SerializeField]
    TextMeshProUGUI TimerTitle;

    [Header("Tools")]
    // [SerializeField]
    // GameObject ExtraTime_Sample;
    [SerializeField]
    GameObject Tools_Parent;
    [SerializeField]
    TextMeshProUGUI ToolsTitle;

    [Header("ACoin")]
    // [SerializeField]
    // GameObject ExtraTime_Sample;
    [SerializeField]
    GameObject ACoin_Parent;
    [SerializeField]
    TextMeshProUGUI ACoinTitle;

    [Header("購買彈窗")]
    [SerializeField]
    GameObject MallMsg;
    [SerializeField]
    TextMeshProUGUI MallMsgTitle;
    [SerializeField]
    Image iconSprite;
    [SerializeField]
    Button Cancle, Confirm, CloseBtn;
    [SerializeField]
    GameObject PurchaseSuccessUI;
    [SerializeField]
    TextMeshProUGUI MallMsgInfo, Cancle_Text, Confirm_Text, PurchaseSuccessText;


    ItemList shopItemsList = new();
    Dictionary<ItemType, GameObject> ItemList;


    /// <summary>
    /// 當前商店物品類型
    /// </summary>
    ItemType itemType;


    enum ItemType
    {
        All,
        Energy,
        Timer,
        Gold,
        Tools,
        ACoin,
    }
    enum CurrencyType
    {
        Gold,
        ACoin,
        UCoin,
    }
    private void UpdateLanguage()
    {
        ALLTog_Text.text = LanguageManager.Instance.GetText("ALL");
        EnergyTog_Text.text = LanguageManager.Instance.GetText("ENERGY");
        GoldTog_Text.text = LanguageManager.Instance.GetText("GOLD");
        TimerTog_Text.text = LanguageManager.Instance.GetText("TIMER");
        ToolsTog_Text.text = LanguageManager.Instance.GetText("TOOLS");
        ACoinTog_Text.text = LanguageManager.Instance.GetText("ACoin");

        EnergyTitle_Text.text = LanguageManager.Instance.GetText("Energy");
        GoldTitle_Text.text = LanguageManager.Instance.GetText("Gold");
        TimerTitle_Text.text = LanguageManager.Instance.GetText("Timer");
        ToolsTitle_Text.text = LanguageManager.Instance.GetText("Tools");
        ACoinTitle_Text.text = LanguageManager.Instance.GetText("ACoin");

        EnergyTitle.text = LanguageManager.Instance.GetText("Energy");
        GoldTitle.text = LanguageManager.Instance.GetText("Gold");
        TimerTitle.text = LanguageManager.Instance.GetText("Timer");
        ToolsTitle.text = LanguageManager.Instance.GetText("Tools");
        ACoinTitle.text = LanguageManager.Instance.GetText("ACoin");

        Cancle_Text.text = LanguageManager.Instance.GetText("CANCLE");
        Confirm_Text.text = LanguageManager.Instance.GetText("CONFIRM");

        MallMsgTitle.text = LanguageManager.Instance.GetText("Mall Message");
        PurchaseSuccessText.text = LanguageManager.Instance.GetText("PurchaseSuccess");
    }


    private void Awake()
    {
        AddItemList();
        ListenerEvent();
        InitShop();

        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
    }


    private void OnEnable()
    {
        itemType = ItemType.All;
        MallMsg.gameObject.SetActive(false);
        OpenShopItem();
        //ShopLayoutGroup();
    }
    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }


    #region 生成初始商店
    public void InitShop()
    {
        SwaggerAPIManager.Instance.SendGetAPI("api/app/items/get-list", CallBackOnGetList, null, true);
        // CreateShopItem(All_ShopItem_Sample[0], All_ShopItem_Sample[0].transform.parent.gameObject, DataManager.Stamina_Shop, AlbumEnum.Shop_StaminaAlbum);
        // CreateShopItem(All_ShopItem_Sample[1], All_ShopItem_Sample[1].transform.parent.gameObject, DataManager.Gold_Shop, AlbumEnum.Shop_GoldAlbum);
        // CreateShopItem(All_ShopItem_Sample[2], All_ShopItem_Sample[2].transform.parent.gameObject, DataManager.ExtraTime_Shop, AlbumEnum.Shop_ExtraTimeAlbum);

        // CreateShopItem(Stamina_Sample, Stamina_Parent, DataManager.Stamina_Shop, AlbumEnum.Shop_StaminaAlbum);
        // CreateShopItem(Gold_Sample, Gold_Parent, DataManager.Gold_Shop, AlbumEnum.Shop_GoldAlbum);
        // CreateShopItem(ExtraTime_Sample, ExtraTime_Parent, DataManager.ExtraTime_Shop, AlbumEnum.Shop_ExtraTimeAlbum);
    }
    void CallBackOnGetList(string data)
    {
        Debug.Log(data);
        ClearChilds(ShopItemView[0].transform.GetChild(1).transform);
        ClearChilds(ShopItemView[1].transform.GetChild(1).transform);
        ClearChilds(ShopItemView[2].transform.GetChild(1).transform);
        ClearChilds(Gold_Parent.transform);
        ClearChilds(Timer_Parent.transform);
        ClearChilds(Energy_Parent.transform);
        shopItemsList = JsonConvert.DeserializeObject<ItemList>(data);
        Debug.Log(shopItemsList.items.Count);
        foreach (var item in shopItemsList.items)
        {
            switch (item.category)
            {
                case 0:
                    Debug.Log("GOLD");
                    Gold_Tog.gameObject.SetActive(true);
                    ShopItemView[0].gameObject.SetActive(true);
                    CreateShopItem(Shop_Item, ShopItemView[0].transform.GetChild(1).transform, item);
                    CreateShopItem(Shop_Item, Gold_Parent.transform, item);
                    break;
                case 1:
                    Debug.Log("Energy");
                    Energy_Tog.gameObject.SetActive(true);
                    ShopItemView[1].gameObject.SetActive(true);
                    CreateShopItem(Shop_Item, ShopItemView[1].transform.GetChild(1).transform, item);
                    CreateShopItem(Shop_Item, Energy_Parent.transform, item);
                    break;
                case 2:
                    Debug.Log("Timer");
                    Timer_Tog.gameObject.SetActive(true);
                    ShopItemView[2].gameObject.SetActive(true);
                    CreateShopItem(Shop_Item, ShopItemView[2].transform.GetChild(1).transform, item);
                    CreateShopItem(Shop_Item, Timer_Parent.transform, item);
                    break;
                case 3:
                    Debug.Log("Tools");
                    Tools_Tog.gameObject.SetActive(true);
                    ShopItemView[3].gameObject.SetActive(true);
                    CreateShopItem(Shop_Item, ShopItemView[3].transform.GetChild(1).transform, item);
                    CreateShopItem(Shop_Item, Tools_Parent.transform, item);
                    break;
                case 4:
                    Debug.Log("Tools");
                    ACoin_Tog.gameObject.SetActive(true);
                    ShopItemView[4].gameObject.SetActive(true);
                    CreateShopItem(Shop_Item, ShopItemView[4].transform.GetChild(1).transform, item);
                    CreateShopItem(Shop_Item, ACoin_Parent.transform, item);
                    break;
            }
        }
    }

    void ShowHideObject()
    {

    }
    #endregion

    #region     商品字典初始化
    private void AddItemList()
    {
        ItemList = new Dictionary<ItemType, GameObject>();

        ItemList.Add(ItemType.All, All_Area);
        ItemList.Add(ItemType.Gold, Gold_Area);
        ItemList.Add(ItemType.Energy, Energy_Area);
        ItemList.Add(ItemType.Timer, Timer_Area);
        ItemList.Add(ItemType.Tools, Tools_Area);
        ItemList.Add(ItemType.ACoin, ACoin_Area);
    }

    #endregion

    #region All欄位自動排列
    /// <summary>
    /// All欄位自動排列
    /// </summary>
    private void ShopLayoutGroup()
    {
        for (int i = 1; i < ShopItemView.Length; i++)
        {
            int index = (All_ShopItem_Sample[i - 1].transform.parent.childCount - 1) / 3;

            RectTransform CurrentRect = ShopItemView[i - 1].GetComponent<RectTransform>();
            RectTransform NextRect = ShopItemView[i].GetComponent<RectTransform>();

            if ((All_ShopItem_Sample[i - 1].transform.parent.childCount - 1) % 3 > 0)
            {
                NextRect.localPosition = new Vector2(CurrentRect.localPosition.x, ((CurrentRect.localPosition.y - 170)) - (index * 143));
            }
            else
            {
                NextRect.localPosition = new Vector2(CurrentRect.localPosition.x, ((CurrentRect.localPosition.y - 170)) - ((index - 1) * 143));
            }
        }

        RectTransform contentRect = content.GetComponent<RectTransform>();
        float rectTemp = 0;

        rectTemp = ShopItemView[ShopItemView.Length - 1].transform.localPosition.y - 200 * 2;

        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, -rectTemp);
    }

    #endregion



    /// <summary>
    /// 事件
    /// </summary>
    private void ListenerEvent()
    {
        //  切換全商品介面
        All_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.All;
                OpenShopItem();
            }
        });

        //  切換耐力商品介面  
        Energy_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.Energy;
                OpenShopItem();
            }
        });

        //切換金幣商品介面
        Gold_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.Gold;
                OpenShopItem();
            }
        });

        Timer_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.Timer;
                OpenShopItem();
            }
        });

        //切換加時商品介面
        Tools_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.Tools;
                OpenShopItem();
            }
        });
        ACoin_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.ACoin;
                OpenShopItem();
            }
        });


        //  關閉BuyPopUpUI
        CloseBtn.onClick.AddListener(() =>
        {
            MallMsg.SetActive(!MallMsg.activeSelf);
            iconSprite.gameObject.SetActive(true);
            Confirm.onClick.RemoveAllListeners();       //  取消事件訂閱
        });
        //  取消Buying
        Cancle.onClick.AddListener(() =>
        {
            MallMsg.SetActive(!MallMsg.activeSelf);
            iconSprite.gameObject.SetActive(true);
            Confirm.onClick.RemoveAllListeners();       //  取消事件訂閱
        });
        PurchaseSuccessUI.GetComponent<Button>().onClick.AddListener(() =>
        {
            MallMsg.SetActive(false);
            iconSprite.gameObject.SetActive(true);
            Confirm.onClick.RemoveAllListeners();

        });

    }

    /// <summary>
    /// 開關ShopUI
    /// </summary>
    /// <param name="itemType">物件類型</param>
    private void ActiveShopUI(ItemType itemType)
    {

        foreach (var item in ItemList)
        {
            if (item.Key == itemType)
                item.Value.SetActive(true);
            else
                item.Value.SetActive(false);
        }
    }

    /// <summary>
    /// 生成商店物件
    /// </summary>
    /// <param name="Sample">生成Item</param>
    /// <param name="SampleParent">Item父物件</param>
    /// <param name="shopDatas">傳入商店資料</param>
    /// <param name="albumEnum">圖集枚舉</param>
    private void CreateShopItem(GameObject Sample, GameObject SampleParent, List<ShopData> shopDatas, AlbumEnum albumEnum)
    {
        Sample.SetActive(false);

        for (int i = 0; i < shopDatas.Count; i++)
        {
            RectTransform rect = Instantiate(Sample, SampleParent.transform).GetComponent<RectTransform>();
            rect.gameObject.SetActive(true);
            var shopSample = rect.GetComponent<ShopSample>();
            shopSample.SetShopItemData(Sample, shopDatas[i], i, albumEnum);
            //shopSample.OnBuyAddListener(this, MallMsg, shopDatas[i], iconSprite, MallMsgInfo, Sample.name);
        }
    }

    public void CreateShopItem(GameObject shopItem, Transform itemParent, ShopItem item)
    {
        ShopSample newShopItem = Instantiate(shopItem, itemParent).GetComponent<ShopSample>();
        newShopItem.SetShopItemData(item);
        newShopItem.OnBuyAddListener(this, MallMsg, item, iconSprite, MallMsgInfo);
    }
    public void ClearChilds(Transform _stransform)
    {
        foreach (Transform child in _stransform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 購買UI彈窗事件訂閱
    /// </summary>
    /// <param name="shopSample"></param>
    /// <param name="shopData">商品資料</param>
    /// <param name="itemCategory">商品名</param>
    public void OnBuyingPopupUI(ShopSample shopSample, ShopItem shopData)
    {
        Confirm.onClick.AddListener(() =>
        {
            PurchaseItem itemToPurchase = new PurchaseItem()
            {
                itemId = shopData.id,
                playerId = Services.PlayerService.GetPlayer().memberId,
            };
            if (DataManager.UserAChips < shopData.price)
            {
                shopSample.InsufficientBalance(iconSprite, MallMsgInfo);
                //Debug.Log("餘額不足");
                return;
            }
            else
            {
                SwaggerAPIManager.Instance.SendPostAPI<PurchaseItem>($"api/app/items/purchase-item?itemId={itemToPurchase.itemId}&playerId={itemToPurchase.playerId}", null, (data) =>
                {
                    Debug.Log(data);
                    PurchaseSuccessUI.SetActive(!PurchaseSuccessUI.activeSelf);

                    switch (shopData.category)
                    {
                        case 0:
                            DataManager.UserGold += shopData.targetItemQuantity;
                            break;
                        case 1:
                            DataManager.UserEnergy += shopData.targetItemQuantity;
                            break;
                        case 2:
                            DataManager.UserTimer += shopData.targetItemQuantity;
                            break;
                        case 3:
                            DataManager.UserTools += shopData.targetItemQuantity;
                            break;
                        case 4:
                            DataManager.UserAChips += (int)shopData.targetItemQuantity;
                            break;
                    }

                    switch (shopData.currency)
                    {
                        case 0:
                            Debug.Log("Gold");
                            DataManager.UserGold -= shopData.price;
                            break;
                        case 1:
                            Debug.Log("UCoin");
                            DataManager.UserUChips -= (int)shopData.price;
                            break;
                        case 2:
                            Debug.Log("ACoin");
                            DataManager.UserAChips -= (int)shopData.price;
                            break;
                    }

                    DataManager.DataUpdated = true;
                }, (errMsg) =>
                {
                    Debug.LogError(errMsg);
                }, true, true);

                //Debug.Log($"您已購買 {itemName} {shopData.BuffAmount}");
                //DataManager.UserAChips -= shopData.price;
                //Debug.Log($"餘額 {DataManager.UserVCChips}");
            }

        });
    }

    private void OpenShopItem()
    {
        switch (itemType)
        {
            case ItemType.All:
                ActiveShopUI(ItemType.All);
                break;

            case ItemType.Energy:
                ActiveShopUI(ItemType.Energy);
                break;

            case ItemType.Gold:
                ActiveShopUI(ItemType.Gold);
                break;

            case ItemType.Timer:
                ActiveShopUI(ItemType.Timer);
                break;

            case ItemType.Tools:
                ActiveShopUI(ItemType.Tools);
                break;
            case ItemType.ACoin:
                ActiveShopUI(ItemType.ACoin);
                break;

        }
    }

}
