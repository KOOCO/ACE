using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public static class StringUtils
{
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
    async public static void ChipsChangeEffect(Text txtObj, double targetNum, string addStartStr = "", string addEndStr = "")
    {
        float during = 0.5f;

        DateTime startTime = DateTime.Now;
        string str = txtObj.text.Replace("$", "");
        float initNum = txtObj.text == "" ? 0 : (float)JudgeUnit(str);

        float currNum = initNum;

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
        else if (str.EndsWith("B")) return double.Parse(str.Replace("K", "")) * 1000000;
        else if (str.EndsWith("T")) return double.Parse(str.Replace("K", "")) * 100000000;
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
            return $"${SetChipsComma(chips.ToString("F0"))}";
        }
        else if (chips / 10000 < 1000)
        {
            //萬
            return $"${((double)chips / 10000):f2}K";
        }
        else if (chips / 10000000 < 1000)
        {
            //億
            return $"${((double)chips / 10000000):f2}B";
        }
        else if (chips / 100000000 < 1000)
        {
            //兆
            return $"${((double)chips / 100000000):f2}T";
        }
        return $"${chips:f0}";
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
                sb.Insert(0, ",");
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
    public static string GetPhoneAddCode(Dropdown dropdown, string phoneNumber)
    {
        string countryCode = dropdown.options[dropdown.value].text;
        countryCode = countryCode.Replace("+", "");
        if (phoneNumber[0] == '0')
        {
            phoneNumber = phoneNumber.Substring(1);
        }
        
        return $"{countryCode}-{phoneNumber}";
    }

    /// <summary>
    /// 重製密碼輸入內容
    /// </summary>
    /// <param name="img"></param>
    /// <param name="inputField"></param>
    /// <returns></returns>
    public static bool InitPasswordContent(Image img, InputField inputField)
    {
        //眼睛圖案紐
        img.sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.PasswordEye).album[0];
        //輸入框
        inputField.contentType = InputField.ContentType.Password;
        inputField.text = "";

        return false;
    }

    /// <summary>
    /// 檢查手機格式正確性
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static bool CheckPhoneNumber(string number)
    {
        if (number.Length < 9)
        {
            return false;
        }

        return true;
    }
}
