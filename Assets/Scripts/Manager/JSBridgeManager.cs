using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Proyecto26;
using System.Threading.Tasks;
#if UNITY_ANDROID
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
#endif
using System;

public class JSBridgeManager : UnitySingleton<JSBridgeManager>
{
    public override void Awake()
    {
        base.Awake();
#if UNITY_ANDROID
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
#endif
    }

#if UNITY_ANDROID
    DatabaseReference GetCurrReference(string releaseType)
    {
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        //print(reference.Child(releaseType).Key);
        return reference.Child(releaseType);
    }
#endif

#region reCAPTCHA(暫不使用)

    //#if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_SetupRecaptchaVerifier();
    //#endif
    //    private static void SetupRecaptchaVerifierAndroid()
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //            javaClass.CallStatic("setupRecaptchaVerifier");
    //        }
    //    }

    //    /// <summary>
    //    /// 設置Recaptcha驗證監聽
    //    /// </summary>
    //    /// <returns></returns>
    //    public void SetupRecaptchaVerifier()
    //    {
    //#if UNITY_WEBGL
    //        JS_SetupRecaptchaVerifier();
    //#elif UNITY_ANDROID
    //        SetupRecaptchaVerifierAndroid();
    //#endif
    //    }

    //#if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_OpenRecaptchaTool();
    //#endif
    //    private static void OpenRecaptchaToolAndroid()
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //            javaClass.CallStatic("openRecaptchaTool");
    //        }
    //    }
    //    /// <summary>
    //    /// 開啟Recaptcha小工具
    //    /// </summary>
    //    /// <returns></returns>
    //    public void OpenRecaptchaTool()
    //    {
    //#if UNITY_WEBGL
    //        JS_OpenRecaptchaTool();
    //#elif UNITY_ANDROID
    //        OpenRecaptchaToolAndroid();
    //#endif
    //    }

    //#if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_CloseRecaptchaTool();
    //#endif
    //    private static void CloseRecaptchaToolAndroid()
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //            javaClass.CallStatic("closeRecaptchaTool");
    //        }
    //    }
    //    /// <summary>
    //    /// 關閉Recaptcha小工具
    //    /// </summary>
    //    /// <returns></returns>
    //    public void CloseRecaptchaTool()
    //    {
    //#if UNITY_WEBGL
    //        JS_CloseRecaptchaTool();
    //#elif UNITY_ANDROID
    //        CloseRecaptchaToolAndroid();
    //#endif
    //    }

    //#if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_TriggerRecaptcha(string phoneNumber);
    //#endif
    //    private static void TriggerRecaptchaAndroid(string phoneNumber)
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //            javaClass.CallStatic("triggerRecaptcha", phoneNumber);
    //        }
    //    }
    //    /// <summary>
    //    /// 觸發Recaptcha驗證
    //    /// </summary>
    //    /// <returns></returns>
    //    public void TriggerRecaptcha(string phoneNumber)
    //    {
    //#if UNITY_WEBGL
    //        JS_TriggerRecaptcha(phoneNumber);
    //#elif UNITY_ANDROID
    //        TriggerRecaptchaAndroid(phoneNumber);
    //#endif
    //    }

#endregion

#region Firebase

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_FirebaseVerifyCode(string code, string typePtr);
#endif

