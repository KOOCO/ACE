using TMPro;
using UnityEngine;

public class LanguageItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //public TextMeshProUGUI Text;
    public string ID;
    private void Start()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
    }
    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }
    private void UpdateLanguage()
    {
        if (ID != null && gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetText(ID);
        }
        else
        {
            Debug.LogError("text or id is null");
        }
    }
}
