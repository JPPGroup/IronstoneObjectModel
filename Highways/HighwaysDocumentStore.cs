﻿using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    public class HighwaysDocumentStore : DocumentStore
    {
        public HighwaysDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager layerManager) : base(doc, managerTypes, log, layerManager) { }
    }
}
