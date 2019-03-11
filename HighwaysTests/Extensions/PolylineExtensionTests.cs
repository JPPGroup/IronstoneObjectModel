using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Extensions
{
    [TestFixture(@"..\..\..\Drawings\PolylineExTests1.dwg", 5)]
    [TestFixture(@"..\..\..\Drawings\PolylineExTests2.dwg", 1)]
    [TestFixture(@"..\..\..\Drawings\PolylineExTests3.dwg", 2)]
    [TestFixture(@"..\..\..\Drawings\PolylineExTests4.dwg", 6)]
    public class PolylineExtensionTests : BaseNUnitTestFixture
    {
        private readonly int _polyLineSegments;

        public PolylineExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(PolylineExtensionTests)) { }
        public PolylineExtensionTests(string drawingFile, int polyLineSegments) : base(Assembly.GetExecutingAssembly(), typeof(PolylineExtensionTests), drawingFile)
        {
            _polyLineSegments = polyLineSegments;
        }
       
        [Test]
        public void VerifyExplodeAndErase()
        {
            var result = RunTest<int>("VerifyExplodeAndEraseResident");
            Assert.AreEqual(_polyLineSegments, result, "Incorrect number of segments from polyline.");
        }

        public int VerifyExplodeAndEraseResident()
        {
            var dwg = Application.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return 0;
            if (res.Value == null || res.Value.Count != 1) return 0;

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var pLine = acTrans.GetObject(res.Value[0].ObjectId, OpenMode.ForWrite) as Polyline;
                return pLine.ExplodeAndErase().Count;
            }
        }
    }
}
