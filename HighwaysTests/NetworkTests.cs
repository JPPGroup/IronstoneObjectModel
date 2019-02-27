﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using HighwaysTests.Response;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Highways.Objectmodel;
using Jpp.Ironstone.Highways.Objectmodel.Extensions;
using NUnit.Framework;

namespace HighwaysTests
{
    [TestFixture(@"..\..\Drawings\NetworkTests1.dwg", 49, 11, 10, 6, 4 )]
    [TestFixture(@"..\..\Drawings\NetworkTests2.dwg", 102, 10, 11, 3, 8)]
    [TestFixture(@"..\..\Drawings\NetworkTests3.dwg", 131, 30, 41, 15, 26)]
    public class NetworkTests : BaseNUnitTestFixture
    {
        private readonly int _centreLines;
        private readonly int _roads;
        private readonly int _junctions;
        private readonly int _rightTurn;
        private readonly int _leftTurn;

        public NetworkTests() : base(Assembly.GetExecutingAssembly(), typeof(NetworkTests)) { }
        public NetworkTests(string drawingFile, int centreLines, int roads, int junctions, int rightTurn, int leftTurn) : base(Assembly.GetExecutingAssembly(), typeof(NetworkTests), drawingFile)
        {
            _centreLines = centreLines;
            _roads = roads;
            _junctions = junctions;
            _rightTurn = rightTurn;
            _leftTurn = leftTurn;
        }

        [Test]
        public void VerifyInitialiseNetwork()
        {
            var result = RunTest<NetworkProperties>("VerifyInitialiseNetworkResident");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(_centreLines, result.CentreLineCount, "Incorrect number of centre lines.");
                Assert.AreEqual(_roads, result.RoadCount, "Incorrect number of roads built.");
                Assert.AreEqual(_junctions, result.JunctionCount, "Incorrect number of junctions built.");
                Assert.AreEqual(_rightTurn, result.JunctionRightCount, "Incorrect number of right hand turns built.");
                Assert.AreEqual(_leftTurn, result.JunctionLeftCount, "Incorrect number of left hand turns junctions built.");
            });
        }

        public NetworkProperties VerifyInitialiseNetworkResident()
        {
            var result = new NetworkProperties();
            var network = new Network();
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var ed = acDoc.Editor;
            var res = ed.SelectAll();

            if (res.Status != PromptStatus.OK) return result;
            if (res.Value == null || res.Value.Count == 0) return result;
        
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var centreLines = GetCentreLinesFromSelection(res.Value);
                result.CentreLineCount = centreLines.Count;

                network.InitialiseNetworkFromCentreLines(centreLines);
                result.RoadCount = network.Roads.Count;
                result.JunctionCount = network.Junctions.Count;
                var rightCount = 0;
                var leftCount = 0;
                foreach (var junction in network.Junctions)
                {
                    if (junction.Turn == TurnTypes.Right) rightCount++;
                    if (junction.Turn == TurnTypes.Left) leftCount++;
                }

                result.JunctionRightCount = leftCount;
                result.JunctionLeftCount = rightCount;
            }

            return result;
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

