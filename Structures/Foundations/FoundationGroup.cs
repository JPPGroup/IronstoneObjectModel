using System.Collections.Generic;
using Jpp.Ironstone.Structures.ObjectModel.Ground;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationGroup
    {
        public const string FOUNDATION_CENTRE_LOAD_KEY = "JPP_FoundationCentreline_Load";
        public const string FOUNDATION_CENTRE_OVERLAPLOAD_KEY = "JPP_FoundationCentreline_OverlapLoad";

        public List<string> Plots { get; }

        public List<FoundationNode> Nodes { get; }
        public List<FoundationCentreLine> Centrelines { get; }

        public FoundationGroup()
        {
            Plots = new List<string>();
            Nodes = new List<FoundationNode>();
            Centrelines = new List<FoundationCentreLine>();
        }

        public void Rebuild(SoilSurfaceContainer soilSurfaceContainer)
        {
            //DetermineDepths(soilSurfaceContainer);

            // TODO: Recalc widths based on depths

            AddRequiredBottomSteps();
            AddRequiredTopSteps();
            AdjustBottomSteps();
            AdjustTopSteps();
            DetermineWidths(soilSurfaceContainer);
        }

        public void Delete()
        {
            foreach (FoundationCentreLine foundationCentreLine in Centrelines)
            {
                foundationCentreLine.Erase();
            }
        }

        private void DetermineWidths(SoilSurfaceContainer soilSurfaceContainer)
        {
            foreach (FoundationCentreLine foundationCentreLine in Centrelines)
            {
                foundationCentreLine.AddWidths(soilSurfaceContainer);
            }

            foreach (FoundationNode foundationNode in Nodes)
            {
                foundationNode.TrimFoundations();
            }
        }

        private void DetermineDepths(SoilSurfaceContainer soilSurfaceContainer)
        {
            foreach (FoundationCentreLine foundationCentreLine in Centrelines)
            {
                foundationCentreLine.CalculateDepths(soilSurfaceContainer);
            }
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
