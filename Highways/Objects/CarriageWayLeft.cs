using Jpp.Ironstone.Highways.ObjectModel.Abstact;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    public class CarriageWayLeft : CentreLineOffset
    {
        public override SidesOfCentre Side { get; }
        public override OffsetTypes OffsetType { get; }
        public override double Distance { get; }
        //public override List<JunctionIntersection> Intersects { get; }
        //public override bool Ignore { get; set; }

        public CarriageWayLeft(double distance)
        {
            Distance = distance;
            Side = SidesOfCentre.Left;
            OffsetType = OffsetTypes.CarriageWay;
            //Intersects = new List<JunctionIntersection>();
        }

        //public CarriageWayRight Copy()
        //{
        //    return new CarriageWayRight(Distance);
        //}
    }
}
