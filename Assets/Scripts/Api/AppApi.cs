using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class AppApi : MonoBehaviour
{
    static string apiEndpoint = "";
    public static void OnLeaveRoom(LeaveRoom leaveRoom, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = $"/api/app/rooms/leave-table?memberId={leaveRoom.memberId}&roomId={leaveRoom.roomId}&amount={leaveRoom.amount}&type={leaveRoom.type}&rankPoint={leaveRoom.rankPoint}";

        SwaggerAPIManager.Instance.SendPostAPI<LeaveRoom>(apiEndpoint, null, _success, _error, true, true);
        //  ) (data) =>
        // {
        //     Debug.Log("Player successfully left the room.");
        //     DataManager.UserUChips += leaveRound.amount;
        //     DataManager.DataUpdated = true;
        //     OnLeaveTable();
        // },
        // (error) =>
        // {
        //     Debug.LogError($"Failed to leave the room. Error: {error}");
        // }, true, true);
    }

    public static void OnJoinRoom(JoinRoom joinRoom, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = $"/api/app/rooms/join-round?memberId={joinRoom.memberId}&tableId={joinRoom.tableId}&amount={joinRoom.amount}";

        SwaggerAPIManager.Instance.SendPostAPI<JoinRoom>(apiEndpoint, null, _success, _error, true, true);
        //  ) (data) =>
        // {
        //     Debug.Log("Player successfully left the room.");
        //     DataManager.UserUChips += leaveRound.amount;
        //     DataManager.DataUpdated = true;
        //     OnLeaveTable();
        // },
        // (error) =>
        // {
        //     Debug.LogError($"Failed to leave the room. Error: {error}");
        // }, true, true);
    }

    public static void OnPurchaseItem(PurchaseItem itemToPurchase, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = $"/api/app/items/purchase-item?itemId={itemToPurchase.itemId}&playerId={itemToPurchase.playerId}&maxCount={1000}";
        SwaggerAPIManager.Instance.SendPostAPI<PurchaseItem>(apiEndpoint, null, _success, _error, true, true);
    }

    public static void RegisterPasswordLess(RegisterPasswordLess register_Passwordless, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = "/api/app/ace-accounts/register-passwordless";
        SwaggerAPIManager.Instance.SendPostAPI<RegisterPasswordLess>(apiEndpoint, register_Passwordless, _success, _error);
    }

    public static void LoginRequest(LoginRequest loginRequest, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = "/api/app/ace-accounts/login";
        SwaggerAPIManager.Instance.SendPostAPI<LoginRequest>(apiEndpoint, loginRequest, _success, _error);
    }

    public static void RegisterRequest(Register register, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = "/api/app/ace-accounts/register";
        SwaggerAPIManager.Instance.SendPostAPI<Register>(apiEndpoint, register, _success, _error);
    }

    public static void PasswordLessLogin(PasswordLessLogin passwordless_Login, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = "/api/app/ace-accounts/passwordless-login";
        SwaggerAPIManager.Instance.SendPostAPI<PasswordLessLogin>(apiEndpoint, passwordless_Login, _success, _error);
    }

    public static void DecryptSession(string loginString, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = $"/api/app/games/ace/decrypt-session?session={loginString}";
        SwaggerAPIManager.Instance.SendPostAPI<LoginRequest>(apiEndpoint, null, _success, _error, false, true);
    }
    public static void OnRoundFinish(ResultHistoryData resultHistoryData, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        Debug.Log("Round Finish API :: " + JsonConvert.SerializeObject(resultHistoryData));
        apiEndpoint = $"/api/app/rooms/finish-round";
        SwaggerAPIManager.Instance.SendPostAPI<ResultHistoryData>(apiEndpoint, resultHistoryData, _success, _error, true);
    }

    public static void PlayerStatistics(UnityAction<string> _success = null, UnityAction _error = null)
    {
        apiEndpoint = $"/api/app/round-data/player-statistics/?playerName={DataManager.UserNickname}";
        SwaggerAPIManager.Instance.SendGetAPI(apiEndpoint, _success, _error, true);
    }

    public static void LogoutRequest(UnityAction<string> _success = null, UnityAction _error = null)
    {
        apiEndpoint = $"/api/account/logout";
        SwaggerAPIManager.Instance.SendGetAPI(apiEndpoint, _success, _error);

    }

    #region Generate Aes
    public static Aes CreateAes()
    {
        // 指定的 Key 和 IV
        byte[] key = new byte[]
        {
            0x5d, 0xb2, 0x3a, 0xe7, 0xc9, 0x88, 0x16, 0xf3, 0x7e, 0x41,
            0x2d, 0xac, 0x5f, 0x9b, 0x64, 0x3c, 0x8a, 0x72, 0x19, 0xeb,
            0x4c, 0xfd, 0x0b, 0x38, 0xa7, 0x51, 0x6e, 0x24, 0xc1, 0xda,
            0x93, 0x80
        };

        byte[] iv = new byte[]
        {
            0x7e, 0x41, 0x2d, 0xac, 0x5f, 0x9b, 0x64, 0x3c, 0x8a, 0x72,
            0x19, 0xeb, 0x4c, 0xfd, 0x0b, 0x38
        };

        // 使用 Aes.Create() 来生成 AES 对象
        Aes aes = Aes.Create();
        // 设置加密密钥和向量（可以根据需求配置或生成）
        aes.KeySize = 256;               // 设置密钥大小
        aes.Key = key;
        aes.IV = iv;

        return aes;
    }
    #endregion

    #region Encrypt
    public static string EncryptJson(byte[] byteRaw)
    {
        // 創建 AES 實例
        using (Aes aes = CreateAes())
        {
            // 創建加密器
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            // 加密 JSON 字符串
            byte[] encryptedBytes = encryptor.TransformFinalBlock(byteRaw, 0, byteRaw.Length);

            // 將加密後的位元組數組轉換為 Base64 字串
            return Convert.ToBase64String(encryptedBytes);
        }
    }
    #endregion
}
