using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MarkdownToRtf
{
    public class RtfColorInfo
    {
        public Color Color;
        public int ColorTableNumber;

        public RtfColorInfo(Color color, int colorTableNumber)
        {
            this.Color = color;
            this.ColorTableNumber = colorTableNumber;
        }
        

        public string asFontColor()
        {
            return $"\\cf{ColorTableNumber} ";
        }

        public string asBackgroundColor()
        {
            return $"\\highlight{ColorTableNumber} ";
        }
    }
}
