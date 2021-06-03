using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Jpp.Common;
using Jpp.Ironstone.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            set { SetField(ref _soilShrinkability, value, nameof(SoilShrinkability)); }
        }

        private Shrinkage _soilShrinkability;

        public Boolean Granular
        {
            get { return _granular; }
            set { SetField(ref _granular, value, nameof(Granular)); }
        }
        private Boolean _granular;

        public float TargetStepSize
        {
            get { return _targetStepSize; }
            set { SetField(ref _targetStepSize, value, nameof(TargetStepSize)); }
        }
        private float _targetStepSize;

        public double GroundBearingPressure
        {
            get { return _groundBearingPressure; }
            set { SetField(ref _groundBearingPressure, value, nameof(GroundBearingPressure)); }
        }

        private double _groundBearingPressure;

        public ObservableCollection<DepthBand> DepthBands
        {
            get
            {
                if (_depthBands == null)
                    _depthBands = LoadDefaultBands();

                return _depthBands;
            }
            set { _depthBands = value; }
        }

        private ObservableCollection<DepthBand> _depthBands;

        public SoilProperties()
        {
            //Init to conservative starting values
            _soilShrinkability = Shrinkage.Medium;
            _granular = false;
            _targetStepSize = 0.3f;
            _groundBearingPressure = 50d;

        }

        private ObservableCollection<DepthBand> LoadDefaultBands()
        {
            var settings = CoreExtensionApplication._current.Container.GetRequiredService<IConfiguration>();

            ObservableCollection<DepthBand> bands = new ObservableCollection<DepthBand>();

            //var defaults = settings.Get<List<DepthBand>>("structures:foundations:depthBands");//settings.GetObject<List<DepthBand>>("structures.foundations.depthBands");
            var defaults = new List<DepthBand>();
            settings.Bind("structures:foundations:depthBands", defaults);
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
