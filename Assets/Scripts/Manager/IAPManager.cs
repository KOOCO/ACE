using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System;
using TMPro;

public class IAPManager : MonoBehaviour
{
    string crystalTest = "test001";

    private void Start()
    {
         //StartCoroutine(checkIAPInitial());
    }

    IEnumerator checkIAPInitial()
    {
        while (!CodelessIAPStoreListener.initializationComplete)
        {
            UnityPurchasing.Initialize(CodelessIAPStoreListener.Instance, ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()));
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Initialized Successfully");

        foreach (var product in controller.products.all)
        {
            Debug.Log($"Product ID: {product.definition.id}, Available: {product.availableToPurchase}");
        }
    }

    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == crystalTest)
        {
            print("you've gain 170 crystals");
            //NoodleApi.PostTransaction(DataManager.UserNickname, 170);
            //不要用這個方法(已禁用)，應該改成通知後台，讓Noodle幫玩家上分
        }
    }

    public void OnPurchaseFetched(Product product)
    {
        
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        print(product.definition.id + " failed because " + failureReason);
    }

    void updateButtonText(Button button, Product product)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = product.metadata.localizedPrice + " " + product.metadata.isoCurrencyCode;
        }
    }
}
