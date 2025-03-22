using System;
using System.Collections.Generic;
using System.Numerics;

namespace PlayerList.Utils;

/// <summary>
/// Represents a node in the rich text tree.
/// </summary>
internal class RichTextNode
{
  /// <summary>
  /// For allowed tags like &quot;b&quot;, &quot;i&quot;, &quot;color&quot;, etc.
  /// </summary>
  public string Tag { get; set; }
  /// <summary>
  /// e.g. for &lt;color=#ffffff&gt;, holds &quot;#ffffff&quot;
  /// </summary>
  public string Attribute { get; set; }
  public List<RichTextNode> Children { get; set; } = [];
  /// <summary>
  /// Non-null for text nodes
  /// </summary>
  public string Text { get; set; }
}

/// <summary>
/// The safe parser builds a tree from the input.
/// If a tag is not allowed, its entire span is consumed as literal text.
/// </summary>
internal class XMLParser(string input)
{
  private readonly string input = input;
  private int pos;

  /// <summary>
  /// Allowed tags from TextMeshPro rich text.
  /// </summary>
  private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
        {
          "b",
          "i",
          "u",
          "s",
          "color",
          "size",
          "align",
          "font",
          "material",
          "sprite",
          "quad",
          "link",
          "mark",
          "sub",
          "sup",
          "br",
          "noparse",
          "rotate",
          "voffset",
        };

  /// <summary>
  /// Entry point: parse the entire input into a document node.
  /// </summary>
  public List<TextSegment> Parse() => TextSegmentFlattener.Flatten(new() { Children = ParseNodes() });

  /// <summary>
  /// ParseNodes reads a sequence of nodes until end of input or a valid closing tag.
  /// </summary>
  private List<RichTextNode> ParseNodes()
  {
    var nodes = new List<RichTextNode>();

    while (pos < input.Length)
    {
      if (input[pos] == '<')
      {
        // Check if this is a closing tag.
        if (pos + 1 < input.Length && input[pos + 1] == '/')
        {
          // Peek the tag name.
          string closingName = PeekTagName();
          // If it's a valid allowed closing tag, let the caller handle it.
          if (closingName is not null && AllowedTags.Contains(closingName))
          {
            break;
          }
          else
          {
            // Unknown closing tag: treat as literal.
            nodes.Add(new RichTextNode() { Text = ConsumeUnknownTag() });
          }
        }
        else
        {
          // Peek tag name of the opening tag.
          string tagName = PeekTagName();
          if (tagName is not null && AllowedTags.Contains(tagName))
          {
            // Parse the allowed element.
            RichTextNode element = ParseElement();
            if (element is not null)
            {
              nodes.Add(element);
            }
            else
            {
              // Fallback: treat as literal.
              nodes.Add(new RichTextNode() { Text = ConsumeUnknownTag() });
            }
          }
          else
          {
            // Unknown tag – consume the entire tag (and if possible its closing pair) as literal.
            nodes.Add(new RichTextNode() { Text = ConsumeUnknownTag() });
          }
        }
      }
      else
      {
        // Regular text: consume until the next '<'
        int start = pos;
        while (pos < input.Length && input[pos] != '<')
          pos++;

        nodes.Add(new RichTextNode() { Text = input[start..pos] });
      }
    }

    Plugin.Log.LogInfo($"Count: {nodes.Count}");
    foreach (RichTextNode node in nodes)
      Plugin.Log.LogInfo($"Node: {node.Text}");

    return nodes;
  }

  /// <summary>
  /// PeekTagName reads a tag name without advancing pos.
  /// It looks past '<' and an optional '/'.
  /// </summary>
  private string PeekTagName()
  {
    int temp = pos;

    if (temp >= input.Length || input[temp] != '<')
      return null;

    temp++; // skip '<'

    if (temp < input.Length && input[temp] == '/')
      temp++; // skip '/'

    int start = temp;
    while (temp < input.Length && char.IsLetterOrDigit(input[temp]))
      temp++;

    if (start == temp)
      return null;

    return input[start..temp];
  }

