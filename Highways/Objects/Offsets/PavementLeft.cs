using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class PavementLeft : Pavement
    {
        private const SidesOfCentre SIDES_OF_CENTRE = SidesOfCentre.Left;

        public PavementLeft() : base(Constants.DEFAULT_PAVEMENT, SIDES_OF_CENTRE) { }
        public PavementLeft(double distance) : base(distance, SIDES_OF_CENTRE) { }

        public new void Create(CarriageWay carriageWay, RoadCentreLine centreLine)
        {
            if (centreLine.Road.LeftPavementActive)
            {
                base.Create(carriageWay, centreLine);
            }
        }
    }
}
