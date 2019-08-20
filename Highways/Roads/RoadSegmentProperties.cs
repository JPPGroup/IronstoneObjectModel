using Jpp.Common;
using System;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    public class RoadSegmentProperties : BaseNotify
    {
        private double _rightCarriagewayWidth = Constants.DEFAULT_CARRIAGEWAY_WIDTH;
        private double _leftCarriagewayWidth = Constants.DEFAULT_CARRIAGEWAY_WIDTH;
        private double _rightFootwayWidth = Constants.DEFAULT_FOOTWAY_WIDTH;
        private double _leftFootwayWidth = Constants.DEFAULT_FOOTWAY_WIDTH;

        public double RightCarriagewayWidth
        {
            get => _rightCarriagewayWidth;
            set => SetField(ref _rightCarriagewayWidth, value, nameof(RightCarriagewayWidth));
        }
        public double LeftCarriagewayWidth
        {
            get => _leftCarriagewayWidth;
            set => SetField(ref _leftCarriagewayWidth, value, nameof(LeftCarriagewayWidth));
        }
        public double RightFootwayWidth
        {
            get => _rightFootwayWidth;
            set => SetField(ref _rightFootwayWidth, value, nameof(RightFootwayWidth));
        }
        public double LeftFootwayWidth
        {
            get => _leftFootwayWidth;
            set => SetField(ref _leftFootwayWidth, value, nameof(LeftFootwayWidth));
        }

        [XmlIgnore] public FootwayTypes RightFootwayType => GetFootwayType(RightFootwayWidth);
        [XmlIgnore] public FootwayTypes LeftFootwayType => GetFootwayType(LeftFootwayWidth);
        [XmlIgnore] public double RightFootwayWidthFromCentre => RightCarriagewayWidth + RightFootwayWidth;
        [XmlIgnore] public double LeftFootwayWidthFromCentre => LeftCarriagewayWidth + LeftFootwayWidth;

        private static FootwayTypes GetFootwayType(double width)
        {
            return width < Constants.MINIMUM_FOOTWAY_WIDTH ? FootwayTypes.ServiceStrip : FootwayTypes.Footway;
        }
    }
}
