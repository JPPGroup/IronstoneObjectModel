using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Highways.ObjectModel.Exceptions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    public abstract class CentreLineOffset
    {
        private CentreLine _centreLine;

        public SidesOfCentre Side { get; }
        public OffsetTypes OffsetType { get;  }
        public double DistanceFromCentre { get;  }
        public PersistentObjectIdCollection Curves { get; }
        [XmlIgnore] public CentreLine CentreLine
        {
            get => _centreLine;
            set
            {
                if (!IsValid(value)) throw new ObjectException("Invalid centre line for offset.", value.BaseObject);

                _centreLine = value;              
            }
        }

        protected CentreLineOffset(double distance, SidesOfCentre side, OffsetTypes type, CentreLine centreLine)
        {
            DistanceFromCentre = distance;
            Side = side;
            OffsetType = type;
            CentreLine = centreLine;
            Curves = new PersistentObjectIdCollection();
        }

        public abstract void Create();

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

        private bool IsValid(CentreLine centre)
        {
            if (!(centre.GetCurve() is Arc arc)) return true;

            return arc.Radius > DistanceFromCentre;
        }
    }
}
