using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class DrawingArea
    {
        public double Bottom { get; private set; }
        public double Top { get; private set; }
        public double Left { get; private set; }
        public double Right { get; private set; }

        
        private Layout _layout;
        private ViewportDrawingObject[,] _viewportGrid;

        public DrawingArea(Layout layout, double bottom, double top, double left, double right)
        {
            Bottom = bottom;
            Top = top;
            Left = left;
            Right = right;
            _layout = layout;

            // TODO: Add code for allowing different grid sizes
            _viewportGrid = new ViewportDrawingObject[1,1];
        }

        public ViewportDrawingObject AddFullViewport()
        {
            ViewportDrawingObject viewport = ViewportDrawingObject.Create(_layout, Bottom, Top, Left, Right);
            _viewportGrid[0, 0] = viewport;
            return viewport;
        }
    }
}
