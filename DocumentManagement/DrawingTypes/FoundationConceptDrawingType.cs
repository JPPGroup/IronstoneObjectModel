using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel.DrawingTypes
{
    public class FoundationConceptDrawingType : AbstractDrawingType
    {
        private ILogger<CoreExtensionApplication> _logger;
        private IConfiguration _settings;

        public FoundationConceptDrawingType(ILogger<CoreExtensionApplication> logger, IConfiguration settings)
        {
            _logger = logger;
            _settings = settings;

            DefaultFilename = "000P1 - Preliminary Foundation Assessment.dwg";
        }

        public override void Initialise(Document doc)
        {
            Application.DocumentManager.MdiActiveDocument = doc;
            using (DocumentLock issueDocumentLock = doc.LockDocument())
            using(Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayoutSheetController controller = new LayoutSheetController(_logger, doc.Database, _settings);
                LayoutSheet newSheet = controller.AddLayout("000 - PFA", PaperSize.A1Landscape);
                newSheet.TitleBlock.DrawingNumber = "000";
                newSheet.TitleBlock.Revision = "P1";
                newSheet.TitleBlock.ProjectNumber = ParentController.ProjectNumber;
                newSheet.TitleBlock.Project = ParentController.ProjectName;
                newSheet.TitleBlock.Client = ParentController.Client;
                newSheet.TitleBlock.Title = "Preliminary Foundation Assessment";
                newSheet.TitleBlock.DrawnBy = "DRA";
                newSheet.TitleBlock.Date = DateTime.Now.ToString("MMMM yy");

                ViewportDrawingObject viewport = newSheet.DrawingArea.AddFullViewport();

                BlockReference conceptFoundations = this.ParentController.GetXref<FoundationXrefDrawingType>().AddToDrawingAsXref(doc);
                viewport.FocusOn(_settings, conceptFoundations.GeometricExtents);
                controller.RemoveDefaultLayouts();

                trans.Commit();
            }
        }
    }
}
