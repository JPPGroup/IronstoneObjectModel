using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class ExcelBook
    {
        protected string ReadCellValue(Cell cell, SharedStringTablePart stringTable)
        {
            int? intValue = ReadNumeric(cell);

            if (intValue.HasValue)
            {
                return intValue.Value.ToString();
            }

            InlineString inlineString = cell.InlineString;

            return ReadString(cell, stringTable);
        }

        private string ReadString(Cell cell, SharedStringTablePart stringTable)
        {
            if (cell.InnerText.Length > 0)
            {
                int value = int.Parse(cell.InnerText);

                if (cell.DataType?.Value == CellValues.SharedString)
                {
                    return stringTable.SharedStringTable
                        .ElementAt(value).InnerText;
                }
            }

            return null;
        }

        private int? ReadNumeric(Cell cell)
        {
            if (cell.DataType == null && cell.InnerText.Length > 0)
            {
                return int.Parse(cell.InnerText);
            }

            return null;
        }

        protected void WriteString(Cell cell, SharedStringTable sharedStringTable, string s)
        {
            cell.CellValue = new CellValue(GetStringReference(s, sharedStringTable));
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
        }

        private string GetStringReference(string s, SharedStringTable sharedStringTable)
        {
            int i = 0;
            foreach (SharedStringItem item in sharedStringTable)
            {
                if (item.InnerText == s)
                    return i.ToString();

                i++;
            }

            sharedStringTable.AppendChild(new SharedStringItem(new Text(s)));
            sharedStringTable.Save();

            return i.ToString();
        }
    }
}
