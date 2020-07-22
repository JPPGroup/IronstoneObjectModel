namespace Jpp.Ironstone.Structures.ObjectModel
{
    class Constants
    {
        public const string EXISTING_TREE_LAYER = "structures.foundations.layers.treerings_existing";
        public const string PROPOSED_TREE_LAYER = "structures.foundations.layers.treerings_proposed";
        public const string PILED_LAYER = "structures.foundations.layers.treerings_piled";
        public const string HEAVE_LAYER = "structures.foundations.layers.treerings_heave";
        public const string LABEL_LAYER = "structures.foundations.layers.treerings_label";

        /*
         * Need to ensure layer properties are in settings file.
         *
         * public static LayerInfo EXISTING_TREE_LAYER = new LayerInfo() {LayerId = "JPP_Foundations_TreeRings_Existing", Linetype = "1MM-HIDDEN"};
         * public static LayerInfo PROPOSED_TREE_LAYER = new LayerInfo() { LayerId = "JPP_Foundations_TreeRings_Proposed"};
         * public static LayerInfo PILED_LAYER = new LayerInfo() { LayerId = "JPP_Foundations_Pilled", IndexColor = 10};
         * public static LayerInfo HEAVE_LAYER = new LayerInfo() { LayerId = "JPP_Foundations_TreeRings_Proposed", IndexColor = 200};
         *
         */
    }
}
