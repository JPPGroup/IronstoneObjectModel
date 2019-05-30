using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Drainage.ObjectModel.Factories;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Managers
{
    [Serializable]
    public class DrainageRoutesManager : AbstractNotifiableDrawingObjectManagerManager<DrainageRoute>
    {
        public DrainageRoutesManager(Document document) : base(document) { }
        private DrainageRoutesManager() : base() { }

        public override void UpdateDirty()
        {
            UpdateAll();

            OnPropertyChanged("ManagedObjects");
        }

        public override void UpdateAll()
        {
            base.UpdateAll();

            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                ManagedObjects.ForEach(r => r.Generate());
                acTrans.Commit();
            }

            OnPropertyChanged("ManagedObjects");
        }

        public void BuildNewRoute(double initialInvert, double gradient, List<DrainageVertex> vertices)
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var route = new DrainageRoute(initialInvert, gradient, vertices);
                route.CreateActiveObject();
                ManagedObjects.Add(route);
                acTrans.Commit();
            }

            UpdateAll();
        }

        public void RemoveRoute(DrainageRoute route)
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                route.Erase();
                acTrans.Commit();
            }
        }

        public void HighlightRoute(DrainageRoute route)
        {
            foreach (var managedObject in ManagedObjects)
            {
                if (route != null && managedObject.Equals(route)) managedObject.Highlight();
                else managedObject.Unhighlight();
            }
        }
    }
}
