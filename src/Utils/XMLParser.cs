using System;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;

using MelonLoader;

namespace PlayerList.Utils;

/// <summary>
/// Represents a parsed rich text tag with its attributes and content.
/// </summary>
internal class RichTextTag
{
  public string Name { get; set; }
  public Dictionary<string, string> Attributes { get; set; } = new();
  public string Value { get; set; } // Primary value for simple tags like <color=red>
  public bool IsSelfClosing { get; set; }
  public bool IsClosing { get; set; }
}

/// <summary>
/// Represents a segment of formatted text.
/// </summary>
internal class TextSegment
{
  public string Text { get; set; }
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4 Color { get; set; } = new(1, 1, 1, 1);
  public float Size { get; set; } = 1.0f;
  public string Align { get; set; }
  public float Alpha { get; set; } = 1.0f;
  public string Font { get; set; }
  public int Rotate { get; set; }
  public float VOffset { get; set; }
  public bool Uppercase { get; set; }
  public bool Lowercase { get; set; }
  public bool SmallCaps { get; set; }

  public override string ToString()
  {
    return $"\"{Text}\" (B:{Bold}, I:{Italic}, U:{Underline}, S:{Strikethrough}, Color:{Color}, Size:{Size}, Alpha:{Alpha})";
  }
}

/// <summary>
/// Active formatting state stack for nested tags.
/// </summary>
internal class FormattingState
{
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4 Color { get; set; } = new(1, 1, 1, 1);
  public float Size { get; set; } = 1.0f;
  public string Align { get; set; }
  public float Alpha { get; set; } = 1.0f;
  public string Font { get; set; }
  public int Rotate { get; set; }
  public float VOffset { get; set; }
  public bool Uppercase { get; set; }
  public bool Lowercase { get; set; }
  public bool SmallCaps { get; set; }

  public FormattingState Clone() => new()
  {
    Bold = Bold,
    Italic = Italic,
    Underline = Underline,
    Strikethrough = Strikethrough,
    Color = Color,
    Size = Size,
    Align = Align,
    Alpha = Alpha,
    Font = Font,
    Rotate = Rotate,
    VOffset = VOffset,
    Uppercase = Uppercase,
    Lowercase = Lowercase,
    SmallCaps = SmallCaps
  };
}

/// <summary>
/// TextMeshPro-compatible rich text parser.
/// Matches TMP's parsing behavior as closely as possible.
/// </summary>
internal class XMLParser
{
  private readonly string input;
  private int pos;
  
  /// <summary>
  /// All supported TextMeshPro rich text tags.
  /// Based on TMP documentation and common usage patterns.
  /// </summary>
  private static readonly HashSet<string> SupportedTags = new(StringComparer.OrdinalIgnoreCase)
  {
    "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
    "gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase",
    "margin", "mark", "mspace", "noparse", "nobr", "page", "pos", "rotate",
    "s", "size", "smallcaps", "space", "sprite", "style", "sub", "sup",
    "u", "uppercase", "voffset", "width", "material", "quad"
  };

  /// <summary>
  /// Tags that are self-closing and don't require end tags.
  /// </summary>
  private static readonly HashSet<string> SelfClosingTags = new(StringComparer.OrdinalIgnoreCase)
  {
    "br", "sprite", "quad", "space", "page"
  };

  public XMLParser(string text)
  {
    input = text ?? "";
    pos = 0;
  }

  /// <summary>
  /// Parse the input text into formatted text segments.
  /// </summary>
  public List<TextSegment> Parse()
  {
    var segments = new List<TextSegment>();
    var stateStack = new Stack<FormattingState>();
    var currentState = new FormattingState();
    
    pos = 0;
    
    while (pos < input.Length)
    {
      if (input[pos] == '<')
      {
        var tag = ParseTag();
        if (tag != null && SupportedTags.Contains(tag.Name))
        {
          ProcessTag(tag, currentState, stateStack);
        }
        else
        {
          // Invalid or unsupported tag - treat as literal text
          segments.Add(CreateSegment("<", currentState));
          pos++;
        }
      }
      else
      {
        // Regular text
        var text = ParseText();
        if (!string.IsNullOrEmpty(text))
        {
          segments.Add(CreateSegment(text, currentState));
        }
      }
    }

    return segments;
  }

