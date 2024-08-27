using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;
using TMPro;

public static class StringUtils
{
    /// <summary>
    /// 產生隨機字串
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GenerateRandomString(int length)
    {
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        System.Random random = new ();
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }

    /// <summary>
    /// 文字前方圖片跟隨(提示類驚嘆號等功能)
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="img"></param>
    /// <param name="space">間距</param>
    public static void TextInFrontOfImageFollow(TextMeshProUGUI textComponent, Image img, float space = 2)
    {
        float txtWidth = textComponent.preferredWidth;

        textComponent.rectTransform.sizeDelta = new Vector2(txtWidth, 
                                                            textComponent.rectTransform.sizeDelta.y);

        Vector2 size = img.rectTransform.sizeDelta;
        img.transform.SetParent(textComponent.transform);
        img.rectTransform.anchorMax = new Vector2(0, 0.5f);
        img.rectTransform.anchorMin = new Vector2(0, 0.5f);
        img.rectTransform.pivot = new Vector2(0, 0.5f);
        img.rectTransform.sizeDelta = size;
        img.rectTransform.anchoredPosition = new Vector2(-size.x - space, 0);
    }

    /// <summary>
    /// 文字內容超過Text元件添加...
    /// </summary>
    /// <param name="content">顯示內容</param>
    /// <param name="textComponent">Text元件</param>
    public static void StrExceedSize(string content, TextMeshProUGUI textComponent)
    {
        if (string.IsNullOrEmpty(content))
        {
            textComponent.text = "";
            return;
        }

        string currStr = "";
        for (int i = 0; i < content.Length; i++)
        {
            currStr += content[i];
            textComponent.text = currStr;
            if (textComponent.preferredWidth > textComponent.rectTransform.rect.width)
            {
                //文字長度超過了Text元件的寬度
                string str = currStr.Substring(0, currStr.Length - 4) + "...";
                textComponent.text = str;
                break;
            }
        }
    }

    /// <summary>
    /// 複製文字
    /// </summary>
    /// <param name="str"></param>
    public static void CopyText(string str)
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        JSBridgeManager.Instance.CopyString(str);

#else

        TextEditor textEditor = new TextEditor()
        {
            text = str
        };
        textEditor.SelectAll();
        textEditor.Copy();

#endif

