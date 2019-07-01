using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Layouts
{
    //MOVE: To Core...
    public interface ILayout
    {     
        string Name { get; }
        int Columns { get; }
        int Row { get; }
        double ColumnSize { get; }
        double RowSize { get; }
        Point3d TopCorner { get; }       
    }
}
