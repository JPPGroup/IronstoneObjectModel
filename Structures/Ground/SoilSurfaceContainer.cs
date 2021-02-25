using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Jpp.DesignCalculations.Calculations.Design.Foundations;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Structures.ObjectModel.Ground
{
    public class SoilSurfaceContainer
    {
        private Document _hostDocument;

        public SoilProperties SoilProperties { get; set; }

        public CivSurface ExistingGround { get; set; }

        public CivSurface ProposedGround { get; set; }

        public SoilSurfaceContainer(Document doc)
        {
            _hostDocument = doc;

            SoilProperties = DataService.Current.GetStore<StructureDocumentStore>(_hostDocument.Name).SoilProperties;
            GetSurfaces();
        }

        private void GetSurfaces()
        {
            //Get the target surface
            ObjectIdCollection SurfaceIds = CivilApplication.ActiveDocument.GetSurfaceIds();

            foreach (ObjectId surfaceId in SurfaceIds)
            {
                // Direct cast is safe as collection is filtered down to surfaces by Autocad
                CivSurface temp = (CivSurface)surfaceId.GetObject(OpenMode.ForRead);

                // Continue is not used, incase user has set the same surface as both
                if (temp.Name ==  SoilProperties.ProposedGroundSurfaceName)
                {
                    ProposedGround = temp;
                }
                if (temp.Name == SoilProperties.ExistingGroundSurfaceName)
                {
                    ExistingGround = temp;
                }
            }
        }

        public double GetGroundBearingPressure(Point3d startPoint, Point3d endPoint)
        {
            return SoilProperties.GroundBearingPressure;
        }

        public FeatureLine GetFeatureLine(ObjectId curve, CivSurface targetSurface)
        {
            Database acCurDb = _hostDocument.Database;
            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

            ObjectId perimId = FeatureLine.Create(Guid.NewGuid().ToString(), curve);
            FeatureLine perim = acTrans.GetObject(perimId, OpenMode.ForWrite) as FeatureLine;
            perim.AssignElevationsFromSurface(targetSurface.Id, true);

            return perim;
        }

        public double GetDepthAtPoint(Point3d point, FeatureLine existingGround, FeatureLine proposedGround)
        {
            NHBC2020FoundationDepth depth = new NHBC2020FoundationDepth();
            
            depth.ExistingGroundLevel = existingGround.GetElevationAtPoint(point);
            depth.ProposedGroundLevel = proposedGround.GetElevationAtPoint(point);

            /*if (ProposedGround != null)
            {
                FeatureLine proposedLine = GetFeatureLine(this.BaseObject, soilSurfaceContainer.ProposedGround);
                depth.ProposedGroundLevel = proposedLine.MinElevation;
            }*/

            switch (SoilProperties.SoilShrinkability)
            {
                case Shrinkage.High:
                    depth.SoilPlasticity = VolumeChangePotential.High;
                    break;

                case Shrinkage.Medium:
                    depth.SoilPlasticity = VolumeChangePotential.Medium;
                    break;

                case Shrinkage.Low:
                    depth.SoilPlasticity = VolumeChangePotential.Low;
                    break;
            }

            depth.Run();

            return depth.FoundationDepth.Value;
        }
    }
}
