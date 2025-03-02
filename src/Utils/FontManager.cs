using System.IO;
using System.Reflection.Metadata;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Utilities;

namespace PlayerList.Utils;

public enum FontWeight
{
  Regular,
  Bold,
  Italic,
  BoldItalic
}

public static class FontsManager
{
  public static ImFontPtr RegularFont { get; private set; }
  public static ImFontPtr BoldFont { get; private set; }
  public static ImFontPtr ItalicFont { get; private set; }
  public static ImFontPtr BoldItalicFont { get; private set; }
  public static int DefaultFontSize { get; } = 18;

  public static void Setup(string path, string name)
  {
    RegularFont = LoadFont(path, name, FontWeight.Regular);
    BoldFont = LoadFont(path, name, FontWeight.Bold);
    ItalicFont = LoadFont(path, name, FontWeight.Italic);
    BoldItalicFont = LoadFont(path, name, FontWeight.BoldItalic);
  }

  private static ImFontPtr LoadFont(string path, string name, FontWeight weight) => new ImGuiFontBuilder()
    .AddDefaultFont()
    .SetOption(cfg =>
    {
      cfg.MergeMode = false;
      cfg.OversampleH = 1;
      cfg.OversampleV = 1;
      cfg.PixelSnapH = true;
      cfg.FontBuilderFlags |= (uint)ImGuiFreeTypeBuilderFlags.LoadColor;
    })
    .AddFontFromFileTTF(Path.Combine(path, $"{name}-{weight}.ttf"), DefaultFontSize, [0x1, 0x1FFFF])
    .Build();
}
