using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class LobbyShopView : MonoBehaviour
{
    [SerializeField]
    Toggle All_Tog, Stamina_Tog, Gold_Tog, ExtraTime_Tog;

    [SerializeField]
    GameObject All_Area,Stamina_Area, Gold_Area, ExtraTime_Area;
    [SerializeField]
    TextMeshProUGUI ALLTog_Text,StaminaTog_Text, GoldTog_Text, ExtraTimeTog_Text;
    [SerializeField]
    TextMeshProUGUI StaminaTitle_Text, GoldTitle_Text, ExtraTimeTitle_Text;

    [Header("全部商品欄位")]
    [SerializeField]
    GameObject content;
    [SerializeField]
    GameObject[] All_ShopItem_Sample;
    [SerializeField]
    GameObject[] ShopItemView;

    [Header("耐力商品欄位")]
    [SerializeField]
    GameObject Stamina_Sample;
    [SerializeField]
    GameObject Stamina_Parent;
    [SerializeField]
    TextMeshProUGUI StaminaTitle;

    [Header("金幣商品欄位")]
    [SerializeField]
    GameObject Gold_Sample;
    [SerializeField]
    GameObject Gold_Parent;
    [SerializeField]
    TextMeshProUGUI GoldTitle;

    [Header("加時商品欄位")]
    [SerializeField]
    GameObject ExtraTime_Sample;
    [SerializeField]
    GameObject ExtraTime_Parent;
    [SerializeField]
    TextMeshProUGUI ExtraTimeTitle;

    [Header("購買彈窗")]
    [SerializeField]
    GameObject MallMsg;
    [SerializeField]
    TextMeshProUGUI MallMsgTitle;
    [SerializeField]
    Image iconSprite;
    [SerializeField]
    Button Cancle, Confirm,CloseBtn;
    [SerializeField]
    GameObject PurchaseSuccessUI;
    [SerializeField]
    TextMeshProUGUI MallMsgInfo,Cancle_Text,Confirm_Text,PurchaseSuccessText;
    


    Dictionary<ItemType,GameObject> ItemList;


    /// <summary>
    /// 當前商店物品類型
    /// </summary>
    ItemType itemType;


    enum ItemType
    {
        All,
        Stamina,
        Gold, 
        ExtraTime,
    }

    private void UpdateLanguage()
    {
        ALLTog_Text.text = LanguageManager.Instance.GetText("ALL");
        //StaminaTog_Text.text = LanguageManager.Instance.GetText("STAMINA");
        GoldTog_Text.text = LanguageManager.Instance.GetText("GOLD");
        //ExtraTimeTog_Text.text = LanguageManager.Instance.GetText("EXTRATIME");

        StaminaTitle_Text.text = LanguageManager.Instance.GetText("Stamina");
        GoldTitle_Text.text = LanguageManager.Instance.GetText("Gold");
        ExtraTimeTitle_Text.text = LanguageManager.Instance.GetText("EXTRATIME");

        StaminaTitle.text =  LanguageManager.Instance.GetText("Stamina");
        GoldTitle.text = LanguageManager.Instance.GetText("Gold");
        ExtraTimeTitle.text = LanguageManager.Instance.GetText("EXTRATIME");

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

        CreateShopItem(All_ShopItem_Sample[0], All_ShopItem_Sample[0].transform.parent.gameObject, DataManager.Stamina_Shop, AlbumEnum.Shop_StaminaAlbum);
        CreateShopItem(All_ShopItem_Sample[1], All_ShopItem_Sample[1].transform.parent.gameObject, DataManager.Gold_Shop, AlbumEnum.Shop_GoldAlbum);
        CreateShopItem(All_ShopItem_Sample[2], All_ShopItem_Sample[2].transform.parent.gameObject, DataManager.ExtraTime_Shop, AlbumEnum.Shop_ExtraTimeAlbum);

        CreateShopItem(Stamina_Sample, Stamina_Parent, DataManager.Stamina_Shop, AlbumEnum.Shop_StaminaAlbum);
        CreateShopItem(Gold_Sample,Gold_Parent,DataManager.Gold_Shop, AlbumEnum.Shop_GoldAlbum);
        CreateShopItem(ExtraTime_Sample,ExtraTime_Parent,DataManager.ExtraTime_Shop, AlbumEnum.Shop_ExtraTimeAlbum);
    }
    #endregion

    #region     商品字典初始化
    private void AddItemList()
    {
        ItemList = new Dictionary<ItemType, GameObject>();

        ItemList.Add(ItemType.All, All_Area);
        ItemList.Add(ItemType.Stamina, Stamina_Area);
        ItemList.Add(ItemType.Gold, Gold_Area);
        ItemList.Add(ItemType.ExtraTime, ExtraTime_Area);
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
            int index = (All_ShopItem_Sample[i-1].transform.parent.childCount - 1) / 3;

            RectTransform CurrentRect = ShopItemView[i-1].GetComponent<RectTransform>();
            RectTransform NextRect = ShopItemView[i].GetComponent<RectTransform>();

            if ((All_ShopItem_Sample[i-1].transform.parent.childCount - 1)  % 3 > 0)
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
        Stamina_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.Stamina;
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

        //切換加時商品介面
        ExtraTime_Tog.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                itemType = ItemType.ExtraTime;
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
    private void CreateShopItem(GameObject Sample,GameObject SampleParent,List<ShopData> shopDatas,AlbumEnum albumEnum)
    {
        Sample.SetActive(false);

        for (int i=0;i<shopDatas.Count;i++)
        {
            RectTransform rect = Instantiate(Sample, SampleParent.transform).GetComponent<RectTransform>();
            rect.gameObject.SetActive(true);
            var shopSample = rect.GetComponent<ShopSample>();
            shopSample.SetShopItemData(Sample,shopDatas[i], i, albumEnum);
            shopSample.OnBuyAddListener(this,MallMsg,shopDatas[i], iconSprite, MallMsgInfo,Sample.name);
        }
    }
    

    /// <summary>
    /// 購買UI彈窗事件訂閱
    /// </summary>
    /// <param name="shopSample"></param>
    /// <param name="shopData">商品資料</param>
    /// <param name="itemName">商品名</param>
    public void OnBuyingPopupUI(ShopSample shopSample, ShopData shopData, string itemName)
    {
        Confirm.onClick.AddListener(() =>
        {

            if (DataManager.UserAChips < shopData.BuffAmount)
            {
                shopSample.InsufficientBalance(iconSprite, MallMsgInfo);
                //Debug.Log("餘額不足");
                return;
            }
            else
            {
                PurchaseSuccessUI.SetActive(!PurchaseSuccessUI.activeSelf);
                
                switch (itemName)
                {
                    case "Stamina":
                        DataManager.UserStamina += shopData.BuffAmount;
                        break;
                    case "Gold":
                        DataManager.UserGold += shopData.BuffAmount;
                        break;
                    case "Extra Time":
                        DataManager.UserOTProps += shopData.BuffAmount;
                        break;
                }

                //Debug.Log($"您已購買 {itemName} {shopData.BuffAmount}");
                DataManager.UserAChips -= shopData.BuffAmount;
                //Debug.Log($"餘額 {DataManager.UserVCChips}");
            }

        });
    }



    private void OpenShopItem()
    {
        switch(itemType)
        {
            case ItemType.All:
                ActiveShopUI(ItemType.All);
                break;

            case ItemType.Stamina:
                ActiveShopUI(ItemType.Stamina);
                break;

            case ItemType.Gold:
                ActiveShopUI(ItemType.Gold);
                break;

            case ItemType.ExtraTime:
                ActiveShopUI(ItemType.ExtraTime);
                break;
        }
    }

}
