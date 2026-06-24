using MelonLoader;
using UnityEngine;

namespace IronNestFCS.Logic;

/// <summary>
/// 火控系统的 IMGUI 窗口。只负责绘制与把用户操作转发给 <see cref="FSC"/>。
/// 不含领域逻辑——按钮点击后调用 logic 的方法。
///
/// 实现说明：在 MelonLoader IL2CPP 下，MelonMod.OnGUI 每帧只触发一次，
/// 无法保证 IMGUI 所需的 Layout / event 多 pass。GUILayout 依赖 Layout pass
/// 预算尺寸，pass 不一致时 controlID 会错位，表现为"只有第一个按钮能点"。
/// 因此这里改用绝对 Rect 的 GUI.* API（不走布局系统），并且不套 GUI.Window
/// （避免回调委托封送丢失 pass）。控件 controlID 仅取决于调用顺序，稳定可靠。
/// </summary>
public class FcsWindow
{
    private readonly FSC fcs;

    private bool showWindow = true;
    private Rect defaultWindowRect = new(40, 40, 260, 170);

    public FcsWindow(FSC fcs)
    {
        this.fcs = fcs;
    }

    public void OnGui()
    {
        if (!showWindow)
            return;

        var windowRect = defaultWindowRect;

        if (fcs.LeftTask != null) {
            windowRect.height += 28f;
        }
        if (fcs.RightTask != null) {
            windowRect.height += 28f;
        }
        windowRect.height += fcs.QueueCan.Count * 28f;

        // 背景框
        GUI.Box(windowRect, "IronNest FCS");
        
        float x = windowRect.x + 10f;
        float w = windowRect.width - 20f;
        float y = windowRect.y + 25f;
        const float h = 24f;
        const float gap = 4f;

        if (!fcs.IsBound)
        {
            GUI.Label(new Rect(x, y, w, h), "Dial 未绑定。按 F9 在正确场景重载。");
            return;
        }

        GUI.Label(new Rect(x, y, w, h), "Left Gun:");
        y += h + gap;
        if (fcs.LeftTask != null) {
            GUI.Label(new Rect(x, y, w, h), $"  T{fcs.LeftTask.targetId} {fcs.LeftTask.bulletType} {fcs.LeftTask.progress}");
            y += h + gap;
            GUI.Label(new Rect(x, y, w, h), $"  Target: {fcs.LeftTask.angel:F1}°, {fcs.LeftTask.distance:F2}km");
            y += h + gap;
        }
        else {
            GUI.Label(new Rect(x, y, w, h), "  Idle");
            y += h + gap;
        }
        GUI.Label(new Rect(x, y, w, h), "Right Gun:");
        y += h + gap;
        if (fcs.RightTask != null) {
            GUI.Label(new Rect(x, y, w, h), $"  T{fcs.RightTask.targetId} {fcs.RightTask.bulletType} {fcs.RightTask.progress}");
            y += h + gap;
            GUI.Label(new Rect(x, y, w, h), $"  Target: {fcs.RightTask.angel:F1}°, {fcs.RightTask.distance:F2}km");
            y += h + gap;
        }
        else {
            GUI.Label(new Rect(x, y, w, h), "  Idle");
            y += h + gap;
        }

        GUI.Label(new Rect(x, y, w, h), $"Queued: {fcs.PendingCount}");
        y += h + gap;
        foreach (var item in fcs.QueueCan)
        {
            GUI.Label(new Rect(x, y, w, h), $"  target{item.targetId} { ConvertPosition(item.position)} {item.angel,5:F1}°/{item.distance,5:F2}km {item.bulletType.ToString()} ");
            y += h + gap;
        }

    }

    /// <summary> 计算坐标点所对应的区域字符串 </summary>
    public static string ConvertPosition(Vector3 position)
    {
        int leterIndex = (int)position.x;
        string zoneCol = leterIndex >= 0 && leterIndex < 26 ? ((char)('A' + leterIndex)).ToString() : "#";
        int zoneRow = (int)position.y + 1;
        int subCol = (int)(position.x * 10) % 10;  // B: 第一位小数
        int subRow = (int)(position.y * 10) % 10;  // B: 第一位小数

        return $"{zoneCol}{zoneRow}  {subCol}:{subRow}";
    }
}