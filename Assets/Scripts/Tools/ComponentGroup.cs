using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class ComponentGroup : MonoBehaviour
{
    [SerializeField] private List<Behaviour> components = new List<Behaviour>();
    Outline outline;
    private Vector2 startScale = new Vector2(1.5f, 1.5f);
    private Vector2 targetScale = new Vector2(3f, 3f);

    public void SetGroupActive(bool isActive)
    {
        foreach (var component in components)
        {
            if (component != null)
                component.enabled = isActive; // 啟用/停用組件
        }
    }

    public void flashFrame()
    {
        if (components.Count != 0)
            outline = components[0].GetComponent<Outline>();
        else
            return;

        StartCoroutine(scaleLoop(2));
    }

    IEnumerator scaleLoop(int loopTime)
    {
        for(int i = 0; i < loopTime; i++)
        {
            yield return flashScale(startScale, targetScale, 0.4f);
            yield return flashScale(targetScale, startScale, 0.4f);
        }
    }
    IEnumerator flashScale(Vector2 from, Vector2 to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            outline.effectDistance = Vector2.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        outline.effectDistance = to;
    }
}
