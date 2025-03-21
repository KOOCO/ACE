using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/HDR Outline", 15)]
public class HDSROutline : Outline
{
    [ColorUsage(true, true)] // 让 Inspector 支持 HDR 颜色
    public Color hdrColor = Color.white;

    protected override void Start()
    {
        base.Start();
        ApplyHDRColor();
    }

    public void ApplyHDRColor()
    {
        effectColor = hdrColor;  // 直接覆盖原本的 effectColor
        if (graphic != null)
        {
            graphic.SetVerticesDirty(); // 强制 UI 重新绘制
        }
    }

    protected void OnValidate()
    {
        ApplyHDRColor();
    }
}
