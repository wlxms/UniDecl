using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace UniDecl.Runtime.MD
{
    /// <summary>
    /// A lightweight, dependency-free Markdown parser that converts a Markdown string into a
    /// list of <see cref="MdBlock"/> objects, each optionally containing <see cref="MdInline"/>
    /// nodes for inline formatting.
    ///
    /// Supported syntax:
    ///   Block:   ATX headings (#–######), setext headings (=== / ---), paragraphs,
    ///            fenced code blocks (``` / ~~~), unordered lists (-, *, +),
    ///            ordered lists (1., 2., …), blockquotes (&gt;), horizontal rules (---, ***, ___).
    ///   Inline:  bold (**), italic (*), bold-italic (***), inline code (`),
    ///            links ([text](url)), images (![alt](url)), hard line breaks.
    /// </summary>
    public static class MdParser
    {
        // =====================================================================
        // Pre-compiled Regex patterns
        // =====================================================================

        private static readonly Regex RxAtxHeading =
            new Regex(@"^(#{1,6})\s+(.+?)(?:\s+#+\s*)?$", RegexOptions.Compiled);

        private static readonly Regex RxHorizontalRule =
            new Regex(@"^(\*{3,}|-{3,}|_{3,})\s*$", RegexOptions.Compiled);

        private static readonly Regex RxUnorderedListItem =
            new Regex(@"^[ \t]*[-*+][ \t]+(.*)", RegexOptions.Compiled);

        private static readonly Regex RxOrderedListItem =
            new Regex(@"^[ \t]*\d+\.[ \t]+(.*)", RegexOptions.Compiled);

        private static readonly Regex RxSetextH1 =
            new Regex(@"^={3,}\s*$", RegexOptions.Compiled);

        private static readonly Regex RxSetextH2 =
            new Regex(@"^-{3,}\s*$", RegexOptions.Compiled);

        private static readonly Regex RxAtxHeadingStart =
            new Regex(@"^#{1,6}\s+", RegexOptions.Compiled);

        private static readonly Regex RxUnorderedListStart =
            new Regex(@"^[ \t]*[-*+][ \t]+", RegexOptions.Compiled);

        private static readonly Regex RxOrderedListStart =
            new Regex(@"^[ \t]*\d+\.[ \t]+", RegexOptions.Compiled);

        // =====================================================================
        // Public entry point
        // =====================================================================

        public static List<MdBlock> Parse(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return new List<MdBlock>();

            var normalized = markdown.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = normalized.Split('\n');
            return ParseLines(lines, 0, lines.Length);
        }

        // =====================================================================
        // Block-level parsing
        // =====================================================================

        private static List<MdBlock> ParseLines(string[] lines, int start, int end)
        {
            var blocks = new List<MdBlock>();
            int i = start;

            while (i < end)
            {
                var line = lines[i];

                // Blank lines between blocks
                if (string.IsNullOrWhiteSpace(line)) { i++; continue; }

                var trimmed = line.TrimStart();

                // Fenced code block
                if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
                {
                    i = ParseCodeBlock(lines, i, end, blocks);
                    continue;
                }

                // ATX heading: # H1 … ###### H6
                var headingMatch = RxAtxHeading.Match(line);
                if (headingMatch.Success)
                {
                    var content = headingMatch.Groups[2].Value.Trim();
                    blocks.Add(new MdBlock
                    {
                        Type = MdBlockType.Heading,
                        Level = headingMatch.Groups[1].Value.Length,
                        RawText = content,
                        Inlines = ParseInlines(content),
                    });
                    i++;
                    continue;
                }

                // Horizontal rule: ---, ***, ___
                if (RxHorizontalRule.IsMatch(line))
                {
                    blocks.Add(new MdBlock { Type = MdBlockType.HorizontalRule });
                    i++;
                    continue;
                }

                // Blockquote: > …
                if (trimmed.StartsWith(">"))
                {
                    i = ParseBlockquote(lines, i, end, blocks);
                    continue;
                }

                // Unordered list: -, *, +
                if (RxUnorderedListStart.IsMatch(line))
                {
                    i = ParseUnorderedList(lines, i, end, blocks);
                    continue;
                }

                // Ordered list: 1., 2., …
                if (RxOrderedListStart.IsMatch(line))
                {
                    i = ParseOrderedList(lines, i, end, blocks);
                    continue;
                }

                // Setext headings: look ahead to next non-empty line
                if (i + 1 < end)
                {
                    var next = lines[i + 1];
                    if (RxSetextH1.IsMatch(next))
                    {
                        var content = line.Trim();
                        blocks.Add(new MdBlock
                        {
                            Type = MdBlockType.Heading, Level = 1,
                            RawText = content, Inlines = ParseInlines(content),
                        });
                        i += 2; continue;
                    }
                    if (RxSetextH2.IsMatch(next))
                    {
                        var content = line.Trim();
                        blocks.Add(new MdBlock
                        {
                            Type = MdBlockType.Heading, Level = 2,
                            RawText = content, Inlines = ParseInlines(content),
                        });
                        i += 2; continue;
                    }
                }

                // Paragraph
                i = ParseParagraph(lines, i, end, blocks);
            }

            return blocks;
        }

        private static int ParseCodeBlock(string[] lines, int start, int end, List<MdBlock> blocks)
        {
            var firstTrimmed = lines[start].TrimStart();
            var fence = firstTrimmed.StartsWith("```") ? "```" : "~~~";
            var language = firstTrimmed.Substring(fence.Length).Trim();

            var sb = new StringBuilder();
            int i = start + 1;

            while (i < end)
            {
                var line = lines[i];
                if (line.TrimStart().StartsWith(fence)) { i++; break; }
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(line);
                i++;
            }

            blocks.Add(new MdBlock
            {
                Type = MdBlockType.CodeBlock,
                Language = language,
                RawText = sb.ToString(),
            });
            return i;
        }

        private static int ParseBlockquote(string[] lines, int start, int end, List<MdBlock> blocks)
        {
            var quoteLines = new List<string>();
            int i = start;

            while (i < end && !string.IsNullOrWhiteSpace(lines[i]) && lines[i].TrimStart().StartsWith(">"))
            {
                var trimmed = lines[i].TrimStart();
                // Strip leading '>' and optional single space
                quoteLines.Add(trimmed.Length > 1 && trimmed[1] == ' '
                    ? trimmed.Substring(2)
                    : trimmed.Substring(1));
                i++;
            }

            var innerBlocks = ParseLines(quoteLines.ToArray(), 0, quoteLines.Count);
            blocks.Add(new MdBlock { Type = MdBlockType.Blockquote, Blocks = innerBlocks });
            return i;
        }

        private static int ParseUnorderedList(string[] lines, int start, int end, List<MdBlock> blocks)
        {
            var items = new List<MdListItem>();
            int i = start;

            while (i < end)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) { i++; break; }

                var m = RxUnorderedListItem.Match(line);
                if (!m.Success) break;

                items.Add(new MdListItem { Inlines = ParseInlines(m.Groups[1].Value) });
                i++;
            }

            blocks.Add(new MdBlock { Type = MdBlockType.UnorderedList, Items = items });
            return i;
        }

        private static int ParseOrderedList(string[] lines, int start, int end, List<MdBlock> blocks)
        {
            var items = new List<MdListItem>();
            int i = start;
            int order = 1;

            while (i < end)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) { i++; break; }

                var m = RxOrderedListItem.Match(line);
                if (!m.Success) break;

                items.Add(new MdListItem
                {
                    IsOrdered = true,
                    OrderIndex = order++,
                    Inlines = ParseInlines(m.Groups[1].Value),
                });
                i++;
            }

            blocks.Add(new MdBlock { Type = MdBlockType.OrderedList, Items = items });
            return i;
        }

        private static int ParseParagraph(string[] lines, int start, int end, List<MdBlock> blocks)
        {
            var sb = new StringBuilder();
            int i = start;

            while (i < end)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) { i++; break; }

                var trimmed = line.TrimStart();

                // Stop on any new block type
                if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~")) break;
                if (RxAtxHeadingStart.IsMatch(line)) break;
                if (RxHorizontalRule.IsMatch(line)) break;
                if (trimmed.StartsWith(">")) break;
                if (RxUnorderedListStart.IsMatch(line)) break;
                if (RxOrderedListStart.IsMatch(line)) break;

                if (sb.Length > 0) sb.Append('\n');
                sb.Append(line);
                i++;
            }

            var text = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(text))
            {
                blocks.Add(new MdBlock
                {
                    Type = MdBlockType.Paragraph,
                    RawText = text,
                    Inlines = ParseInlines(text),
                });
            }

            return i;
        }

        // =====================================================================
        // Inline-level parsing
        // =====================================================================

        /// <summary>Parses a single-line (or multi-line) text string into inline elements.</summary>
        public static List<MdInline> ParseInlines(string text)
        {
            var result = new List<MdInline>();
            if (string.IsNullOrEmpty(text)) return result;

            int i = 0;
            var buf = new StringBuilder();

            while (i < text.Length)
            {
                char c = text[i];

                // Backslash escape
                if (c == '\\' && i + 1 < text.Length)
                {
                    buf.Append(text[i + 1]);
                    i += 2;
                    continue;
                }

                // Hard line break (newline in source)
                if (c == '\n')
                {
                    FlushBuf(buf, result);
                    result.Add(MdInline.LineBreak());
                    i++;
                    continue;
                }

                // Inline code: `…`
                if (c == '`')
                {
                    FlushBuf(buf, result);
                    int close = text.IndexOf('`', i + 1);
                    if (close > i)
                    {
                        result.Add(MdInline.Code(text.Substring(i + 1, close - i - 1)));
                        i = close + 1;
                    }
                    else { buf.Append(c); i++; }
                    continue;
                }

                // Image: ![alt](url)
                if (c == '!' && i + 1 < text.Length && text[i + 1] == '[')
                {
                    var (ok, linkText, url, ni) = TryParseLink(text, i + 1);
                    if (ok)
                    {
                        FlushBuf(buf, result);
                        result.Add(MdInline.Image(linkText, url));
                        i = ni;
                        continue;
                    }
                }

                // Link: [text](url)
                if (c == '[')
                {
                    var (ok, linkText, url, ni) = TryParseLink(text, i);
                    if (ok)
                    {
                        FlushBuf(buf, result);
                        result.Add(MdInline.Link(linkText, url));
                        i = ni;
                        continue;
                    }
                }

                // Bold+Italic: *** or ___
                if ((c == '*' || c == '_') && i + 2 < text.Length && text[i + 1] == c && text[i + 2] == c)
                {
                    var delim = new string(c, 3);
                    var (ok, content, ni) = TryParseDelimited(text, i, delim);
                    if (ok)
                    {
                        FlushBuf(buf, result);
                        result.Add(MdInline.BoldItalic(content));
                        i = ni;
                        continue;
                    }
                }

                // Bold: ** or __
                if ((c == '*' || c == '_') && i + 1 < text.Length && text[i + 1] == c)
                {
                    var delim = new string(c, 2);
                    var (ok, content, ni) = TryParseDelimited(text, i, delim);
                    if (ok)
                    {
                        FlushBuf(buf, result);
                        result.Add(MdInline.Bold(content));
                        i = ni;
                        continue;
                    }
                }

                // Italic: * or _
                if (c == '*' || c == '_')
                {
                    var delim = c.ToString();
                    var (ok, content, ni) = TryParseDelimited(text, i, delim);
                    if (ok)
                    {
                        FlushBuf(buf, result);
                        result.Add(MdInline.Italic(content));
                        i = ni;
                        continue;
                    }
                }

                buf.Append(c);
                i++;
            }

            FlushBuf(buf, result);
            return result;
        }

        // =====================================================================
        // Inline helpers
        // =====================================================================

        private static void FlushBuf(StringBuilder sb, List<MdInline> result)
        {
            if (sb.Length > 0)
            {
                result.Add(MdInline.PlainText(sb.ToString()));
                sb.Clear();
            }
        }

        /// <summary>Tries to parse [text](url) starting at <paramref name="start"/> (must point to '[').</summary>
        private static (bool ok, string text, string url, int newIndex) TryParseLink(string input, int start)
        {
            if (start >= input.Length || input[start] != '[')
                return (false, null, null, start);

            int bracketClose = FindClosing(input, start, '[', ']');
            if (bracketClose < 0) return (false, null, null, start);

            int parenOpen = bracketClose + 1;
            if (parenOpen >= input.Length || input[parenOpen] != '(')
                return (false, null, null, start);

            int parenClose = FindClosing(input, parenOpen, '(', ')');
            if (parenClose < 0) return (false, null, null, start);

            var linkText = input.Substring(start + 1, bracketClose - start - 1);
            var url = input.Substring(parenOpen + 1, parenClose - parenOpen - 1).Trim();
            return (true, linkText, url, parenClose + 1);
        }

        /// <summary>Finds the index of the matching closing bracket, handling nesting.</summary>
        private static int FindClosing(string text, int openPos, char open, char close)
        {
            int depth = 0;
            for (int k = openPos; k < text.Length; k++)
            {
                if (text[k] == open) depth++;
                else if (text[k] == close) { depth--; if (depth == 0) return k; }
            }
            return -1;
        }

        /// <summary>Tries to parse <c>delimiter…delimiter</c> starting at <paramref name="start"/>.</summary>
        private static (bool ok, string content, int newIndex) TryParseDelimited(string text, int start, string delimiter)
        {
            if (start + delimiter.Length > text.Length) return (false, null, start);
            if (text.Substring(start, delimiter.Length) != delimiter) return (false, null, start);

            int contentStart = start + delimiter.Length;
            int closeIdx = text.IndexOf(delimiter, contentStart, StringComparison.Ordinal);
            if (closeIdx <= contentStart) return (false, null, start);  // no match or empty span

            return (true, text.Substring(contentStart, closeIdx - contentStart), closeIdx + delimiter.Length);
        }
    }
}
