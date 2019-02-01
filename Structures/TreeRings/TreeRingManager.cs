using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;

namespace Jpp.Ironstone.Structures.Objectmodel.TreeRings
{
    //TODO: Review class
    class TreeRingManager : AbstractDrawingObjectManager
    {
        public List<NHBCTree> Trees { get; set; }

        public PersistentObjectIdCollection RingsCollection { get; set; }

        public TreeRingManager()
        {
            Trees = new List<NHBCTree>();
            RingsCollection = new PersistentObjectIdCollection();
        }

        public override void UpdateDirty()
        {
            //TODO: Optimise
            RemoveErased();
            UpdateAll();
        }

        public override void UpdateAll()
        {
            RemoveErased();
            GenerateRings();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public void AddTree(NHBCTree tree)
        {
            Trees.Add(tree);
            tree.DirtyAdded = true;
            //TODO: Move into base
            //DrawingObjectManagerCollection.Current.FlagDirty();
        }

        public override void ActivateObjects()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (NHBCTree nhbcTree in Trees)
                {
                    nhbcTree.CreateActiveObject();
                }
            }
        }

        private void RemoveErased()
        {
            for (int i = Trees.Count - 1; i >= 0; i--)
            {
                if (Trees[i].DirtyRemoved)
                {
                    Trees.RemoveAt(i);
                }
            }
        }

        private void GenerateRings()
        {
            //TODO: Resolve properly
            SoilProperties sp = DataCoordinator.Current.Resolve<SoilProperties>();
            //SoilProperties sp = new SoilProperties() {SoilShrinkability = Shrinkage.High, TargetStepSize = 0.3f};
            if (sp == null)
            {
                sp = new SoilProperties(); // { SoilShrinkability = Shrinkage.High, TargetStepSize = 0.3f };
                DataCoordinator.Current.Register(sp);
            }

            //If grnaular soil return immediately as no need to do rings
            if (sp.Granular)
                return;

            int[] ringColors = new int[] {10, 200, 20, 180, 40, 160, 60, 140, 80, 120, 100};

            float StartDepth;

            //Determine start depth
            switch (sp.SoilShrinkability)
            {
                case Shrinkage.High:
                    StartDepth = 1;
                    break;

                case Shrinkage.Medium:
                    StartDepth = 0.9f;
                    break;

                case Shrinkage.Low:
                    StartDepth = 0.75f;
                    break;

                default:
                    StartDepth = 1f;
                    break;
            }

            int maxSteps = 0;

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            //Why openclose??
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {

                //Delete existing rings
                foreach (ObjectId obj in RingsCollection.Collection)
                {
                    if (!obj.IsErased)
                    {
                        acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
                    }

                    RingsCollection.Clear();
                }

                //Add the merged ring to the drawing
                BlockTableRecord acBlkTblRec = Core.Utilities.ModelSpace();

                List<DBObjectCollection> rings = new List<DBObjectCollection>();

                //Generate the rings for each tree
                foreach (NHBCTree tree in Trees)
                {
                    DBObjectCollection collection = tree.DrawRings(sp.SoilShrinkability, StartDepth, sp.TargetStepSize);
                    if (collection.Count > maxSteps)
                    {
                        maxSteps = collection.Count;
                    }

                    rings.Add(collection);
                }

                for (int ringIndex = 0; ringIndex < maxSteps; ringIndex++)
                {
                    //Determine overlaps
                    List<Curve> currentStep = new List<Curve>();
                    DBObjectCollection splitCurves = new DBObjectCollection();

                    //Build a collection of the outer rings only
                    foreach (DBObjectCollection col in rings)
                    {
                        //Check not stepping beyond
                        if (col.Count > ringIndex)
                        {
                            if (col[ringIndex] is Curve)
                            {
                                currentStep.Add(col[ringIndex] as Curve);
                            }
                        }
                    }

                    List<Region> createdRegions = new List<Region>();

                    //Create regions
                    foreach (Curve c in currentStep)
                    {
                        DBObjectCollection temp = new DBObjectCollection();
                        temp.Add(c);
                        DBObjectCollection regions = Region.CreateFromCurves(temp);
                        foreach (Region r in regions)
                        {
                            createdRegions.Add(r);
                        }
                    }

                    Region enclosed = createdRegions[0];

                    for (int i = 1; i < createdRegions.Count; i++)
                    {
                        enclosed.BooleanOperation(BooleanOperationType.BoolUnite, createdRegions[i]);
                    }

                    //Protection for color overflow, loop around
                    if (ringIndex >= ringColors.Length)
                    {
                        int multiple = (int) Math.Floor((double) (ringIndex / ringColors.Length));
                        enclosed.ColorIndex = ringColors[ringIndex - multiple * ringColors.Length];
                    }
                    else
                    {
                        enclosed.ColorIndex = ringColors[ringIndex];
                    }

                    RingsCollection.Add(acBlkTblRec.AppendEntity(enclosed));
                    acTrans.AddNewlyCreatedDBObject(enclosed, true);

                }

                acTrans.Commit();
            }
        }

        public override void AllDirty()
        {
            foreach (NHBCTree tree in Trees)
            {
                tree.DirtyModified = true;
            }
        }
    }
}
