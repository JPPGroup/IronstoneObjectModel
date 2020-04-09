using System;
using System.Drawing;
using Color = Autodesk.AutoCAD.Colors.Color;

namespace Jpp.Ironstone.Structures.ObjectModel
{
    [Serializable]
    public class DepthBand
    {
        public double StartDepth { get; set; }
        public double EndDepth { get; set; }
        
        public Color Color {
            get { return Autodesk.AutoCAD.Colors.Color.FromColor(ColorTranslator.FromHtml(HexColor)); }
            set { HexColor = ColorTranslator.ToHtml(value.ColorValue); }
        }

        public string HexColor { get; set; }
    }
}
