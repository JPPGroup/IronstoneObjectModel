using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class LayoutSheet
    {
        public string Name { get; private set; }

        public TitleBlock TitleBlock { get; private set; }

        public StatusBlock StatusBlock { get; private set; }

        public IReadOnlyList<RevisionBlock> RevisionBlocks { get { return _revisionBlocks; } }
        private List<RevisionBlock> _revisionBlocks = new List<RevisionBlock>();

        private Layout _layout;

        public ObjectId LayoutID { get; private set; }

        public PaperSize Size { get; private set; }
        private ILogger<CoreExtensionApplication> _logger;

        public DrawingArea DrawingArea { get; private set; }
        public NoteArea NoteArea { get; private set; }
        
        public LayoutSheet(ILogger<CoreExtensionApplication> logger, Layout layout, bool JPPLayout = true)
        {
            _layout = layout;
            LayoutID = layout.Id;
            Name = layout.LayoutName;

            if (JPPLayout)
            {
                SetSize(_layout.CanonicalMediaName);
                FindTitleBlock();
                FindStatusBlock();
                FindRevisionBlocks();
            }
        }

        private void SetSize(string canonicalMediaName)
        {
            switch (canonicalMediaName)
            {
                case "User2059":
                    Size = PaperSize.A0Landscape;
                    DrawingArea = new DrawingArea(_layout, 60, 810, 15, 880);
                    NoteArea = new NoteArea(60, 810, 880, 1168);
                    return;

                case "User3082":
                    Size = PaperSize.A1Landscape;
                    DrawingArea = new DrawingArea(_layout, 74.434, 579.435, 15.403, 564.653);
                    NoteArea  = new NoteArea(74.434, 579.435, 564.653, 825.403);
                    return;

                case "User1090":
                    Size = PaperSize.A2Landscape;
                    DrawingArea = new DrawingArea(_layout, 23.278, 386.309, 0.201, 419.701);
                    NoteArea = new NoteArea(3.278, 386.309, 419.701, 563.701);
                    return;

                case "A3":
                    Size = PaperSize.A3Landscape;
                    DrawingArea = new DrawingArea(_layout, 30, 276.85,7.5,258);
                    NoteArea = new NoteArea(30, 276.85, 258, 401.95);
                    return;
            }

            throw new NotImplementedException();
        }

        public string GetPDFName()
        {
            return $"{TitleBlock.ProjectNumber} - {TitleBlock.DrawingNumber}{TitleBlock.Revision} - {TitleBlock.Title}.pdf";
        }

        public void Plot(string fileName, PlotEngine pe, PlotProgressDialog ppd)
        {
            using (Transaction trans = LayoutID.Database.TransactionManager.StartTransaction())
            {
                Layout layout = (Layout) trans.GetObject(LayoutID, OpenMode.ForRead);
                LayoutManager.Current.CurrentLayout = layout.LayoutName;

                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = LayoutID;

                // Set plot settings
                PlotSettings ps = new PlotSettings(layout.ModelType);
                ps.CopyFrom(layout);

                PlotSettingsValidator psv = PlotSettingsValidator.Current;
                psv.SetPlotType(ps, Autodesk.AutoCAD.DatabaseServices.PlotType.Layout);
                psv.SetUseStandardScale(ps, true);
                psv.SetStdScaleType(ps, StdScaleType.StdScale1To1);

                /*PlotConfig pConfig = PlotConfigManager.SetCurrentConfig("DWG To PDF.pc3");
                var devices = psv.GetPlotDeviceList();*/
                psv.SetPlotConfigurationName(ps, "DWG To PDF.pc3", null);
                psv.RefreshLists(ps);
                var media = psv.GetCanonicalMediaNameList(ps);

                psv.SetPlotConfigurationName(ps, "DWG To PDF.pc3", GetMediaName());

                plotInfo.OverrideSettings = ps;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                piv.Validate(plotInfo);

                pe.BeginDocument(plotInfo, fileName, null, 1, true, fileName);

                ppd.OnBeginSheet();
                ppd.LowerSheetProgressRange = 0;
                ppd.UpperSheetProgressRange = 100;
                ppd.SheetProgressPos = 0;

                PlotPageInfo ppi = new PlotPageInfo();
                pe.BeginPage(ppi, plotInfo, true, null);
                pe.BeginGenerateGraphics(null);
                pe.EndGenerateGraphics(null);

                pe.EndPage(null);
                ppd.SheetProgressPos = 100;
                ppd.OnEndSheet();

                pe.EndDocument(null);
            }
        }

        private string GetMediaName()
        {
            // TODO: Add other paper sizes
            switch (Size)
            {
                case PaperSize.A1Landscape:
                    return "ISO_expand_A1_(841.00_x_594.00_MM)";
            }
            
            throw new ArgumentOutOfRangeException();
        }

        private string GetTitleBlockName()
        {
            switch (Size)
            {
                case PaperSize.A0Landscape:
                    return "A0 Mask";

                case PaperSize.A1Landscape:
                    return "A1 Mask";

                case PaperSize.A2Landscape:
                    return "A2 Mask";

                case PaperSize.A3Landscape:
                    return "A3 Mask";

                case PaperSize.A0Portrait:
                    return "A0mask (portrait)";

                case PaperSize.A1Portrait:
                    return "A1mask (portrait)";

                case PaperSize.A2Portrait:
                    return "A2mask (portrait)";

                case PaperSize.A3Portrait:
                    return "A3mask (portrait)";

                case PaperSize.A4Portrait:
                    return "A4 Mask";
            }

            throw new ArgumentOutOfRangeException();
        }

        private string GetRevisioneBlockName()
        {
            switch (Size)
            {
                /*case PaperSize.A0Landscape:
                    return "A0 Mask";

                case PaperSize.A1Landscape:
                    return "A1revisionblock";

                case PaperSize.A2Landscape:
                    return "A2 Mask";

                case PaperSize.A3Landscape:
                    return "A3 Mask";

                case PaperSize.A0Portrait:
                    return "A0mask (portrait)";

                case PaperSize.A1Portrait:
                    return "A1mask (portrait)";

                case PaperSize.A2Portrait:
                    return "A2mask (portrait)";

                case PaperSize.A3Portrait:
                    return "A3mask (portrait)";

                case PaperSize.A4Portrait:
                    return "A4 Mask";*/

                case PaperSize.A1Landscape:
                case PaperSize.A1Portrait:
                    return "A1revisionblock";
            }

            throw new ArgumentOutOfRangeException();
        }

        private void FindTitleBlock()
        {
            Transaction acTrans = _layout.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(_layout.BlockTableRecordId, OpenMode.ForRead);

            var blocks = _layout.GetBlockReferences().Select(br => new BlockRefDrawingObject(_layout.Database, br));
            blocks = blocks.Where(br => br.BlockName == GetTitleBlockName());

            if (blocks.Count() != 1)
            {
                _logger.LogWarning("No title block found for sheet");
                return;
            }

            TitleBlock = new TitleBlock(blocks.ElementAt(0));
        }

        private void FindStatusBlock()
        {
            Transaction acTrans = _layout.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(_layout.BlockTableRecordId, OpenMode.ForRead);

            var blocks = _layout.GetBlockReferences().Select(br => new BlockRefDrawingObject(_layout.Database, br));
            blocks = blocks.Where(br => br.BlockName == "STATUS");

            if (blocks.Count() != 1)
            {
                _logger.LogWarning("No status block found for sheet");
                return;
            }

            StatusBlock = new StatusBlock(blocks.ElementAt(0));
        }

        private void FindRevisionBlocks()
        {
            _revisionBlocks = new List<RevisionBlock>();

            Transaction acTrans = _layout.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(_layout.BlockTableRecordId, OpenMode.ForRead);

            var blocks = _layout.GetBlockReferences().Select(br => new BlockRefDrawingObject(_layout.Database, br));
            blocks = blocks.Where(br => br.BlockName == GetRevisioneBlockName());

            if (blocks.Count() == 0)
            {
                _logger.LogWarning("No revision blocks found for sheet");
                return;
            }

            foreach(var block in blocks)
            {
                _revisionBlocks.Add(new RevisionBlock(block));
            }            
        }

        public RevisionBlock AddRevision(string revision, string description, string drawn, string checker, string date)
        {
            const float Height = 5;
            Point3d insertionPoint = new Point3d(NoteArea.Left, NoteArea.Bottom + RevisionBlocks.Count() * Height, 0);
            var newBlock = RevisionBlock.Create(_layout.Database, insertionPoint, GetRevisioneBlockName());
            newBlock.Revision = revision;
            newBlock.Description = description;
            newBlock.DrawnBy = drawn;
            newBlock.CheckedBy = checker;
            newBlock.Date = date;

            return newBlock;
        }
    }
}
