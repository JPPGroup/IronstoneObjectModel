using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Drainage.ObjectModel.Factories;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Managers
{
    [Serializable]
    public class DrainageRoutesManager : AbstractNotifiableDrawingObjectManagerManager<DrainageRoute>
    {
        public DrainageRoutesManager(Document document, ILogger log) : base(document, log) { }
        private DrainageRoutesManager() : base() { }

        public override void UpdateDirty()
        {
            UpdateAll();

            OnPropertyChanged(nameof(ActiveObjects));
        }

        public override void UpdateAll()
        {
            base.UpdateAll();

            using (var acTrans = TransactionFactory.CreateFromNew())
            {

                foreach (var obj in ActiveObjects)
                {
                    obj.Generate();
                }

                acTrans.Commit();
            }

            OnPropertyChanged(nameof(ActiveObjects));
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
