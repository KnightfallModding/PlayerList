using Hexa.NET.ImGui;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PlayerList.Utils.RichTextParser;

public class TextSegment
{
  public string Content { get; set; }
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4? Color { get; set; } // e.g. "#FF0000" or "red"
  public string Size { get; set; } // e.g. "20" or "small"
  public string Mark { get; set; } // e.g. a highlight color
  public string Align { get; set; } // e.g. "left", "center", etc.
  public int Rotate { get; set; }
  public int CSpace { get; set; }
  public bool IsSprite { get; set; }
  public int SpriteIndex { get; set; }

  public override string ToString()
  {
    if (IsSprite)
      return $"[Sprite: Index={SpriteIndex}, Bold:{Bold}, Italic:{Italic}, Underline:{Underline}, Strikethrough:{Strikethrough}, Color:{Color}, Size:{Size}, Mark:{Mark}, Align:{Align}], Rotate={Rotate}, CSpace={CSpace}";
    return $"[Content:\"{Content}\", Bold:{Bold}, Italic:{Italic}, Underline:{Underline}, Strikethrough:{Strikethrough}, Color:{Color}, Size:{Size}, Mark:{Mark}, Align:{Align}], Rotate={Rotate}, CSpace={CSpace}";
  }
}

public class TextStyle
{
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4 Color { get; set; }
  public string Size { get; set; }
  public string Mark { get; set; }
  public string Align { get; set; }
  public int Rotate { get; set; }
  public int CSpace { get; set; }

  public TextStyle Clone()
  {
    return new TextStyle
    {
      Bold = Bold,
      Italic = Italic,
      Underline = Underline,
      Strikethrough = Strikethrough,
      Color = Color,
      Size = Size,
      Mark = Mark,
      Align = Align,
      Rotate = Rotate,
      CSpace = CSpace,
    };
  }
}

public class MarkupParser(string input)
{
  private readonly string input = input;
  private int pos = 0;
  private readonly List<TextSegment> segments = [];

  public List<TextSegment> Parse()
  {
    var defaultStyle = new TextStyle();
    ParseInternal(defaultStyle);

    return segments;
  }

