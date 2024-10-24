using UnityEngine.Events;

public class NoodleApi
{
    public static void GetBalance(UnityAction<string> _success = null, UnityAction _error = null)
    {
        SwaggerAPIManager.Instance.SendGetAPI($"/api/ace/balance/{DataManager.AccessCode}/{DataManager.MemberId}", _success, _error);
    }

    public static void GetTableAvailableChips(UnityAction<string> _success = null, UnityAction _error = null)
    {
        SwaggerAPIManager.Instance.SendGetAPI($"/api/app/games/ace/table-available-chips/{DataManager.MemberId}/{DataManager.TableId}", _success, _error);
    }
    public static void PostTableBuyIn(double amount, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        Tablebuyin tablebuyin = new Tablebuyin(DataManager.AccessCode, DataManager.MemberId, DataManager.TableId, amount);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-buy-in", tablebuyin, _success, _error);
    }
    public static void PostTableCashOut(UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        TableCashout tableCashout = new TableCashout(DataManager.AccessCode, DataManager.MemberId, DataManager.TableId);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-cash-out", tableCashout, _success, _error);
    }

    public static void PostTableChipsTransaction(string roundId, double amount, int chipTransactionType, string transferId, UnityAction<string> _success = null, UnityAction<string> _error = null)
    {
        TableChipsTransaction tableChipsTransaction = new TableChipsTransaction(DataManager.AccessCode, DataManager.MemberId, DataManager.TableId, roundId, amount, chipTransactionType, transferId);
        SwaggerAPIManager.Instance.SendPostAPI($"/api/app/games/ace/table-chips-transaction", tableChipsTransaction, _success, _error);
    }

}
