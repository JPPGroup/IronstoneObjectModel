using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    //TODO: Review class
    public class TreeRingManager : AbstractDrawingObjectManager
    {
        public List<NHBCTree> Trees { get; set; }

        public PersistentObjectIdCollection RingsCollection { get; set; }

        public TreeRingManager(Document document) : base(document)
        {
            Trees = new List<NHBCTree>();
            RingsCollection = new PersistentObjectIdCollection();
        }

        public TreeRingManager() : base()
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
            Database acCurDb = HostDocument.Database;

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
            SoilProperties sp = DataService.Current.GetStore<StructureDocumentStore>(HostDocument.Name).SoilProperties;
            
            int[] ringColors = new int[] { 102, 80, 60, 50, 20, 12, 14, 16, 18 };

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

            int maxExistingSteps = 0;
            int maxProposedSteps = 0;

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            //Why openclose??
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                //Set required layers
                acCurDb.RegisterLayer(Constants.EXISTING_TREE_LAYER);
                acCurDb.RegisterLayer(Constants.PROPOSED_TREE_LAYER);
                acCurDb.RegisterLayer(Constants.PILED_LAYER);
                acCurDb.RegisterLayer(Constants.HEAVE_LAYER);

                //Delete existing rings
                foreach (ObjectId obj in RingsCollection.Collection)
                {
                    if (!obj.IsErased)
                    {
                        acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
                    }

                    RingsCollection.Clear();
                }

                //If grnaular soil return immediately as no need to do rings
                if (sp.Granular)
                {
                    acTrans.Commit();
                    return;
                }

                //Add the merged ring to the drawing
                BlockTableRecord acBlkTblRec = HostDocument.Database.GetModelSpace();

                List<DBObjectCollection> existingRings = new List<DBObjectCollection>();
                List<DBObjectCollection> proposedRings = new List<DBObjectCollection>();
                DBObjectCollection pillingRings = new DBObjectCollection();
                DBObjectCollection heaveRings = new DBObjectCollection();

                //Generate the rings for each tree
                foreach (NHBCTree tree in Trees)
                {
                    DBObjectCollection collection = tree.DrawRings(sp.SoilShrinkability, StartDepth, sp.TargetStepSize);
                    Circle circ = tree.DrawRing(2.5f);
                    if(circ != null)
                        pillingRings.Add(circ);

                    Circle heaveCirc = tree.DrawRing(1.5f);
                    if (heaveCirc != null)
                        heaveRings.Add(heaveCirc);

                    switch (tree.Phase)
                    {
                        case Phase.Existing:
                            existingRings.Add(collection);
                            if (collection.Count > maxExistingSteps)
                                maxExistingSteps = collection.Count;
                            break;

                        case Phase.Proposed:
                            proposedRings.Add(collection);
                            if (collection.Count > maxProposedSteps)
                                maxProposedSteps = collection.Count;
                            break;
                    }
                }

                ObjectId currentLayer = acCurDb.Clayer;
                for (int ringIndex = 0; ringIndex < maxExistingSteps; ringIndex++)
                {
                    acCurDb.Clayer = acCurDb.GetLayer(Constants.EXISTING_TREE_LAYER.LayerId).ObjectId;
                    GenerateRing(existingRings, ringIndex, ringColors, acBlkTblRec, acTrans);
                }
                for (int ringIndex = 0; ringIndex < maxProposedSteps; ringIndex++)
                {
                    acCurDb.Clayer = acCurDb.GetLayer(Constants.PROPOSED_TREE_LAYER.LayerId).ObjectId;
                    GenerateRing(proposedRings, ringIndex, ringColors, acBlkTblRec, acTrans);
                }


                //Add hatching for piling
                acCurDb.Clayer = acCurDb.GetLayer(Constants.PILED_LAYER.LayerId).ObjectId;
                List<Region> createdRegions = new List<Region>(); 
                foreach (Curve c in pillingRings)
                {
                    DBObjectCollection temp = new DBObjectCollection();
                    temp.Add(c);
                    DBObjectCollection regions = Region.CreateFromCurves(temp);
                    foreach (Region r in regions)
                    {
                        createdRegions.Add(r);
                    }
                }

                if (createdRegions.Count > 0)
                {
                    DBObjectCollection intersectedRegions = new DBObjectCollection();
                    intersectedRegions.Add(createdRegions[0]);
                    for (int i = 1; i < createdRegions.Count; i++)
                    {
                        Region testRegion = createdRegions[i].Clone() as Region;

                        bool united = false;
                        foreach (Region r in intersectedRegions)
                        {
                            Region origin = r.Clone() as Region;
                            testRegion.BooleanOperation(BooleanOperationType.BoolIntersect, origin);
                            if (testRegion.Area > 0)
                            {
                                r.BooleanOperation(BooleanOperationType.BoolUnite, createdRegions[i]);
                                united = true;
                            }
                        }

                        if (!united)
                        {
                            intersectedRegions.Add(createdRegions[i]);
                        }
                    }


                    using (Hatch acHatch = new Hatch())
                    {
                        RingsCollection.Add(acBlkTblRec.AppendEntity(acHatch));
                        acTrans.AddNewlyCreatedDBObject(acHatch, true);

                        // Set the properties of the hatch object
                        // Associative must be set after the hatch object is appended to the 
                        // block table record and before AppendLoop
                        acHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                        acHatch.Associative = true;

                        foreach (Entity enclosed in intersectedRegions)
                        {
                            ObjectIdCollection boundary = new ObjectIdCollection();
                            RingsCollection.Add(acBlkTblRec.AppendEntity(enclosed));
                            acTrans.AddNewlyCreatedDBObject(enclosed, true);
                            boundary.Add(enclosed.ObjectId);
                            acHatch.AppendLoop(HatchLoopTypes.Outermost, boundary);
                        }

                        acHatch.HatchStyle = HatchStyle.Ignore;
                        acHatch.EvaluateHatch(true);

                        Byte alpha = (Byte) (255 * (100 - 80) / 100);
                        acHatch.Transparency = new Transparency(alpha);

                        DrawOrderTable dot =
                            (DrawOrderTable) acTrans.GetObject(acBlkTblRec.DrawOrderTableId, OpenMode.ForWrite);
                        ObjectIdCollection tempCollection = new ObjectIdCollection();
                        tempCollection.Add(acHatch.ObjectId);
                        dot.MoveToBottom(tempCollection);
                    }
                }

                //Add heave line
                acCurDb.Clayer = acCurDb.GetLayer(Constants.HEAVE_LAYER.LayerId).ObjectId;
                createdRegions = new List<Region>();
                foreach (Curve c in heaveRings)
                {
                    DBObjectCollection temp = new DBObjectCollection();
                    temp.Add(c);
                    DBObjectCollection regions = Region.CreateFromCurves(temp);
                    foreach (Region r in regions)
                    {
                        createdRegions.Add(r);
                    }
                }

                Region heaveEnclosed = createdRegions[0];

                for (int i = 1; i < createdRegions.Count; i++)
                {
                    heaveEnclosed.BooleanOperation(BooleanOperationType.BoolUnite, createdRegions[i]);
                }

                RingsCollection.Add(acBlkTblRec.AppendEntity(heaveEnclosed));
                acTrans.AddNewlyCreatedDBObject(heaveEnclosed, true);

                acCurDb.Clayer = currentLayer;
                acTrans.Commit();
            }
        }

        private void GenerateRing(List<DBObjectCollection> existingRings, int ringIndex, int[] ringColors, BlockTableRecord acBlkTblRec, Transaction acTrans)
        {
            if (existingRings.Count > 0)
            {
                //Determine overlaps
                List<Curve> currentStep = new List<Curve>();

                //Build a collection of the outer rings only
                foreach (DBObjectCollection col in existingRings)
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
