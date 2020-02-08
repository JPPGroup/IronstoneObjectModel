﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jpp.Common;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Unity;

namespace Jpp.Ironstone.Structures.ObjectModel
{
    public class SoilProperties : BaseNotify
    {
        public string ProposedGroundSurfaceName
        {
            get { return _proposedGroundSurfaceName; }
            set { SetField(ref _proposedGroundSurfaceName, value, nameof(ProposedGroundSurfaceName)); }
        }

        public string ExistingGroundSurfaceName
        {
            get { return _existingGroundSurfaceName; }
            set { SetField(ref _existingGroundSurfaceName, value, nameof(ExistingGroundSurfaceName)); }
        }

        private string _proposedGroundSurfaceName, _existingGroundSurfaceName;

        public Shrinkage SoilShrinkability
        {
            get { return _soilShrinkability; }
            set { SetField(ref _soilShrinkability, value, "SoilShrinkability"); }
        }

        private Shrinkage _soilShrinkability;

        public Boolean Granular
        {
            get { return _granular; }
            set { SetField(ref _granular, value, "Granular"); }
        }
        private Boolean _granular;

        public float TargetStepSize
        {
            get { return _targetStepSize; }
            set { SetField(ref _targetStepSize, value, "TargetStepSize"); }
        }
        private float _targetStepSize;

        public ObservableCollection<DepthBand> DepthBands { get; }

        public SoilProperties()
        {
            //Init to conservative starting values
            _soilShrinkability = Shrinkage.Medium;
            _granular = false;
            _targetStepSize = 0.3f;
            DepthBands = LoadDefaultBands();
        }

        private ObservableCollection<DepthBand> LoadDefaultBands()
        {
            IUserSettings settings = CoreExtensionApplication._current.Container.Resolve<IUserSettings>();

            ObservableCollection<DepthBand> bands = new ObservableCollection<DepthBand>();

            var defaults = settings.GetObject<List<DepthBand>>("structures.foundations.depthBands");
            foreach (DepthBand band in defaults)
            {
                bands.Add(band);
            }

            return bands;
        }

        /*private void Update()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            // Draws a circle and zooms to the extents or 
            // limits of the drawing
            acDoc.SendStringToExecute("._regen ", false, false, true);
        }*/
    }
}