    /// <summary>
    /// 驗證OTP
    /// </summary>
    /// <param name="code">OTP Code</param>
    /// <param name="typePtr">回傳物件名</param>
    [Obsolete]
    public void FirebaseVerifyCode(string code, string typePtr)
    {
#if UNITY_WEBGL
        JS_FirebaseVerifyCode(code, typePtr);
#elif UNITY_ANDROID
        Application.ExternalCall("JS_FirebaseVerifyCode", code, typePtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_StartListenerConnectState(string pathPtr);
#endif
    /// <summary>
    /// 監測連線狀態
    /// </summary>
    /// <param name="pathPtr">監測路徑</param>
    [Obsolete]
    public void StartListenerConnectState(string pathPtr)
    {
#if UNITY_WEBGL
        JS_StartListenerConnectState(pathPtr);
#elif UNITY_ANDROID
        Application.ExternalCall("JS_StartListenerConnectState", pathPtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_RemoveListenerConnectState(string IdPtr);
#endif
    /// <summary>
    /// 移除監測連線狀態
    /// </summary>
    /// <param name="pathPtr">監測路徑</param>
    /// <param name="id">監測ID</param>
    [Obsolete]
    public void RemoveListenerConnectState(string IdPtr)
    {
#if UNITY_EDITOR
        return;
#elif UNITY_WEBGL
        JS_RemoveListenerConnectState(IdPtr);
#elif UNITY_ANDROID
        Application.ExternalCall("JS_RemoveListenerConnectState", IdPtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_StartListeningForDataChanges(string pathPtr, string objNamePtr, string callbackFunPtr);
#endif
    /// <summary>
    /// 開始監聽資料
    /// </summary>
    /// <param name="pathPtr">資料路徑</param>
    /// <param name="objNamePtr">回傳物件名</param>
    /// <param name="callbackFunPtr">回傳方法名</param>
    public void StartListeningForDataChanges(string pathPtr, string objNamePtr, string callbackFunPtr)
    {
#if UNITY_WEBGL
        JS_StartListeningForDataChanges(pathPtr,
                                        objNamePtr,
                                        callbackFunPtr);
#elif UNITY_ANDROID
        FirebaseListenerManager.inst.Subscribe(pathPtr, objNamePtr, callbackFunPtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_StopListeningForDataChanges(string pathPtr);
#endif
    /// <summary>
    /// 停止監聽資料
    /// </summary>
    /// <param name="pathPtr">資料路徑</param>
    public void StopListeningForDataChanges(string pathPtr)
    {
#if UNITY_EDITOR
        return;
#elif UNITY_WEBGL
        JS_StopListeningForDataChanges(pathPtr);
#elif UNITY_ANDROID
        FirebaseListenerManager.inst.Unsubscribe(pathPtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_WriteDataFromFirebase(string refPathPtr, string jsonDataPtr, string objNamePtr = null, string callbackFunPtr = null);
#endif
    /// <summary>
    /// 寫入資料
    /// </summary>
    /// <param name="refPathPtr">資料路徑</param>
    /// <param name="data">資料</param>
    public void WriteDataFromFirebase(string refPathPtr, Dictionary<string, object> data, string objNamePtr = null, string callbackFunPtr = null)
    {
        string jsonData = JsonConvert.SerializeObject(data);

#if UNITY_EDITOR

        RestClient.Post($"{DataManager.DatabaseUrl}{refPathPtr}.json", jsonData).Then(response =>
        {
            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, response.Text);
            }
        }).Catch(error =>
        {
            Debug.LogError("Write Data Error: " + error);
        });

#elif UNITY_WEBGL
        JS_WriteDataFromFirebase(refPathPtr,
                                 jsonData,
                                 objNamePtr,
                                 callbackFunPtr);

#elif UNITY_ANDROID
        GetCurrReference(refPathPtr).SetRawJsonValueAsync(jsonData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print($"Saved quit data - {jsonData}");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, jsonData);
                }
            }
        });
#endif
    }

    public void WriteDataToFirebase(string refPathPtr, string data, string objNamePtr = null, string callbackFunPtr = null, bool isHistory = true)
    {
        //string jsonData = JsonConvert.SerializeObject(data);

#if UNITY_EDITOR

        RestClient.Post($"{DataManager.DatabaseUrl}{refPathPtr}.json", data).Then(response =>
        {
            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, response.Text);
            }
        }).Catch(error =>
        {
            Debug.LogError("Write Data Error: " + error);
        });

#elif UNITY_WEBGL
        JS_WriteDataFromFirebase(refPathPtr,
                                 data,
                                 objNamePtr,
                                 callbackFunPtr);
#elif UNITY_ANDROID
        GetCurrReference(refPathPtr).SetRawJsonValueAsync(data).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print($"Saved quit data - {data}");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, data);
                }
            }
        });
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_UpdateDataFromFirebase(string refPathPtr, string jsonDataPtr, string objNamePtr = null, string callbackFunPtr = null);
#endif
    /// <summary>
    /// 修改與擴充資料
    /// </summary>
    /// <param name="refPathPtr">資料路徑</param>
    /// <param name="data">資料</param>
    public void UpdateDataFromFirebase(string refPathPtr, Dictionary<string, object> data, string objNamePtr = null, string callbackFunPtr = null)
    {
        string jsonData = JsonConvert.SerializeObject(data);

#if UNITY_EDITOR

        RestClient.Patch($"{DataManager.DatabaseUrl}{refPathPtr}.json", jsonData).Then(response =>
        {
            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, response.Text);
            }
        }).Catch(error =>
        {
            Debug.LogError(jsonData);
            Debug.LogError($"{refPathPtr}/{objNamePtr}/{callbackFunPtr}");
            Debug.LogError("Update Data Error: " + error);
        });

