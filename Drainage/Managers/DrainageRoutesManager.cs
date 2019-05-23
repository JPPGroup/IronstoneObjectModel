using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Drainage.ObjectModel.Factories;
using Jpp.Ironstone.Drainage.ObjectModel.Objects;

namespace Jpp.Ironstone.Drainage.ObjectModel.Managers
{
    [Serializable]
    public class DrainageRoutesManager : AbstractNotifiableDrawingObjectManagerManager
    {
        public List<DrainageRoute> Routes { get; }

        public DrainageRoutesManager(Document document) : base(document)
        {
            Routes = new List<DrainageRoute>();
        }

        private DrainageRoutesManager() : base()
        {
            Routes = new List<DrainageRoute>();
        }

        public override void UpdateDirty()
        {
            UpdateAll();
        }

        public override void UpdateAll()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                Routes.ForEach(r => r.Generate());
                acTrans.Commit();
            }
        }

        public override void Clear()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                Routes.ForEach(r =>
                {
                    r.Clear();
                    Routes.Remove(r);
                });

                acTrans.Commit();
            }
        }

        public override void AllDirty()
        {
            throw new NotImplementedException();
        }

        public override void ActivateObjects()
        {
            throw new NotImplementedException();
        }

        public void BuildNewRoute(double initialInvert, double gradient, List<DrainageVertex> vertices)
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                Routes.Add(new DrainageRoute(initialInvert, gradient, vertices));
                acTrans.Commit();
            }

            UpdateAll();

            OnPropertyChanged("Routes");
        }

        public void HighlightRoute(DrainageRoute selectedRouteModel)
        {
            foreach (var route in Routes)
            {
                if (selectedRouteModel != null && route.Equals(selectedRouteModel)) route.Highlight();
                else route.Unhighlight();
            }
        }

        public void RemoveRoute(DrainageRoute selectedRouteModel)
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                selectedRouteModel.Clear();
                Routes.Remove(selectedRouteModel);

                acTrans.Commit();
            }

            OnPropertyChanged("Routes");
        }
    }
}
