using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class NoteArea
    {
        public double Bottom { get; private set; }
        public double Top { get; private set; }
        public double Left { get; private set; }
        public double Right { get; private set; }

        public string Notes { get { return _text.Contents; } set { _text.Contents = value; } }

        private LayoutSheet _layoutSheet;

        public NoteArea(LayoutSheet layout, double bottom, double top, double left, double right)
        {
            _layoutSheet = layout;

            Bottom = bottom;
            Top = top;
            Left = left;
            Right = right;

            _text = FindText();
        }

        private MTextDrawingObject FindText()
        {
            var text = _layoutSheet._layout.GetEntities<MText>();
            foreach(var entity in text)
            {
                MTextDrawingObject textObj = new MTextDrawingObject();
                textObj.BaseObject = entity.Id;

                if (textObj.HasKey("noteblock"))
                    return textObj;
            }

            Point3d point = new Point3d(Left, Top, 0);
            var newText = MTextDrawingObject.Create(_layoutSheet._layout.Database, _layoutSheet.GetBlockTableRecord(), point, "Notes");
            newText["noteblock"] = "true";
            return newText;
        }

        private MTextDrawingObject _text;
    }
}
