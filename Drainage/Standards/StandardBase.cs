using System;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Standards
{
    public class StandardBase : IDrainageStandard
    {
        public virtual void Apply(Manhole manhole)
        {
            if (manhole.IncomingPipes.Capacity == 0) throw new ArgumentException("Manhole does not have any incoming pipes.");
        }
    }
}
