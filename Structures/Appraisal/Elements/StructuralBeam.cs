using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.DesignCalculations.Calculations;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.ObjectModel.Appraisal.Elements
{
    public class StructuralBeam : StructuralSupportLine, IAppraisalObject
    {
        public static StructuralBeam Create(Database database, Point2d start, Point2d end)
        {
            Transaction trans = database.TransactionManager.TopTransaction;
            StructuralBeam newBeam = new StructuralBeam();

            Polyline acPoly = new Polyline();
            acPoly.SetDatabaseDefaults();
            acPoly.AddVertexAt(0, start, 0, 0, 0);
            acPoly.AddVertexAt(1, end, 0, 0, 0);

            newBeam.BaseObject = database.GetModelSpace(true).AppendEntity(acPoly);
            trans.AddNewlyCreatedDBObject(acPoly, true);

            return newBeam;
        }

        public Calculation Calculation { get; }
    }
}
