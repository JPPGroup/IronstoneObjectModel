using System;
using System.Xml.Serialization;
using Jpp.Ironstone.Highways.ObjectModel.Objects;
using Jpp.Ironstone.Highways.ObjectModel.Objects.Features;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    [XmlInclude(typeof(CrossOver))]
    public abstract class RoadFeature
    {
        public event EventHandler RoadFeatureErased;

        public enum RoadFeatureTypes { CrossOver }
        public RoadFeatureTypes Type { get; }

        protected RoadFeature(RoadFeatureTypes type)
        {
            Type = type;
        }

        public abstract bool Generate(Road road);
        public abstract void Clear();

        protected virtual void OnRoadFeatureErased()
        {
            var handler = RoadFeatureErased;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
