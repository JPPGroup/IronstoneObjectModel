using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    public class TreeRing : RegionDrawingObject
    {
        public double Depth { get; set; }

        private TreeRing() : base()
        { }

        private TreeRing(Document doc) : base(doc)
        { }

        public static TreeRing Create(Document host, ICollection<Curve> enclosedCurves)
        {
            TreeRing ring = new TreeRing(host);

            if(enclosedCurves.Count < 0)
                throw new InvalidOperationException("No object to form region from.");

            List<Region> createdRegions = new List<Region>();

            //Create regions
            foreach (Curve c in enclosedCurves)
            {
                DBObjectCollection temp = new DBObjectCollection();
                temp.Add(c);
                DBObjectCollection regions = Region.CreateFromCurves(temp);
                foreach (Region r in regions)
                {
                    createdRegions.Add(r);
                }
            }

            Region enclosed = createdRegions[0];
            for (int i = 1; i < createdRegions.Count; i++)
            {
                enclosed.BooleanOperation(BooleanOperationType.BoolUnite, createdRegions[i]);
            }

            ring.BaseObject = host.Database.GetModelSpace(true).AppendEntity(enclosed);
            host.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(enclosed, true);

            return ring;
        }

        public static TreeRing Create(Document host, DBObjectCollection collection)
        {
            List<Curve> enclosedCurves = new List<Curve>();
            foreach (Curve c in collection)
            {
                enclosedCurves.Add(c);
            }

            return Create(host, enclosedCurves);
        }
    }
}
