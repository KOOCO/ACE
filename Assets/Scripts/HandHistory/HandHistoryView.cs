using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandHistoryView : MonoBehaviour
{
    [SerializeField]
    GameObject HistorySampleObj;
    [SerializeField]
    Transform HistroyParent;

    private void OnEnable()
    {
        UpdateHitoryDate();
    }

    /// <summary>
    /// 更新紀錄資料
    /// </summary>
    public void UpdateHitoryDate()
    {
        for (int i = 0; i < HistroyParent.childCount; i++)
        {
            Destroy(HistroyParent.GetChild(i).gameObject);
        }

        CreateHistory();
    }

    /// <summary>
    /// 創建結果紀錄
    /// </summary>
    private void CreateHistory()
    {
        HistorySampleObj.gameObject.SetActive(false);
        List<ResultHistoryData> resultDatas = HandHistoryManager.Instance.GetResultDataList();
        for (int i = resultDatas.Count - 1; i >= 0; i--)
        {
            if (resultDatas == null || resultDatas[i] == null)
            {
                HandHistoryManager.Instance.OnDeleteHistoryData();
                return;
            }

            HistorySample historySample = Instantiate(HistorySampleObj, HistroyParent).GetComponent<HistorySample>();
            historySample.gameObject.SetActive(true);
            historySample.SetData(resultDatas[i], i);
        }
    }
}
