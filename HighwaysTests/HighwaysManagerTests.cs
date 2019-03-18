using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Tests.Response;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    [TestFixture(@"..\..\..\Drawings\NetworkTests1.dwg", 49, 11, 10, 4, 6 )]
    [TestFixture(@"..\..\..\Drawings\NetworkTests2.dwg", 102, 10, 11, 8, 3)]
    [TestFixture(@"..\..\..\Drawings\NetworkTests3.dwg", 131, 30, 41, 26, 15)] //Layout will fail invalid due to arc radius...
    [TestFixture(@"..\..\..\Drawings\NetworkTests4.dwg", 0, 0, 0, 0, 0)]
    public class HighwaysManagerTests : IronstoneTestFixture
    {
        private readonly int _centreLines;
        private readonly int _roads;
        private readonly int _junctions;
        private readonly int _rightTurn;
        private readonly int _leftTurn;

        public HighwaysManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(HighwaysManagerTests)) { }
        public HighwaysManagerTests(string drawingFile, int centreLines, int roads, int junctions, int rightTurn, int leftTurn) : base(Assembly.GetExecutingAssembly(), typeof(HighwaysManagerTests), drawingFile)
        {
            _centreLines = centreLines;
            _roads = roads;
            _junctions = junctions;
            _rightTurn = rightTurn;
            _leftTurn = leftTurn;
        }

        [Test]
        public void VerifyManagerLoaded()
        {
            var result = RunTest<bool>(nameof(VerifyManagerLoadedResident));
            Assert.IsTrue(result, "Manager not loaded.");
        }

        public bool VerifyManagerLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = DataService.Current;
                var store = ds.GetStore<HighwaysDocumentStore>(acDoc.Name);
                var manager = store.GetManager<HighwaysManager>();

                return manager != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void VerifyHighwaysManagerInitialLayout()
        {
            var result = RunTest<HighwaysManagerProperties>(nameof(VerifyHighwaysManagerInitialLayoutResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(_centreLines, result.CentreLineCount, "Incorrect number of centre lines.");
                Assert.AreEqual(_roads, result.RoadCount, "Incorrect number of roads built.");
                Assert.AreEqual(_junctions, result.JunctionCount, "Incorrect number of junctions built.");
                Assert.AreEqual(_rightTurn, result.JunctionRightCount, "Incorrect number of right hand turns built.");
                Assert.AreEqual(_leftTurn, result.JunctionLeftCount, "Incorrect number of left hand turns junctions built.");
            });
        }

        public HighwaysManagerProperties VerifyHighwaysManagerInitialLayoutResident()
        {
            var result = new HighwaysManagerProperties();
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var highway = DataService.Current.GetStore<HighwaysDocumentStore>(acDoc.Name).GetManager<HighwaysManager>();
            var acCurDb = acDoc.Database;
            var ed = acDoc.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return result;
            if (res.Value == null || res.Value.Count == 0) return result;

            var acTrans = acCurDb.TransactionManager.StartTransaction();
            try
            {
                var curves = GetCurvesFromSelection(res.Value);                    

                highway.InitialiseFromCurves(curves);

                result.CentreLineCount = highway.Roads.Select(r => r.CentreLines.Count).Sum();
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

                acTrans.Commit();
            }
            catch (Exception)
            {
                acTrans.Abort();
            }

            return result;         
        }

        private static IEnumerable<Curve> GetCurvesFromSelection(IEnumerable acSSet)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var curveList = new List<Curve>();
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
                        curveList.Add(acPolyCurve);
                    }
                }
                else
                {
                    curveList.Add(acCurve);
                }
            }
            return curveList;
        }
    }
}