  /// <summary>
  /// ConsumeUnknownTag consumes the entire tag (and its matching closing tag if present) as literal text.
  /// It returns the literal substring.
  /// </summary>
  private string ConsumeUnknownTag()
  {
    int start = pos;
    // First, consume until the end of the current tag.
    while (pos < input.Length && input[pos] != '>')
      pos++;

    if (pos < input.Length)
      pos++; // consume '>'

    // If this is an opening tag, try to find the matching closing tag.
    string tagName = PeekTagNameFrom(start);
    if (!string.IsNullOrEmpty(tagName))
    {
      // Look ahead for a matching closing tag.
      string closingTag = "</" + tagName + ">";
      int closingIndex = input.IndexOf(closingTag, pos, StringComparison.OrdinalIgnoreCase);
      if (closingIndex != -1)
        pos = closingIndex + closingTag.Length;
    }
    return input[start..pos];
  }

  /// <summary>
  /// PeekTagNameFrom is similar to PeekTagName but starts at a given position.
  /// </summary>
  private string PeekTagNameFrom(int position)
  {
    int temp = position;
    if (temp >= input.Length || input[temp] != '<')
      return null;

    temp++; // skip '<'
    if (temp < input.Length && input[temp] == '/')
      temp++; // skip '/'

    int start = temp;
    while (temp < input.Length && char.IsLetterOrDigit(input[temp]))
      temp++;

    if (start == temp)
      return null;

    return input[start..temp];
  }

  /// <summary>
  /// ParseElement parses an allowed element (whose tag is in AllowedTags).
  /// </summary>
  private RichTextNode ParseElement()
  {
    int originalPos = pos;
    if (pos >= input.Length || input[pos] != '<')
      return null;

    pos++; // Skip '<'

    // Parse tag name.
    int tagNameStart = pos;
    while (pos < input.Length && char.IsLetterOrDigit(input[pos]))
      pos++;

    string tagName = input[tagNameStart..pos];
    // (At this point we expect tagName is allowed.)
    // Skip whitespace.
    SkipWhitespace();

    string attributeValue = null;
    if (pos < input.Length && input[pos] == '=')
    {
      pos++; // Skip '='
      SkipWhitespace();
      attributeValue = (pos < input.Length && input[pos] == '\"') ? ParseQuotedValue() : ParseUnquotedValue();
      SkipWhitespace();
    }

    var selfClosing = false;
    if (pos < input.Length && input[pos] == '/')
    {
      selfClosing = true;
      pos++;
    }

    if (pos >= input.Length || input[pos] != '>')
    {
      // If malformed, revert and treat as literal.
      pos = originalPos;
      return null;
    }
    pos++; // Skip '>'

    var node = new RichTextNode() { Tag = tagName, Attribute = attributeValue };

    if (selfClosing || tagName.Equals("br", StringComparison.OrdinalIgnoreCase))
      return node;

    // Special handling for <noparse>
    if (tagName.Equals("noparse", StringComparison.OrdinalIgnoreCase))
    {
      int contentStart = pos;
      while (pos < input.Length)
      {
        if (input[pos] == '<' && pos + 1 < input.Length && input[pos + 1] == '/')
        {
          string closingName = PeekTagName();
          if (closingName?.Equals("noparse", StringComparison.OrdinalIgnoreCase) == true)
            break;
        }
        pos++;
      }
      node.Children.Add(new RichTextNode() { Text = input[contentStart..pos] });
      _ = TryConsumeClosingTag(tagName);
      return node;
    }

    // Parse inner content.
    node.Children = ParseNodes();
    if (pos < input.Length && input[pos] == '<' && pos + 1 < input.Length && input[pos + 1] == '/')
    {
      if (!TryConsumeClosingTag(tagName))
      {
        // If closing tag not properly consumed, revert.
        pos = originalPos;
        return null;
      }
    }
    else
    {
      // If no closing tag found, revert.
      pos = originalPos;
      return null;
    }
    return node;
  }

  /// <summary>
  /// Attempts to consume a closing tag for the given tagName.
  /// </summary>
  private bool TryConsumeClosingTag(string tagName)
  {
    SkipWhitespace();
    if (pos >= input.Length || input[pos] != '<')
      return false;

    int temp = pos;
    temp++; // Skip '<'
    if (temp >= input.Length || input[temp] != '/')
      return false;

    temp++; // Skip '/'
    int nameStart = temp;
    while (temp < input.Length && char.IsLetterOrDigit(input[temp]))
      temp++;

    string closingName = input.Substring(nameStart, temp - nameStart);
    if (!closingName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
      return false;

    SkipWhitespaceAt(ref temp);
    if (temp >= input.Length || input[temp] != '>')
      return false;

    temp++; // Skip '>'
    pos = temp;
    return true;
  }

  private string ParseQuotedValue()
  {
    pos++; // Skip opening quote
    int start = pos;
    while (pos < input.Length && input[pos] != '\"')
      pos++;

    if (pos >= input.Length)
      return null;

    string value = input[start..pos];
    pos++; // Skip closing quote
    return value;
  }

  private string ParseUnquotedValue()
  {
    int start = pos;
    while (pos < input.Length && !char.IsWhiteSpace(input[pos]) && input[pos] != '>')
      pos++;

    return input.Substring(start, pos - start);
  }

  private void SkipWhitespace()
  {
    while (pos < input.Length && char.IsWhiteSpace(input[pos]))
      pos++;
  }

  private void SkipWhitespaceAt(ref int position)
  {
    while (position < input.Length && char.IsWhiteSpace(input[position]))
      position++;
  }
}

/// <summary>
/// A flat text segment with formatting properties.
/// </summary>
internal class TextSegment
{
  public string Text { get; set; }
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4 Color { get; set; }
  public string Size { get; set; }
  public int Rotate { get; set; }

