using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Drainage.ObjectModel
{
    public abstract class AbstractNotifiableDrawingObjectManagerManager : AbstractDrawingObjectManager, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected AbstractNotifiableDrawingObjectManagerManager(Document document) : base(document) { }
        protected AbstractNotifiableDrawingObjectManagerManager() { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
