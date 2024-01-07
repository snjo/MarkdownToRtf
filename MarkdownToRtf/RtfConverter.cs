using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownToRtf
{
    public class RtfConverter
    {

        // https://manpages.ubuntu.com/manpages/jammy/man3/RTF::Cookbook.3pm.html   RTF cookbook
        // https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/   RTF Pocket Guide  // scroll down for table of contents
        // https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/ch04.html   ASCII-RTF Character Chart / RTF escaped characters
        // https://www.biblioscape.com/rtf15_spec.htm   Rich Text Format (RTF) Version 1.5 Specification
        // https://latex2rtf.sourceforge.net/rtfspec.html   Rich Text Format (RTF) Specification,version 1.6

        // To set table column width, add a line before the table like this, each column value enclosed by : on both sides
        // <!---CW:2000:4000:1000:-->
        // Where the widths are listed in Twips, 1/20th of a point or 1/1440th of an inch.

        public Color TextColor = Color.Black;
        public Color HeadingColor = Color.SteelBlue;
        public Color CodeFontColor = Color.DarkSlateGray;
        public Color CodeBlockColor = Color.Lavender;
        public string Font = "fswiss Helvetica";
        public string CodeFont = "fmodern Courier New";
        public int DefaultPointSize = 10;
        public int H1PointSize = 24;
        public int H2PointSize = 18;
        public int H3PointSize = 15;
        public int H4PointSize = 13;
        public int H5PointSize = 11;
        public int H6PointSize = 10;
        public int CodeBlockPaddingWidth = 50;
        private int currentPaddingWidth;
        public ParseErrorOutput parseErrorOutput = ParseErrorOutput.ErrorTextAndRawText;
        public List<string> Errors;

        public enum ParseErrorOutput
        {
            NoOutput,
            RawText,
            ErrorText,
            ErrorTextAndRawText
        }

        private bool codeBlockActive = false;

        public RtfConverter()
        {
            //lines = new List<string>();
        }

        public string ConvertText(string text)
        {
            List<string> lines = new();
            lines.Clear();
            using (StringReader sr = new(text))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            return ConvertText(lines);
        }



        public string ConvertText(List<string> lines)
        {
            Errors = new List<string>();
            int[] textSizes = new int[7] { DefaultPointSize * 2, H1PointSize * 2, H2PointSize * 2, H3PointSize * 2, H4PointSize * 2, H5PointSize * 2, H6PointSize * 2 };
            List<int> columnSizes = new();
            var text = new StringBuilder();

            string colorTable = @"{\colortbl;" + ColorToTableDef(TextColor) + ColorToTableDef(HeadingColor) + ColorToTableDef(CodeFontColor) + ColorToTableDef(CodeBlockColor) + "}";

            text.AppendLine("{\\rtf1\\ansi\\deff0 {\\fonttbl{\\f0\\" + Font + ";}{\\f1\\" + CodeFont + ";}}" + colorTable + "\\pard");
            //string fontTable = @"\deff0{\fonttbl{\f0\fnil Default Sans Serif;}{\f1\froman Times New Roman;}{\f2\fswiss Arial;}{\f3\fmodern Courier New;}{\f4\fscript Script MT Bold;}{\f5\fdecor Old English Text MT;}}";
            text.Append(@"\cf1 ");
            
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                int numReplaced;

                //Debug.WriteLine($"¤{line}¤");

                try
                {

                    // code block, skip all other formatting
                    if (line.StartsWith('\t') || line.StartsWith("    "))
                    {
                        (line, numReplaced) = SetEscapeCharacters(line, false);

                        bool codeBlockStarting = false;
                        if (codeBlockActive == false)
                        {
                            int longestLine = CheckMaxLineLength(lines, i);
                            currentPaddingWidth = Math.Max(longestLine, CodeBlockPaddingWidth) + 3;
                            codeBlockStarting = true;
                        }
                        codeBlockActive = true;
                        if (codeBlockStarting)
                        {
                            //insert a blank line if it's the start of a block
                            text.Append(CodeblockLine("\t", currentPaddingWidth));
                        }

                        // count TABs in line as more characters than normal
                        int tabCount = line.AllIndexesOf("\t").Count() - 1;
                        int tabLength = 5;
                        // instert the actual text
                        line = CodeblockLine(line, currentPaddingWidth + numReplaced - (tabCount * tabLength));
                        text.Append(line);
                    }
                    else // normal processing
                    {
                        line = SetEscapeCharacters(line, true).text;

                        if (codeBlockActive == true)
                        {
                            text.Append(CodeblockLine("\t", currentPaddingWidth));
                            codeBlockActive = false;
                        }

                        line = SetHeading(textSizes, line);

                        line = EscapeNonStyleTags(line, new char[] { '*', '_' });

                        line = SetStyle(line, "**", "b"); // bold
                        line = SetStyle(line, "__", "b"); // bold
                        line = SetStyle(line, "*", "i"); // italic
                        line = SetStyle(line, "_", "i"); // italic

                        line = SetImage(line);

                        if (line.Contains("<!--"))
                        {
                            if (line.Contains("<!---CW:"))
                            {
                                var newColumnSizes = SetColumnWidths(line);
                                if (newColumnSizes.Count > 0)
                                {
                                    columnSizes = newColumnSizes;
                                }
                            }
                            line = RemoveComment(line);
                            //continue; // skip this line, it's a "<!--" comment
                        }

                        if (line.TrimStart().StartsWith('|'))
                        {
                            (line, i) = CreateTable(i, lines, columnSizes);
                        }

                        text.AppendLine(line);

                        text.AppendLine("\\par ");

                        //TEST error
                        //if (i % 5 == 1) throw new Exception();
                    }
                }
                catch
                {
                    Debug.WriteLine($"Error parsing line {i}: " + line);
                    bool outputError = false;
                    bool outputRawText = false;
                    
                    if (parseErrorOutput == ParseErrorOutput.ErrorText || parseErrorOutput == ParseErrorOutput.ErrorTextAndRawText)
                    {
                        outputError = true;
                    }
                    if (parseErrorOutput == ParseErrorOutput.RawText || parseErrorOutput == ParseErrorOutput.ErrorTextAndRawText)
                    {
                        outputRawText = true;
                    }

                    if (outputError) { text.Append("PARSE ERROR"); }
                    if (outputError && outputRawText) { text.Append(": "); }
                    if (outputRawText) { text.Append(line); }

                    Errors.Add($"Parse error on line {i.ToString().PadLeft(3)}: {line}");

                    text.AppendLine("\\par ");
                }
            }

            text.AppendLine("}");
            return text.ToString();
        }

        private string EscapeNonStyleTags(string line, char[] tagChars)
        {
            foreach (char tagChar in tagChars)
            {
                if (!line.Contains(tagChar)) continue;
                //Debug.WriteLine("EscapleNonStylTags, line:" + line);
                string nonTag = String.Concat(Enumerable.Repeat(tagChar, 3));
                bool found = true;
                string esc = ToUnicode(tagChar);
                //Debug.WriteLine("Esc: " + esc);
                
                //line = line.Replace(nonTag, escNonTag);
                int loopCount = 0;
                while (loopCount < 10)
                {
                    // get first 3*nonTag (*** or ___)
                    int match = line.IndexOf(nonTag);
                    
                    // count sequence length
                    int sequenceLength = 0;
                    if (match == -1) break; // stop looping, no match found
                    //Debug.WriteLine("match:" + match);
                    for (int i = match; i < line.Length; i++)
                    {
                        if (line[i] == tagChar)
                        {
                            sequenceLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    string escNonTag = String.Concat(Enumerable.Repeat(esc, sequenceLength));//sequenceLength));
                    if (match >= 0)
                    {
                        //Debug.WriteLine("Before: " + line);
                        line = line.Substring(0, match) + escNonTag + line.Substring(match+sequenceLength);
                        //Debug.WriteLine("After: " + line);
                        loopCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return line;
        }

        private int CheckMaxLineLength(List<string> lines, int startLine)
        {
            int longestLine = 0;

            for (int i = startLine; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("\t") == false && lines[i].StartsWith("    ") == false) break;
                longestLine = Math.Max(longestLine, lines[i].Length);
            }    
            return longestLine;
        }

        private string RemoveComment(string line)
        {
            string startTag = "<!--";
            string endTag = "-->";
            int commentStart = line.IndexOf(startTag);
            int commentEnd = line.IndexOf(endTag);
            StringBuilder stringBuilder = new StringBuilder();
            if (commentStart > 0) stringBuilder.Append(line.AsSpan(0, commentStart));
            if (commentEnd < line.Length) stringBuilder.Append(line.AsSpan(commentEnd + endTag.Length));
            return stringBuilder.ToString();
        }

        private static string CodeblockLine(string line, int padding)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(@"\cf3 ");
            stringBuilder.Append(@"\f1 ");
            stringBuilder.Append("\\highlight4 ");
            stringBuilder.Append(line.PadRight(padding));
            stringBuilder.Append("\\highlight0 ");
            stringBuilder.Append(@"\f0 ");
            stringBuilder.Append(@"\cf1 ");
            stringBuilder.AppendLine("\\par ");
            return stringBuilder.ToString();
        }

        private static string ColorToTableDef(Color color)
        {
            return @"\red" + color.R + @"\green" + color.G + @"\blue" + color.B + ";";
        }

        private static (string text, int numReplaced) SetEscapeCharacters(string line, bool doubleToSingleBackslash = true)
        {
            //https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/ch04.html
            string result = line;
            int numReplaced = 0;
            // IMPORTANT: \’7d is not the same as \'7d, the ' character matters

            // Escaped markdown characters
            if (doubleToSingleBackslash)
            {
                result = result.ReplaceAndCount(@"\\", @"\'5c", out numReplaced, numReplaced); // right curly brace
            }
            else
            {
                result = result.ReplaceAndCount(@"\\", @"\'5c\'5c", out numReplaced, numReplaced);
            }
            result = result.ReplaceAndCount(@"\#", @"\'23", out numReplaced, numReplaced); // number / hash, to prevent deliberate # from being used as heading
            result = result.ReplaceAndCount(@"\*", @"\'2a", out numReplaced, numReplaced); // asterisk, not font style
            result = result.ReplaceAndCount(@"\_", @"\'5f", out numReplaced, numReplaced); // underscore, not font style
            result = result.ReplaceAndCount(@"\[", @"\'5b", out numReplaced, numReplaced); // left square brace
            result = result.ReplaceAndCount(@"\]", @"\'5d", out numReplaced, numReplaced); // right square brace
            result = result.ReplaceAndCount(@"\{", @"\'7b", out numReplaced, numReplaced); // left curly brace
            result = result.ReplaceAndCount(@"\}", @"\'7d", out numReplaced, numReplaced); // right curly brace
            result = result.ReplaceAndCount(@"\`", @"\'60", out numReplaced, numReplaced); // grave
            result = result.ReplaceAndCount(@"\(", @"\'28", out numReplaced, numReplaced); // left parenthesis
            result = result.ReplaceAndCount(@"\)", @"\'29", out numReplaced, numReplaced); // right parenthesis
            result = result.ReplaceAndCount(@"\+", @"\'2b", out numReplaced, numReplaced); // plus
            result = result.ReplaceAndCount(@"\-", @"\'2d", out numReplaced, numReplaced); // minus
            result = result.ReplaceAndCount(@"\.", @"\'2e", out numReplaced, numReplaced); // period
            result = result.ReplaceAndCount(@"\!", @"\'21", out numReplaced, numReplaced); // exclamation
            result = result.ReplaceAndCount(@"\|", @"\'7c", out numReplaced, numReplaced); // pipe / vertical bar

            // Escape RTF special characters (what remains after escaping the above)
            // replace backslashes not followed by a '
            string regMatchBS = @"\\+(?!')";
            Regex reg = new (regMatchBS);
            result = ReplaceAndCountRegEx(result, reg, @"\'5c", out numReplaced, numReplaced);

            // replace curly braces
            result = result.ReplaceAndCount(@"{", @"\'7b", out numReplaced, numReplaced); // left curly brace
            result = result.ReplaceAndCount(@"}", @"\'7d", out numReplaced, numReplaced); // right curly brace

            result = GetRtfUnicodeEscapedString(result);

            return (result, numReplaced);
        }

        private static string ReplaceAndCountRegEx(string text, Regex reg, string newValue, out int count, int addToValue)
        {
            int countBefore = text.Length;
            string result = reg.Replace(text, newValue);
            int countAfter = result.Length;
            int change = countAfter - countBefore;
            count = change + addToValue;
            return result;
        }


        public static string GetRtfUnicodeEscapedString(string s)
        {
            //https://stackoverflow.com/questions/1368020/how-to-output-unicode-string-to-rtf-using-c
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (c <= 0x7f)
                    sb.Append(c);
                else
                {
                    sb.Append("\\u" + Convert.ToUInt32(c) + "?");
                    //sb.Append("\\'" + ((int)c).ToString("X"));
                }

            }
            return sb.ToString();
        }

        public static string ToUnicode(char c)
        {
            return ("\\u" + Convert.ToUInt32(c) + "?");
        }

        private static List<int> SetColumnWidths(string line)
        {
            List<int> result = new();
            // line is e.g.: <!---CW:2000:4000:1000:-->
            if (line.Contains("<!---CW:"))
            {
                string[] columnWidths = line.Split(':');
                foreach (string cw in columnWidths)
                {
                    if (int.TryParse(cw, out int width))
                    {
                        result.Add(width);
                    }
                }
            }
            return result;
        }

        private static (string line, int i) CreateTable(int i, List<string> lines, List<int> columSizes)
        {
            StringBuilder result = new();
            int tableRows = 0;
            bool foundRow = true;
            int columns = lines[i].AllIndexesOf("|").Count() - 1;

            for (int j = i; foundRow && j < lines.Count; j++)
            {
                if (lines[j].TrimStart().StartsWith('|'))
                {
                    tableRows++;
                    foundRow = true;
                }
                else
                {
                    foundRow = false;
                }
            }

            if (tableRows > 2)
            {

                for (int r = i; r < i + tableRows; r++)// string line in lines)
                {
                    int lastColumnWidth = 0;
                    if (r == i + 1) continue; // skip row with dashes that separates headings from rows
                    result.AppendLine("\\trowd\\trgaph150");
                    for (int c = 0; c < columns; c++)
                    {
                        //cellx crashes the rtf load
                        //int newWidth = (c + 1) * 3000;
                        if (columSizes.Count >= columns)
                        {
                            int newWidth = lastColumnWidth + columSizes[c];
                            result.AppendLine($"\\cellx{newWidth}");
                            lastColumnWidth = newWidth;
                        }
                        else
                        {
                            int newWidth = (c + 1) * 2000;
                            result.AppendLine($"\\cellx{newWidth}");
                        }
                    }

                    string[] split = lines[r].Trim().Split('|');
                    for (int c = 1; c < split.Length - 1; c++) // string column in split)
                    {
                        string colWord = split[c].Trim();
                        colWord = SetStyle(colWord, "**", "b"); // bold
                        colWord = SetStyle(colWord, "__", "b"); // bold
                        colWord = SetStyle(colWord, "*", "i"); // italic
                        colWord = SetStyle(colWord, "_", "i"); // italic
                        result.Append(colWord);
                        result.AppendLine("\\intbl\\cell");
                    }
                    result.AppendLine("\\row ");
                }
                result.AppendLine("\\pard");

                return (result.ToString(), i + tableRows);
            }
            else
            {
                return (lines[i], i);
            }
        }

        private static string SetImage(string line)
        {
            if (line.Contains("![image]", StringComparison.InvariantCultureIgnoreCase))
            {
                int startTagPos = line.IndexOf("![image]", StringComparison.InvariantCultureIgnoreCase);
                int endTagPos = line.IndexOf(')') + 1;
                if (startTagPos < 0 || endTagPos < 0) return line;
                if (startTagPos >= line.Length) return line;
                if (endTagPos >= line.Length) return line.Substring(0, startTagPos); // tag end is at the end of the line, skip the end string
                return string.Concat(line.AsSpan(0, startTagPos), line.AsSpan(endTagPos));
            }
            return line;
        }

        private static string SetStyle(string line, string tag, string rtfTag)
        {
            // TODO handle many *'s in a row that are not style tags *** ***************

            if (line.Contains(tag))
            {
                StringBuilder sb = new();
                List<int> matches = line.AllIndexesOf(tag).ToList();
                if (matches.Count > 0)
                {
                    //Debug.WriteLine($"SetStyle start, tag: {tag} to {rtfTag}");
                    sb.Append(line.AsSpan(0, matches[0])); // add first chunk before a tag

                    int lastTagIndex = 0;

                    for (int i = 0; i < matches.Count; i++) 
                    {
                        if (i+1 < matches.Count) // is there a second closing tag?
                        {
                            //TEST DEBUG
                            //Debug.WriteLine("line: " + line);
                            //Debug.WriteLine("sb  : " + sb.ToString());
                            //Debug.WriteLine($"i: {i}, mC: {matches.Count}, lineL: {line.Length}");
                            //Debug.Write("AllIndexes : ");
                            //foreach (int m in matches)
                            //{
                            //    Debug.Write(m + ", ");
                            //}
                            //Debug.WriteLine("");
                            // TEST DEBUG END

                            sb.Append($"\\{rtfTag} "); // start the styled text
                            if (matches[i] < line.Length && matches[i + 1] < line.Length)
                            {
                                try
                                {
                                    string words = line.Substring(matches[i], matches[i + 1] - matches[i]); // get the styled text inside the tags
                                    string add = words.Replace(tag, "");
                                    sb.Append(words.Replace(tag, "")); // remove the tags from the text
                                    //Debug.WriteLine($"1 Appending {add}");
                                }
                                catch
                                {
                                    Debug.WriteLine($"SetStyle, match index {matches[i]} and {matches[i + 1]}, line length is {line.Length} from:\n{line}");
                                }
                            }

                            sb.Append($"\\{rtfTag}0 "); // end the styled text
                            if (matches.Count > i + 2)
                            {
                                
                                int startChunkIndex = matches[i + 1] + tag.Length;
                                int chunkLength = matches[i + 2] - matches[i + 1] - tag.Length;
                                string chunk = line.Substring(startChunkIndex, chunkLength);
                                sb.Append(chunk);
                                //Debug.WriteLine($"Appending chunk from {startChunkIndex}, length {chunkLength}:{chunk}");
                                lastTagIndex = startChunkIndex + chunkLength;
                            }

                            //Debug.WriteLine($"ending? i: {i}, mC: {matches.Count}");
                            if (i+2 == matches.Count)
                            {
                                int endChunkIndex = matches[i + 1] + tag.Length;
                                string endChunk1 = line.Substring(endChunkIndex);
                                sb.Append(endChunk1);
                                //Debug.WriteLine($"End chunk from {endChunkIndex}:" + endChunk1);
                                lastTagIndex = endChunkIndex;
                            }

                            i++;
                        }
                        else // there is no closing tag, output the tag as text
                        {
                            string escapedTag = "";
                            foreach (char c in tag.ToCharArray())
                            {
                                escapedTag += SetEscapeCharacters("\\" + c.ToString()).text;
                            }
                            //Debug.WriteLine($"Escaped tag: {escapedTag}");
                            sb.Append(escapedTag);
                            //int endChunkIndex = matches[0] + tag.Length;
                            int endChunkIndex = Math.Max(lastTagIndex+tag.Length, matches[0] + tag.Length);
                            string endChunk2 = "";
                            if (endChunkIndex < line.Length)
                                endChunk2 = line.AsSpan(endChunkIndex).ToString();
                            //Debug.WriteLine($"Unclosed End Span from {endChunkIndex}:{endChunk2}");
                            //sb.Append(line.AsSpan(matches[0] + tag.Length));
                            sb.Append(endChunk2);
                        }
                    }
                    //Debug.WriteLine("Done: " + sb.ToString() + "\n");
                    line = sb.ToString();
                }
                
            }
            return line;
        }

        private static string SetHeading(int[] textSizes, string line)
        {
            if (line.TrimStart().StartsWith("#"))
            {
                StringBuilder sb = new();

                //string lineStart = line.Substring(0, 6);
                //string lineEnd = line.Substring(6);
                //int headingSize = lineStart.Split('#').Length - 1; // smaller numbers are bigger text
                line = line.TrimStart();

                int headingSize = 0;

                foreach (char c in line)
                {
                    if (c == '#')
                    {
                        headingSize++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (headingSize >= textSizes.Length)
                {
                    //not a valid heading, too many #
                    return line;
                }
                else
                {
                    sb.Append(@"\cf2 "); // set heading color
                    sb.Append($"\\fs{textSizes[headingSize]} "); // set heading size
                    int trimStart = headingSize;
                    if (line.Substring(headingSize, Math.Min(1, line.Length-headingSize)) == " ")
                    {
                        // remove the first space after heading indicator
                        trimStart++;
                    }
                    sb.Append(line.AsSpan(trimStart));
                    //sb.Append(lineEnd);
                    sb.Append($"\\fs{textSizes[0]}"); // set normal size
                    sb.Append(@"\cf1 "); // set normal color
                    line = sb.ToString();
                    return line;
                }
            }
            return line;
        }
    }

    public static class ExtensionMethods
    {
        public static IEnumerable<int> AllIndexesOf(this string str, string searchstring)
        {
            int minIndex = str.IndexOf(searchstring);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
            }
        }

        public static string ReplaceAndCount(this string text, string oldValue, string newValue, out int count, int addToCount = 0)
        {
            int lenghtDiff = newValue.Length - oldValue.Length;
            count = addToCount + ((text.Split(oldValue).Length - 1) * lenghtDiff);
            return text.Replace(oldValue, newValue);
        }
    }
}
