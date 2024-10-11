using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ScriptableObject", menuName = "MyMenu/Create StringAlbum", order = 1)]
[System.Serializable]
public sealed class StringAlbum : ScriptableObject
{
    public string[] strAlbum;
}