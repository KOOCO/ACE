using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Runtime.InteropServices;
using System;

public class SettingsView : MonoBehaviour
{
    [Header("選單")]
    [SerializeField]
    Button language_Btn, contactUs_Btn, terms_Btn, privacy_Btn, logOut_Btn;

    [Header("語言")]
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
    ScrollRect Privacy_scroll, Term_scroll;

    /// <summary>
    /// 更新文本翻譯
    /// </summary>
    private void UpdateLanguage()
    {
        #region 隱私政策物件
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
    }

    private void CallJSWindowClose(string unused)
    {
        JSBridgeManager.Instance.WindowClose();
    }

    public void OnClickLogOutBtn()
    {
        AppApi.LogoutRequest(CallJSWindowClose);
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

        getSelect(lan_Index);
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        language_Btn.onClick.AddListener(() =>
        {
            languageArea.SetActive(true);
            getSelect(LanguageManager.Instance.GetCurrLanguageIndex());
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
            Term_scroll.verticalNormalizedPosition = 1;
        });
        privacy_Btn.onClick.AddListener(() =>
        {
            Term_text.SetActive(false);
            Privacy_text.SetActive(true);
            Privacy_scroll.verticalNormalizedPosition = 1;
        });

        logOut_Btn.onClick.AddListener(OnClickLogOutBtn);

        en_Tog.onValueChanged.AddListener((value) =>
        {
            en_Tog.isOn = value;
            LanguageManager.Instance.ChangeLanguage(0);
            getSelect(LanguageManager.Instance.GetCurrLanguageIndex());
        });
        zh_Tog.onValueChanged.AddListener((value) =>
        {
            zh_Tog.isOn = value;
            LanguageManager.Instance.ChangeLanguage(1);
            getSelect(LanguageManager.Instance.GetCurrLanguageIndex());
        });
    }

    //偵測當前選中語言
    void getSelect(int index)
    {
        switch (index)
        {
            case 0:
                en_Tog.OnSelect(null);
                zh_Tog.OnDeselect(null);
                break;
            case 1:
                en_Tog.OnDeselect(null);
                zh_Tog.OnSelect(null);
                break;
        }
    }
}
