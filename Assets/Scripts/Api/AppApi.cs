using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AppApi : MonoBehaviour
{
    static string apiEndpoint = "";
    public static void OnLeaveRoom(LeaveRoom leaveRoom, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        apiEndpoint = $"/api/app/rooms/leave-table?memberId={leaveRoom.memberId}&amount={leaveRoom.amount}&type={leaveRoom.type}&rankPoint={leaveRoom.rankPoint}";

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
        apiEndpoint = $"/api/app/rooms/finish-round";
        SwaggerAPIManager.Instance.SendPostAPI<ResultHistoryData>(apiEndpoint, resultHistoryData, _success, _error);
    }

}
