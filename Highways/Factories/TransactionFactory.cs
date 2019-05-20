using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Exceptions;

namespace Jpp.Ironstone.Highways.ObjectModel.Factories
{
    //MOVE: To Core
    public static class TransactionFactory
    {
        public static Transaction CreateFromTop()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var trans = db.TransactionManager.TopTransaction;
            if (trans == null) throw new TransactionException("No top transaction");

            return trans;
        }

        public static Transaction CreateFromNew()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;

            return db.TransactionManager.StartTransaction();
        }
    }
}
