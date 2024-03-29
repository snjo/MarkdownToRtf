﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkdownToRtf
{
    public class RtfConverter
    {
        // IMPORTANT NOTE REGARDING IMAGES IN MARKDOWN
        // The RichTextBox Readonly value must be False, otherwise images will not load. This is a bug, since at least 2018

        // https://manpages.ubuntu.com/manpages/jammy/man3/RTF::Cookbook.3pm.html   RTF cookbook
        // https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/   RTF Pocket Guide  // scroll down for table of contents
        // https://www.oreilly.com/library/view/rtf-pocket-guide/9781449302047/ch04.html   ASCII-RTF Character Chart / RTF escaped characters
        // https://www.biblioscape.com/rtf15_spec.htm   Rich Text Format (RTF) Version 1.5 Specification
        // https://latex2rtf.sourceforge.net/rtfspec.html   Rich Text Format (RTF) Specification,version 1.6

        // To set table column width, add a line before the table like this, each column value enclosed by : on both sides
        // <!---CW:2000:4000:1000:-->
        // Where the widths are listed in Twips, 1/20th of a point or 1/1440th of an inch.

        //public Color TextColor = Color.Black;
        //public Color HeadingColor = Color.SteelBlue;
        //public Color CodeFontColor = Color.DarkSlateGray;
        //public Color CodeBackgroundColor = Color.Lavender;
        //public Color ListPrefixColor = Color.Blue;
        //public Color LinkColor = Color.Blue;

        private RtfColorInfo rtfTextColor = new RtfColorInfo(Color.Black, 1);
        private RtfColorInfo rtfHeadingColor = new RtfColorInfo(Color.SteelBlue, 2);
        private RtfColorInfo rtfCodeFontColor = new RtfColorInfo(Color.DarkSlateGray, 3);
        private RtfColorInfo rtfCodeBackgroundColor = new RtfColorInfo(Color.Lavender, 4);
        private RtfColorInfo rtfListPrefixColor = new RtfColorInfo(Color.Blue, 5);
        private RtfColorInfo rtfLinkColor = new RtfColorInfo(Color.CornflowerBlue, 6);

        public Color TextColor
        {
            get { return rtfTextColor.Color; }
            set { rtfTextColor = new RtfColorInfo(value, 1); }
        }
        public Color HeadingColor
        {
            get { return rtfHeadingColor.Color; }
            set { rtfHeadingColor = new RtfColorInfo(value, 2); }
        }
        public Color CodeFontColor
        {
            get { return rtfCodeFontColor.Color; }
            set { rtfCodeFontColor = new RtfColorInfo(value, 3); }
        }
        public Color CodeBackgroundColor
        {
            get { return rtfCodeBackgroundColor.Color; }
            set { rtfCodeBackgroundColor = new RtfColorInfo(value, 4); }
        }
        public Color ListPrefixColor
        {
            get { return rtfListPrefixColor.Color; }
            set { rtfListPrefixColor = new RtfColorInfo(value, 5); }
        }
        public Color LinkColor
        {
            get { return rtfLinkColor.Color; }
            set { rtfLinkColor = new RtfColorInfo(value, 6); }
        }

        public string Font = "fswiss Segoe UI"; //"fswiss Tahoma"; // "fswiss Calibri"; //"fswiss Segoe UI";
        public string CodeFont = "fmodern Courier New";
        public int DefaultPointSize = 10;
        public int H1PointSize = 24;
        public int H2PointSize = 18;
        public int H3PointSize = 15;
        public int H4PointSize = 13;
        public int H5PointSize = 11;
        public int H6PointSize = 10;
        private int CodeBlockPaddingWidth = 50;
        public ParseErrorOutput parseErrorOutput = ParseErrorOutput.ErrorTextAndRawText;
        public List<string> Errors = new List<string>();
        public bool AllowUnderscoreBold = true;
        public bool AllowUnderscoreItalic = true;
        public bool AllowOrderedList = true;
        public bool AllowUnOrderedList = true;
        public int tabLength = 5; // some systems use 8, some use 5 spaces as a tab character. Output in Winforms RTF box is 5

        private int currentPaddingWidth;
        private RtfColorInfo currentFontColor;
        private RtfColorInfo previousFontColor;
        private string FileName;

        public enum ParseErrorOutput
        {
            NoOutput,
            RawText,
            ErrorText,
            ErrorTextAndRawText
        }

        private bool codeBlockActive = false;

        public RtfConverter(string fileName)
        {
            currentFontColor = rtfTextColor;
            previousFontColor = rtfTextColor;
            FileName = fileName;
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

            string colorTable = @"{\colortbl;" + ColorToTableDef(TextColor) + ColorToTableDef(HeadingColor) + ColorToTableDef(CodeFontColor) + ColorToTableDef(CodeBackgroundColor) + ColorToTableDef(ListPrefixColor) + ColorToTableDef(LinkColor) + "}";
            string fontTable = "{\\fonttbl{\\f0\\" + Font + "; }{\\f1\\" + CodeFont + "; }}";
            text.AppendLine("{\\rtf1\\ansi\\deff0 " + fontTable + colorTable + "\\pard");
            //string fontTable = @"\deff0{\fonttbl{\f0\fnil Default Sans Serif;}{\f1\froman Times New Roman;}{\f2\fswiss Arial;}{\f3\fmodern Courier New;}{\f4\fscript Script MT Bold;}{\f5\fdecor Old English Text MT;}}";
            text.Append(UseFontColor(rtfTextColor, "Start convert"));
            
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string nextLine = string.Empty;
                if (lines.Count > i+1)
                {
                    nextLine = lines[i+1];
                }

                //Debug.WriteLine($"¤{line}¤");

                try
                { 

                    // Code block, skip all other formatting
                    if (line.StartsWith('\t') || line.StartsWith("    ")) // lines starting with TAB or four spaces is a code block
                    {
                        CreateCodeBlock(lines, text, i, ref line, codeBlockActive);
                        codeBlockActive = true;
                    }
                    else // normal processing
                    {
                        line = SetEscapeCharacters(line, true).text;

                        if (codeBlockActive == true) // exiting a code block from the previous lines
                        {
                            text.Append(CodeblockLine("\t", currentPaddingWidth));
                            codeBlockActive = false;
                        }

                        // # to ####### at the start of a line is a heading
                        line = SetHeading(textSizes, line);

                        // if there are three or more * or _ in a row, its text, not a font style marker
                        line = EscapeNonStyleTags(line, new char[] { '*', '_' });

                        // Font style, * _ ** __ used for bold and italic
                        line = SetStyle(line, "**", "b"); // bold
                        line = SetStyle(line, "*", "i"); // italic
                        // Option: in cases where unescaped underlines cause problems mid text, disable underscore as font style
                        if (AllowUnderscoreBold) 
                        {
                            line = SetStyle(line, "__", "b"); // bold
                        }
                        if (AllowUnderscoreItalic)
                        {
                            line = SetStyle(line, "_", "i"); // italic
                        }

                        // Images. Currently images are removed, TODO: inline images
                        // ![image title](http://example.com/picture.png)
                        line = SetImage(line);

                        // Comment tag. Remove or implement special behavior in the comment
                        if (line.Contains("<!--"))
                        {
                            if (line.Contains("<!---CW:")) // Commen Widths instruction, set the Twip width of the following tables, until a new CW is defined
                            {
                                var newColumnSizes = SetColumnWidths(line);
                                if (newColumnSizes.Count > 0)
                                {
                                    columnSizes = newColumnSizes;
                                }
                            }
                            line = RemoveComment(line);
                            if (line.Length == 0)
                            {
                                continue; // skip this line, it's a "<!--" comment, with no other text on the line
                            }
                        }

                        // lists, ordered and unordered
                        line = SetListSymbols(line, nextLine);

                        // Table. Create table if at least one line followin also start with |.
                        // Using the format | one | two | three |   // headings
                        //                  |-----|-----|-------|   // this line is skipped
                        //                  | a   | b   | c     |   // content
                        if (line.TrimStart().StartsWith('|')) 
                        {
                            (line, i) = CreateTable(i, lines, columnSizes);
                        }

                        // insert links if there's a [example title](http://example.com) in text. Actual link click handling is handled by the rich text box in your application
                        line = SetLink(line);

                        // add the finished line and insert line break
                        text.AppendLine(line);
                        //text.Append(RevertFontColor());
                        text.AppendLine("\\par ");
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

            // end the rtf file
            text.AppendLine("}");
            return text.ToString();
        }


        private string UseFontColor(RtfColorInfo newColor, string debugHint = "???")
        {
            //Debug.WriteLine($"Use font color, hint: {debugHint}. new:{newColor.Color}, previous:{previousFontColor.Color}, current:{currentFontColor.Color}");
            previousFontColor = currentFontColor;
            currentFontColor = newColor;
            return newColor.asFontColor();
        }

        //private string RevertFontColor(string debugHint = "???")
        //{

        //        Debug.WriteLine($"Reverting font color, hint: {debugHint}. previous:{previousFontColor.Color}, current:{currentFontColor.Color}");

        //    RtfColorInfo tempColor = currentFontColor;
        //    currentFontColor = previousFontColor;
        //    previousFontColor = tempColor;
        //    return previousFontColor.asFontColor();
        //}

        private string SetLink(string line)
        {
            string linkTitle = "";
            string linkUrl = "";
            int endLinkUrl;
            int startLinkTitle = line.IndexOf("[");
            if (startLinkTitle == -1)
            {
                return line;
            }
            else
            {

                int endLinkTitle = line.IndexOf("]", startLinkTitle);
                if (endLinkTitle > -1)
                {
                    int startLinkUrl = line.IndexOf("(", endLinkTitle);
                    if (startLinkUrl > -1 && startLinkUrl < endLinkTitle + 2)  // the ( must immediately follow the closing ]
                    {
                        endLinkUrl = line.IndexOf(")", startLinkUrl);
                        if (endLinkUrl > -1)
                        {
                            linkTitle = line.Substring(startLinkTitle + 1, endLinkTitle - startLinkTitle - 1);
                            linkUrl = line.Substring(startLinkUrl + 1, endLinkUrl - startLinkUrl - 1);
                            StringBuilder sb = new StringBuilder();
                            sb.Append(line.AsSpan(0, startLinkTitle));
                            sb.Append(UseFontColor(rtfLinkColor,"Link"));
                            sb.Append(CreateLinkCode(linkTitle, linkUrl));
                            if (LineIsHeading)
                            {
                                sb.Append(rtfHeadingColor.asFontColor());
                            }
                            else
                            {
                                sb.Append(rtfTextColor.asFontColor());
                            }
                            sb.Append(line.AsSpan(endLinkUrl+1));
                            return sb.ToString();
                        }
                    }
                }
            }
            return line;
        }

        private string CreateLinkCode(string linkTitle, string linkURL)
        {
            string result = "{\\field{\\*\\fldinst HYPERLINK \"" + linkURL + "\"}{\\fldrslt " + linkTitle + "}}";
            //string result = "{\\field{\\*\\fldinst HYPERLINK \"http://www.google.com/\"}{\\fldrslt Google}}";
            return result;
        }

        private string SetImage(string line)
        {
            // IMPORTANT
            // The RichTextBox Readonly value must be False, otherwise images will not load. This is a bug, since at least 2018
            
            string imageTitle = "";
            string imageUrl = "";
            int startImageTitle = line.IndexOf("![");
            if (startImageTitle == -1)
            {
                return line;
            }
            else
            {
                int endImageTitle = line.IndexOf("]", startImageTitle);
                if (endImageTitle > -1)
                {
                    int startImageUrl = line.IndexOf("(", endImageTitle);
                    if (startImageUrl > -1 && startImageUrl < endImageTitle +2) // the ( must immediately follow the closing ]
                    {
                        int endImageUrl = line.IndexOf(")", startImageUrl);
                        if (endImageUrl > -1)
                        {
                            imageTitle = line.Substring(startImageTitle + 2, endImageTitle - startImageTitle - 2); // text inside []
                            imageUrl = line.Substring(startImageUrl + 1, endImageUrl - startImageUrl - 1); // text inside ()

                            StringBuilder sb = new StringBuilder();
                            sb.Append(line.AsSpan(0, startImageTitle)); // text before the image
                            sb.Append(CreateImageCode(imageTitle, imageUrl)); // the pict code
                            sb.Append(line.AsSpan(endImageUrl+1)); // text after the image
                            return sb.ToString();
                        }
                    }
                }
            }
            return line;
        }

        private string CreateImageCode(string imageTitle, string imageUrl)
        {
            // IMPORTANT
            // The RichTextBox Readonly value must be False, otherwise images will not load. This is a bug, since at least 2017

            // https://www.codeproject.com/Articles/4544/Insert-Plain-Text-and-Images-into-RichTextBox-at-R
            //    {\pict\wmetafile8\picw[N]\pich[N]\picwgoal[N]\pichgoal[N] [BYTES]}
            // OR {\pict\pngblip\picw[N]\pich[N]\picwgoal[N]\pichgoal[N] [BYTES]}
            // \pict - The starting picture or image tag
            // \wmetafile[N] - Indicates that the image type is a Windows Metafile. [N] = 8 specifies that the metafile's axes can be sized independently.
            // \picw[N] and \pich[N] - Define the size of the image, where[N] is in units of hundreths of millimeters(0.01)mm.
            // \picwgoal[N] and \pichgoal[N] - Define the target size of the image, where[N] is in units of twips.
            // [BYTES] - The HEX representation of the image.

            // \emfblip      Source of the picture is an EMF (enhanced metafile).
            // \pngblip      Source of the picture is a PNG.
            // \jpegblip     Source of the picture is a JPEG.
            // \shppict      Specifies a Word 97-2000 picture. This is a destination control word.
            // \nonshppict   Specifies that Word 97-2000 has written a {\pict destination that it will not read on input. This keyword is for compatibility with other readers.
            // \macpict      Source of the picture is QuickDraw.
            // \pmmetafileN  Source of the picture is an OS/2 metafile. The N argument identifies the metafile type. The N values are described in the \pmmetafile table below.
            // \wmetafileN   Source of the picture is a Windows metafile. The N argument identifies the metafile type (the default is 1).
            // \dibitmapN    Source of the picture is a Windows device-independent bitmap. The N argument identifies the bitmap type (must equal 0).The information to be included in RTF from a Windows device-independent bitmap is the concatenation of the BITMAPINFO structure followed by the actual pixel data.    
            // \wbitmapN     Source of the picture is a Windows device-dependent bitmap. The N argument identifies the bitmap type (must equal 0).The information to be included in RTF from a Windows device-dependent bitmap is the result of the GetBitmapBits function.

            // couldn't get metafile to work, using png
            // https://www.codeproject.com/Articles/177394/Working-with-Metafile-Images-in-NET

            // This worked using pngblip, along with turning readonly off
            // https://itecnote.com/tecnote/c-programmatically-adding-images-to-rtf-document/

            #pragma warning disable CA1416 // Validate platform compatibility

            string docPath;
            string imagePath;
            Image? img = null;
            byte[]? bytes = null;
            MemoryStream stream = new MemoryStream();
            int imageWidth = 100;
            int imageHeight = 100;
            if (imageUrl.StartsWith("http") || imageUrl.StartsWith("ftp"))
            {
                // load file from web
                //imageUrlIsWebAddress = true;
                imagePath = imageUrl;
                using (WebClient client = new())
                {
                    bytes = client.DownloadData(imageUrl);
                    using (var ms = new MemoryStream(bytes))
                    {
                        img = Image.FromStream(ms);
                    }
                    imageWidth = img.Width;
                    imageHeight = img.Height;
                    img.Dispose();
                    img = null;

                }
            }
            else
            {
                // load file from disk
                //imageUrlIsWebAddress = false;
                docPath = Path.GetDirectoryName(FileName) + "";
                imagePath = Path.Combine(docPath, imageUrl);
                if (File.Exists(imagePath))
                {
                    Debug.WriteLine("Loading file from disk: " +  imagePath);
                    
                    img = Image.FromFile(imagePath);
                    img.Save(stream, ImageFormat.Png);
                    bytes = stream.ToArray();
                    imageWidth = img.Width;
                    imageHeight = img.Height;
                    img.Dispose();
                    img = null;
                }
                else
                {
                    Debug.WriteLine("File not found: " + imagePath);
                }
            }

            if (bytes != null)
            {
                #pragma warning disable CA1416 // Validate platform compatibility
                Debug.WriteLine($"Load Image {imageTitle} URL {imagePath}");

                //Debug.WriteLine("Image width: " + img.Width);

                StringBuilder sb = new StringBuilder();
                sb.Append(@"{\pict\pngblip");
                sb.Append("\\picw" + imageWidth); //width source
                sb.Append("\\pich" + imageHeight); //height source 
                int imageTwipsWidth = imageWidth * 15;
                int imageTwipsHeight = imageHeight * 15;
                sb.Append("\\picwgoal" + imageTwipsWidth); //width in twips
                sb.Append("\\pichgoal" + imageTwipsHeight); //height in twips
                sb.Append("\\hex ");

                //MemoryStream stream = new MemoryStream();
                //img.Save(stream, ImageFormat.Png);

                //byte[] bytes = stream.ToArray();
                string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty);

                sb.Append(str);

                sb.Append("}");
                Debug.WriteLine(sb.ToString());
                return sb.ToString();
            }
            else
            {
                
                Debug.WriteLine($"Image {imageTitle} could not be found from URL {imagePath}");
                //return $"\\u128444? ({imagePath.Replace("\\", "\\\\")})"; // outputs an icon of a framed picture (fallback) 🖼
                return CreateLinkCode($"\\u128444? ({imageTitle}: {imageUrl})", imageUrl); //.Replace("\\","\\\\")}
            }

            #pragma warning restore CA1416 // Validate platform compatibility
        }

        private string SetListSymbols(string line, string nextLine)
        {
            string updatedLine = line;
            if (AllowUnOrderedList)
            {
                updatedLine = UnorderedListSymbol(line, nextLine);
            }
            if (updatedLine != line)
            {
                return updatedLine;
            }
            else if (AllowOrderedList)
            {
                return OrderedListSymbol(line, nextLine);
            }
            return line;
        }

        int OrderedListCurrentNumber = -1;
        bool OrderedListActive = false;
        private string OrderedListSymbol(string line, string nextLine)
        {
            StringBuilder sb = new();
            int prefixLenght = 1;
            
            if (line.Length == 0)
            {
                return line;
            }
            char firstChar = line[0];
            char firstCharNextLine = ' ';
            if (nextLine.Length > 0)
            {
                firstCharNextLine = nextLine[0];
            }
            

            bool lineHasNumber = (Char.IsNumber(firstChar));
            bool nextLineHasNumber = (Char.IsNumber(firstCharNextLine));

            if (lineHasNumber == false)
            {
                OrderedListActive = false;
                return line; // not a list, exit.
            }
            
            if (OrderedListActive == false && nextLineHasNumber == false)
            {
                OrderedListActive = false;
                return line; // not a list, exit.
            }
                
            // start making the list
            if (OrderedListActive == false)
            {
                OrderedListCurrentNumber = 1;
            }
            OrderedListActive = true;

            // if prefix is more than 1 digit
            bool listSymbolValid = false;
            while (line.Length > prefixLenght)
            {
                char nextChar = line[prefixLenght];
                if (Char.IsNumber(nextChar))
                {
                    prefixLenght++;
                }
                else
                {
                    if (nextChar == '.' || nextChar == ')')
                    {
                        listSymbolValid = true;
                        prefixLenght++;
                    }
                    break;
                }
            }

            if (!listSymbolValid)
            {
                return line;
            }

            sb.Append(UseFontColor(rtfListPrefixColor, "Ordered List"));
            sb.Append(OrderedListCurrentNumber.ToString().PadLeft(prefixLenght).PadRight(4));
            //sb.Append(RevertFontColor("Ordered List"));
            sb.Append(UseFontColor(rtfTextColor, "Ordered List"));
            OrderedListCurrentNumber++;
            sb.Append(line.AsSpan(prefixLenght));
            return sb.ToString();
        }

        bool UnOrderedListActive = false;
        private string UnorderedListSymbol(string line, string nextLine)
        {
            string asteriskEsc = @"\'2a ";
            string[] unOrderedListPrefixes = { "- ", "+ ", "* ", asteriskEsc };
            //bool unOrderedList = false;
            string currentPrefix = "";
            string unOrderedOutSymbol = " • ";

            
            bool thisLineHasPrefix = false;
            bool nextLineHasPrefix = false;
            foreach (string prefix in unOrderedListPrefixes)
            {
                thisLineHasPrefix = line.StartsWith(prefix);
                nextLineHasPrefix = nextLine.StartsWith(prefix);
                
                if (thisLineHasPrefix && (nextLineHasPrefix || UnOrderedListActive))
                {
                    currentPrefix = prefix;
                    UnOrderedListActive = true;
                    break;
                }
            }

            if (thisLineHasPrefix == false || (nextLineHasPrefix == false && UnOrderedListActive == false))
            {
                UnOrderedListActive = false;
                return line;
            }

            if (UnOrderedListActive)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(UseFontColor(rtfListPrefixColor,"UnOrdered List"));
                sb.Append(unOrderedOutSymbol.PadRight(4));
                sb.Append(UseFontColor(rtfTextColor, "UnOrdered List"));
                sb.Append(line.AsSpan(currentPrefix.Length));
                line = sb.ToString();
            }
            return line;
        }

        private void CreateCodeBlock(List<string> lines, StringBuilder text, int i, ref string line, bool blockStartedPreviously)
        {
            int numReplaced;
            (line, numReplaced) = SetEscapeCharacters(line, false);

            bool codeBlockStarting = false;
            if (blockStartedPreviously == false)
            {
                // the whole code block has a text background color, and must be padded for the lines to end evenly
                int longestLine = CheckMaxLineLength(lines, i);
                currentPaddingWidth = Math.Max(longestLine, CodeBlockPaddingWidth) + 3;
                codeBlockStarting = true;
            }
            
            if (codeBlockStarting)
            {
                //insert a blank line if it's the start of a block
                text.Append(CodeblockLine("\t", currentPaddingWidth));
            }

            // count TABs in line as more characters than normal
            int tabCount = line.AllIndexesOf("\t").Count() - 1;
            // instert the actual text
            line = CodeblockLine(line, currentPaddingWidth + numReplaced - (tabCount * tabLength));
            text.Append(line);
        }

        private string EscapeNonStyleTags(string line, char[] tagChars)
        {
            foreach (char tagChar in tagChars)
            {
                if (!line.Contains(tagChar)) continue;
                //Debug.WriteLine("EscapleNonStylTags, line:" + line);
                string nonTag = String.Concat(Enumerable.Repeat(tagChar, 3));
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

        private string CodeblockLine(string line, int padding)
        {
            StringBuilder sb = new();
            sb.Append(UseFontColor(rtfCodeFontColor, "Code block"));
            sb.Append(@"\f1 ");
            sb.Append(rtfCodeBackgroundColor.asBackgroundColor());
            sb.Append(line.PadRight(padding));
            sb.Append("\\highlight0 ");
            sb.Append(@"\f0 ");
            sb.Append(UseFontColor(rtfTextColor, "Code Block"));
            sb.AppendLine("\\par ");
            return sb.ToString();
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
                if (c <= 0x7f) // if ((int)c <= 127)
                    sb.Append(c);
                else
                {
                    //string converted = "\\u" + Convert.ToUInt32(c) + "?";
                    string converted = "\\u" + (int)c + "?";
                    sb.Append(converted);
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

        private (string line, int i) CreateTable(int i, List<string> lines, List<int> columSizes)
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

            if (tableRows > 2) // if there are at least two lines starting with |, it's treated as a table
            {
                for (int r = i; r < i + tableRows; r++)// string line in lines)
                {
                    int lastColumnWidth = 0;
                    if (r == i + 1) continue; // skip row with dashes that separates headings from rows
                    result.AppendLine("\\trowd\\trgaph150");
                    for (int c = 0; c < columns; c++)
                    {
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
                        colWord = SetStyle(colWord, "*", "i"); // italic
                        if (AllowUnderscoreBold)
                        {
                            colWord = SetStyle(colWord, "__", "b"); // bold
                        }
                        if (AllowUnderscoreItalic)
                        {
                            colWord = SetStyle(colWord, "_", "i"); // italic
                        }

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

        

        private static string SetStyle(string line, string tag, string rtfTag)
        {
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

        bool LineIsHeading = false;
        private string SetHeading(int[] textSizes, string line)
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
                    LineIsHeading = false;
                    return line;
                }
                else
                {
                    sb.Append(UseFontColor(rtfHeadingColor, "Heading")); // set heading color
                    string headingSizeText = $"\\fs{textSizes[headingSize]} ";
                    sb.Append(headingSizeText); // set heading size
                    int trimStart = headingSize;
                    if (line.Substring(headingSize, Math.Min(1, line.Length-headingSize)) == " ")
                    {
                        // remove the first space after heading indicator
                        trimStart++;
                    }
                    sb.Append(line.AsSpan(trimStart));
                    //sb.Append(lineEnd);
                    
                    sb.Append($"\\fs{textSizes[0]}"); // set normal size
                    sb.Append(UseFontColor(rtfTextColor, "Heading")); // set normal color
                    line = sb.ToString();
                    LineIsHeading = true;
                    return line;
                }
            }
            LineIsHeading = false;
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
