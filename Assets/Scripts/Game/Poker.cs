using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Poker : MonoBehaviour
{
    [SerializeField]
    RectTransform thisRt;
    [SerializeField]
    Image Number_Img, Back_Img;

    int pokerNumber;    //撲克數字(-1=背面)

    /// <summary>
    /// 撲克數字(-1=背面)
    /// </summary>
    public int PokerNum
    {
        get
        {
            return pokerNumber;
        }
        set
        {
            pokerNumber = value;
            Back_Img.gameObject.SetActive(value <= -1);
            if (value >= 0)
            {
                Sprite[] pokersNum = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.PokerNumAlbum).album;
                Number_Img.sprite = pokersNum[value];
            }          
        }
    }

    /// <summary>
    /// 撲克效果激活
    /// </summary>
    public bool PokerEffectEnable
    {
        set
        {
            thisRt.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 設定撲克Alpha值
    /// </summary>
    public float SetColor
    {
        set
        {
            Number_Img.color = new Color(value, value, value, 1);
        }
    }

    /// <summary>
    /// 播放贏家效果
    /// </summary>
    public void StartWinEffect()
    {
        thisRt.localScale = new Vector3(1.1f, 1.1f, 1);
    }

    /// <summary>
    /// 水平翻牌效果
    /// </summary>
    /// <param name="frontNum">正面數字</param>
    /// <returns></returns>
    public IEnumerator IHorizontalFlopEffect(int frontNum)
    {
        Number_Img.rectTransform.rotation = Quaternion.Euler(Number_Img.rectTransform.eulerAngles.x, 180, Number_Img.rectTransform.eulerAngles.z);
        Back_Img.gameObject.SetActive(true);

        float turnTime = 0.5f;
        DateTime startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < turnTime)
        {
            float progess = (float)(DateTime.Now - startTime).TotalSeconds / turnTime;
            float rotY = Mathf.Lerp(180, 0, progess);
            Number_Img.rectTransform.rotation = Quaternion.Euler(Number_Img.rectTransform.eulerAngles.x, rotY, Number_Img.rectTransform.eulerAngles.z);

            if (rotY <= 90)
            {
                PokerNum = frontNum;
            }

            yield return null;
        }

        Number_Img.rectTransform.rotation = Quaternion.Euler(Number_Img.rectTransform.eulerAngles.x, 0, Number_Img.rectTransform.eulerAngles.z);
    }
}
