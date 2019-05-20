﻿using System;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Highways.ObjectModel.Exceptions
{
    //MOVE: To Core
    public class ObjectException : Exception
    {
        public ObjectException(string message, ObjectId objectId) : base(message) { }
    }
}
