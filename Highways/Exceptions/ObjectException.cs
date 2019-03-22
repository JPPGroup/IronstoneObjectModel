using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Highways.ObjectModel.Exceptions
{
    public class ObjectException : Exception
    {
        public ObjectException(string message, ObjectId objectId) : base(message) { }
    }
}
