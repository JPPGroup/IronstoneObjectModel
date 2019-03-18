using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class CarriageWayLeft : CarriageWay
    {
        private const SidesOfCentre SIDES_OF_CENTRE = SidesOfCentre.Left;
        public PavementLeft Pavement { get; set; } //TODO: Checks on setter...

        public CarriageWayLeft() : base(SIDES_OF_CENTRE)
        {
            Pavement = new PavementLeft();
        }

        public CarriageWayLeft(double distance, double pavementWidth) : base(distance, SIDES_OF_CENTRE)
        {
            Pavement = new PavementLeft(distance + pavementWidth);
        }

        public override void Clear()
        {
            Pavement.Clear();
            base.Clear();
        }

        public override void Create(RoadCentreLine centreLine)
        {
            base.Create(centreLine);
            Pavement.Create(this, centreLine);
        }
    }
}
