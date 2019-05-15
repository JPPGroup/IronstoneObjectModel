using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Drainage.ObjectModel.Factories
{
    //MOVE: To Core...
    public static class TransactionFactory
    {
        public static Transaction CreateFromTop()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var trans = db.TransactionManager.TopTransaction;
            if (trans == null) throw new ArgumentException("No top transaction");

            return trans;
        }

        public static Transaction CreateFromNew()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            return db.TransactionManager.StartTransaction();
        }
    }
}
