using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebGLSupport;

public class CheckTransactionManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private Button sumbit_Btn;
    [SerializeField]
    private TMP_InputField address_Field, hash_Field;
    [SerializeField]
    private GameObject checkingTip_Obj;
    private void Start()
    {
        sumbit_Btn.onClick.AddListener(() =>
        {
            Debug.Log(string.IsNullOrEmpty(address_Field.text));
            if (!string.IsNullOrEmpty(address_Field.text) && !string.IsNullOrEmpty(hash_Field.text))
            {
                checkingTip_Obj.SetActive(true);
            }
        });
    }

}
