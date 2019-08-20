using System;
using System.ComponentModel;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    public class RoadSegment
    {
        private double _chainage;

        public RoadSegmentProperties Properties { get; set; }
        public double Chainage { 
            get => _chainage;
            set => _chainage = Math.Round(value,4);
        }

        public RoadSegment()
        {
            Properties = new RoadSegmentProperties();
            Properties.PropertyChanged += PropertiesOnPropertyChanged;
        }

        private static void PropertiesOnPropertyChanged(object sender, PropertyChangedEventArgs e) { }
    }
}
