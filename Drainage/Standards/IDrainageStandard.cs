using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Standards
{
    public interface IDrainageStandard
    {
        void Apply(Manhole manhole);
    }
}
