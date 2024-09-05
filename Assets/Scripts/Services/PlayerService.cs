using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerService : MonoBehaviour
{
    private const string PLAYER_KEY = "PlayerObjString";
    private const string PLAYER_LOGIN_KEY = "PlayerObjLoginResponseString";
    [SerializeField]
    private Player _player = null;

    public void Awake()
    {
        string userString = PlayerPrefs.GetString(PLAYER_KEY, null);
        if (!string.IsNullOrEmpty(userString) && !userString.Equals("null"))
        {
            _player = JsonConvert.DeserializeObject<Player>(userString);
        }
    }
    private void SaveUser(bool saveUserOnline = false)
    {

        PlayerPrefs.SetString(PLAYER_KEY, JsonUtility.ToJson(_player));
        Debug.Log("Player Data Saved " + PlayerPrefs.GetString(PLAYER_KEY, null));
        if (saveUserOnline)
        {
            //Save User Online functionality will go here.
        }
    }
    public void SaveUser(string playerData)
    {
        PlayerPrefs.SetString(PLAYER_KEY, playerData);
        _player = JsonConvert.DeserializeObject<Player>(playerData);
        Debug.Log("Player Data Saved " + PlayerPrefs.GetString(PLAYER_KEY, null));
    }
    public void ResetPlayer()
    {
        _player = null;
        SaveUser();
    }

    #region Public Api's
    public string GetAccessToken()
    {
        return _player.accessToken;
    }

    public Player GetPlayer()
    {
        return _player;
    }
    #endregion
}

