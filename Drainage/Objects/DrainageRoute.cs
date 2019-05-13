using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Drainage.ObjectModel.Factories;
using Jpp.Ironstone.Drainage.ObjectModel.Helpers;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects
{
    [Serializable]
    public class DrainageRoute
    {
        public long LineObjPtr { get; private set; }
        public List<DrainageVertex> Vertices { get; }
        public double InitialLevel { get; set; } = 10.500;
        public double Cover { get; set; } = Constants.DEFAULT_COVER;
        public double Gradient { get; set; } = 150;
        public PersistentObjectIdCollection LeaderCollection { get; }

        public DrainageRoute(List<DrainageVertex> vertices)
        {
            Vertices = vertices;
            LeaderCollection = new PersistentObjectIdCollection();

            Vertices.ForEach(v =>
            {
                v.Cover = Cover;
                v.Gradient = Gradient;
            });
        }

        private DrainageRoute()
        {
            Vertices = new List<DrainageVertex>();
            LeaderCollection = new PersistentObjectIdCollection();
        }

        public void Highlight()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
            using (var acTrans =TransactionFactory.CreateFromNew())
            {
                var line = (Polyline)acTrans.GetObject(lineObj, OpenMode.ForRead);
                line.Highlight();
            }
        }

        public void Unhighlight()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var line = (Polyline)acTrans.GetObject(lineObj, OpenMode.ForRead);
                line.Unhighlight();
            }
        }

        public void Generate()
        {
            if (LineObjPtr != 0)
            {
                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
                if (lineObj.IsValid) CheckLine(lineObj);
            }
            
            Clear();

            GenerateLine();
            GenerateStartLabel();
            GenerateCoverLabels();
        }

        private void CheckLine(ObjectId lineObj)
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var line = (Polyline) acTrans.GetObject(lineObj, OpenMode.ForRead);

                if (line.NumberOfVertices - 1 != Vertices.Count) return;

                for (var i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i].StartPoint = line.GetLineSegment2dAt(i).StartPoint;
                    Vertices[i].EndPoint = line.GetLineSegment2dAt(i).EndPoint;
                }
            }
        }

        private void GenerateLine()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (acBlkTbl != null)
                {
                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec != null)
                    {
                        using (var acPoly = new Polyline())
                        {
                            acPoly.AddVertexAt(acPoly.NumberOfVertices, Vertices[0].StartPoint, 0, 0, 0);
                            Vertices.ForEach(p => acPoly.AddVertexAt(acPoly.NumberOfVertices, p.EndPoint, 0, 0, 0));
                            acPoly.Layer = Constants.LAYER_DEF_POINTS_NAME;
                            LineObjPtr = acBlkTblRec.AppendEntity(acPoly).Handle.Value;
                            acTrans.AddNewlyCreatedDBObject(acPoly, true);
                        }
                    }
                }

                acTrans.Commit();
            }
        }

        private void GenerateCoverLabels()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
                var polyLine = (Polyline)acTrans.GetObject(lineObj, OpenMode.ForRead);

                var level = InitialLevel;
                for (var i = 0; i < Vertices.Count; i++)
                {
                    var vertex = Vertices[i];
                    var gradValue = 1 / vertex.Gradient;

                    var line = polyLine.GetLineSegment2dAt(i);
                    if (line == null) continue;
                    level += line.Length * gradValue;
                    if (i < Vertices.Count - 1)
                    {
                        var l1 = polyLine.GetLineSegmentAt(i);
                        var l2 = polyLine.GetLineSegmentAt(i + 1);
                        var angle = GetPolylineShape(l1, l2, polyLine.Normal);

                        if (angle < Math.PI)
                        {
                            angle += (Math.PI * 2 - angle) / 2;
                        }
                        else
                        {
                            angle -= angle / 2;
                        }

                        line.TransformBy(Matrix2d.Rotation(angle, line.EndPoint));

                    }
                    else
                    {
                        line.TransformBy(Matrix2d.Rotation(Math.PI * 1.5, line.EndPoint));
                    }

                    var lineVector = line.StartPoint.GetAsVector() - line.EndPoint.GetAsVector();
                    var textPt = line.EndPoint + lineVector * 0.2;

                    var coverString = new StringBuilder();
                    var pipeDia = Vertices[i].Diameter;
                    if (i < Vertices.Count - 1)
                    {
                        var pipeDiaAlt = Vertices[i + 1].Diameter;
                        if (pipeDia.Equals(pipeDiaAlt))
                        {
                            coverString.Append($"Minimum cover level: {Math.Round(level, 3) + vertex.Cover + (pipeDia / 1000)}\n");
                            coverString.Append($"Invert level: {Math.Round(level, 3)} ({pipeDia} {((char)216).ToString()})");
                        }
                        else
                        {
                            var altInvert = level + ((pipeDia - pipeDiaAlt) / 1000);
                            coverString.Append($"Minimum cover level: {Math.Round(level, 3) + vertex.Cover + (pipeDia / 1000)}\n");
                            coverString.Append($"Invert level: {Math.Round(level, 3)} ({pipeDia} {((char)216).ToString()})\n");
                            coverString.Append($"Invert level: {Math.Round(altInvert, 3)} ({pipeDiaAlt} {((char)216).ToString()})");
                            level = altInvert;
                        }
                    }
                    else
                    {
                        coverString.Append($"Minimum cover level: {Math.Round(level, 3) + vertex.Cover + (pipeDia / 1000)}\n");
                        coverString.Append($"Invert level: {Math.Round(level, 3)} ({pipeDia} {((char)216).ToString()})");
                    }

                    LeaderCollection.Add(LeaderHelper.GenerateLeader(coverString.ToString(), polyLine.GetPoint3dAt(i+1), new Point3d(textPt.X, textPt.Y, 0)));
                }

                acTrans.Commit();
            }
        }

        private void GenerateStartLabel()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
                var line = (Polyline)acTrans.GetObject(lineObj, OpenMode.ForRead);

                var initLine = line.GetLineSegment2dAt(0);

                initLine.TransformBy(Matrix2d.Rotation(Math.PI * 1.5, initLine.StartPoint));
                var initLineVector = initLine.EndPoint.GetAsVector() - initLine.StartPoint.GetAsVector();
                var initPt = initLine.StartPoint + initLineVector * 0.2;

                var initString = new StringBuilder();
                initString.Append($"Initial level: {InitialLevel}\n");
                initString.Append($"Gradient: 1:{Gradient}");

                LeaderCollection.Add(LeaderHelper.GenerateLeader(initString.ToString(), line.GetPoint3dAt(0),new Point3d(initPt.X, initPt.Y, 0)));

                acTrans.Commit();
            }
        }

        public void Clear()
        {
            var acTrans = TransactionFactory.CreateFromTop();
            foreach (ObjectId obj in LeaderCollection.Collection)
            {
                if (!obj.IsErased)
                {
                    acTrans.GetObject(obj, OpenMode.ForWrite, true).Erase();
                }
            }
            LeaderCollection.Clear();

            if (LineObjPtr == 0) return;
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var lineObj = acCurDb.GetObjectId(false, new Handle(LineObjPtr), 0);
            if (!lineObj.IsValid) return;

            if (!lineObj.IsErased) acTrans.GetObject(lineObj, OpenMode.ForWrite, true).Erase();
            LineObjPtr = 0;
        }

        private static double GetPolylineShape(LineSegment3d l1, LineSegment3d l2, Vector3d normal)
        {
            var v1 = l1.EndPoint.GetVectorTo(l1.StartPoint);
            var v2 = l2.StartPoint.GetVectorTo(l2.EndPoint);
            return v1.GetAngleTo(v2, normal);
        }
    }
}
