# Markdown to RTF

Convert *Markdown* (.md) files to *RTF* Rich text files.

- Use as a dependency to display Markdown files in a Rich Text field in a C# Winforms application [MarkdownToRTF](https://github.com/snjo/MarkdownToRtf/tree/master/MarkdownToRtf)
- Use the viewer/editor in [MarkdownViewer](https://github.com/snjo/MarkdownToRtf/tree/master/MarkdownViewer)

## Supported markdown

- Escape special characters like \* \** \{ \} \\
- Limited Unicode character support
- Removes comments <\!-- to -->
- Define custom text colors for normal text, headings, code and list prefixes
- Define custom fonts for text and code
- Define custom point size for text and headings
- Options for text output when the parser fails
- Options to disable ordered lists
- Options to disable underscores as font style tags

## Unsupported markdown (for now)

- Images
- Links (various kinds)
- Quote blocks
- HTML tags

### Custom code inside a Markdown document

Table widths can be defined using <\!---CW:2500:2000:1000:-->
