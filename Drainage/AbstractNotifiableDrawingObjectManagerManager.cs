using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Drainage.ObjectModel
{
    //MOVE: To Core...
    public abstract class AbstractNotifiableDrawingObjectManagerManager<T> : AbstractDrawingObjectManager<T>, INotifyPropertyChanged where T : DrawingObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected AbstractNotifiableDrawingObjectManagerManager(Document document) : base(document) { }
        protected AbstractNotifiableDrawingObjectManagerManager() { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<TF>(ref TF field, TF value, string propertyName)
        {
            if (EqualityComparer<TF>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
