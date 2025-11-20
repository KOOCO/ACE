using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System;
using TMPro;

public class IAPManager : MonoBehaviour
{
    public bool firstStored1;
    public bool firstStored2;
    public bool firstStored3;
    public bool firstStored4;
    public bool firstStored5;
    public bool firstStored6;

    string crystalTest = "test001";

    private void Start()
    {
        //firstStored1 = FirebaseEmailManager.inst.FirstStored1;
        //firstStored2 = FirebaseEmailManager.inst.FirstStored2;
        //firstStored3 = FirebaseEmailManager.inst.FirstStored3;
        //firstStored4 = FirebaseEmailManager.inst.FirstStored4;
        //firstStored5 = FirebaseEmailManager.inst.FirstStored5;
        //firstStored6 = FirebaseEmailManager.inst.FirstStored6;

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
            NoodleApi.PostTransaction(DataManager.UserNickname, 170);
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

    public void loadPurchase()
    {
        //FirebaseEmailManager.inst.coLoadPurchase();
    }
}
