using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Objects;
using Jpp.Ironstone.Highways.ObjectModel.Tests.Response;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    [TestFixture(@"..\..\..\Drawings\NetworkTests1.dwg", 49, 11, 10, 6, 4 )]
    [TestFixture(@"..\..\..\Drawings\NetworkTests2.dwg", 102, 10, 11, 3, 8)]
    [TestFixture(@"..\..\..\Drawings\NetworkTests3.dwg", 131, 30, 41, 15, 26)]
    [TestFixture(@"..\..\..\Drawings\NetworkTests4.dwg", 0, 0, 0, 0, 0)]
    public class HighwayManagerTests : BaseNUnitTestFixture
    {
        private readonly int _centreLines;
        private readonly int _roads;
        private readonly int _junctions;
        private readonly int _rightTurn;
        private readonly int _leftTurn;

        public HighwayManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(HighwayManagerTests)) { }
        public HighwayManagerTests(string drawingFile, int centreLines, int roads, int junctions, int rightTurn, int leftTurn) : base(Assembly.GetExecutingAssembly(), typeof(HighwayManagerTests), drawingFile)
        {
            _centreLines = centreLines;
            _roads = roads;
            _junctions = junctions;
            _rightTurn = rightTurn;
            _leftTurn = leftTurn;
        }

        [Test]
        public void VerifyInitialiseHighwayManager()
        {
            var result = RunTest<HighwayManagerProperties>("VerifyInitialiseHighwayManagerResident");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(_centreLines, result.CentreLineCount, "Incorrect number of centre lines.");
                Assert.AreEqual(_roads, result.RoadCount, "Incorrect number of roads built.");
                Assert.AreEqual(_junctions, result.JunctionCount, "Incorrect number of junctions built.");
                Assert.AreEqual(_rightTurn, result.JunctionRightCount, "Incorrect number of right hand turns built.");
                Assert.AreEqual(_leftTurn, result.JunctionLeftCount, "Incorrect number of left hand turns junctions built.");
            });
        }

        public HighwayManagerProperties VerifyInitialiseHighwayManagerResident()
        {
            var result = new HighwayManagerProperties();
            var highway = new HighwayManager();
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var ed = acDoc.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return result;
            if (res.Value == null || res.Value.Count == 0) return result;
        
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var centreLines = GetCentreLinesFromSelection(res.Value);                    

                    highway.InitialiseFromCentreLines(centreLines);

                    result.CentreLineCount = highway.CentreLines.Count;
                    result.RoadCount = highway.Roads.Count;
                    result.JunctionCount = highway.Junctions.Count;

                    var rightCount = 0;
                    var leftCount = 0;
                    foreach (var junction in highway.Junctions)
                    {
                        if (junction.Turn == TurnTypes.Right) rightCount++;
                        if (junction.Turn == TurnTypes.Left) leftCount++;
                    }

                    result.JunctionRightCount = leftCount;
                    result.JunctionLeftCount = rightCount;
                }
                catch (Exception)
                {
                    // ignored
                }

                return result;
            }

            
        }

        private static ICollection<CentreLine> GetCentreLinesFromSelection(IEnumerable acSSet)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var centreLines = new List<CentreLine>();
            var acTrans = acCurDb.TransactionManager.TopTransaction;

            foreach (SelectedObject acSsObj in acSSet)
            {
                if (acSsObj == null) continue;

                var acCurve = acTrans.GetObject(acSsObj.ObjectId, OpenMode.ForWrite) as Curve;
                if (acCurve == null) continue;

                if (acCurve is Polyline pLine)
                {
                    foreach (Entity polyObjId in pLine.ExplodeAndErase())
                    {
                        var acPolyCurve = acTrans.GetObject(polyObjId.Id, OpenMode.ForWrite) as Curve;
                        if (acPolyCurve == null) continue;

                        var centre = new CentreLine {BaseObject = acPolyCurve.Id};
                        centreLines.Add(centre);
                    }
                }
                else
                {
                    var centre = new CentreLine { BaseObject = acCurve.Id };
                    centreLines.Add(centre);
                }
            }
            return centreLines;
        }
    }
}

