# Markdown to RTF

Convert *Markdown* (.md) files to *RTF* Rich text files.

- Use as a dependency to display Markdown files in a Rich Text field in a C# Winforms application [MarkdownToRTF](https://github.com/snjo/MarkdownToRtf/tree/master/MarkdownToRtf)
- Use the viewer/editor in [MarkdownViewer](https://github.com/snjo/MarkdownToRtf/tree/master/MarkdownViewer)

## Supported markdown

- Images using !\[\title]\(URL\)
- Links using \[\title]\(\URL)
- Define custom text colors for normal text, headings, code and list prefixes
- Define custom fonts for text and code
- Define custom point size for text and headings
- Options for text output when the parser fails
- Options to disable ordered lists
- Options to disable underscores as font style tags
- Escapes special characters like \* \** \{ \} \\
- Removes comments <\!-- to -->
- Unicode character support

## Unsupported markdown (for now)

- links using \<URL\>
- Quote blocks
- HTML tags
- Lists inside lists (i. ii. iii. inside 1. or - )

## Images

Images will *only* display if the RichTextField you're updating has **ReadOnly** set to **False**.  
You can re-enable ReadOnly after the RichTextBox.Rtf value has been set.

### Custom code inside a Markdown document

Table widths can be defined using <\!---CW:2500:2000:1000:-->
