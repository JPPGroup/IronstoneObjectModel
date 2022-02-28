using System;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class TitleBlock : BlockRefDrawingObject
    {
        public TitleBlock(BlockRefDrawingObject reference) : base()
        {
            this._document = reference.Document;
            this._database = reference.Database;
            this.BaseObject = reference.BaseObject;

            GetProperties();
        }

        public string Client
        {
            get
            {
                string client1 = GetCachedProperty<string>("CLIENT1");
                string client2 = GetCachedProperty<string>("CLIENT2");

                return String.IsNullOrEmpty(client2) ? client1 : $"{client1}\n{client2}";
            }
            set
            {
                string[] parts = value.Split('\n'); 
                SetProperty("CLIENT1", parts[0]);
                if (parts.Length > 1)
                {
                    SetProperty("CLIENT2", parts[1]);
                }
            }
        }

        public string Project
        {
            get
            {
                string project1 = GetCachedProperty<string>("PROJECT1");
                string project2 = GetCachedProperty<string>("PROJECT2");
                string project3 = GetCachedProperty<string>("PROJECT3");

                return String.IsNullOrEmpty(project3)
                    ? String.IsNullOrEmpty(project2) ? project1 : $"{project1}\n{project2}"
                    : $"{project1}\n{project2}\n{project3}";
            }
            set
            {
                string[] parts = value.Split('\n'); 
                SetProperty("PROJECT1", parts[0]);
                if (parts.Length > 1)
                {
                    SetProperty("PROJECT2", parts[1]);
                }
                if (parts.Length > 2)
                {
                    SetProperty("PROJECT3", parts[2]);
                }
            }
        }

        public string Title
        {
            get
            {
                string title1 = GetCachedProperty<string>("TITLE1");
                string title2 = GetCachedProperty<string>("TITLE2");
                string title3 = GetCachedProperty<string>("TITLE3");

                return String.IsNullOrEmpty(title3)
                    ? String.IsNullOrEmpty(title2) ? title1 : $"{title1}\n{title2}"
                    : $"{title1}\n{title2}\n{title3}";
            }
            set
            {
                string[] parts = value.Split('\n'); 
                SetProperty("TITLE1", parts[0]);
                if (parts.Length > 1)
                {
                    SetProperty("TITLE2", parts[1]);
                }
                if (parts.Length > 2)
                {
                    SetProperty("TITLE3", parts[2]);
                }
            }
        }

        public string ProjectNumber
        {
            get { return GetCachedProperty<string>("PROJECTNO"); }
            set { SetProperty("PROJECTNO", value); }
        }

        public string DrawingNumber
        {
            get { return GetCachedProperty<string>("DRAWINGNO."); }
            set { SetProperty("DRAWINGNO.", value); }

        }

        public string Revision
        {
            get { return GetCachedProperty<string>("REV."); }
            set { SetProperty("REV.", value); }
        }

        public string DrawnBy
        {
            get { return GetCachedProperty<string>("DRAWNBY"); }
            set { SetProperty("DRAWNBY", value); }
        }

        public string CheckedBy
        {
            get { return GetCachedProperty<string>("CHECKEDBY"); }
            set { SetProperty("CHECKEDBY", value); }
        }

        public string Date
        {
            get { return GetCachedProperty<string>("DATE"); }
            set { SetProperty("DATE", value); }
        }

        public string Scale
        {
            get { return GetProperty<string>("SCALE"); }
            set { SetProperty("SCALE", value); }
        }
    }
}
