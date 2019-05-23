using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Drainage.ObjectModel.Layouts;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects.Paper
{
    //MOVE: To Core...
    public class Space
    {
        private readonly List<SpaceView> _viewports;
        private readonly SpaceRegion _rootRegion;

        public ILayout Layout { get; }
        public SpaceView[] Viewports => _viewports.ToArray();
        public enum LayoutOptions { Size, Order }

        public Space(ILayout layout)
        {
            Layout = layout;
            _viewports = new List<SpaceView>();
            _rootRegion = new SpaceRegion { Rows = layout.Row, Columns = layout.Columns };
        }

        public List<SpaceView> PackBySize(List<SpaceView> views)
        {
            var returnViews = new List<SpaceView>();
            _viewports.Clear();
            views = views.OrderByDescending(x => x.Area).ToList();
            foreach (var view in views)
            {
                var region = FindRegion(_rootRegion, view.Rows, view.Columns);
                if (region != null)
                {
                    view.Position = SplitRegion(region, view.Rows, view.Columns);
                    _viewports.Add(view);
                }
                else
                {
                    returnViews.Add(view);
                }
            }

            return returnViews;
        }

        public List<SpaceView> PackByOrder(List<SpaceView> views)
        {
            var returnViews = new List<SpaceView>();
            _viewports.Clear();

            foreach (var view in views)
            {
                var region = _rootRegion;
                while(true)
                {
                    if (region == null)
                    {
                        returnViews.Add(view);
                        break;
                    }
                        
                    if (!region.IsOccupied)
                    {
                        region.IsOccupied = true;
                        if (view.Columns <= region.Columns && view.Rows <= region.Rows)
                        {                            
                            region.RightRegion = new SpaceRegion { PosX = region.PosX + view.Columns, PosY = region.PosY, Columns = region.Columns - view.Columns, Rows = region.Rows};
                            view.Position = region;
                            _viewports.Add(view);
                            break;
                        }

                        var row = _viewports.Where(a => a.Position.PosY == region.PosY).Max(a => a.Rows);
                        if (view.Columns <= Layout.Columns & view.Rows <= region.Rows - row)
                        {
                            region.BottomRegion = new SpaceRegion { PosX = 0, PosY = region.PosY + row, Columns = Layout.Columns, Rows = region.Rows - row };
                        }                       
                    }

                    region = region.RightRegion ?? region.BottomRegion;
                }                
            }

            return returnViews;
        }

        private static SpaceRegion FindRegion(SpaceRegion region, int viewRows, int viewColumns)
        {
            if (region == null) return null;

            if (region.IsOccupied)
            {
                var nextNode = FindRegion(region.RightRegion, viewRows, viewColumns) ?? FindRegion(region.BottomRegion, viewRows, viewColumns);

                return nextNode;
            }

            if (viewRows <= region.Rows && viewColumns <= region.Columns)
            {
                return region;
            }

            return null;
        }

        private static SpaceRegion SplitRegion(SpaceRegion region, int viewRows, int viewColumns)
        {
            region.IsOccupied = true;
            region.BottomRegion = new SpaceRegion { PosX = region.PosX, PosY = region.PosY + viewRows, Columns = region.Columns, Rows = region.Rows - viewRows };
            region.RightRegion = new SpaceRegion { PosX = region.PosX + viewColumns, PosY = region.PosY, Columns = region.Columns - viewColumns, Rows = viewRows };
            return region;
        }

        public Viewport CreateViewport(SpaceView view)
        {
            var viewHeight = Layout.RowSize * view.Rows;
            var viewWidth = Layout.ColumnSize * view.Columns;
            var maxRows =_viewports.Where(a => a.Position.PosY == view.Position.PosY).Max(a => a.Rows);

            var adjustY = maxRows == view.Rows ? 0.0 : (maxRows - view.Rows) / 2.0;

            var centreX = (viewWidth / 2) + (view.Position.PosX * Layout.ColumnSize);
            var centreY = (viewHeight / 2) + (view.Position.PosY * Layout.RowSize) + (Layout.RowSize * adjustY);

            return new Viewport { CenterPoint = new Point3d(Layout.TopCorner.X + centreX, Layout.TopCorner.Y - centreY, 0), Height = viewHeight, Width = viewWidth, CustomScale = view.Scale, ViewTarget = view.ModelTarget };
        }

        public static int[] GetSize(double viewWidth, double viewHeight, ILayout layout)
        {
            var space = new Space(layout);
            var cols = (int)Math.Ceiling(viewWidth / space.Layout.ColumnSize);
            var rows = (int)Math.Ceiling(viewHeight / space.Layout.RowSize);

            return new[] { rows, cols };
        }

        public static Point3d AdjustCentre(Point3d centre, double viewWidth, double viewHeight, double scale, ILayout layout)
        {
            var space = new Space(layout);
            var size = GetSize(viewWidth, viewHeight, layout);
            var newWidth = size[1] * space.Layout.ColumnSize;
            var diffWidth = (newWidth - viewWidth) / 2 / scale;

            return new Point3d(centre.X - diffWidth, centre.Y, centre.Z);
        }
    }
}