#elif UNITY_WEBGL
        JS_UpdateDataFromFirebase(refPathPtr,
                                  jsonData,
                                  objNamePtr,
                                  callbackFunPtr);
#elif UNITY_ANDROID
        GetCurrReference(refPathPtr).SetRawJsonValueAsync(jsonData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print($"Saved quit data - {jsonData}");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, jsonData);
                }
            }
        });
#endif
    }
    public void UpdateDataToFirebase(string refPathPtr, string data, string objNamePtr = null, string callbackFunPtr = null)
    {
        //string jsonData = JsonConvert.SerializeObject(data);

#if UNITY_EDITOR

        RestClient.Patch($"{DataManager.DatabaseUrl}{refPathPtr}.json", data).Then(response =>
        {
            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, response.Text);
            }
        }).Catch(error =>
        {
            Debug.LogError(data);
            Debug.LogError($"{refPathPtr}/{objNamePtr}/{callbackFunPtr}");
            Debug.LogError("Update Data Error: " + error);
        });

#elif UNITY_WEBGL
        JS_UpdateDataFromFirebase(refPathPtr,
                                  data,
                                  objNamePtr,
                                  callbackFunPtr);
#elif UNITY_ANDROID
        GetCurrReference(refPathPtr).SetRawJsonValueAsync(data).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print($"Saved quit data - {data}");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, data);
                }
            }
        });
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_ReadDataFromFirebase(string refPathPtr, string objNamePtr, string callbackFunPtr);
#endif
    /// <summary>
    /// 讀取資料
    /// </summary>
    /// <param name="refPathPtr">資料路徑</param>
    /// <param name="objNamePtr">回傳物件名</param>
    /// <param name="callbackFunPtr">回傳方法名</param>
    public void ReadDataFromFirebase(string refPathPtr, string objNamePtr, string callbackFunPtr)
    {

#if UNITY_EDITOR

        RestClient.Get($"{DataManager.DatabaseUrl}{refPathPtr}.json").Then(response =>
        {
            GameObject obj = GameObject.Find(objNamePtr);
            obj.SendMessage(callbackFunPtr, response.Text);

        }).Catch(error =>
        {
            Debug.LogError("Read Data Error: " + error);
        });

#elif UNITY_WEBGL
        JS_ReadDataFromFirebase(refPathPtr,
                                objNamePtr,
                                callbackFunPtr);
#elif UNITY_ANDROID
        GetDataAsync(refPathPtr, objNamePtr, callbackFunPtr);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_RemoveDataFromFirebase(string refPathPtr, string objNamePtr, string callbackFunPtr);
#endif
    /// <summary>
    /// 移除資料
    /// </summary>
    /// <param name="refPathPtr">資料路徑</param>
    /// <param name="objNamePtr">回傳物件名</param>
    /// <param name="callbackFunPtr">回傳方法名</param>
    public void RemoveDataFromFirebase(string refPathPtr, string objNamePtr = "", string callbackFunPtr = "")
    {
        string objName = string.IsNullOrEmpty(objNamePtr) ?
                         "FirebaseManager" :
                         objNamePtr;

        string callbackFun = string.IsNullOrEmpty(callbackFunPtr) ?
                            "OnRemoveDataCallback" :
                            callbackFunPtr;

        //Debug.Log(nameof(JSBridgeManager) + " :: " + callbackFun + " : " + objName);

#if UNITY_EDITOR

        RestClient.Delete($"{DataManager.DatabaseUrl}{refPathPtr}.json").Catch(error =>
        {
            Debug.LogError("Remove Data Error: " + error);
        }); ;

#elif UNITY_WEBGL
        JS_RemoveDataFromFirebase(refPathPtr,
                                  objName,
                                  callbackFun);
#elif UNITY_ANDROID
        GetCurrReference(refPathPtr).RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                print($"Delete data success");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr);
                }
            }
        });
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_CheckUserDataExist(string keyPtr, string valuePtr, string releaseTypePtr, string objNamePtr, string callbackFunPtr);
#endif
    /// <summary>
    /// 檢查用戶資料是否已存在
    /// </summary>
    /// <param name="keyPtr">資料路徑</param>
    /// <param name="valuePtr">查詢的值</param>
    /// <param name="objNamePtr">回傳物件名</param>
    /// <param name="callbackFunPtr">回傳方法名</param>
    public async void CheckUserDataExist(string keyPtr, string valuePtr, string objNamePtr, string callbackFunPtr)
    {
#if UNITY_WEBGL
        JS_CheckUserDataExist(keyPtr,
                              valuePtr,
                              $"{Entry.Instance.releaseType}",
                              objNamePtr,
                              callbackFunPtr);
#elif UNITY_ANDROID
        string foundPhoneNumber = "";

        try
        {
            DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference(Entry.Instance.releaseType + "/user");
            DataSnapshot snapshot = await userRef.GetValueAsync();

            if (snapshot.Exists)
            {
                var userData = snapshot.Value as Dictionary<string, object>;

                // Check phoneUser
                if (userData.ContainsKey("phoneUser"))
                {
                    var phoneUserData = userData["phoneUser"] as Dictionary<string, object>;
                    foreach (var phoneNumberKey in phoneUserData.Keys)
                    {
                        var phoneNumberData = phoneUserData[phoneNumberKey] as Dictionary<string, object>;
                        if (phoneNumberData.ContainsKey(keyPtr) && phoneNumberData[keyPtr].ToString() == valuePtr)
                        {
                            foundPhoneNumber = phoneNumberData["phoneNumber"].ToString();
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("No data available");
            }

            // Output result
            if (!string.IsNullOrEmpty(foundPhoneNumber))
            {
                Debug.Log($"Sending message: {{exists: \"true\", phoneNumber: \"{foundPhoneNumber}\"}}");
                // Assuming SendMessage is a method to send messages to Unity
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, JsonUtility.ToJson(new { exists = "true", phoneNumber = foundPhoneNumber }));
                }
            }
            else
            {
                Debug.Log($"Sending message: {{exists: \"false\", phoneNumber: \"\"}}");
                if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
                {
                    GameObject obj = GameObject.Find(objNamePtr);
                    obj.SendMessage(callbackFunPtr, JsonUtility.ToJson(new { exists = "false", phoneNumber = ""}));
                }
            }
        }
        catch
        {
            print("Exist data is error");
        }
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_JoinRoomQueryData(string pathPtr, string maxPlayerPtr, string idPtr, string objNamePtr, string callbackFunPtr);
#endif
    /// <summary>
    /// 加入遊戲房間查詢
    /// </summary>
    /// <param name="pathPtr">柴尋路徑</param>
    /// <param name="maxPlayerPtr">最大房間人數</param>
    /// <param name="objNamePtr">回傳物件名</param>
    /// <param name="callbackFunPtr">回傳方法名</param>
    public async void JoinRoomQueryData(string pathPtr, string maxPlayerPtr, string idPtr, string objNamePtr, string callbackFunPtr)
    {
#if UNITY_WEBGL
        JS_JoinRoomQueryData(pathPtr,
                             maxPlayerPtr,
                             idPtr,
                             objNamePtr,
                             callbackFunPtr);
#elif UNITY_ANDROID
        string getRoomName = "false";
        int roomCount = 0;

        try
        {
            var roomRef = FirebaseDatabase.DefaultInstance.GetReference(pathPtr);
            var snapshot = await roomRef.GetValueAsync();
            if (snapshot.Exists)
            {
                var roomsType = snapshot.Value as Dictionary<string, object>;
                roomCount = roomsType.Count;

                foreach (var roomEntry in roomsType)
                {
                    var roomName = roomEntry.Key;
                    var room = roomEntry.Value as Dictionary<string, object>;

                    // Check if all players are offline
                    bool allPlayersOffline = true;
                    var playerDataDic = room["playerDataDic"] as Dictionary<string, object>;

                    foreach (var playerEntry in playerDataDic)
                    {
                        var player = playerEntry.Value as Dictionary<string, object>;
                        if ((bool)player["online"] == true)
                        {
                            allPlayersOffline = false;
                            break;
                        }
                    }

                    if (allPlayersOffline)
                    {
                        await roomRef.Child(roomName).RemoveValueAsync();
                        roomCount--;
                        continue; // Continue checking the next room
                    }

                    // Check for available spots and if the player is not in the room
                    if (playerDataDic.Count < int.Parse(maxPlayerPtr))
                    {
                        bool playerFound = false;

                        foreach (var playerEntry in playerDataDic)
                        {
                            var player = playerEntry.Value as Dictionary<string, object>;
                            if (player["userId"].ToString() == idPtr)
                            {
                                playerFound = true;
                                break;
                            }
                        }

                        if (!playerFound)
                        {
                            getRoomName = roomName;
                            break;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, JsonConvert.SerializeObject(new { getRoomName, roomCount }));
            }
        }
        catch(Exception e)
        {
             if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                GameObject obj = GameObject.Find(objNamePtr);
                obj.SendMessage(callbackFunPtr, JsonConvert.SerializeObject(new { error = e.Message }));
            }
        }
#endif
    }

    /// <summary>
    /// 異步方法for Firebase sdk回調
    /// </summary>
    /// <param name="refPath"></param>
    /// <param name="objNamePtr"></param>
    /// <param name="callbackFunPtr"></param>
    /// <returns></returns>
#if UNITY_ANDROID
    public async void GetDataAsync(string refPath, string objNamePtr, string callbackFunPtr)
    {
        try
        {
            var snapshot = await GetCurrReference(refPath).GetValueAsync();
            string res = "";

            if (snapshot.Value == null)
                print("沒有資料");
            else
            {
                res = snapshot.GetRawJsonValue();
                print("返回結果: " + res);
            }

            if (!string.IsNullOrEmpty(objNamePtr) && !string.IsNullOrEmpty(callbackFunPtr))
            {
                var obj = GameObject.Find(objNamePtr);
                if (obj != null)
                {
                    obj.SendMessage(callbackFunPtr, res);
                }
            }
        }
        catch (Exception ex)
        {
            print("異步方法發生錯誤: " + ex.Message);
        }
    }
#endif

#endregion

#region 錢包(暫不使用)

    //#if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_WindowCheckWallet(string walletName);
    //#endif
    //    private static bool WindowCheckWalletAndroid(string walletName)
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //            return javaClass.CallStatic<bool>("JS_WindowCheckWallet", walletName);
    //        }
    //    }
    //    /// <summary>
    //    /// Window檢查錢包擴充是否安裝
    //    /// </summary>
    //    /// <param name="wallet"></param>
    //    /// <returns></returns>
    //    public bool WindowCheckWallet(WalletEnum wallet)
    //    {
    //#if UNITY_WEBGL
    //        return JS_WindowCheckWallet(wallet.ToString());
    //#elif UNITY_ANDROID
    //        return WindowCheckWalletAndroid(wallet.ToString());
    //#endif
    //    }

    //    #if UNITY_WEBGL
    //    [DllImport("__Internal")]
    //    private static extern bool JS_OpenDownloadWallet(string walletName);
    //#endif
    //    private static bool OpenDownloadWalletAndroid(string walletName)
    //    {
    //        using (var javaClass = new AndroidJavaClass("com.example.APK_Javas"))
    //        {
    //           return  javaClass.CallStatic<bool>("JS_OpenDownloadWallet", walletName);
    //        }
    //    }
    //    /// <summary>
    //    /// 開啟下載錢包分頁
    //    /// </summary>
    //    /// <param name="wallet"></param>
    //    /// <returns></returns>
    //    public bool OpenDownloadWallet(WalletEnum wallet)
    //    {
    //#if UNITY_WEBGL
    //        return JS_OpenDownloadWallet(wallet.ToString());
    //#elif UNITY_ANDROID
    //        return OpenDownloadWalletAndroid(wallet.ToString());
    //#endif
    //}

#endregion

#region 工具

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_GetPlayerIPAddress();
#endif
    /// <summary>
    /// 獲取IP地址
    /// </summary>
    public void GetPlayerIPAddress()
    {
#if UNITY_WEBGL
        JS_GetPlayerIPAddress();
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_ClearUrlQueryString();
#endif
    /// <summary>
    /// 清除URL資料
    /// </summary>
    public void ClearUrlQueryString()
    {
#if UNITY_WEBGL
        JS_ClearUrlQueryString();
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_Share(string title, string content, string url);
#endif
    /// <summary>
    /// 分享
    /// </summary>
    public void Share(string title, string content, string url)
    {
#if UNITY_WEBGL
        JS_Share(title, content, url);
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool JS_Reload();
#endif
    /// <summary>
    /// 瀏覽器重新整理
    /// </summary>
    public void Reload()
    {
#if UNITY_WEBGL
        JS_Reload();
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern string JS_GetBrowserInfo();
#endif
    /// <summary>
    /// 獲取瀏覽器訊息
    /// </summary>
    public void GetBrowserInfo()
    {
#if UNITY_WEBGL
        JS_GetBrowserInfo();
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void JS_LocationHref(string url);
#endif
    /// <summary>
    /// 本地頁面跳轉
    /// </summary>
    /// <param name="url"></param>
    public void LocationHref(string url)
    {
#if UNITY_WEBGL
        JS_LocationHref(url);
#elif UNITY_ANDROID
        return;
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void JS_WindowClose();
#endif
    /// <summary>
    /// 關閉頁面
    /// </summary>
    public void WindowClose()
    {
#if UNITY_WEBGL
        JS_WindowClose();
#endif
        Application.Quit();
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void JS_OpenNewBrowser(string mail, string igIdAndName);
#endif
    /// <summary>
    /// 開啟新瀏覽器
    /// </summary>
    /// <param name="mail"></param>
    /// <param name="igIdAndName"></param>
    public void OpenNewBrowser(string mail, string igIdAndName)
    {
#if UNITY_WEBGL
        JS_OpenNewBrowser(mail, igIdAndName);
#endif
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void JS_CopyString(string copyStr);
#endif
    /// <summary>
    /// Webgl複製文字
    /// </summary>
    /// <param name="copyStr"></param>
    public void CopyString(string copyStr)
    {
#if UNITY_WEBGL
        JS_CopyString(copyStr);
#endif
        TextEditor editor = new TextEditor
        {
            text = copyStr
        };
        editor.SelectAll();
        editor.Copy();
    }

    //  Line加客服好友
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void JS_LineService(string url);
#endif

    public void onLineService(string url)
    {
#if UNITY_WEBGL
        JS_LineService(url);
#elif UNITY_ANDROID
        return;
#endif
    }

#endregion

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void onPageUnload(string scriptName, string functionName);
#endif

    public void RegisterOnPageUnload(string scriptName, string functionName)
    {
#if UNITY_WEBGL
        onPageUnload(scriptName, functionName);
#endif
    }
}
