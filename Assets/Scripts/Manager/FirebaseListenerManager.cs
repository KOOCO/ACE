#if UNITY_ANDROID
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;

public class FirebaseListenerManager : MonoBehaviour
{
    public static FirebaseListenerManager inst;
    private Dictionary<string, System.EventHandler<ValueChangedEventArgs>> listeners = new Dictionary<string, System.EventHandler<ValueChangedEventArgs>>();

    private void Awake()
    {
        inst = this;
    }

    /// <summary>
    /// 訂閱指定節點的 ValueChanged 事件
    /// </summary>
    public void Subscribe(string nodePath, string objNamePtr, string callbackFunPtr)
    {
        if (string.IsNullOrEmpty(objNamePtr) || string.IsNullOrEmpty(callbackFunPtr))
        {
            Debug.LogError("Invalid parameters for subscription.");
            return;
        }

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(nodePath);

        // 確保每個節點只有一個監聽器
        if (listeners.ContainsKey(nodePath))
        {
            Debug.LogWarning($"Listener for node {nodePath} already exists.");
            return;
        }

        // 創建監聽事件處理器
        System.EventHandler<ValueChangedEventArgs> valueChangedHandler = (object sender, ValueChangedEventArgs args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError($"Database Error on node {nodePath}: {args.DatabaseError.Message}");
                return;
            }

            GameObject obj = GameObject.Find(objNamePtr);
            if (obj == null)
            {
                Debug.LogError($"GameObject '{objNamePtr}' not found.");
                return;
            }

            string responseText = args.Snapshot.GetRawJsonValue();
            obj.SendMessage(callbackFunPtr, responseText, SendMessageOptions.DontRequireReceiver);
        };

        // 添加監聽器
        reference.ValueChanged += valueChangedHandler;
        listeners[nodePath] = valueChangedHandler;

        Debug.Log($"Subscribed to node: {nodePath}");
    }

    /// <summary>
    /// 取消訂閱指定節點的 ValueChanged 事件
    /// </summary>
    public void Unsubscribe(string nodePath)
    {
        if (!listeners.ContainsKey(nodePath))
        {
            Debug.LogWarning($"No listener found for node {nodePath}.");
            return;
        }

        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(nodePath);
        reference.ValueChanged -= listeners[nodePath];
        listeners.Remove(nodePath);

        Debug.Log($"Unsubscribed from node: {nodePath}");
    }

    /// <summary>
    /// 移除所有監聽器
    /// </summary>
    private void OnDestroy()
    {
        foreach (var kvp in listeners)
        {
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference(kvp.Key);
            reference.ValueChanged -= kvp.Value;
        }

        listeners.Clear();
    }
}
#endif