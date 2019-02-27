using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.Objectmodel.Extensions;

namespace Jpp.Ironstone.Highways.Objectmodel
{
    public class CentreLine : Segment2d
    {
        public Road Road { get; set; }
      
        public void Reverse()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var trans = acCurDb.TransactionManager.StartTransaction())
            {
                var curve = GetCurve(true);

                curve.ReverseCurve();
                trans.Commit();
            }
        }

        public Curve GetCurve(bool forWrite = false)
        {
            var mode = forWrite ? OpenMode.ForWrite : OpenMode.ForRead;
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var trans = acCurDb.TransactionManager.TopTransaction;
            
            return trans.GetObject(BaseObject, mode) as Curve;            
        }

        public void Highlight()
        {
            GetCurve().Highlight();
        }

        public void Unhighlight()
        {
            GetCurve().Unhighlight();
        }

        public CentreLine Previous()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var prevIdx = Road.PositionInRoad(this) - 1;
            return prevIdx < 0 ? null : Road[prevIdx];
        }

        public CentreLine Next()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var nextIdx = Road.PositionInRoad(this) + 1;
            return nextIdx > Road.CentreLines.Count() - 1 ? null : Road[nextIdx];
        }

        public bool Equals(CentreLine centreLine)
        {
            if (centreLine == null) return false;

            return StartPoint == centreLine.StartPoint && EndPoint == centreLine.EndPoint;
        }

        public Curve GenerateCarriageWayOffset(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    if (Road.CarriageWayLeft != null) return GetCurve().CreateOffset(side, Road.CarriageWayLeft.Distance);
                    break;
                case SidesOfCentre.Right:
                    if (Road.CarriageWayRight != null) return GetCurve().CreateOffset(side, Road.CarriageWayRight.Distance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return null;
        }

        public Curve GenerateCarriageWayOffset(SidesOfCentre side, double extra)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    if (Road.CarriageWayLeft != null) return GetCurve().CreateOffset(side, Road.CarriageWayLeft.Distance + extra);
                    break;
                case SidesOfCentre.Right:
                    if (Road.CarriageWayRight != null) return GetCurve().CreateOffset(side, Road.CarriageWayRight.Distance + extra);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return null;
        }
    }
}
