/*
 * JacobKay --20220903
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 使用示例
/// </summary>
public class ZCalendarDemo : MonoBehaviour
{
    public LobbyReportView lobbyReport;
    public ZCalendar zCalendar;
    // Start is called before the first frame update
    void Start()
    {
        zCalendar.UpdateDateEvent += ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent += ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent += ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent += ZCalendar_CompleteEvent;
        //zCalendar.Init();
        //zCalendar.Init(System.DateTime.Now);
        //zCalendar.Init("2022-02-02");
        //zCalendar.Show();
        //zCalendar.Hide();
    }
    /// <summary>
    /// 加載結束
    /// </summary>
    private void ZCalendar_CompleteEvent()
    {
        Debug.Log("ZCalendar加载结束");
        if (null != zCalendar.CrtTime)
        {
            Debug.Log($"當前時間{zCalendar.CrtTime.Day}");
        }
    }

    /// <summary>
    /// 區間時間
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void ZCalendar_RangeTimeEvent(ZCalendarDayItem arg1, ZCalendarDayItem arg2)
    {
        Debug.Log($"選擇的時間區間：{arg1.Month}/{arg1.Day}到{arg2.Month}/{arg2.Day}");

        //print(arg2.dateTime - arg1.dateTime);
        if ((arg2.dateTime - arg1.dateTime).TotalDays <= 90)
            lobbyReport.confirmDateRange($"{arg1.Year}/{arg1.Month}/{arg1.Day}", $"{arg2.Year}/{arg2.Month}/{arg2.Day}");
        else
            ViewManager.Instance.OpenTipMsgView(lobbyReport.transform, messageStatus.Sending, LanguageManager.Instance.GetText("Exceeded maximum selection date range"));
    }

    /// <summary>
    /// 獲取選擇的日期
    /// </summary>
    /// <param name="obj"></param>
    private void ZCalendar_ChoiceDayEvent(ZCalendarDayItem obj)
    {
        Debug.Log($"選擇的日期：{obj.Day}");

        lobbyReport.confirmDateRange($"{obj.Year}/{obj.Month}/{obj.Day}", "");
    }

    /// <summary>
    /// 切換月份時，可以拿到每一天的item對象
    /// </summary>
    /// <param name="obj"></param>
    private void ZCalendar_UpdateDateEvent(ZCalendarDayItem obj)
    {
        Debug.Log($"加載日期：{obj.Day}");
    }
    private void OnDestroy()
    {
        zCalendar.UpdateDateEvent -= ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent -= ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent -= ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent -= ZCalendar_CompleteEvent;
    }
}