        Debug.Log($"Copy Text : {str}");    
    }

    /// <summary>
    /// 判斷是否為數字或英文字母
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsAlphaNumeric(string str)
    {
        Regex regex = new Regex("^[a-zA-Z0-9]+$");
        return regex.IsMatch(str);
    }

    /// <summary>
    /// 擷取數字
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int RetrieveNumbers(string str)
    {
        return int.Parse(Regex.Replace(str, @"[^\d\s]+", ""));
    }

    /// <summary>
    /// 字串加法
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static string StringAddition(string v1, string v2)
    {
        StringBuilder sb = new StringBuilder();
        Sum(v1.Length - 1, v2.Length - 1, false);
        return sb.ToString();

        //相加
        void Sum(int index1, int index2, bool isCarry)
        {
            if (index1 < 0 && index2 < 0 && !isCarry)
            {
                return;
            }

            int num1 = index1 >= 0 ? Convert.ToInt32(v1[index1].ToString()) : 0;
            int num2 = index2 >= 0 ? Convert.ToInt32(v2[index2].ToString()) : 0;

            int sum = num1 + num2 + (isCarry ? 1 : 0);

            sb.Insert(0, (sum % 10).ToString());

            Sum(index1 - 1, index2 - 1, sum / 10 >= 1);
        }
    }

    /// <summary>
    /// 字串乘法
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static string StringMultiplication(string v1, string v2)
    {
        if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
        {
            return "0";//若有一個為空字符串，則結果為0
        }

        int m = v1.Length;
        int n = v2.Length;
        int[] result = new int[m + n];

        //進行乘法運算
        for (int i = m - 1; i >= 0; i--)
        {
            for (int j = n - 1; j >= 0; j--)
            {
                int num1 = v1[i] - '0';
                int num2 = v2[j] - '0';
                int mul = num1 * num2;
                int sum = mul + result[i + j + 1];

                result[i + j] += sum / 10; //進位
                result[i + j + 1] = sum % 10;
            }
        }

        //將結果轉為字符串
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            int digit = result[i];
            sb.Append(digit);
        }

        //移除前導零
        int startIndex = 0;
        while (startIndex <= sb.Length - 1 && sb[startIndex] == '0')
        {
            startIndex++;
        }

        return sb.ToString().Substring(startIndex);
    }

    /// <summary>
    /// 籌碼變化效果
    /// </summary>
    /// <param name="txtObj"></param>
    /// <param name="targetNum"></param>
    /// <param name="addStartStr">起始添加文字</param>
    /// <param name="addEndStr">結束添加文字</param>
    async public static void ChipsChangeEffect(TextMeshProUGUI txtObj, double targetNum, string addStartStr = "", string addEndStr = "")
    {
        float during = 0.5f;
        
        //匹配所有的数字，保留小數點
        string pattern = @"\d+(\.\d+)?[KBT]?";
        //查找所有匹配项
        MatchCollection matches = Regex.Matches(txtObj.text, pattern);
        //将所有匹配项连接成一个字符串
        string result = string.Join("", matches);        
        string str = string.IsNullOrEmpty(result) ?
                     "0" :
                     result;
        float initNum = txtObj.text == "" ? 0 : (float)JudgeUnit(str);

        float currNum = initNum;
        DateTime startTime = DateTime.Now;
        while (currNum != (float)targetNum)
        {
            float progress = (float)(DateTime.Now - startTime).TotalSeconds / during;
            currNum = Mathf.Lerp(initNum, (float)targetNum, progress);

            if (txtObj != null)
            {
                txtObj.text = $"{addStartStr}{SetChipsUnit(currNum)}{addEndStr}";
            }

            await Task.Yield();
        }
    }

    /// <summary>
    /// 判斷單位
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static double JudgeUnit(string str)
    {
        if (str.EndsWith("K")) return double.Parse(str.Replace("K", "")) * 10000;
        else if (str.EndsWith("B")) return double.Parse(str.Replace("B", "")) * 1000000;
        else if (str.EndsWith("T")) return double.Parse(str.Replace("T", "")) * 100000000;
        else
        {
            //小於1萬
            return double.Parse(str);
        }
    }

    /// <summary>
    /// 設定籌碼單位
    /// </summary>
    /// <param name="chips"></param>
    /// <returns></returns>
    public static string SetChipsUnit(double chips)
    { 
        if (chips / 10000 < 1)
        {
            return $"{SetChipsComma(chips.ToString("F0"))}";
        }
        else if (chips / 10000 < 1000)
        {
            //萬
            return $"{((double)chips / 1000):f2}K";
        }
        else if (chips / 10000000 < 1000)
        {
            //億
            return $"{((double)chips / 10000000):f2}M";
        }
        else if (chips / 100000000 < 1000)
        {
            //兆
            return $"{((double)chips / 100000000):f2}B";
        }
        
        return $"{chips:f0}";
    }

    /// <summary>
    /// 設定籌碼逗點
    /// </summary>
    /// <param name="chips"></param>
    /// <returns></returns>
    public static string SetChipsComma(string chips)
    {
        StringBuilder sb = new StringBuilder();

        int count = 0;
        for (int i = chips.Length - 1; i >= 0; i--)
        {
            sb.Insert(0, chips[i]);

            count++;
            if (count == 3 && i != 0)
            {
               // sb.Insert(0, ",");
                count = 0;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 獲取手機號加國碼
    /// </summary>
    /// <param name="dropdown"></param>
    /// <param name="phoneNumber"></param>
    /// <returns></returns>
    public static string GetPhoneAddCode(TMP_Dropdown dropdown, string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
           
           
            Debug.LogError("Phone Number Empty!!!");
            return "";
        }

        string countryCode = dropdown.options[dropdown.value].text;
        //countryCode = countryCode.Replace("+", "");
        if (phoneNumber[0] == '0')
        {
            phoneNumber = phoneNumber.Substring(1);
        }
        
        return $"{countryCode}{phoneNumber}";
    }

    /// <summary>
    /// 重製密碼輸入內容
    /// </summary>
    /// <param name="img"></param>
    /// <param name="inputField"></param>
    /// <returns></returns>
    public static bool InitPasswordContent(Image img, TMP_InputField inputField)
    {
        //眼睛圖案紐
        img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.PasswordEyeAlbum).album[0];
        //輸入框
        inputField.contentType = TMP_InputField.ContentType.Password;
        inputField.text = "";

        return false;
    }

    /// <summary>
    /// 檢查手機格式正確性
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public static bool CheckPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || 
            phone.Length < 7 ||
            phone.Length > 11)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 判斷是否包含一個大寫和小寫
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool CheckUppercaseAndLowercase(string number)
    {
        Regex regex = new Regex("^(?=.*[A-Z])(?=.*[a-z])");
        return regex.IsMatch(number);
    }

    /// <summary>
    /// 判斷是否包含一個特殊字元
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool CheckSpecialCharacter(string number)
    {
        Regex regex = new Regex("^(?=.*[@$!%*?&#])");
        return regex.IsMatch(number);
    }
}
