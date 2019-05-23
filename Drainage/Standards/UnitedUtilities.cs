using System;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Standards
{
    public  class UnitedUtilities : SewersForAdoption7
    {
        public override void Apply(Manhole manhole)
        {
            base.Apply(manhole);

            if (manhole.LargestInternalPipeDiameter <= 525 && manhole.DepthToSoffitLevel < 6)
            {
                manhole.Type = "TYPE 1";
            }
            else
            {
                throw new ArgumentException("Manhole not Type 1 compliant");
            }

            manhole.MinimumMinorBenching = 325; //TODO: Need to check...
            manhole.SafetyChain = manhole.LargestInternalPipeDiameter > 525;
            manhole.SafetyRail = manhole.LargestInternalPipeDiameter > 525;
                           
            //Set benching
            manhole.MinimumMajorBenching = manhole.LargestInternalPipeDiameter <= 375 ? 600 : manhole.LargestInternalPipeDiameter <= 525 ? 750 : 1100;

            SetManholeCover(manhole);
        }

        private static void SetManholeCover(Manhole manhole)
        {

        }
    }
}