  // Recursive descent parser
  private void ParseInternal(TextStyle style)
  {
    var currentText = new StringBuilder();

    while (pos < input.Length)
    {
      if (input[pos] == '<')
      {
        // Flush any accumulated text.
        if (currentText.Length > 0)
        {
          segments.Add(CreateTextSegment(currentText.ToString(), style));
          currentText.Clear();
        }

        // Check if it is a closing tag.
        if (pos + 1 < input.Length && input[pos + 1] == '/')
        {
          pos = input.IndexOf('>', pos);

          if (pos == -1) pos = input.Length;
          else pos++; // Skip '>'

          return; // End of current tag's inner content.
        }
        else
        {
          // Process an opening tag.
          var tagEnd = input.IndexOf('>', pos);
          if (tagEnd == -1) break;
          var tagContent = input.Substring(pos + 1, tagEnd - pos - 1).Trim();
          pos = tagEnd + 1; // Move past the tag.

          // Handle self-closing or special tags.
          if (tagContent.StartsWith("sprite", StringComparison.OrdinalIgnoreCase))
          {
            var spriteIndex = 0;
            // e.g.: <sprite index=3>
            var parts = tagContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
              if (part.StartsWith("index=", StringComparison.OrdinalIgnoreCase))
              {
                _ = int.TryParse(part["index=".Length..], out spriteIndex);
              }
            }
            segments.Add(new TextSegment
            {
              Content = "",
              IsSprite = true,
              SpriteIndex = spriteIndex,
              Bold = style.Bold,
              Italic = style.Italic,
              Underline = style.Underline,
              Strikethrough = style.Strikethrough,
              Color = style.Color,
              Size = style.Size,
              Mark = style.Mark,
              Align = style.Align,
              Rotate = style.Rotate,
              CSpace = style.CSpace,
            });

            continue;
          }

          if (tagContent.Equals("noparse", StringComparison.OrdinalIgnoreCase))
          {
            // Output raw text until the closing </noparse> tag.
            var endTag = input.IndexOf("</noparse>", pos, StringComparison.OrdinalIgnoreCase);
            if (endTag == -1)
            {
              _ = currentText.Append(input, pos, input.Length - pos);
              pos = input.Length;
            }
            else
            {
              _ = currentText.Append(input.AsSpan(pos, endTag - pos));
              pos = endTag + "</noparse>".Length;
            }
            continue;
          }

          // Create a new style based on the current style.
          var newStyle = style.Clone();

          // Simple tags without parameters.
          if (tagContent.Equals("b", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Bold = true;
          }
          else if (tagContent.Equals("i", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Italic = true;
          }
          else if (tagContent.Equals("u", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Underline = true;
          }
          else if (tagContent.Equals("s", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Strikethrough = true;
          }
          // Tags with parameters.
          else if (tagContent.StartsWith("color=", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Color = ParseColor(tagContent["color=".Length..]);
          }
          else if (tagContent.StartsWith("size=", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Size = tagContent["size=".Length..];
          }
          else if (tagContent.StartsWith("mark=", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Mark = tagContent["mark=".Length..];
          }
          else if (tagContent.StartsWith("align=", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Align = tagContent["align=".Length..];
          }
          else if (tagContent.StartsWith("rotate=", StringComparison.OrdinalIgnoreCase))
          {
            newStyle.Rotate = int.Parse(tagContent["rotate=".Length..].Replace("\"", ""));
          }
          else if (tagContent.StartsWith("rotate=", StringComparison.OrdinalIgnoreCase))
          {
            var value = tagContent["cspace=".Length..];

            // Based on CSS, 1em = 16px
            newStyle.Rotate = int.Parse(
              value.EndsWith("em")
                ? value.Replace("em", "")
                : value.Replace("px", "")
            );
          }
          // (Additional tags can be added here.)

          // Recursively parse inner content with the updated style.
          ParseInternal(newStyle);
        }
      }
      else
      {
        // Regular text.
        currentText.Append(input[pos]);
        pos++;
      }
    }
    if (currentText.Length > 0)
    {
      segments.Add(CreateTextSegment(currentText.ToString(), style));
    }
  }

  private static TextSegment CreateTextSegment(string text, TextStyle style)
  {
    return new TextSegment
    {
      Content = text,
      Bold = style.Bold,
      Italic = style.Italic,
      Underline = style.Underline,
      Strikethrough = style.Strikethrough,
      Color = style.Color,
      Size = style.Size,
      Mark = style.Mark,
      Align = style.Align,
      Rotate = style.Rotate,
      CSpace = style.CSpace,
      IsSprite = false
    };
  }

  public static Vector4 ParseColor(string color)
  {
    Plugin.Log.LogInfo(color);

    if (color.StartsWith("#"))
    {
      var parsedColor = Convert.FromHexString(color[1..]);

      return new(
        parsedColor[0] / 255f,
        parsedColor[1] / 255f,
        parsedColor[2] / 255f,
        1
      );
    }

    if (color.Length != 6)
    {
      return color switch
      {
        "black" => new(0, 0, 0, 1),
        "blue" => new(0, 0, 1, 1),
        "green" => new(0, 1, 0, 1),
        "orange" => new(1, 0.647f, 1, 1),
        "purple" => new(0.502f, 0, 0.502f, 1),
        "red" => new(1, 0, 1, 1),
        _ => new(1, 1, 1, 1),
      };
    }

    var r = Convert.ToInt32(color[..2], 16) / 255f;
    var g = Convert.ToInt32(color.Substring(2, 2), 16) / 255f;
    var b = Convert.ToInt32(color.Substring(4, 2), 16) / 255f;

    Plugin.Log.LogInfo((r, g, b));

    return new(r, g, b, 1);
  }

  public static void RenderRichText(List<TextSegment> segments, ImFontPtr regularFont, ImFontPtr boldFont, ImFontPtr italicFont, ImFontPtr boldItalicFont)
  {
    // Start a new line. You might want to use ImGui.SameLine() if you want segments on one line.
    ImGui.SameLine();
    foreach (var segment in segments)
    {
      // Determine which font to use.
      var fontToUse = regularFont;
      if (segment.Bold && segment.Italic)
        fontToUse = boldItalicFont;
      else if (segment.Bold)
        fontToUse = boldFont;
      else if (segment.Italic)
        fontToUse = italicFont;

      ImGui.PushFont(fontToUse);

      // If a custom color is specified, push it.
      const bool pushedColor = false;

      // Render sprite tags differently.
      if (segment.IsSprite)
      {
        // Assuming you have a loaded texture for sprites,
        // you could draw it here using ImGui.Image(...)
        // For this example, we just display the sprite index as text.
        ImGui.Text($"[Sprite #{segment.SpriteIndex}]");
      }
      else
      {
        // Render the text segment.
        ImGui.TextUnformatted(segment.Content);
      }

      // For underline or strikethrough, you can manually draw lines.
      if (segment.Underline || segment.Strikethrough)
      {
        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        var textSize = ImGui.CalcTextSize(segment.Content);
        if (segment.Underline)
        {
          // Draw an underline slightly below the text.
          drawList.AddLine(new Vector2(pos.X, pos.Y + textSize.Y),
                           new Vector2(pos.X + textSize.X, pos.Y + textSize.Y),
                           ImGui.GetColorU32(ImGuiCol.Text));
        }
        if (segment.Strikethrough)
        {
          // Draw a line through the middle of the text.
          drawList.AddLine(new Vector2(pos.X, pos.Y + (textSize.Y * 0.5f)),
                           new Vector2(pos.X + textSize.X, pos.Y + (textSize.Y * 0.5f)),
                           ImGui.GetColorU32(ImGuiCol.Text));
        }
      }

      // Pop pushed style.
      if (pushedColor) ImGui.PopStyleColor();
      ImGui.PopFont();
    }
  }
}
