using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class PavementRight : Pavement
    {
        private const SidesOfCentre SIDES_OF_CENTRE = SidesOfCentre.Right;

        public PavementRight() : base(Constants.DEFAULT_PAVEMENT, SIDES_OF_CENTRE) { }
        public PavementRight(double distance) : base(distance, SIDES_OF_CENTRE) { }
    }
}
