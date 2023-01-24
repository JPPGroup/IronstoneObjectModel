using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI;
using Jpp.Ironstone.Core.UI.Autocad;
using Jpp.Ironstone.Housing.ObjectModel.Properties;
using Jpp.Ironstone.Housing.ObjectModel.UI.Properties;

namespace Jpp.Ironstone.Housing.ObjectModel.UI
{
    public class SharedUIHelper
    {
        public static RibbonTab HousingConceptTab { get; set; }
        public static RibbonTab MasterCreationContextTab { get; set; }
        public static bool GeneralDesignCreated = false;

        public static bool StructuresAvailable { get; set; }

        public static void CreateSharedElements()
        {
            if (HousingConceptTab == null)
            {
                HousingConceptTab = new RibbonTab();
                HousingConceptTab.Title = Resources.ExtensionApplication_UI_HousingContextTabTitle;
            }
            if (!GeneralDesignCreated)
            {
                RibbonPanel generalPanel = new RibbonPanel();
                RibbonPanelSource generalSource = new RibbonPanelSource();

                RibbonRowPanel hcolumn1 = new RibbonRowPanel();
                hcolumn1.IsTopJustified = true;
                var cmdNewPlotMaster = UIHelper.GetCommandGlobalName(typeof(SharedHouseCommands), nameof(SharedHouseCommands.CreateDetailPlotMaster));
                var btnNewPlotMaster = UIHelper.CreateButton(Resources.SharedUIHelper_UI_NewPlotMaster, Resources.Houses_Small, RibbonItemSize.Standard, cmdNewPlotMaster);
                hcolumn1.Items.Add(btnNewPlotMaster);

                generalSource.Items.Add(hcolumn1);
                generalSource.Title = Resources.ExtensionApplication_UI_HousingDesignTabTitle;
                generalPanel.Source = generalSource;

                RibbonControl rc = Autodesk.Windows.ComponentManager.Ribbon;
                RibbonTab primaryTab = rc.FindTab(Jpp.Ironstone.Core.Constants.IronstoneTabId);
                primaryTab.Panels.Add(generalPanel);

                GeneralDesignCreated = true;
            }

            CreateBlockTab();
        }

        private static void CreateBlockTab()
        {
            if (MasterCreationContextTab == null)
            {
                MasterCreationContextTab = new RibbonTab();
                MasterCreationContextTab.Title = Resources.SharedUIHelper_UI_MasterCreationContextTabTitle;
                MasterCreationContextTab.Name = Resources.SharedUIHelper_UI_MasterCreationContextTabTitle;
                MasterCreationContextTab.Id = "MASTER_BLOCK";

                CoreUIExtensionApplication.Current.RegisterConceptTab(MasterCreationContextTab, () => { return true; }, ContextualMode.BlockEdit | ContextualMode.ModelSpace);
            }
        }
    }
}
