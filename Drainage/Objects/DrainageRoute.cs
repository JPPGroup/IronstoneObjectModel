﻿using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Drainage.ObjectModel.Factories;
using Jpp.Ironstone.Drainage.ObjectModel.Helpers;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects
{
    [Serializable]
    public class DrainageRoute : DrawingObject
    {
        private double _cover = Constants.DEFAULT_COVER;
        private double _gradient;

        public List<DrainageVertex> Vertices { get; }
        public double InitialInvert { get; set; }
        public double Cover
        {
            get => _cover;
            set
            {
                Vertices.ForEach(v =>
                {
                    if (v.Cover.Equals(_cover)) v.Cover = value;
                });

                _cover = value;
            }
        }
        public double Gradient
        {
            get => _gradient;
            set
            {
                Vertices.ForEach(v =>
                {
                    if (v.Gradient.Equals(_gradient)) v.Gradient = value;
                });

                _gradient = value;
            }
        }
        public PersistentObjectIdCollection LeaderCollection { get; }

        public DrainageRoute(double initialInvert, double gradient, List<DrainageVertex> vertices)
        {
            Vertices = new List<DrainageVertex>();

            InitialInvert = initialInvert;
            Gradient = gradient;
         
            LeaderCollection = new PersistentObjectIdCollection();

            vertices.ForEach(v =>
            {
                v.Cover = Cover;
                v.Gradient = Gradient;
            });

            Vertices = vertices;
        }

        private DrainageRoute()
        {
            Vertices = new List<DrainageVertex>();
            LeaderCollection = new PersistentObjectIdCollection();
        }

        public void Highlight()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var line = (Polyline)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                line.Highlight();
            }
        }

        public void Unhighlight()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var line = (Polyline)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                line.Unhighlight();
            }
        }

        protected override void GenerateBase()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = TransactionFactory.CreateFromNew())
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
                            BaseObjectPtr = acBlkTblRec.AppendEntity(acPoly).Handle.Value;
                            acTrans.AddNewlyCreatedDBObject(acPoly, true);
                        }
                    }
                }

                acTrans.Commit();
            }
        }

        protected override void ObjectModified(object sender, EventArgs e) { }
        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            ClearLeaders();
        }

        public override void Generate()
        {
            CheckBasePolyline();

            ClearLeaders();

            GenerateStartLabel();
            GenerateCoverLabels();
        }

        public override void Erase()
        {
            ClearLeaders();

            var acTrans = TransactionFactory.CreateFromTop();
            if (!BaseObject.IsErased) acTrans.GetObject(BaseObject, OpenMode.ForWrite, true).Erase();
            BaseObjectPtr = 0;
        }

        public override Point3d Location { get; set; }
        public override double Rotation { get; set; }

        private void CheckBasePolyline()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var line = (Polyline) acTrans.GetObject(BaseObject, OpenMode.ForRead);

                if (line.NumberOfVertices - 1 != Vertices.Count) return;

                for (var i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i].StartPoint = line.GetLineSegment2dAt(i).StartPoint;
                    Vertices[i].EndPoint = line.GetLineSegment2dAt(i).EndPoint;
                }
            }
        }

        private void GenerateCoverLabels()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var polyLine = (Polyline)acTrans.GetObject(BaseObject, OpenMode.ForRead);

                var level = InitialInvert;
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

                    var leader = LeaderHelper.GenerateLeader(coverString.ToString(), polyLine.GetPoint3dAt(i + 1),new Point3d(textPt.X, textPt.Y, 0));
                    LeaderCollection.Add(leader);
                }

                acTrans.Commit();
            }
        }

        private void GenerateStartLabel()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var line = (Polyline)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                var initLine = line.GetLineSegment2dAt(0);

                initLine.TransformBy(Matrix2d.Rotation(Math.PI * 1.5, initLine.StartPoint));
                var initLineVector = initLine.EndPoint.GetAsVector() - initLine.StartPoint.GetAsVector();
                var initPt = initLine.StartPoint + initLineVector * 0.2;

                var initString = new StringBuilder();
                initString.Append($"Initial invert level: {InitialInvert}\n");
                initString.Append($"Gradient: 1:{Gradient}");

                var leader = LeaderHelper.GenerateLeader(initString.ToString(), line.GetPoint3dAt(0), new Point3d(initPt.X, initPt.Y, 0));
                LeaderCollection.Add(leader);

                acTrans.Commit();
            }
        }

        private void ClearLeaders()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                foreach (ObjectId obj in LeaderCollection.Collection)
                {
                    if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite, true).Erase();
                }
                LeaderCollection.Clear();

                acTrans.Commit();
            }
        }

        private static double GetPolylineShape(LineSegment3d l1, LineSegment3d l2, Vector3d normal)
        {
            var v1 = l1.EndPoint.GetVectorTo(l1.StartPoint);
            var v2 = l2.StartPoint.GetVectorTo(l2.EndPoint);
            return v1.GetAngleTo(v2, normal);
        }
    }
}
