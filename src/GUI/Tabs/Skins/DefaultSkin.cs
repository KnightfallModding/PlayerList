using Hexa.NET.ImGui;

using PlayerList.Utils;

using System.Numerics;

namespace PlayerList.GUI.Skins.Default;

internal static class DefaultSkin
{
  public static void Setup()
  {
    ImGuiStylePtr style = ImGui.GetStyle();
    System.Span<Vector4> colors = style.Colors;

    // Primary background
    colors[(int)ImGuiCol.WindowBg] = new Vector4(0.07f, 0.07f, 0.09f, ConfigManager.Opacity.Value);  // #131318
    colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.07f, 0.07f, 0.09f, 1f); // #131318
    colors[(int)ImGuiCol.PopupBg] = new Vector4(0.18f, 0.18f, 0.22f, 1f); // 

    // Headers
    colors[(int)ImGuiCol.Header] = new Vector4(0.18f, 0.18f, 0.22f, 1f);
    colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.3f, 0.3f, 0.40f, 1f);
    colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.25f, 0.25f, 0.35f, 1f);

    // Buttons
    colors[(int)ImGuiCol.Button] = new Vector4(0.2f, 0.22f, 0.27f, 1f);
    colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.3f, 0.32f, 0.40f, 1f);
    colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.35f, 0.38f, 0.5f, 1f);

    // Frame BG
    colors[(int)ImGuiCol.FrameBg] = new Vector4(0.15f, 0.15f, 0.18f, 1f);
    colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.22f, 0.22f, 0.27f, 1f);
    colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.25f, 0.25f, 0.3f, 1f);

    // Tabs
    colors[(int)ImGuiCol.Tab] = new Vector4(0.18f, 0.18f, 0.22f, 1f);
    colors[(int)ImGuiCol.TabHovered] = new Vector4(0.35f, 0.35f, 0.5f, 1f);

    // Title
    colors[(int)ImGuiCol.TitleBg] = new Vector4(0.12f, 0.12f, 0.15f, 1f);
    colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.15f, 0.15f, 0.2f, 1f);
    colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1f, 0.1f, 0.12f, 1f);

    // Borders
    colors[(int)ImGuiCol.Border] = new Vector4(0.2f, 0.2f, 0.25f, 0.5f);
    colors[(int)ImGuiCol.BorderShadow] = new Vector4(0f, 0f, 0f, 0f);

    // Text
    colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.90f, 0.95f, 1f);
    colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.5f, 0.5f, 0.55f, 1f);

    // Highlights
    colors[(int)ImGuiCol.CheckMark] = new Vector4(0.5f, 0.7f, 1f, 1f);
    colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.5f, 0.7f, 1f, 1f);
    colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.6f, 0.80f, 1f, 1f);
    colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.5f, 0.7f, 1f, 0.5f);
    colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.6f, 0.80f, 1f, 0.75f);
    colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.7f, 0.90f, 1f, 1f);

    // Scrollbar
    colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.1f, 0.1f, 0.12f, 1f);
    colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.3f, 0.3f, 0.35f, 1f);
    colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.5f, 1f);
    colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.45f, 0.45f, 0.55f, 1f);

    // Style tweaks
    style.WindowRounding = 5.0f;
    style.FrameRounding = 5.0f;
    style.GrabRounding = 5.0f;
    style.TabRounding = 5.0f;
    style.PopupRounding = 5.0f;
    style.ScrollbarRounding = 5.0f;
    style.WindowPadding = new Vector2(10, 10);
    style.FramePadding = new Vector2(6, 4);
    style.ItemSpacing = new Vector2(8, 6);
    style.PopupBorderSize = 0;
  }
}
