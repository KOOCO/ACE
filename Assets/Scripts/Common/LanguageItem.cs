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

    private void Update()
    {

    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }
    private void UpdateLanguage()
    {
        if (ID != null && gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            if(!string.IsNullOrEmpty(ID))
                gameObject.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetText(ID);
            changeFont();
        }
        else
        {
            Debug.LogError("text or id is null");
        }
    }

    private void changeFont()
    {
        int index = LanguageManager.Instance.GetCurrLanguageIndex();
        switch (index)
        {
            case 0:
                gameObject.GetComponent<TextMeshProUGUI>().font = LanguageManager.Instance.getFont(4);
                break;
            case 1:
                gameObject.GetComponent<TextMeshProUGUI>().font = LanguageManager.Instance.getFont(4);
                break;
            case 2:
                gameObject.GetComponent<TextMeshProUGUI>().font = LanguageManager.Instance.getFont(2);
                break;
            case 3:
                gameObject.GetComponent<TextMeshProUGUI>().font = LanguageManager.Instance.getFont(1);
                break;
        }
    }
}