  /// <summary>
  /// Parse a tag starting at current position.
  /// </summary>
  private RichTextTag ParseTag()
  {
    if (pos >= input.Length || input[pos] != '<')
      return null;

    int startPos = pos;
    pos++; // Skip '<'

    // Check for closing tag
    bool isClosing = false;
    if (pos < input.Length && input[pos] == '/')
    {
      isClosing = true;
      pos++;
    }

    // Parse tag name
    int nameStart = pos;
    while (pos < input.Length && IsTagNameChar(input[pos]))
      pos++;

    if (nameStart == pos)
    {
      // No valid tag name
      pos = startPos;
      return null;
    }

    string tagName = input[nameStart..pos];
    
    var tag = new RichTextTag
    {
      Name = tagName,
      IsClosing = isClosing
    };

    if (!isClosing)
    {
      SkipWhitespace();
      
      // Parse attributes
      if (pos < input.Length && input[pos] == '=')
      {
        // Simple value: <color=red>
        pos++; // Skip '='
        SkipWhitespace();
        string value = ParseAttributeValue();
        if (value != null)
        {
          tag.Value = value;
          tag.Attributes["value"] = value;
        }
      }
      else
      {
        // Named attributes: <gradient from="red" to="blue">
        while (pos < input.Length && input[pos] != '>' && input[pos] != '/')
        {
          SkipWhitespace();
          if (pos >= input.Length || input[pos] == '>' || input[pos] == '/')
            break;

          // Parse attribute name
          int attrStart = pos;
          while (pos < input.Length && IsTagNameChar(input[pos]))
            pos++;

          if (attrStart == pos)
            break;

          string attrName = input[attrStart..pos];
          SkipWhitespace();

          if (pos < input.Length && input[pos] == '=')
          {
            pos++; // Skip '='
            SkipWhitespace();
            string attrValue = ParseAttributeValue();
            if (attrValue != null)
              tag.Attributes[attrName] = attrValue;
          }
          else
          {
            // Boolean attribute
            tag.Attributes[attrName] = "true";
          }
        }
      }

      SkipWhitespace();

      // Check for self-closing
      if (pos < input.Length && input[pos] == '/')
      {
        tag.IsSelfClosing = true;
        pos++;
        SkipWhitespace();
      }
    }

    // Expect closing '>'
    if (pos >= input.Length || input[pos] != '>')
    {
      pos = startPos;
      return null;
    }

    pos++; // Skip '>'
    return tag;
  }

  /// <summary>
  /// Parse regular text until next tag.
  /// </summary>
  private string ParseText()
  {
    int start = pos;
    while (pos < input.Length && input[pos] != '<')
      pos++;
    
    return start < pos ? input[start..pos] : "";
  }

  /// <summary>
  /// Parse an attribute value (quoted or unquoted).
  /// </summary>
  private string ParseAttributeValue()
  {
    if (pos >= input.Length)
      return null;

    if (input[pos] == '"')
    {
      // Quoted value
      pos++; // Skip opening quote
      int start = pos;
      while (pos < input.Length && input[pos] != '"')
        pos++;
      
      if (pos >= input.Length)
        return null; // Unclosed quote
      
      string value = input[start..pos];
      pos++; // Skip closing quote
      return value;
    }
    else
    {
      // Unquoted value
      int start = pos;
      while (pos < input.Length && !char.IsWhiteSpace(input[pos]) && input[pos] != '>' && input[pos] != '/')
        pos++;
      
      return start < pos ? input[start..pos] : null;
    }
  }

  /// <summary>
  /// Process a parsed tag and update formatting state.
  /// </summary>
  private void ProcessTag(RichTextTag tag, FormattingState currentState, Stack<FormattingState> stateStack)
  {
    if (tag.IsClosing)
    {
      // Handle closing tag - restore previous state if available
      if (stateStack.Count > 0)
      {
        var previousState = stateStack.Pop();
        RestoreStateForTag(tag.Name, currentState, previousState);
      }
      else
      {
        // No previous state - reset to default for this tag
        ResetTagState(tag.Name, currentState);
      }
    }
    else
    {
      // Opening tag - save current state and apply new formatting
      if (!tag.IsSelfClosing && !SelfClosingTags.Contains(tag.Name))
      {
        stateStack.Push(currentState.Clone());
      }
      
      ApplyTag(tag, currentState);
    }
  }

