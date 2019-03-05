using Jpp.Ironstone.Highways.Objectmodel.Abstact;

namespace Jpp.Ironstone.Highways.Objectmodel.Objects
{
    public class CarriageWayRight : CentreLineOffset
    {
        public override SidesOfCentre Side { get; }
        public override OffsetTypes OffsetType { get; }
        public override double Distance { get; }
        //public override List<JunctionIntersection> Intersects { get; }
        //public override bool Ignore { get; set; }

        public CarriageWayRight(double distance)
        {
            Distance = distance;
            Side = SidesOfCentre.Right;
            OffsetType = OffsetTypes.CarriageWay;
            //Intersects = new List<JunctionIntersection>();
        }

        //public CarriageWayRight Copy()
        //{
        //    return new CarriageWayRight(Distance);
        //}
    }
}
