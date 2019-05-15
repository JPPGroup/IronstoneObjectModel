using System;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Standards
{
    public class SewersForAdoption7 : StandardBase
    {
        public override void Apply(Manhole manhole)
        {
            base.Apply(manhole);

            //TODO: Check diameter - B 3.2 12
            var minimumDiameter = 0.0;
            if (manhole.LargestInternalPipeDiameter < 375) minimumDiameter = 1200;
            if (manhole.LargestInternalPipeDiameter >= 375 && manhole.LargestInternalPipeDiameter < 450) minimumDiameter = 1350;
            if (manhole.LargestInternalPipeDiameter >= 450 && manhole.LargestInternalPipeDiameter < 700) minimumDiameter = 1500;
            if (manhole.LargestInternalPipeDiameter >= 700 && manhole.LargestInternalPipeDiameter < 900) minimumDiameter = 1800;
            if (manhole.LargestInternalPipeDiameter >= 900) minimumDiameter = manhole.LargestInternalPipeDiameter + 900;

            if (manhole.Diameter < minimumDiameter) throw new ArgumentException("Manhole does not meet minimum dimensions size for pipes - increase to " + minimumDiameter);
        }
    }
}
