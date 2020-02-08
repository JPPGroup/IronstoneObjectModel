using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class LayoutSheet
    {
        public string Name { get; private set; }

        public TitleBlock TitleBlock { get; private set; }

        private Layout _layout;

        public ObjectId LayoutID { get; private set; }

        public PaperSize Size { get; private set; }
        private ILogger _logger;

        public DrawingArea DrawingArea { get; private set; }
        public NoteArea NoteArea { get; private set; }

        public LayoutSheet(ILogger logger, Layout layout)
        {
            _layout = layout;
            LayoutID = layout.Id;
            Name = layout.LayoutName;
            SetSize(_layout.CanonicalMediaName);

            FindTitleBlock();
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

        public void Plot(string fileName, PlotEngine pe, PlotProgressDialog ppd)
        {
            Transaction trans = LayoutID.Database.TransactionManager.TopTransaction;
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
            var devices = psv.GetPlotDeviceList();
            psv.RefreshLists(ps);
            var media = psv.GetCanonicalMediaNameList(ps);*/

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

        private string GetMediaName()
        {
            // TODO: Add other paper sizes
            switch (Size)
            {
                case PaperSize.A1Landscape:
                    return "ISO_expand_A1(841.00_x_594.00_MM)";
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

        private void FindTitleBlock()
        {
            Transaction acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(_layout.BlockTableRecordId, OpenMode.ForRead);

            var blocks = _layout.GetBlockReferences().Select(br => new BlockRefDrawingObject(br));
            blocks = blocks.Where(br => br.BlockName == GetTitleBlockName());

            if (blocks.Count() != 1)
            {
                _logger.Entry("No title block found for sheet");
                return;
            }

            TitleBlock = new TitleBlock(blocks.ElementAt(0));
        }
    }

    public enum PaperSize
    {
        A0Landscape,
        A1Landscape,
        A2Landscape,
        A3Landscape,
        A0Portrait,
        A1Portrait,
        A2Portrait,
        A3Portrait,
        A4Portrait
    }
}
