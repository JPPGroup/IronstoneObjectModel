using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Highways.ObjectModel.Old.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Old.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Abstract
{
    public abstract class CentreLineOffset
    {
        public SidesOfCentre Side { get; set;  }
        public OffsetTypes OffsetType { get; set; }
        public double DistanceFromCentre { get; set; } //TODO: Need to mark as dirty if changed...
        public PersistentObjectIdCollection Curves { get; }

        protected CentreLineOffset(double distance, SidesOfCentre side, OffsetTypes type)
        {
            DistanceFromCentre = distance;
            Side = side;
            OffsetType = type;
            Curves = new PersistentObjectIdCollection();
        }

        public virtual void Clear()
        {
            var acTrans = TransactionFactory.CreateFromTop();
            foreach (ObjectId obj in Curves.Collection)
            {
                if (!obj.IsErased)
                {
                    acTrans.GetObject(obj, OpenMode.ForWrite, true).Erase();
                }
            }

            Curves.Clear();
        }

        public bool IsValid(RoadCentreLine centre, double distance)
        {
            if (!(centre.GetCurve() is Arc arc)) return true;

            return arc.Radius > distance && distance > 0;
        }
    }
}
