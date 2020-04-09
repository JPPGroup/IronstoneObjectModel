using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Sheet = DocumentFormat.OpenXml.Spreadsheet.Sheet;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class DrawingRegister : ExcelBook
    {
        private const string STRUCTURAL_SHEET = "T11-DR Struct Drawing Regis";
        private const string CIVIL_SHEET = "T11-DR Civils Drawing Regis";

        private const int FIRST_DATA_ROW = 15;
        private const int DRAWING_NUMBER_COLUMN = 0;
        private const int DRAWING_TITLE_COLUMN = 1;
        private const int DRAWING_TYPE_COLUMN = 3;
        private const int DRAWING_CURRENT_ISSUE_COLUMN = 4;
        private const int PAGE_WIDTH = 28; //28 CELLS PER PAGE

        private const int ISSUE_ROW = 12;

        public IEnumerable<DrawingInformation> Drawings
        {
            get { return _drawings; }
        }

        private string _path;

        private List<DrawingInformation> _drawings;
        private List<DateTime> _civilDates, _structuralDates;

        public IReadOnlyCollection<DateTime> CivilDates
        {
            get { return _civilDates; }
        }

        public IReadOnlyCollection<DateTime> StructuralDates
        {
            get { return _structuralDates; }
        }

        public DrawingRegister(string path)
        {
            _path = path;
            _drawings = new List<DrawingInformation>();
            _civilDates = new List<DateTime>();
            _structuralDates = new List<DateTime>();

            if (File.Exists(_path))
            {
                Read();
            }
            else
            {
                /*string sourcePath =
                    Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "DrawingRegister\\T17-1 Scheme Tracker V26.xlsx");
                File.Copy(sourcePath, _path);*/
                using (Stream s = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Jpp.Ironstone.DocumentManagement.ObjectModel.Resources.SchemeTrackerV26.xlsx"))
                {
                    using (FileStream outStream = File.OpenWrite(_path))
                    {
                        // TODO: Add null checks/testing here
                        s.CopyTo(outStream);
                    }
                }
            }
        }

        #region Persistence
        private void Read()
        {
            _drawings.Clear();
            _civilDates.Clear();
            _structuralDates.Clear();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(_path, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheets sheets = wbPart.Workbook.Sheets;

                SharedStringTablePart stringTable = wbPart.GetPartsOfType<SharedStringTablePart>()
                        .FirstOrDefault();

                foreach (Sheet sheet in sheets)
                {
                    if (sheet.Name.Value == CIVIL_SHEET)
                    {
                        ParseSheet(document, sheet, stringTable, DrawingType.Civil);
                    }

                    if (sheet.Name.Value == STRUCTURAL_SHEET)
                    {
                        ParseSheet(document, sheet, stringTable, DrawingType.Structural);
                    }
                }
            }
        }

        public void Write()
        {
            WriteAs(_path);
        }

        public void WriteAs(string path)
        {
            if (path != _path)
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(_path, true))
                {
                    var doc = document.SaveAs(path);
                    doc.Dispose();
                }

                _path = path;
            }

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(_path, true))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheets sheets = wbPart.Workbook.Sheets;

                SharedStringTablePart stringTable = wbPart.GetPartsOfType<SharedStringTablePart>()
                    .FirstOrDefault();

                foreach (Sheet sheet in sheets)
                {
                    if (sheet.Name.Value == CIVIL_SHEET)
                    {
                        WriteDrawings(document, sheet, stringTable, DrawingType.Civil);
                    }

                    if (sheet.Name.Value == STRUCTURAL_SHEET)
                    {
                        WriteDrawings(document, sheet, stringTable, DrawingType.Structural);
                    }
                }

                document.Save();
            }
        }
        #endregion

        // Reads a drawing register sheet
        // Data rows are assumed to be continuous with no gaps
        private void ParseSheet(SpreadsheetDocument doc, Sheet s, SharedStringTablePart stringTable, DrawingType drawingType)
        {
            string relationshipId = s.Id.Value;
            WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(relationshipId);
            Worksheet worksheet = worksheetPart.Worksheet;

            switch (drawingType)
            {
                case DrawingType.Civil:
                    _civilDates = GetIssueDates(worksheet, stringTable);
                    break;
                
                case DrawingType.Structural:
                    _structuralDates = GetIssueDates(worksheet, stringTable);
                    break;
            }

            bool rowHasData = true;
            for (int i = FIRST_DATA_ROW; rowHasData; i++)
            {
                Row row = worksheet.GetFirstChild<SheetData>()
                    .Elements<Row>().First(r => r.RowIndex == i);

                rowHasData = ReadRowAsDrawing(row, stringTable, drawingType);
            }
        }

        private List<DateTime> GetIssueDates(Worksheet worksheet, SharedStringTablePart stringTable)
        {
            var temp = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>();
            Row day = temp.First(r => r.RowIndex == ISSUE_ROW);

            Row month = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>().First(r => r.RowIndex == ISSUE_ROW + 1);

            Row year = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>().First(r => r.RowIndex == ISSUE_ROW + 2);
            
            List<DateTime> dates = new List<DateTime>();

            bool cellHasData = true;
            for (int i = 0; cellHasData; i++)
            {
                int column = DateIndexToColumn(i);

                Cell dayCell = day.Elements<Cell>().ElementAt(column);
                Cell monthCell = month.Elements<Cell>().ElementAt(column);
                Cell yearCell = year.Elements<Cell>().ElementAt(column);

                string dayString = ReadCellValue(dayCell, stringTable);
                string monthString = ReadCellValue(monthCell, stringTable);
                string yearString = $"20{ReadCellValue(yearCell, stringTable)}";

                if (string.IsNullOrWhiteSpace(dayString) || string.IsNullOrWhiteSpace(monthString) ||
                    string.IsNullOrWhiteSpace(yearString))
                {
                    cellHasData = false;
                    break;
                }

                int dayNumber = int.Parse(dayString);
                int monthNumber = int.Parse(monthString);
                int yearNumber = int.Parse(yearString);

                dates.Add(new DateTime(yearNumber, monthNumber, dayNumber));
            }

            return dates;
        }

        private int DateIndexToColumn(int index)
        {
            int datesPerPage = PAGE_WIDTH - (DRAWING_CURRENT_ISSUE_COLUMN + 1);
            int pageNumber = index / datesPerPage;
            int pageOffset = index % datesPerPage;

            return pageNumber * PAGE_WIDTH + (DRAWING_CURRENT_ISSUE_COLUMN + 1) + pageOffset;
        }

        // Reads a row to extract the information
        private bool ReadRowAsDrawing(Row row, SharedStringTablePart stringTable, DrawingType drawingType)
        {
            Cell drawingNumberCell = row.Elements<Cell>().ElementAt(DRAWING_NUMBER_COLUMN);

            string drawingNumber = ReadCellValue(drawingNumberCell, stringTable);

            if (string.IsNullOrWhiteSpace(drawingNumber))
                return false;

            Cell drawingTitleCell = row.Elements<Cell>().ElementAt(DRAWING_TITLE_COLUMN);
            Cell issueTypeCell = row.Elements<Cell>().ElementAt(DRAWING_TYPE_COLUMN);
            Cell currentIssueCell = row.Elements<Cell>().ElementAt(DRAWING_CURRENT_ISSUE_COLUMN);

            string drawingTitle = ReadCellValue(drawingTitleCell, stringTable);
            string issueType = ReadCellValue(issueTypeCell, stringTable);
            string currentIssue = ReadCellValue(currentIssueCell, stringTable);

            DrawingInformation dwgInfo = new DrawingInformation()
            {
                DrawingNumber = drawingNumber,
                DrawingTitle = drawingTitle,
                IssueType = issueType,
                CurrentIssue = currentIssue,
                Type = drawingType
            };

            List<DateTime> dates = null;

            switch (drawingType)
            {
                case DrawingType.Civil:
                    dates = _civilDates;
                    break;
                
                case DrawingType.Structural:
                    dates = _structuralDates;
                    break;
            }

            for (int i = 0; i < dates.Count; i++)
            {
                Cell revisionCell = row.Elements<Cell>().ElementAt(DateIndexToColumn(i));
                dwgInfo.Revisions.Add(ReadCellValue(revisionCell, stringTable));
            }

            _drawings.Add(dwgInfo);
            return true;
        }

        public void WriteLayoutSheet(LayoutSheet sheet, DrawingType type)
        {
            DrawingInformation drawingInformation = _drawings.FirstOrDefault(di => di.DrawingNumber == sheet.TitleBlock.DrawingNumber);
            if (drawingInformation == null)
            {
                drawingInformation = new DrawingInformation();
                drawingInformation.DrawingNumber = sheet.TitleBlock.DrawingNumber;
                _drawings.Add(drawingInformation);

                // TODO: Add code here for sorting into a sensible order
            }

            drawingInformation.DrawingTitle = sheet.TitleBlock.Title;
            drawingInformation.Type = type;
            // drawingInformation.IssueType = sheet.TitleBlock. // TODO: This needs to be found
            drawingInformation.CurrentIssue = sheet.TitleBlock.Revision;
        }

        private void WriteDrawings(SpreadsheetDocument doc, Sheet s, SharedStringTablePart stringTable, DrawingType drawingType)
        {
            IEnumerable<DrawingInformation> drawings = _drawings.Where(di => di.Type == drawingType);

            string relationshipId = s.Id.Value;
            WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(relationshipId);
            Worksheet worksheet = worksheetPart.Worksheet;

            List<DateTime> dates = null;
            switch (drawingType)
            {
                case DrawingType.Civil:
                    dates = _civilDates;
                    break;
                
                case DrawingType.Structural:
                    dates = _structuralDates;
                    break;
            }

            //Write issue information
            WriteIssueDates(worksheet, stringTable.SharedStringTable, dates);

            for (int i = 0; i < drawings.Count(); i++)
            {
                Row row = worksheet.GetFirstChild<SheetData>()
                    .Elements<Row>().First(r => r.RowIndex == i + 15);

                WriteDrawingInformation(drawings.ElementAt(i), row, stringTable.SharedStringTable, dates);
            }

            worksheet.Save();
        }

        // TODO: Check for overwrites/ordering etc.
        private void WriteIssueDates(Worksheet worksheet, SharedStringTable sharedStringTable, List<DateTime> dates)
        {
            Row day = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>().First(r => r.RowIndex == ISSUE_ROW);

            Row month = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>().First(r => r.RowIndex == ISSUE_ROW + 1);

            Row year = worksheet.GetFirstChild<SheetData>()
                .Elements<Row>().First(r => r.RowIndex == ISSUE_ROW + 2);
            
            for (int i = 0; i < dates.Count; i++)
            {
                int column = DateIndexToColumn(i);
                DateTime issue = dates[i];

                Cell dayCell = day.Elements<Cell>().ElementAt(column);
                Cell monthCell = month.Elements<Cell>().ElementAt(column);
                Cell yearCell = year.Elements<Cell>().ElementAt(column);

                WriteString(dayCell, sharedStringTable, issue.Day.ToString());
                WriteString(monthCell, sharedStringTable, issue.Month.ToString());
                WriteString(yearCell, sharedStringTable, issue.ToString("yy"));
            }
        }

        private void WriteDrawingInformation(DrawingInformation di, Row row, SharedStringTable sharedStringTable, List<DateTime> dates)
        {
            Cell drawingNumberCell = row.Elements<Cell>().ElementAt(DRAWING_NUMBER_COLUMN);
            Cell drawingTitleCell = row.Elements<Cell>().ElementAt(DRAWING_TITLE_COLUMN);
            Cell issueTypeCell = row.Elements<Cell>().ElementAt(DRAWING_TYPE_COLUMN);
            Cell currentIssueCell = row.Elements<Cell>().ElementAt(DRAWING_CURRENT_ISSUE_COLUMN);

            WriteString(drawingNumberCell, sharedStringTable, di.DrawingNumber);
            WriteString(drawingTitleCell, sharedStringTable, di.DrawingTitle);
            WriteString(issueTypeCell, sharedStringTable, di.IssueType);
            WriteString(currentIssueCell, sharedStringTable, di.CurrentIssue);

            for (int i = 0; i < dates.Count; i++)
            {
                int column = DateIndexToColumn(i);

                Cell issueCell = row.Elements<Cell>().ElementAt(column);
                WriteString(issueCell, sharedStringTable, di.Revisions[i]);
            }
        }
    }
}

