using System.IO;
using System.Reflection.Metadata;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Utilities;
using Il2CppSystem.Runtime.Remoting.Messaging;

namespace PlayerList.Utils;

public enum FontWeight
{
  Regular,
  Bold,
  Italic,
  BoldItalic
}

public class FontsManager
{
  public static ImFontPtr RegularFont { get; private set; }
  public static ImFontPtr BoldFont { get; private set; }
  public static ImFontPtr ItalicFont { get; private set; }
  public static ImFontPtr BoldItalicFont { get; private set; }
  public static int DefaultFontSize { get; } = 16;

  private string path;
  private string fontName;
  private string emojisFontName;

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
    new ImGuiFontBuilder()
    .AddDefaultFont()
    .SetOption(cfg =>
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
    return new ImGuiFontBuilder().SetOption(cfg =>
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
