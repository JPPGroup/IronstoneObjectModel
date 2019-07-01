using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Drainage.ObjectModel.Extensions
{
    //MOVE: To Core...
    public static class TableExtension
    {
        public static void SetTableRow(this Table tb, string header, string contents, int row)
        {
            tb.Cells[row, 0].TextHeight = 40;
            tb.Cells[row, 0].TextString = header;
            tb.Cells[row, 0].Alignment = CellAlignment.MiddleCenter;

            tb.Cells[row, 1].TextHeight = 40;
            tb.Cells[row, 1].TextString = contents;
            tb.Cells[row, 1].Alignment = CellAlignment.MiddleCenter;
        }
    }
}
