﻿using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class PavementRight : Pavement
    {
        private const SidesOfCentre SIDES_OF_CENTRE = SidesOfCentre.Right;

        public PavementRight() : base(Constants.DEFAULT_PAVEMENT, SIDES_OF_CENTRE) { }

        public new void Create(CarriageWay carriageWay, RoadCentreLine centreLine)
        {
            if (centreLine.Road.RightPavementActive)
            {
                base.Create(carriageWay, centreLine);
            }
        }

    }
}
