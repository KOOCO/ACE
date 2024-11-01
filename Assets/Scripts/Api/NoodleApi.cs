using Newtonsoft.Json;
using UnityEngine.Events;
using UnityEngine;

public class NoodleApi
{
    public static void SetUserBalance(string _balance)
    {
        NoodleBalanceResponse noodleBalanceData = JsonConvert.DeserializeObject<NoodleBalanceResponse>(_balance);
        DataManager.UserChips = noodleBalanceData.data.balance;
        DataManager.DataUpdated = true;
    }
    public static void GetBalance(UnityAction<string> _success = null, UnityAction _error = null)
    {
        SwaggerAPIManager.Instance.SendGetAPI($"/api/app/games/ace/balance/{DataManager.NoodleMemberId}/{DataManager.AccessCode}", SetUserBalance, _error);
    }

    public static void GetTableAvailableChips(UnityAction<string> _success = null, UnityAction _error = null)
    {
        SwaggerAPIManager.Instance.SendGetAPI($"/api/app/games/ace/table-available-chips/{DataManager.NoodleMemberId}/{int.Parse(DataManager.RoomId)}/{DataManager.TableId}", _success, _error);
    }
    public static void PostTableBuyIn(double amount, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        Tablebuyin tablebuyin = new Tablebuyin(DataManager.AccessCode, DataManager.NoodleMemberId, long.Parse(DataManager.RoomId), DataManager.TableId, amount);
        string data = JsonConvert.SerializeObject(tablebuyin);
        Debug.Log("TableBuyIn Data :: " + data);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-buy-in", tablebuyin, _success, _error);
    }
    public static void PostTableCashOut(UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        TableCashout tableCashout = new TableCashout(DataManager.AccessCode, DataManager.NoodleMemberId, long.Parse(DataManager.RoomId), DataManager.TableId);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-cash-out", tableCashout, _success, _error);
    }

    public static void PostTableChipsTransaction(string memberId, string roundId, double amount, int chipTransactionType, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        TableChipsTransaction tableChipsTransaction = new TableChipsTransaction(DataManager.AccessCode, DataManager.NoodleMemberId, memberId, long.Parse(DataManager.RoomId), DataManager.TableId, roundId, amount, chipTransactionType);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-chips-transaction", tableChipsTransaction, _success, _error);
    }

}
