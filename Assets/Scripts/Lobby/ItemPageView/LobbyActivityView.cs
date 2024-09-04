using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyActivityView : MonoBehaviour
{
    [SerializeField]
    GameObject QuestView;
    [SerializeField]
    Button Daily_Btn, Weekly_Btn;

    private void Awake()
    {
        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        Daily_Btn.onClick.AddListener(() =>
        {
            QuestView questView = Instantiate(QuestView, transform).GetComponent<QuestView>();
            questView.ShowQuest(QuestEnum.Daily);
        });

        Weekly_Btn.onClick.AddListener(() =>
        {
            QuestView questView = Instantiate(QuestView, transform).GetComponent<QuestView>();
            questView.ShowQuest(QuestEnum.Weekly);
        });
    }
}
