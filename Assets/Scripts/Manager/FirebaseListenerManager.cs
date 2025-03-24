using System.Collections.Generic;
#if UNITY_ANDROID
using Firebase.Database;
#endif
using UnityEngine;

public class FirebaseListenerManager : MonoBehaviour
{
#if UNITY_ANDROID
    public static FirebaseListenerManager inst;
    private Dictionary<string, DatabaseReference> activeReferences = new Dictionary<string, DatabaseReference>();
    private Dictionary<DatabaseReference, System.EventHandler<ValueChangedEventArgs>> listeners = new Dictionary<DatabaseReference, System.EventHandler<ValueChangedEventArgs>>();

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
        activeReferences[nodePath] = reference;

        // 確保每個節點只有一個監聽器
        if (listeners.ContainsKey(reference))
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

            string responseText = string.IsNullOrEmpty(args.Snapshot.GetRawJsonValue()) || args.Snapshot.GetRawJsonValue() == "null" ? "" : args.Snapshot.GetRawJsonValue(); 
            obj.SendMessage(callbackFunPtr, responseText, SendMessageOptions.DontRequireReceiver);
        };

        // 添加監聽器
        reference.ValueChanged += valueChangedHandler;
        listeners[reference] = valueChangedHandler;

        Debug.Log($"訂閱節點: {nodePath}");
        Debug.Log($"監聽器數: {listeners.Count}");
    }

    /// <summary>
    /// 取消訂閱指定節點的 ValueChanged 事件
    /// </summary>
    public void Unsubscribe(string nodePath)
    {
        if (activeReferences.TryGetValue(nodePath, out var reference))
        {
            if (listeners.TryGetValue(reference, out var handler))
            {
                reference.ValueChanged -= handler;
                listeners.Remove(reference);
            }

            Debug.Log($"移除節點: {reference}");
            activeReferences.Remove(nodePath);
        }

        Debug.Log($"監聽器數: {listeners.Count}");
    }

    /// <summary>
    /// 移除所有監聽器
    /// </summary>
    private void OnDestroy()
    {
        foreach (var kvp in listeners)
        {
            DatabaseReference reference = kvp.Key;
            reference.ValueChanged -= kvp.Value;
        }

        listeners.Clear();
    }
#endif
}