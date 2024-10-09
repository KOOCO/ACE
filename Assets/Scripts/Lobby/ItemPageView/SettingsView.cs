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

    [Header("隱私政策物件")]
    [SerializeField]
    GameObject Privacy_Obj, Privacy_text, Term_text, Privacy_obj_Scroll, Term_obj_Scroll,
      Privacy_text_CH, Term_text_CH, Privacy_text_EN, Term_text_EN;
    [SerializeField]
    TextMeshProUGUI Privacy_Title, Term_Title,
                    TermsConfirm_Btn_Txt, PrivacyConfirm_Btn_Txt;

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

        #region 隱私政策物件

        TermsConfirm_Btn_Txt.text = LanguageManager.Instance.GetText("i GOT IT");
        PrivacyConfirm_Btn_Txt.text = LanguageManager.Instance.GetText("i GOT IT");
        //PrivacyConfirmBtn_Txt.text = LanguageManager.Instance.GetText("Confirm");
        Privacy_Title.text = LanguageManager.Instance.GetText("Asia Poker privacy policy");
        Term_Title.text = LanguageManager.Instance.GetText("Asia Poker Terms of Service");
        if (LanguageManager.Instance.GetCurrLanguageIndex() == 0)
        {
            Term_obj_Scroll.GetComponent<ScrollRect>().content = Term_text_EN.GetComponent<RectTransform>();


            Privacy_obj_Scroll.GetComponent<ScrollRect>().content = Privacy_text_EN.GetComponent<RectTransform>();
            Privacy_text_EN.SetActive(true);
            Privacy_text_CH.SetActive(false);
            Term_text_EN.SetActive(true);
            Term_text_CH.SetActive(false);

        }
        else if (LanguageManager.Instance.GetCurrLanguageIndex() == 1)
        {

            Term_obj_Scroll.GetComponent<ScrollRect>().content = Term_text_CH.GetComponent<RectTransform>();

            Privacy_obj_Scroll.GetComponent<ScrollRect>().content = Privacy_text_CH.GetComponent<RectTransform>();
            Term_text_CH.SetActive(true);
            Term_text_EN.SetActive(false);
            Privacy_text_CH.SetActive(true);
            Privacy_text_EN.SetActive(false);

        }
        #endregion
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
        int lan_Index = LanguageManager.Instance.GetCurrLanguageIndex();

        switch (lan_Index)
        {
            case 0:
                en_Tog.isOn = true;
                zh_Tog.isOn = false;
                break;
            case 1:
                en_Tog.isOn = false;
                zh_Tog.isOn = true;
                break;
        }
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

        contactUs_Btn.onClick.AddListener(() =>
        {
            string authUrl = $"https://line.me/ti/p/@309jwned";
            JSBridgeManager.Instance.onLineService(authUrl);
        });

        terms_Btn.onClick.AddListener(() =>
        {
            Term_text.SetActive(true);
            Privacy_text.SetActive(false);
        });
        privacy_Btn.onClick.AddListener(() =>
        {
            Term_text.SetActive(false);
            Privacy_text.SetActive(true);
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
