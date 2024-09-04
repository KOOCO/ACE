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


    public void SetShopItemData(Item item)
    {
        Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
        ItemIcon.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.Shop_GoldAlbum).album[0];


        switch (item.name)
        {
            case "Stamina":
                Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
                Buff_Text.text = $"+ {item.price} {LanguageManager.Instance.GetText("Percentage")}";
                break;
            case "Gold":
                Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
                Buff_Text.text = $"+ {item.price} {LanguageManager.Instance.GetText("Points")}";
                break;
            case "Extra Time":
                Debug.Log($"Setting Shop Data :: {item.name} {item.price}");
                Buff_Text.text = $"+ {item.price} {LanguageManager.Instance.GetText("Second")}";
                break;

        }
        CostAmount_Text.text = $"{item.price}";
    }

    /// <summary>
    /// 購買商品事件
    /// </summary>
    /// <param name="shopView">商店物件</param>
    /// <param name="MallMsg">購買訊息彈窗</param>
    /// <param name="shopData">商品資料</param>
    /// <param name="img">商品Icon</param>
    /// <param name="info">購買訊息欄位</param>
    /// <param name="ItemName">商品名</param>
    public void OnBuyAddListener(LobbyShopView shopView, GameObject MallMsg, ShopData shopData, Image img, TextMeshProUGUI info, string ItemName)
    {
        BuyBtn.onClick.AddListener(() =>
        {
            MallMsg.SetActive(!MallMsg.activeSelf);
            img.sprite = ItemIcon.sprite;

            switch (ItemName)
            {
                case "Stamina":
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {shopData.BuffAmount} {LanguageManager.Instance.GetText("Stamina")}";
                    break;
                case "Gold":
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {shopData.BuffAmount} {LanguageManager.Instance.GetText("Gold")}";
                    break;
                case "ExtraTime":
                    info.text = $"{LanguageManager.Instance.GetText("Purchase")} {shopData.BuffAmount} {LanguageManager.Instance.GetText("ExtraTime")}";
                    break;
            }

            shopView.GetComponent<LobbyShopView>().OnBuyingPopupUI(this, shopData, ItemName);
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
