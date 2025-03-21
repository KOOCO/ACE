using UnityEngine;
using System.Collections.Generic;

public class ComponentGroup : MonoBehaviour
{
    [SerializeField] private List<Behaviour> components = new List<Behaviour>();

    public void SetGroupActive(bool isActive)
    {
        foreach (var component in components)
        {
            if (component != null)
                component.enabled = isActive; // 啟用/停用組件
        }
    }
}
