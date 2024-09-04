using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Services : UnitySingleton<Services>
{
    [SerializeField]
    private PlayerService playerService;

    public static PlayerService PlayerService
    {
        get { return Instance.playerService; }
    }


}
