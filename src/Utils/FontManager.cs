using System.IO;

using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Utilities;

namespace PlayerList.Utils;

public enum FontWeight
{
  Regular = 0,
  Bold = 1,
  Italic = 2,
  BoldItalic = 3,
}

internal class FontsManager
{
  public static ImFontPtr RegularFont { get; private set; }
  public static ImFontPtr BoldFont { get; private set; }
  public static ImFontPtr ItalicFont { get; private set; }
  public static ImFontPtr BoldItalicFont { get; private set; }
  public static int DefaultFontSize { get; } = 16;
  public static int MinFontSize { get; } = 14;
  public static int MaxFontSize { get; } = 18;

  private readonly string path;
  private readonly string fontName;
  private readonly string emojisFontName;

  public FontsManager(string path, string fontName, string emojisFontName)
  {
    this.path = path;
    this.fontName = fontName;
    this.emojisFontName = emojisFontName;

    RegularFont = LoadFont(FontWeight.Regular);
    BoldFont = LoadFont(FontWeight.Bold);
    ItalicFont = LoadFont(FontWeight.Italic);
    BoldItalicFont = LoadFont(FontWeight.BoldItalic);
  }

  private ImFontPtr LoadFont(FontWeight weight)
  {
    _ = new ImGuiFontBuilder()
      .AddDefaultFont()
      .SetOption(static cfg =>
      {
        cfg.MergeMode = false;
        cfg.OversampleH = 1;
        cfg.OversampleV = 1;
        cfg.PixelSnapH = true;
        cfg.FontBuilderFlags |= (uint)ImGuiFreeTypeBuilderFlags.LoadColor;
      })
      .AddFontFromFileTTF(Path.Combine(path, $"{fontName}-{weight}.ttf"), DefaultFontSize, [0x1, 0x1FFFF])
      .Build();

    return LoadEmojisFont();
  }

  private ImFontPtr LoadEmojisFont()
  {
    return new ImGuiFontBuilder().SetOption(static cfg =>
    {
      cfg.MergeMode = true;
      cfg.OversampleH = 1;
      cfg.OversampleV = 1;
      cfg.PixelSnapH = true;
      cfg.FontBuilderFlags |= (uint)ImGuiFreeTypeBuilderFlags.LoadColor;
    })
      .AddFontFromFileTTF(Path.Combine(path, $"{emojisFontName}.ttf"), DefaultFontSize, [0x1, 0x1FFFF])
      .Build();
  }
}
