using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    [XmlInclude(typeof(HedgeRow))]
    [Layer(Name = Constants.LABEL_LAYER)]
    public class Tree : CircleDrawingObject
    {
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private string _ID;

        public string Comments { get; set; }

        public float Height
        {
            get
            {
                if (ActualHeight > MaxSpeciesHeight)
                {
                    return ActualHeight;
                }

                return ActualHeight < MaxSpeciesHeight / 2 ? ActualHeight : MaxSpeciesHeight;
            }
        }

        public float MaxSpeciesHeight
        {
            get
            {
                var speciesList = GetSpeciesList(WaterDemand, TreeType);
                return (float)speciesList[Species];
            }
        }

        public static Dictionary<string, int> GetSpeciesList(WaterDemand waterDemand, TreeType type)
        {
            switch (waterDemand)
            {
                case WaterDemand.High when type == TreeType.Deciduous:
                    return Tree.DeciduousHigh;
                case WaterDemand.High when type == TreeType.Coniferous:
                    return Tree.ConiferousHigh;
                case WaterDemand.Medium when type == TreeType.Deciduous:
                    return Tree.DeciduousMedium;
                case WaterDemand.Medium when type == TreeType.Coniferous:
                    return Tree.ConiferousMedium;
                case WaterDemand.Low when type == TreeType.Deciduous:
                    return Tree.DeciduousLow;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ActualHeight { get; set; }

        public string Species { get; set; }

        public WaterDemand WaterDemand { get; set; }

        public TreeType TreeType { get; set; }

        public Phase Phase { get; set; }

        public bool ExceedsNHBC { get; }

        public bool ToBeRemoved { get; set; }

        //TODO: Rework to use subobjects
        public MTextDrawingObject Label { get; set; }        

        public Tree() : base()
        {

        }

        public override void Generate()
        {

        }

        public override void Erase()
        {
            throw new NotImplementedException();
        }

        protected override void GenerateBase()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

            // Open the Block table for read
            BlockTable acBlkTbl;
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the Block table record Model space for write
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            //Draw Trunk
            Circle trunk = new Circle
            {
                Center = new Point3d(0, 0, 0), 
                Radius = 0.25
            };
            // Add the new object to the block table record and the transaction
            this.BaseObject = acBlkTblRec.AppendEntity(trunk);
            acTrans.AddNewlyCreatedDBObject(trunk, true);
        }

        public void AddLabel()
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            LayerManager layerManager = DataService.Current.GetStore<DocumentStore>(acDoc.Name).LayerManager;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

            // Open the Block table for read
            BlockTable acBlkTbl;
            acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

            // Open the Block table record Model space for write
            BlockTableRecord acBlkTblRec;
            acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            string nhbcWarning = "";
            if (ExceedsNHBC)
            {
                nhbcWarning = "*";
            }            

            MTextDrawingObject mText = MTextDrawingObject.Create(this.Database, Location, $"No. {ID}\\P{Species}\\PDesign Height: {Height}m{nhbcWarning}\\PActual Height: {ActualHeight}m");
            mText.SetLayer(Constants.LABEL_LAYER);                      

            Label = mText;
        }

        public virtual DBObjectCollection DrawRings(Shrinkage shrinkage, double StartDepth, double Step)
        {
            DBObjectCollection collection = new DBObjectCollection();

            bool next = true;
            double currentDepth = StartDepth;

            while (next)
            {
                Circle acCirc = DrawShape(currentDepth, shrinkage) as Circle;
                if (acCirc != null)
                {
                    collection.Add(acCirc);
                }
                else
                {
                    next = false;
                }

                currentDepth = currentDepth + Step;
            }

            return collection;
        }

        public virtual Curve DrawShape(double depth, Shrinkage shrinkage)
        {
            return DrawRing(depth, shrinkage, Location);
        }

        protected Circle DrawRing(double depth, Shrinkage shrinkage, Point3d location)
        {
            var radius = GetRingRadius(depth, shrinkage);

            if (radius <= 0) return null;

            return new Circle
            {
                Center = new Point3d(location.X, location.Y, 0), 
                Radius = radius
            };
        }

        protected double M(Shrinkage shrinkage)
        {
            switch (TreeType)
            {
                case TreeType.Coniferous:
                    switch (shrinkage)
                    {
                        case Shrinkage.High:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.25;

                                case WaterDemand.Medium:
                                    return -0.25;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;

                        case Shrinkage.Medium:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.2869;

                                case WaterDemand.Medium:
                                    return -0.3107;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;

                        case Shrinkage.Low:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.3432;

                                case WaterDemand.Medium:
                                    return -0.4127;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;
                    }
                    break;

                case TreeType.Deciduous:
                    switch (shrinkage)
                    {
                        case Shrinkage.High:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.5;

                                case WaterDemand.Medium:
                                    return -0.542;

                                case WaterDemand.Low:
                                    return -0.625;
                            }
                            break;

                        case Shrinkage.Medium:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.5907;

                                case WaterDemand.Medium:
                                    return -0.6837;

                                case WaterDemand.Low:
                                    return -0.8333;
                            }
                            break;

                        case Shrinkage.Low:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return -0.7204;

                                case WaterDemand.Medium:
                                    return -0.8625;

                                case WaterDemand.Low:
                                    return -1.1111;
                            }
                            break;
                    }
                    break;
            }

            return 0;
        }

        protected double C(Shrinkage shrinkage)
        {
            switch (TreeType)
            {
                case TreeType.Coniferous:
                    switch (shrinkage)
                    {
                        case Shrinkage.High:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 0.85;

                                case WaterDemand.Medium:
                                    return 0.6;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;

                        case Shrinkage.Medium:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 0.8586;

                                case WaterDemand.Medium:
                                    return 0.6401;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;

                        case Shrinkage.Low:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 0.8601;

                                case WaterDemand.Medium:
                                    return 0.6608;

                                case WaterDemand.Low:
                                    throw new NotImplementedException();
                            }
                            break;
                    }
                    break;

                case TreeType.Deciduous:
                    switch (shrinkage)
                    {
                        case Shrinkage.High:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 1.75;

                                case WaterDemand.Medium:
                                    return 1.29;

                                case WaterDemand.Low:
                                    return 1.125;
                            }
                            break;

                        case Shrinkage.Medium:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 1.7783;

                                case WaterDemand.Medium:
                                    return 1.3643;

                                case WaterDemand.Low:
                                    return 1.25;
                            }
                            break;

                        case Shrinkage.Low:
                            switch (WaterDemand)
                            {
                                case WaterDemand.High:
                                    return 1.7912;

                                case WaterDemand.Medium:
                                    return 1.4109;

                                case WaterDemand.Low:
                                    return 1.3333;
                            }
                            break;
                    }
                    break;
            }

            return 0;
        }

        protected double GetRingRadius(double foundationDepth, Shrinkage shrinkage)
        {
            double dh = M(shrinkage) * foundationDepth + C(shrinkage);
            double actualRadius = dh * Height;
            double roundedRadius = Math.Ceiling(actualRadius * 100) / 100;

            //Check for "sapling" to avoid excessive rings
            if (ToBeRemoved && ActualHeight < MaxSpeciesHeight / 2)
            {
                if (actualRadius < 2)
                {
                    return -1;
                }
            }

            return roundedRadius;
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            //Nothing to do
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            //Nothing to do on erase
        }

        #region Tree Data
        public static Dictionary<string, int> DeciduousHigh = new Dictionary<string, int>()
        {
            { "EnglishElm",24 },
            { "WheatleyElm",22 },
            { "WHychElm",18 },
            { "EUcalyptus",18 },
            { "Hawthorn",10 },
            { "ENglishOak",20 },
            { "HOlmOak",16 },
            { "RedOak",24 },
            { "TurkeyOak",24 },
            { "HYbridBlackPoplar",28 },
            { "LombardyPoplar",25 },
            { "WHItePoplar",15 },
            { "CrackWillow",24 },
            { "WEepingWillow",16 },
            { "WHITeWillow",24 },
        };

        public static Dictionary<string, int> DeciduousMedium = new Dictionary<string, int>()
        {
            { "Acacia",18 }, 
            { "ALder",18 },
            { "APple",10 },
            { "ASh",23 },
            { "BayLaurel",10 },
            { "BEech",20 },
            { "BLackthorn",8 },
            { "JapaneseCherry",9 },
            { "LaurelCherry",8 },
            { "OrchardCherry",12 },
            { "WildCherry",17 },
            { "HorseChestnut",20 },
            { "SweetChestnut",24 },
            { "LIme",22 },
            { "JApaneseMaple",8 },
            { "NorwayMaple",18 },
            { "MountainAsh",11 },
            { "Pear",12 },
            { "PLane",26 },
            { "PLUm",10 },
            { "SYcamore",22 },
            { "TreeOfHeaven",20 },
            { "WAlnut",18 },
            { "WHitebeam",12 }
        };

        public static Dictionary<string, int> DeciduousLow = new Dictionary<string, int>()
        {
            { "Birch",14 },
            { "Elder",10 },
            { "Fig",8 },
            { "Hazel",8 },
            { "HOlly",12 },
            { "HONeyLocust",14 },
            { "HORnbeam",17 },
            { "Laburnum",12 },
            { "Magnolia",9 },
            { "MUlberry",9 },
            { "TulipTree",20 },

        };

        public static Dictionary<string, int> ConiferousHigh = new Dictionary<string, int>()
        {
            { "LawsonsCypress",18 },
            { "LEylandCypress",20 },
            { "MontereyCypress",20 },
        };

        public static Dictionary<string, int> ConiferousMedium = new Dictionary<string, int>()
        {
            { "Cedar",20 },
            { "DouglasFir",20 },
            { "Larch",20 },
            { "MonkeyPuzzle",18 },
            { "Pine",20 },
            { "Spruce",18 },
            { "Wellingtonia",30 },
            { "Yew",12 },
        };

        #endregion

    }

    public enum WaterDemand
    {
        Low,
        Medium,
        High
    }

    public enum TreeType
    {
        Deciduous,
        Coniferous
    }

    public enum Phase
    {
        Proposed,
        Existing
    }
}
