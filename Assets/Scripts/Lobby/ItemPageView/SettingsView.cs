using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SettingsView : MonoBehaviour
{
    [SerializeField]
    Button Close_Btn;
    [SerializeField]
    TextMeshProUGUI Title_Txt;

    [Header("語言")]
    [SerializeField]
    TMP_Dropdown Language_Dd;
    [SerializeField]
    TextMeshProUGUI LanguageTitle_Txt;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        Title_Txt.text = LanguageManager.Instance.GetText("SETTINGS");
        LanguageTitle_Txt.text = LanguageManager.Instance.GetText("Language");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        Utils.SetOptionsToDropdown(Language_Dd,
                                   LanguageManager.Instance.languageShowName.ToList());
    }

    private void OnEnable()
    {
        Language_Dd.value = LanguageManager.Instance.GetCurrLanguageIndex();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //關閉按鈕
        Close_Btn.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        //更換語言
        Language_Dd.onValueChanged.AddListener((value) =>
        {
            LanguageManager.Instance.ChangeLanguage(value);
           
        });
    }
}
