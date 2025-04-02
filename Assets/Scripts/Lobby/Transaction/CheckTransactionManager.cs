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
    private TextMeshProUGUI tip_Txt;
    private void Start()
    {
        sumbit_Btn.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(address_Field.text) && !string.IsNullOrEmpty(hash_Field.text) && !string.IsNullOrEmpty(amount_Field.text))
            {
                string hashKey = hash_Field.text;
                float amount;
                if (hashKey.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(hashKey, "^[a-fA-F0-9]+$"))
                {
                    if (float.TryParse(amount_Field.text, out amount))
                    {
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
                            tip_Txt.text = LanguageManager.Instance.GetText("CheckTransaction");
                        }, (error) => tip_Txt.text = LanguageManager.Instance.GetText("ValidationError"));
                    }
                    else
                    {
                        tip_Txt.text = LanguageManager.Instance.GetText("AmountError");
                    }
                }
                else
                {
                    tip_Txt.text = LanguageManager.Instance.GetText("HashKeyError");
                }
            }
            else
            {
                tip_Txt.text = LanguageManager.Instance.GetText("CannotEmpty");
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
