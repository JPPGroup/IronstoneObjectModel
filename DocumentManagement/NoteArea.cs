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

        public NoteArea(double bottom, double top, double left, double right)
        {
            Bottom = bottom;
            Top = top;
            Left = left;
            Right = right;
        }
    }
}