  public override string ToString()
  {
    return $"\"{Text}\" (Bold: {Bold}, Italic: {Italic}, Underline: {Underline}, Strikethrough: {Strikethrough}, Color: {Color}, Size: {Size}, Rotate={Rotate})";
  }
}

/// <summary>
/// Tracks active formatting.
/// </summary>
internal class Formatting
{
  public bool Bold { get; set; }
  public bool Italic { get; set; }
  public bool Underline { get; set; }
  public bool Strikethrough { get; set; }
  public Vector4 Color { get; set; }
  public string Size { get; set; }
  public int Rotate { get; set; }

  public Formatting Clone() => new()
  {
    Bold = Bold,
    Italic = Italic,
    Underline = Underline,
    Strikethrough = Strikethrough,
    Color = Color,
    Size = Size,
  };
}

/// <summary>
/// Flattens the nested RichTextNode tree into a flat list of TextSegment objects.
/// </summary>
internal static class TextSegmentFlattener
{
  public static List<TextSegment> Flatten(RichTextNode root)
  {
    var segments = new List<TextSegment>();
    var initialFormatting = new Formatting();
    foreach (RichTextNode node in root.Children)
      FlattenNode(node, initialFormatting, segments);

    Plugin.Log.LogInfo(segments);

    return segments;
  }

  private static void FlattenNode(RichTextNode node, Formatting current, List<TextSegment> segments)
  {
    // If node is text, add a segment.
    if (!string.IsNullOrEmpty(node.Text))
    {
      segments.Add(new TextSegment()
      {
        Text = node.Text,
        Bold = current.Bold,
        Italic = current.Italic,
        Underline = current.Underline,
        Strikethrough = current.Strikethrough,
        Color = current.Color,
        Size = current.Size,
        Rotate = current.Rotate,
      });
    }

    // Update formatting based on allowed tags.
    Formatting newFormatting = current.Clone();
    if (!string.IsNullOrEmpty(node.Tag))
    {
      switch (node.Tag.ToLower())
      {
        case "b":
          newFormatting.Bold = true;
          break;
        case "i":
          newFormatting.Italic = true;
          break;
        case "u":
          newFormatting.Underline = true;
          break;
        case "s":
          newFormatting.Strikethrough = true;
          break;
        case "color":
          newFormatting.Color = ParseColor(node.Attribute);
          break;
        case "size":
          newFormatting.Size = node.Attribute;
          break;
        case "rotate":
          newFormatting.Rotate = int.Parse(node.Attribute);
          break;
          // Add other allowed tags as needed.
      }
    }

    foreach (RichTextNode child in node.Children)
      FlattenNode(child, newFormatting, segments);
  }

  public static Vector4 ParseColor(string color)
  {
    return color switch
    {
      "black" => new(0, 0, 0, 1),
      "blue" => new(0, 0, 1, 1),
      "green" => new(0, 1, 0, 1),
      "orange" => new(1, 0.647f, 0, 1),
      "purple" => new(0.502f, 0, 0.502f, 1),
      "red" => new(1, 0, 0, 1),
      _ => ParseColorFallback(color)
    };
  }

  private static Vector4 ParseColorFallback(string color)
  {
    if (color.StartsWith("#"))
    {
      byte[] parsedColor = Convert.FromHexString(color[1..]);

      return new Vector4(
        parsedColor[0] / 255f,
        parsedColor[1] / 255f,
        parsedColor[2] / 255f,
        1
      );
    }
    else
    {
      return default;
      // return new(1, 1, 1, 1); // Default to white
    }
  }
}
