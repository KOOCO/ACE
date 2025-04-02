using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebGLSupport;
using System;

public class CheckTransactionManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Button sumbit_Btn;
    [SerializeField]
    private TMP_InputField address_Field, hash_Field, amount_Field;
    [SerializeField]
    private GameObject checkingTip_Obj;
    private void Start()
    {
        sumbit_Btn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(address_Field.text) && !string.IsNullOrEmpty(hash_Field.text) && !string.IsNullOrEmpty(amount_Field.text))
            {
                string hashKey = hash_Field.text;
                float amount;
                if (float.TryParse(amount_Field.text, out amount))
                {
                    checkingTip_Obj.SetActive(true);
                    CryptoTransaction transaction = new CryptoTransaction()
                    {
                        uniqueSerial = Guid.NewGuid().ToString(),
                        noodleMemberId = DataManager.NoodleMemberId,
                        memberId = DataManager.UserId,
                        hashKey = hashKey,
                        currencyCode = 100003,
                        transactionType = 1,
                        amount = amount
                    };
                    Debug.Log(DataManager.UserId);
                    AppApi.SendTransaction(transaction, (x) =>
                    {
                        Debug.Log(x);
                    }, (error) => Debug.LogError("交易驗證錯誤"));
                }
                else
                {
                    Debug.Log("金額輸入格式錯誤");
                }

            }
        });
    }

}
public class CryptoTransaction
{
    public string uniqueSerial;
    public string noodleMemberId;
    public string memberId;
    public string hashKey;
    public int currencyCode;
    public int transactionType;
    public float amount;
}
public class encryptCryptoTransaction
{
    public string text;

    public encryptCryptoTransaction(string text)
    {
        this.text = text;
    }
}