  /// <summary>
  /// Apply tag formatting to current state.
  /// </summary>
  private void ApplyTag(RichTextTag tag, FormattingState state)
  {
    switch (tag.Name.ToLower())
    {
      case "b":
        state.Bold = true;
        break;
      case "i":
        state.Italic = true;
        break;
      case "u":
        state.Underline = true;
        break;
      case "s":
        state.Strikethrough = true;
        break;
      case "color":
        if (!string.IsNullOrEmpty(tag.Value))
          state.Color = ParseColor(tag.Value);
        break;
      case "size":
        if (!string.IsNullOrEmpty(tag.Value))
          state.Size = ParseSize(tag.Value);
        break;
      case "alpha":
        if (!string.IsNullOrEmpty(tag.Value))
          state.Alpha = ParseAlpha(tag.Value);
        break;
      case "align":
        state.Align = tag.Value;
        break;
      case "font":
        state.Font = tag.Value;
        break;
      case "rotate":
        if (!string.IsNullOrEmpty(tag.Value) && int.TryParse(tag.Value, out int rotation))
          state.Rotate = rotation;
        break;
      case "voffset":
        if (!string.IsNullOrEmpty(tag.Value) && float.TryParse(tag.Value, out float offset))
          state.VOffset = offset;
        break;
      case "uppercase":
      case "allcaps":
        state.Uppercase = true;
        break;
      case "lowercase":
        state.Lowercase = true;
        break;
      case "smallcaps":
        state.SmallCaps = true;
        break;
    }
  }

  /// <summary>
  /// Restore specific tag state from previous formatting.
  /// </summary>
  private void RestoreStateForTag(string tagName, FormattingState current, FormattingState previous)
  {
    switch (tagName.ToLower())
    {
      case "b":
        current.Bold = previous.Bold;
        break;
      case "i":
        current.Italic = previous.Italic;
        break;
      case "u":
        current.Underline = previous.Underline;
        break;
      case "s":
        current.Strikethrough = previous.Strikethrough;
        break;
      case "color":
        current.Color = previous.Color;
        break;
      case "size":
        current.Size = previous.Size;
        break;
      case "alpha":
        current.Alpha = previous.Alpha;
        break;
      case "align":
        current.Align = previous.Align;
        break;
      case "font":
        current.Font = previous.Font;
        break;
      case "rotate":
        current.Rotate = previous.Rotate;
        break;
      case "voffset":
        current.VOffset = previous.VOffset;
        break;
      case "uppercase":
      case "allcaps":
        current.Uppercase = previous.Uppercase;
        break;
      case "lowercase":
        current.Lowercase = previous.Lowercase;
        break;
      case "smallcaps":
        current.SmallCaps = previous.SmallCaps;
        break;
    }
  }

  /// <summary>
  /// Reset specific tag state to default.
  /// </summary>
  private void ResetTagState(string tagName, FormattingState state)
  {
    switch (tagName.ToLower())
    {
      case "b":
        state.Bold = false;
        break;
      case "i":
        state.Italic = false;
        break;
      case "u":
        state.Underline = false;
        break;
      case "s":
        state.Strikethrough = false;
        break;
      case "color":
        state.Color = new Vector4(1, 1, 1, 1);
        break;
      case "size":
        state.Size = 1.0f;
        break;
      case "alpha":
        state.Alpha = 1.0f;
        break;
      case "align":
        state.Align = null;
        break;
      case "font":
        state.Font = null;
        break;
      case "rotate":
        state.Rotate = 0;
        break;
      case "voffset":
        state.VOffset = 0;
        break;
      case "uppercase":
      case "allcaps":
        state.Uppercase = false;
        break;
      case "lowercase":
        state.Lowercase = false;
        break;
      case "smallcaps":
        state.SmallCaps = false;
        break;
    }
  }

  /// <summary>
  /// Create a text segment with current formatting.
  /// </summary>
  private TextSegment CreateSegment(string text, FormattingState state)
  {
    // Apply text case transformations
    if (state.Uppercase)
      text = text.ToUpper();
    else if (state.Lowercase)
      text = text.ToLower();
    // Note: SmallCaps would need special handling in rendering

    return new TextSegment
    {
      Text = text,
      Bold = state.Bold,
      Italic = state.Italic,
      Underline = state.Underline,
      Strikethrough = state.Strikethrough,
      Color = state.Color,
      Size = state.Size,
      Align = state.Align,
      Alpha = state.Alpha,
      Font = state.Font,
      Rotate = state.Rotate,
      VOffset = state.VOffset,
      Uppercase = state.Uppercase,
      Lowercase = state.Lowercase,
      SmallCaps = state.SmallCaps
    };
  }

