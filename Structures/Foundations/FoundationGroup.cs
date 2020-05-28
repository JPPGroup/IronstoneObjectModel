using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationGroup
    {
        public const string FOUNDATION_CENTRE_LOAD_KEY = "JPP_FoundationCentreline_Load";

        public List<string> Plots { get; }

        /*public List<FoundationNode> Nodes { get; }*/
        public List<FoundationCentreLine> Centrelines { get; }

        public FoundationGroup()
        {
            Plots = new List<string>();
            //Nodes = new List<FoundationNode>();
            Centrelines = new List<FoundationCentreLine>();
        }

        public void Rebuild()
        {
            DetermineWidths();
            DetermineDepths();

            // TODO: Recalc widths based on depths

            AddRequiredBottomSteps();
            AddRequiredTopSteps();
            AdjustBottomSteps();
            AdjustTopSteps();
        }

        private void DetermineWidths()
        {
            foreach (FoundationCentreLine foundationCentreLine in Centrelines)
            {
                foundationCentreLine.AddWidths();
            }
        }

        private void DetermineDepths()
        {

        }

        private void AddRequiredBottomSteps()
        {

        }

        private void AdjustBottomSteps()
        {

        }

        private void AddRequiredTopSteps()
        {

        }

        private void AdjustTopSteps()
        {

        }
    }
}
