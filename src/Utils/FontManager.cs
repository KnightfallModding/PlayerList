using System.IO;
using Hexa.NET.ImGui;

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
  private readonly string emojisFontName;
  private readonly string fontName;
  private readonly ImGuiIOPtr io;

  private readonly string path;

  public FontsManager(string path, string fontName, string emojisFontName)
  {
    this.path = path;
    this.fontName = fontName;
    this.emojisFontName = emojisFontName;
    io = ImGui.GetIO();

    RegularFont = LoadFont(FontWeight.Regular);
    BoldFont = LoadFont(FontWeight.Bold);
    ItalicFont = LoadFont(FontWeight.Italic);
    BoldItalicFont = LoadFont(FontWeight.BoldItalic);

    io.FontDefault = RegularFont;
  }

  public static ImFontPtr RegularFont { get; private set; }
  public static ImFontPtr BoldFont { get; private set; }
  public static ImFontPtr ItalicFont { get; private set; }
  public static ImFontPtr BoldItalicFont { get; private set; }
  public static int DefaultFontSize => 16;
  public static int MinFontSize => 14;
  public static int MaxFontSize => 18;

  private unsafe ImFontPtr LoadFont(FontWeight weight)
  {
    io.Fonts.AddFontFromFileTTF(Path.Combine(path, $"{fontName}-{weight}.ttf"), DefaultFontSize);

    return LoadEmojisFont();
  }

  private unsafe ImFontPtr LoadEmojisFont()
  {
    var fontConfig = ImGui.ImFontConfig();
    fontConfig.MergeMode = true;
    fontConfig.FontLoaderFlags = (uint)ImGuiFreeTypeLoaderFlags.LoadColor;

    return io.Fonts.AddFontFromFileTTF(Path.Combine(path, $"{emojisFontName}.ttf"), DefaultFontSize, fontConfig);
  }
}
