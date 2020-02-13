using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Jpp.Ironstone.Core.Autocad;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class LayoutSheet
    {
        public string DrawingNumber { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public string JobNumber { get; set; }

        private Layout _layout;

        public ObjectId LayoutID { get; set; }

        public LayoutSheet()
        {
            Layout _layout = Application.DocumentManager.MdiActiveDocument.Database.GetLayout(Name);

            ParseTitleBlock();
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
            return "ISO_expand_A1(841.00_x_594.00_MM)";
        }

        private void ParseTitleBlock()
        {
            Transaction acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.TopTransaction;
            
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(_layout.BlockTableRecordId, OpenMode.ForRead);

            foreach (ObjectId objectId in btr)
            {
                Entity ent = (Entity)acTrans.GetObject(objectId, OpenMode.ForRead);
                if (ent.GetType() == typeof(BlockReference))
                {
                    BlockReference blkRef = (BlockReference)ent;

                    string name = blkRef.Name;
                    bool dynamic = blkRef.IsDynamicBlock;

                    AttributeCollection attCol = blkRef.AttributeCollection;

                    foreach (ObjectId objID in attCol)
                    {
                        DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;
                        AttributeReference acAttRef = dbObj as AttributeReference;
                    }

                }
            }
        }
    }
}
