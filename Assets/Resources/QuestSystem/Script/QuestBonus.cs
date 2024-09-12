using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBonus : MonoBehaviour
{
    [Header("進度條")]
    [SerializeField]
    Slider slider;

    [Header("Rocket父物件")]
    [SerializeField]
    GameObject RocketParent;

    [Header("Bonus字串")]
    [SerializeField]
    List<GameObject> BonusRocket;

    int BonusIndex;                             //  計算紅利獎勵數值

    private void Awake()
    {
        DataManager.BonusMax = 100;             //  設置初始紅利最大值

        BonusIndex = DataManager.BonusMax / RocketParent.transform.childCount;

        CreateBonusRocket();
        SetBonus();
    }

    /// <summary>
    /// 生成紅利物件
    /// </summary>
    public void CreateBonusRocket()
    {
        slider.maxValue = DataManager.BonusMax;
        slider.minValue = BonusIndex;
        slider.value = DataManager.CurrentBonusAmount;

        for (int i = 0; i < RocketParent.transform.childCount; i++)
        {
            BonusRocket.Add(RocketParent.transform.GetChild(i).gameObject);

            //  設置紅利數值
            Text bonusText = BonusRocket[i].transform.GetChild(0).GetComponent<Text>();
            bonusText.text = (BonusIndex * (i + 1)).ToString();
        }

    }

    /// <summary>
    /// 設置紅利進度
    /// </summary>
    public void SetBonus()
    {
        slider.value = DataManager.CurrentBonusAmount;
        BonusIndex = DataManager.BonusMax / RocketParent.transform.childCount;

        for (int i = 0; i < RocketParent.transform.childCount; i++)
        {
            if (DataManager.CurrentBonusAmount / BonusIndex >= i + 1)
            {
                BonusRocket[i].GetComponent<Image>().sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.BonusRocketAlbum).album[1];
            }
            else
                BonusRocket[i].GetComponent<Image>().sprite = AssetsManager.Instance.GetAlbumAsset(AlbumEnum.BonusRocketAlbum).album[0];
        }
    }
}
