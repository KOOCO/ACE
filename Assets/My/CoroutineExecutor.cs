using System.Collections;
using System.Text;
using UnityEngine;

public class CoroutineExecutor : MonoBehaviour
{
    private static CoroutineExecutor _instance;

    public static CoroutineExecutor Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("CoroutineExecutor");
                _instance = obj.AddComponent<CoroutineExecutor>();
            }
            return _instance;
        }
    }

    public void Execute(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