  /// <summary>
  /// Parse color value - supports named colors, hex values, and RGB.
  /// </summary>
  private Vector4 ParseColor(string colorValue)
  {
    if (string.IsNullOrEmpty(colorValue))
      return new Vector4(1, 1, 1, 1);

    // Named colors
    return colorValue.ToLower() switch
    {
      "black" => new Vector4(0, 0, 0, 1),
      "blue" => new Vector4(0, 0, 1, 1),
      "green" => new Vector4(0, 0.502f, 0, 1),
      "cyan" => new Vector4(0, 1, 1, 1),
      "red" => new Vector4(1, 0, 0, 1),
      "magenta" => new Vector4(1, 0, 1, 1),
      "yellow" => new Vector4(1, 0.922f, 0.016f, 1),
      "white" => new Vector4(1, 1, 1, 1),
      "orange" => new Vector4(1, 0.647f, 0, 1),
      "purple" => new Vector4(0.502f, 0, 0.502f, 1),
      "brown" => new Vector4(0.647f, 0.165f, 0.165f, 1),
      "lightblue" => new Vector4(0.678f, 0.847f, 0.902f, 1),
      "lime" => new Vector4(0, 1, 0, 1),
      "pink" => new Vector4(1, 0.753f, 0.796f, 1),
      "silver" => new Vector4(0.753f, 0.753f, 0.753f, 1),
      "gray" or "grey" => new Vector4(0.502f, 0.502f, 0.502f, 1),
      "darkblue" => new Vector4(0, 0, 0.545f, 1),
      "navy" => new Vector4(0, 0, 0.502f, 1),
      "maroon" => new Vector4(0.502f, 0, 0, 1),
      "teal" => new Vector4(0, 0.502f, 0.502f, 1),
      _ => ParseHexColor(colorValue)
    };
  }

  /// <summary>
  /// Parse hex color values (#RGB, #RRGGBB, #RRGGBBAA).
  /// </summary>
  private Vector4 ParseHexColor(string hex)
  {
    if (!hex.StartsWith("#"))
      return new Vector4(1, 1, 1, 1);

    try
    {
      hex = hex[1..]; // Remove #
      
      return hex.Length switch
      {
        3 => new Vector4( // #RGB
          Convert.ToInt32(hex[0].ToString() + hex[0], 16) / 255f,
          Convert.ToInt32(hex[1].ToString() + hex[1], 16) / 255f,
          Convert.ToInt32(hex[2].ToString() + hex[2], 16) / 255f,
          1f),
        6 => new Vector4( // #RRGGBB
          Convert.ToInt32(hex[0..2], 16) / 255f,
          Convert.ToInt32(hex[2..4], 16) / 255f,
          Convert.ToInt32(hex[4..6], 16) / 255f,
          1f),
        8 => new Vector4( // #RRGGBBAA
          Convert.ToInt32(hex[0..2], 16) / 255f,
          Convert.ToInt32(hex[2..4], 16) / 255f,
          Convert.ToInt32(hex[4..6], 16) / 255f,
          Convert.ToInt32(hex[6..8], 16) / 255f),
        _ => new Vector4(1, 1, 1, 1)
      };
    }
    catch
    {
      return new Vector4(1, 1, 1, 1);
    }
  }

  /// <summary>
  /// Parse size value - supports absolute, percentage, em, and px units.
  /// </summary>
  private float ParseSize(string sizeValue)
  {
    if (string.IsNullOrEmpty(sizeValue))
      return 1.0f;

    try
    {
      if (sizeValue.EndsWith("%"))
      {
        // Percentage
        if (float.TryParse(sizeValue[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out float percent))
          return percent / 100f;
      }
      else if (sizeValue.EndsWith("em"))
      {
        // Font units
        if (float.TryParse(sizeValue[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out float em))
          return em;
      }
      else if (sizeValue.EndsWith("px"))
      {
        // Pixel units (treat as absolute)
        if (float.TryParse(sizeValue[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out float px))
          return px / 36f; // Approximate conversion
      }
      else
      {
        // Absolute value
        if (float.TryParse(sizeValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float absolute))
          return absolute;
      }
    }
    catch
    {
      // Fall through to default
    }

    return 1.0f;
  }

  /// <summary>
  /// Parse alpha value (0-1 or 0-255).
  /// </summary>
  private float ParseAlpha(string alphaValue)
  {
    if (string.IsNullOrEmpty(alphaValue))
      return 1.0f;

    if (float.TryParse(alphaValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float alpha))
    {
      // If value > 1, assume it's in 0-255 range
      return alpha > 1.0f ? alpha / 255f : alpha;
    }

    return 1.0f;
  }

  /// <summary>
  /// Check if character is valid for tag names (letters, digits, hyphens).
  /// </summary>
  private static bool IsTagNameChar(char c) => char.IsLetterOrDigit(c) || c == '-';

  /// <summary>
  /// Skip whitespace characters.
  /// </summary>
  private void SkipWhitespace()
  {
    while (pos < input.Length && char.IsWhiteSpace(input[pos]))
      pos++;
  }
}
