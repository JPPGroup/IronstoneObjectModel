using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace Jpp.Ironstone.Structures.Objectmodel
{
    public class SoilProperties
    {
        public Shrinkage SoilShrinkability
        {
            get { return _soilShrinkability; }
            set { _soilShrinkability = value; Update(); }
        }

        private Shrinkage _soilShrinkability;

        public Boolean Granular
        {
            get { return _granular; }
            set { _granular = value; Update(); }
        }
        private Boolean _granular;

        public float TargetStepSize
        {
            get { return _targetStepSize; }
            set { _targetStepSize = value; Update(); }
        }
        private float _targetStepSize;

        public SoilProperties()
        {
            //Init to conservative starting values
            _soilShrinkability = Shrinkage.High;
            _granular = false;
            _targetStepSize = 0.3f;
        }

        private void Update()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            // Draws a circle and zooms to the extents or 
            // limits of the drawing
            acDoc.SendStringToExecute("._regen ", false, false, true);
        }
    }
}
