using System;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Housing.ObjectModel.Concept;
using Jpp.Ironstone.Structures.ObjectModel;
using NUnit.Framework;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Housing.ObjectModel.Tests.Concept
{
    [TestFixture]
    class ConceptualPlotCivil3DTests : IronstoneCivilTestFixture
    {
        public ConceptualPlotCivil3DTests() : base(Assembly.GetExecutingAssembly(), typeof(ConceptualPlotCivil3DTests),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Concept\\ConceptPreppedDrawing.dwg"))
        {
        }
        
        [TestCase("Ex Ground", "Prop Ground", -0.90)]
        [TestCase("Ex Ground", "No Ground", 0.01)]
        public void EstimateFoundations(string exGround, string propGround, double expectedResult)
        {
            FoundationInput input = new FoundationInput()
            {
                ExistingGround = exGround,
                ProposedGround = propGround
            };
            FoundationLevels levels = RunTest<FoundationLevels>(nameof(EstimateFoundationsResident), input);
            Assert.AreEqual(expectedResult, levels.FoundationLevel, 0.001);
        }

        public FoundationLevels EstimateFoundationsResident(FoundationInput input)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            ConceptualPlot plot;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DataService.Current.PopulateStoreTypes();
                SoilProperties props = DataService.Current.GetStore<StructureDocumentStore>(doc.Name).SoilProperties;
                props.ExistingGroundSurfaceName = input.ExistingGround;
                props.ProposedGroundSurfaceName = input.ProposedGround;

                ObjectId obj = doc.Database.GetObjectId(false, new Handle(146034), 0);

                DBObject ent = trans.GetObject(obj, OpenMode.ForWrite);

                PolylineDrawingObject polyObject = new PolylineDrawingObject(ent as Polyline);
                plot = new ConceptualPlot(polyObject);

                CivSurface existingGround = GetSurface(props.ExistingGroundSurfaceName);
                CivSurface proposedGround = GetSurface(props.ProposedGroundSurfaceName);
                plot.EstimateFoundationLevel(existingGround, proposedGround, props);

                FoundationLevels result = new FoundationLevels()
                {
                    FoundationLevel = plot.FoundationDepth
                };
                return result;
            }
        }

        private CivSurface GetSurface(string name)
        {
            //Get the target surface
            ObjectIdCollection SurfaceIds = CivilApplication.ActiveDocument.GetSurfaceIds();

            foreach (ObjectId surfaceId in SurfaceIds)
            {
                // Direct cast is safe as collection is filtered down to surfaces by Autocad
                CivSurface temp = (CivSurface)surfaceId.GetObject(OpenMode.ForRead);

                // Continue is not used, incase user has set the same surface as both
                if (temp.Name == name)
                {
                    return temp;
                }
            }

            return null;
        }

        [Serializable]
        public struct FoundationInput
        {
            public string ExistingGround { get; set; }
            public string ProposedGround { get; set; }
        }

        [Serializable]
        public struct FoundationLevels
        {
            public double FoundationLevel { get; set; }
        }
    }
}
