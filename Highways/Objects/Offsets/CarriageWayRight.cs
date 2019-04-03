using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class CarriageWayRight : CarriageWay
    {
        private const SidesOfCentre SIDES_OF_CENTRE = SidesOfCentre.Right;
        public PavementRight Pavement { get; set; }  //TODO: Checks on setter...

        public CarriageWayRight() : base(SIDES_OF_CENTRE)
        {
            Pavement = new PavementRight();
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
