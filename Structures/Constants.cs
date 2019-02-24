using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.Objectmodel
{
    class Constants
    {
        public static LayerInfo EXISTING_TREE_LAYER =
            new LayerInfo() {LayerId = "JPP_Foundations_TreeRings_Existing", Linetype = "1MM-HIDDEN"};

        public static LayerInfo PROPOSED_TREE_LAYER =
            new LayerInfo() { LayerId = "JPP_Foundations_TreeRings_Proposed"};

        public static LayerInfo PILED_LAYER =
            new LayerInfo() { LayerId = "JPP_Foundations_Pilled", IndexColor = 10};

        public static LayerInfo HEAVE_LAYER =
            new LayerInfo() { LayerId = "JPP_Foundations_TreeRings_Proposed", IndexColor = 200};
    }
}
