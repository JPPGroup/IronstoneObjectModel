using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Common;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel
{
    public class LayoutSheetController
    {
        public SerializibleDictionary<string, LayoutSheet> Sheets;

        public LayoutSheetController()
        {
            Sheets = new SerializibleDictionary<string, LayoutSheet>();
        }
    }
}
