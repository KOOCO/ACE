using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.Events;
using TMPro;

public class LanguageManager
{
    const string LanguagePlayerPrefsKey = "AsiaPoker_LanguageDataList";

    private static readonly object lockObject = new object();
    private static LanguageManager language = null;
    public static LanguageManager Instance
    {
        get
        {
            if (language == null)
            {
                lock (lockObject)
                {
                    if (language == null)
                    {
                        language = new LanguageManager();
                    }
                }
            }
            return language;
        }
    }
    private LanguageManager() 
    {
        fontAssetList = Resources.LoadAll<TMP_FontAsset>("TmpFonts");
        thisData = new ThisData();
    }

    /// <summary>
    /// //字型資源
    /// 0 = 思源黑體-Medium
    /// </summary>
    private TMP_FontAsset[] fontAssetList;          

    /// <summary>
    /// 紀錄翻譯文本
    /// </summary>
    Dictionary<string, string[]> languageDic = new Dictionary<string, string[]>();

    /// <summary>
    /// 紀錄更新語言方法
    /// </summary>
    Dictionary<UnityAction, GameObject> updateLanguageFuncDic = new();

    /// <summary>
    /// 翻譯攔名稱
    /// </summary>
    readonly string[] languageId =
    {
        "English",              //英文
        "zh_TW",                //繁體中文        
    };

    /// <summary>
    /// 更換語言顯示
    /// </summary>
    readonly public string[] languageShowName =
    {
        "English",              //英文
        "繁體中文"              //繁體中文
    };

    /// <summary>
    /// 翻譯資料
    /// </summary>
    private class TranslationData
    {
        public string ID { get; set; }
        public string English { get; set; }
        public string zh_TW { get; set; }

        public string GetId(string language)
        {
            switch (language)
            {
                //英文
                case "English":
                    return English;

                //繁體中文
                case "zh_TW":
                    return zh_TW;

                default:
                    return "";
            }
        }
    }

    private ThisData thisData;
    public class ThisData
    {
        public int CurrLanguageIndex;       //當前語言Index
    }

    /// <summary>
    /// 獲取當前語言Index
    /// </summary>
    /// <returns></returns>
    public int GetCurrLanguageIndex()
    {
        return thisData.CurrLanguageIndex;
    }

    /// <summary>
    /// 載入翻譯文本
    /// </summary>
    public void LoadLangageJson()
    {
        string jsonFile = Resources.Load<TextAsset>("Language/Language").text;
        List<TranslationData> translations = JsonMapper.ToObject<List<TranslationData>>(jsonFile);

        foreach (TranslationData data in translations)
        {
            string serchName = data.ID;
            string[] lang = new string[languageId.Length];

            for (int j = 0; j < languageId.Length; j++)
            {
                lang[j] = data.GetId(languageId[j]);
            }

            if (languageDic.ContainsKey(serchName))
            {
                //Debug.LogError($"Add Language Same Key:{serchName}");
                continue;
            }
            languageDic.Add(serchName, lang);
        }

        //讀取紀錄語言資料
        ChangeLanguage(PlayerPrefs.GetInt(LanguagePlayerPrefsKey));
    }

    /// <summary>
    /// 獲取文本
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetText(string id)
    {
        string str = id.Replace("\n", "\\n");

        if (languageDic.ContainsKey(str))
        {
            return languageDic[str][thisData.CurrLanguageIndex];
        }
        else
        {

            //Debug.LogError($"{id}:翻譯文本不存在");





            return id;
        }
    }

    /// <summary>
    /// 添加更新語言方法
    /// </summary>
    /// <param name="updateFunc"></param>
    /// <param name="obj"></param>
    public void AddUpdateLanguageFunc(UnityAction updateFunc, GameObject obj)
    {
        updateFunc();
        updateLanguageFuncDic.Add(updateFunc, obj);
    }

    /// <summary>
    /// 移除更新語言方法
    /// </summary>
    /// <param name="func"></param>
    public void RemoveLanguageFun(UnityAction func)
    {
        if (updateLanguageFuncDic.ContainsKey(func))
        {
            updateLanguageFuncDic.Remove(func);
        }
        else
        {
            //Debug.LogError("未找到移除更新語言方法!!!");
        }    
    }

    /// <summary>
    /// 更新翻譯
    /// </summary>
    private void UpdateLanguage()
    {



        Debug.Log($"Change Language:{languageId[thisData.CurrLanguageIndex]}");




        foreach (var func in updateLanguageFuncDic)
        {
            if (func.Value != null)
            {
                func.Key?.Invoke();
            }
            else
            {
                //Debug.LogError($"Update Language Error : {func.Value.name}");
            }
        }
    }

    /// <summary>
    /// 更換語言
    /// </summary>
    /// <param name="index"></param>
    public void ChangeLanguage(int index)
    {
        if (index >= languageId.Length || 
            index < 0)
        {
            //Debug.LogError($"Change Language Index:{index}:Error!!!");
            return;
        }

        thisData.CurrLanguageIndex = index;
        PlayerPrefs.SetInt(LanguagePlayerPrefsKey, thisData.CurrLanguageIndex);

        ChangeFont();
        UpdateLanguage();
    }

    /// <summary>
    /// 更換字體
    /// </summary>
    private void ChangeFont()
    {
        TMP_Text[] tmpTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var item in tmpTexts)
        {
            switch (thisData.CurrLanguageIndex)
            {
                //英文
                case 0:
                    item.font = fontAssetList[0];
                    break;

                //繁體中文
                case 1:
                    item.font = fontAssetList[0];
                    break;

                //預設
                default:
                    item.font = fontAssetList[0];
                    break;
            }
        }
    }
}
