using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SettingsView : MonoBehaviour
{
    //[SerializeField]
    //Button Close_Btn;
    //[SerializeField]
    //TextMeshProUGUI Title_Txt;
    [Header("選單")]
    [SerializeField]
    Button language_Btn, contactUs_Btn, terms_Btn, privacy_Btn, logOut_Btn;
    [SerializeField]
    TextMeshProUGUI language_Txt, contactUs_Txt, terms_Txt, privacy_Txt, logOut_Txt;

    [Header("語言")]
    //[SerializeField]
    //TMP_Dropdown Language_Dd;
    [SerializeField]
    GameObject languageArea;
    [SerializeField]
    Toggle en_Tog, zh_Tog;
    [SerializeField]
    TextMeshProUGUI LanguageTitle_Txt;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        //Title_Txt.text = LanguageManager.Instance.GetText("SETTINGS");
        language_Txt.text = LanguageTitle_Txt.text = LanguageManager.Instance.GetText("Language");
        contactUs_Txt.text  = LanguageManager.Instance.GetText("Contact us");
        terms_Txt.text  = LanguageManager.Instance.GetText("Terms");
        privacy_Txt.text  = LanguageManager.Instance.GetText("Privacy Policy");
        logOut_Txt.text  = LanguageManager.Instance.GetText("Log Out");
    }

    private void OnDestroy()
    {
        LanguageManager.Instance.RemoveLanguageFun(UpdateLanguage);
    }

    private void Awake()
    {
        LanguageManager.Instance.AddUpdateLanguageFunc(UpdateLanguage, gameObject);
        ListenerEvent();

        //Utils.SetOptionsToDropdown(Language_Dd,
        //                           LanguageManager.Instance.languageShowName.ToList());
    }

    private void OnEnable()
    {
        //Language_Dd.value = LanguageManager.Instance.GetCurrLanguageIndex();
        zh_Tog.isOn = true;
        en_Tog.isOn = false;
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        //關閉按鈕
        //Close_Btn.onClick.AddListener(() =>
        //{
        //    Destroy(gameObject);
        //});

        language_Btn.onClick.AddListener(() =>
        {
            languageArea.SetActive(true);
        });

        //更換語言
        #region old
        //Language_Dd.onValueChanged.AddListener((value) =>
        //{
        //    LanguageManager.Instance.ChangeLanguage(value);

        //});
        #endregion
        en_Tog.onValueChanged.AddListener((value) =>
        {
            en_Tog.isOn = value;
            LanguageManager.Instance.ChangeLanguage(0);
        });
        zh_Tog.onValueChanged.AddListener((value) =>
        {
            zh_Tog.isOn = value;
            LanguageManager.Instance.ChangeLanguage(1);
        });
    }
}
