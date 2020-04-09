using Autodesk.Windows;
using Jpp.Ironstone.Housing.ObjectModel.Properties;

namespace Jpp.Ironstone.Housing.ObjectModel
{
    public class SharedUIHelper
    {
        public static RibbonTab HousingConceptTab { get; set; }

        public static bool StructuresAvailable { get; set; }

        public static void CreateSharedElements()
        {
            if (HousingConceptTab == null)
            {
                HousingConceptTab = new RibbonTab();
                HousingConceptTab.Title = Resources.ExtensionApplication_UI_HousingContextTabTitle;
            }
        }
    }
}
