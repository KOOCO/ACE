using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSample : MonoBehaviour
{
    [SerializeField]
    Image ItemIcon;

    [SerializeField]
    TextMeshProUGUI Buff_Text, CostAmount_Text;

    [SerializeField]
    Button BuyBtn;

    [SerializeField]
    Image currencyIcon;

    AssetsManager assetsManager = new();


    private void Start()
    {
        assetsManager = AssetsManager.Instance;
    }

    /// <summary>
    /// 設置商店物品資料
    /// </summary>
    /// <param name="shopData">商店資料</param>
    /// <param name="index">圖集index</param>
    /// <param name="albumEnum">圖集枚舉</param>
    public void SetShopItemData(GameObject sample, ShopData shopData, int index, AlbumEnum albumEnum)
    {
        ItemIcon.sprite = AssetsManager.Instance.GetAlbumAsset(albumEnum).album[index];

        switch (sample.name)
        {
            case "Stamina":
                Buff_Text.text = $"+ {shopData.BuffAmount} {LanguageManager.Instance.GetText("Percentage")}";
                break;
            case "Gold":
                Buff_Text.text = $"+ {shopData.BuffAmount} {LanguageManager.Instance.GetText("Points")}";
                break;
            case "ExtraTime":
                Buff_Text.text = $"+ {shopData.BuffAmount} {LanguageManager.Instance.GetText("Second")}";
                break;

        }
        CostAmount_Text.text = $"{shopData.CostCoin}";
    }


    public void SetShopItemData(ShopItem item)
    {
        Services.RemoteImageDownloader.LoadImage(item.imageUrl, ItemIcon);
        currencyIcon.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[item.currency];
        Buff_Text.text = $"{item.name}";
        CostAmount_Text.text = $"{item.price:F2}";

        // switch (item.name)
        // {
        //     // case "Energy":
        //     //     Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
        //     //     //Buff_Text.text = $"+ {item.name} {LanguageManager.Instance.GetText("Percentage")}";
        //     //     Buff_Text.text = $"+ {item.name}";
        //     //     break;
        //     // case "Gold":
        //     //     Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
        //     //     //Buff_Text.text = $"+ {item.name} {LanguageManager.Instance.GetText("Points")}";
        //     //     Buff_Text.text = $"+ {item.name}";
        //     //     break;
        //     // case "Extra Time":
        //     //     Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
        //     //     //Buff_Text.text = $"+ {item.name} {LanguageManager.Instance.GetText("Second")}";
        //     //     Buff_Text.text = $"+ {item.name}";
        //     //     break;
        //     // default:
        //     //     Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
        //     //     //Buff_Text.text = $"+ {item.name} {LanguageManager.Instance.GetText("Coin")}";
        //     //     Buff_Text.text = $"+ {item.name}";
        //     //     break;

        // }
    }

    /// <summary>
    /// 購買商品事件
    /// </summary>
    /// <param name="shopView">商店物件</param>
    /// <param name="MallMsg">購買訊息彈窗</param>
    /// <param name="itemData">商品資料</param>
    /// <param name="img">商品Icon</param>
    /// <param name="info">購買訊息欄位</param>
    /// <param name="itemCategory">商品名</param>
    public void OnBuyAddListener(LobbyShopView shopView, GameObject MallMsg, ShopItem itemData, Image img, TextMeshProUGUI info)
    {
        BuyBtn.onClick.AddListener(() =>
        {
            MallMsg.SetActive(!MallMsg.activeSelf);
            img.sprite = ItemIcon.sprite;
            //{LanguageManager.Instance.GetText("for")} {itemData.price} {assetsManager.GetAlbumAsset(AlbumEnum.CurrencyAlbum).album[itemData.currency].name}
            switch (itemData.category)
            {
                case 0:
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {itemData.targetItemQuantity} {LanguageManager.Instance.GetText("Gold")}";
                    break;
                case 1:
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {itemData.targetItemQuantity} {LanguageManager.Instance.GetText("Energy")}";
                    break;
                case 2:
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {itemData.targetItemQuantity} {LanguageManager.Instance.GetText("Timer")}";
                    break;
                case 3:
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {itemData.targetItemQuantity} {LanguageManager.Instance.GetText("Tools")}";
                    break;
            }

            shopView.GetComponent<LobbyShopView>().OnBuyingPopupUI(this, itemData);
        });
    }

    /// <summary>
    /// 餘額不足介面更新
    /// </summary>
    /// <param name="img">關閉商品icon</param>
    /// <param name="info">顯示餘額不足訊息</param>
    public void InsufficientBalance(Image img, TextMeshProUGUI info)
    {
        img.gameObject.SetActive(false);
        info.text = $"{LanguageManager.Instance.GetText("Insufficient balance")} \n {LanguageManager.Instance.GetText("Please proceed with collateralization")}";
    }

}
