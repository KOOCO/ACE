using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyActivityView : MonoBehaviour
{

    [SerializeField]
    RectTransform QuestRect;
    [SerializeField]
    GameObject QuestView;
    [SerializeField]
    Button ActiveButton;


    private void Awake()
    {
        ListenerEvent();
    }

    /// <summary>
    /// 事件聆聽
    /// </summary>
    private void ListenerEvent()
    {
        ActiveButton.onClick.AddListener(() =>
        {
            RectTransform questRect = Instantiate(QuestView,QuestRect).GetComponent<RectTransform>();
        });


    }
}
