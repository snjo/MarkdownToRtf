# Markdown to RTF

Convert *Markdown* (.md) files to *RTF* files.

## Supported markdown

- Unordered lists
- ...

1.  Ordered lists
1.  ...
1.  ...

Text styles, **bold** and *italic*

#### Headings H1 to H6

    Code blocks
    No formatting is done to the contents

<!---CW:2500:2000:1000:-->
| **Tables** | **Lorem** | *Ipsum* |
|------------|-----------|---------|
| values     | etc       | **etc** |

Table widths can be defined using <\!---CW:2500:2000:1000:-->

- Escape special characters like \* \** \{ \} \\
- Limited Unicode character support
- Removes image tags (including images not yet supported)
- Removes comments <\!-- to -->
- Define custom text colors for normal text, headings, code and list prefixes
- Define custom fonts for text and code
- Define custom point size for text and headings
- Options for text output when the parser fails
- Options to disable ordered lists
- Options to disable underscores as font style tags

------------------------------------------------------

## Edge case tests
*If the parser works correctly, these are output as expected*

an orphan ** tag
normal text
ending orphaned style tag **
normal text
** at the start

Text **bold** and *italic* here
**bold alone**
normal
text **bold end**
normal text
**bold** start
two **bold** things end **boldly**
more **bold** things **boldly** go

*italic* start
end *italic*
middle *italic* text
two *italic* and *italic*
more *italic* and *italic* ending
*italic* and *italic* at start

orphaned asterisk *italic* text * hmmm
orphaned asterisk **bold** text ** hmmm

# Heading H1
## Heading H2
### Heading H1
#### Heading H4
##### Heading H5
###### Heading H6

00)  Ordered lists.
00)  More digits in number
10)  causes extra padding
04)  ...
99)  ...
10)  ...
00)  ...
10)  ...
10)  ...
10)  ...
10)  ...
10)  ...

1  Not an ordered list (no . or ) at then end of the number)
1  ...
1  ...
