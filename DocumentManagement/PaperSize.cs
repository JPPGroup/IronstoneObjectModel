using System.ComponentModel;
using Jpp.Ironstone.Core;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PaperSize
    {
        [Description("A0 Landscape")]
        A0Landscape,
        [Description("A1 Landscape")]
        A1Landscape,
        [Description("A2 Landscape")]
        A2Landscape,
        [Description("A3 Landscape")]
        A3Landscape,
        [Description("A0 Portrait")]
        A0Portrait,
        [Description("A1 Portrait")]
        A1Portrait,
        [Description("A2 Portrait")]
        A2Portrait,
        [Description("A3 Portrait")]
        A3Portrait,
        [Description("A4 Portrait")]
        A4Portrait
    }
}
