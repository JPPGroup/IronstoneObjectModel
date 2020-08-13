using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System.Collections.Generic;
using Jpp.Ironstone.Structures.ObjectModel.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    //TODO: Review class
    [Layer(Name = Constants.PROPOSED_TREE_LAYER)]
    [Layer(Name = Constants.EXISTING_TREE_LAYER)]
    [Layer(Name = Constants.PILED_LAYER)]
    [Layer(Name = Constants.HEAVE_LAYER)]
    public class TreeRingManager : AbstractDrawingObjectManager<Tree>
    {
        internal int[] _ringColors { get; private set; }

        //public PersistentObjectIdCollection RingsCollection { get; set; }

        public SerializableDictionary<double, TreeRing> RingsCollection { get; set; }
        public TreeRing HeaveRing { get; set; }
        public TreeRing PileRing { get; set; }

        public TreeRingManager(Document document, ILogger<CoreExtensionApplication> log, IConfiguration config) : base(document, log, config)
        {
            RingsCollection = new SerializableDictionary<double, TreeRing>();
            _ringColors = _settings.GetObject<int[]>("structures.treeRings.RingColors");
        }

        public TreeRingManager() : base()
        {
            RingsCollection = new SerializableDictionary<double, TreeRing>();
            _ringColors = _settings.GetObject<int[]>("structures.treeRings.RingColors");
        }

        public override void UpdateDirty()
        {
            //TODO: Optimize
            base.UpdateDirty();
            UpdateAll();
        }

        public override void UpdateAll()
        {
            base.UpdateAll();
            GenerateRings();
        }

        public void AddTree(Tree tree)
        {
            ManagedObjects.Add(tree);
            tree.DirtyAdded = true;
            //TODO: Move into base
            //DrawingObjectManagerCollection.Current.FlagDirty();
        }

        private void GenerateRings()
        {
            LayerManager layerManager = DataService.Current.GetStore<DocumentStore>(HostDocument.Name).LayerManager;
            string existingLayer = layerManager.GetLayerName(Constants.EXISTING_TREE_LAYER);
            string proposedLayer = layerManager.GetLayerName(Constants.PROPOSED_TREE_LAYER);
            string heaveLayer = layerManager.GetLayerName(Constants.HEAVE_LAYER);
            string piledLayer = layerManager.GetLayerName(Constants.PILED_LAYER);

            SoilProperties sp = DataService.Current.GetStore<StructureDocumentStore>(HostDocument.Name).SoilProperties;

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

            //Why openclose??
            using (Transaction acTrans = HostDocument.Database.TransactionManager.StartTransaction())
            {
                //Delete existing rings
                foreach (TreeRing obj in RingsCollection.Values)
                {
                    if (!obj.Erased)
                    {
                        obj.Erase();
                    }
                }

                RingsCollection.Clear();

                PileRing?.Erase();
                HeaveRing?.Erase();

                PileRing = null;
                HeaveRing = null;

                acTrans.Commit();

                //If granular soil return immediately as no need to do rings
                if (sp.Granular) return;

                //If no active objects, return immediately as no need to do rings
                if (ActiveObjects.Count == 0) return;
            }

            using (Transaction acTrans = HostDocument.Database.TransactionManager.StartTransaction())
            {
                //Add the merged ring to the drawing
                BlockTableRecord acBlkTblRec = HostDocument.Database.GetModelSpace(true);

                List<DBObjectCollection> existingRings = new List<DBObjectCollection>();
                List<DBObjectCollection> proposedRings = new List<DBObjectCollection>();
                DBObjectCollection pillingRings = new DBObjectCollection();
                DBObjectCollection heaveRings = new DBObjectCollection();

                //Try generate the rings for each tree
                foreach (Tree tree in ActiveObjects)
                {
                    try
                    {
                        DBObjectCollection collection = tree.DrawRings(sp.SoilShrinkability, StartDepth, sp.TargetStepSize);
                        if (collection != null && collection.Count > 0)
                        {
                            Curve circ = tree.DrawShape(2.5f + sp.ClimateReduction, sp.SoilShrinkability);
                            if (circ != null) pillingRings.Add(circ);

                            Curve heaveCirc = tree.DrawShape(1.5f + sp.ClimateReduction, sp.SoilShrinkability);
                            if (heaveCirc != null) heaveRings.Add(heaveCirc);

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
                    }

                    catch (ArgumentException e) //catch expected argument exception from DrawRings
                    {
                        Log.LogError(e, string.Format(Resources.TreeRingManager_Message_ErrorOnBaseRings, tree.ID));

                        acTrans.Abort();
                        return;
                    }
                    catch (Exception e) //catch AutoCAD exception just in case something weird happened
                    {
                        Log.LogError(e, string.Format(Resources.TreeRingManager_Message_ErrorOnBaseRings, tree.ID));

                        acTrans.Abort();
                        return;
                    }
                }

                ObjectId currentLayer = HostDocument.Database.Clayer;
                try
                {
                    for (int ringIndex = 0; ringIndex < maxExistingSteps; ringIndex++)
                    {
                        HostDocument.Database.Clayer = HostDocument.Database.GetLayer(existingLayer).ObjectId;
                        GenerateEnclosedRing(existingRings, ringIndex, _ringColors, StartDepth, sp);
                    }

                    for (int ringIndex = 0; ringIndex < maxProposedSteps; ringIndex++)
                    {
                        HostDocument.Database.Clayer = HostDocument.Database.GetLayer(proposedLayer).ObjectId;
                        GenerateEnclosedRing(proposedRings, ringIndex, _ringColors, StartDepth, sp);
                    }

                    //Add hatching for piling
                    HostDocument.Database.Clayer = HostDocument.Database.GetLayer(piledLayer).ObjectId;
                    if (pillingRings.Count > 0)
                    {
                        PileRing = TreeRing.Create(HostDocument, pillingRings);
                        PileRing.Depth = 2.5d;
                        PileRing.Hatch();
                    }

                    //Add heave line
                    HostDocument.Database.Clayer = HostDocument.Database.GetLayer(heaveLayer).ObjectId;
                    if (heaveRings.Count > 0)
                    {
                        HeaveRing = TreeRing.Create(HostDocument, heaveRings);
                        HeaveRing.Depth = 1.5d;
                    }

                    acTrans.Commit();
                }
                catch (Exception e) //catch AutoCAD exception just in case something weird happened
                {
                    Log.LogError(e, Resources.TreeRingManager_Message_ErrorOnGenerateRings);
                    acTrans.Abort();
                }
                finally
                {
                    HostDocument.Database.Clayer = currentLayer;
                }
            }
        }

        /*private void GeneratePilingRings(DBObjectCollection pillingRings, BlockTableRecord acBlkTblRec, Transaction acTrans)
        {
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

                    Byte alpha = (Byte)(255 * (100 - 80) / 100);
                    acHatch.Transparency = new Transparency(alpha);

                    DrawOrderTable dot =
                        (DrawOrderTable)acTrans.GetObject(acBlkTblRec.DrawOrderTableId, OpenMode.ForWrite);
                    ObjectIdCollection tempCollection = new ObjectIdCollection();
                    tempCollection.Add(acHatch.ObjectId);
                    dot.MoveToBottom(tempCollection);
                }
            }
        }*/

        private void GenerateEnclosedRing(List<DBObjectCollection> existingRings, int ringIndex, int[] ringColors, float startDepth, SoilProperties soilProperties)
        {
            if (existingRings.Count > 0)
            {
                double currentDepth =  Math.Round(startDepth + soilProperties.TargetStepSize * ringIndex - soilProperties.ClimateReduction, 3);

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

                TreeRing newRing = TreeRing.Create(HostDocument, currentStep);
                newRing.Depth = currentDepth;

                //Protection for color overflow, loop around
                if (ringIndex >= ringColors.Length)
                {
                    int multiple = (int)Math.Floor((double)(ringIndex / ringColors.Length));
                    newRing.ColorIndex = ringColors[ringIndex - multiple * ringColors.Length];
                }
                else
                {
                    newRing.ColorIndex = ringColors[ringIndex];
                }

                RingsCollection.Add(currentDepth, newRing);
            }
        }
    }
}
