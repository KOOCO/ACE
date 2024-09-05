using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Services : UnitySingleton<Services>
{
    [SerializeField]
    private PlayerService playerService;

    [SerializeField]
    private RemoteImageDownloader remoteImageDownloader;

    public static PlayerService PlayerService
    {
        get { return Instance.playerService; }
    }

    public static RemoteImageDownloader RemoteImageDownloader
    {
        get { return Instance.remoteImageDownloader; }
    }


}
