using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using System;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    public class RoadLabel : DrawingObject
    {
        protected override void GenerateBase(Database database)
        {
            var acTrans = database.TransactionManager.TopTransaction;
            var acBlkTbl = (BlockTable) acTrans.GetObject(database.BlockTableId, OpenMode.ForRead);
            var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            
            var label = new MText
            {
                Location = new Point3d(0, 0, 0),
                Layer = Constants.LAYER_DEF_POINTS,
                TextHeight = 0.5
            };

            BaseObject = acBlkTblRec.AppendEntity(label);
            acTrans.AddNewlyCreatedDBObject(label, true);
        }

        protected override void ObjectModified(object sender, EventArgs e) { }
        protected override void ObjectErased(object sender, ObjectErasedEventArgs e) { }

        [XmlIgnore] public string Contents
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                return text.Contents;
            }
            set
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
                text.Contents = value;
            }
        }
        [XmlIgnore] public override Point3d Location
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText) acTrans.GetObject(BaseObject, OpenMode.ForRead);
                return text.Location;
            }
            set
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
                text.Location = value;
            }
        }
        [XmlIgnore] public override double Rotation
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                return text.Rotation;
            }
            set
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
                text.Rotation = value;
            }
        }
        [XmlIgnore]
        public AttachmentPoint Attachment
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForRead);
                return text.Attachment;
            }
            set
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
                text.Attachment = value;
            }
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override void Erase()
        {
            using var acTrans = BaseObject.Database.TransactionManager.StartTransaction();
            var text = (MText)acTrans.GetObject(BaseObject, OpenMode.ForWrite);
            text.Erase(true);
            acTrans.Commit();
        }
    }
}
