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
                string client1 = GetProperty<string>("CLIENT1");
                string client2 = GetProperty<string>("CLIENT2");

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
                string project1 = GetProperty<string>("PROJECT1");
                string project2 = GetProperty<string>("PROJECT2");
                string project3 = GetProperty<string>("PROJECT3");

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
                string title1 = GetProperty<string>("TITLE1");
                string title2 = GetProperty<string>("TITLE2");
                string title3 = GetProperty<string>("TITLE3");

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
            get { return GetProperty<string>("PROJECTNO"); }
            set { SetProperty("PROJECTNO", value); }
        }

        public string DrawingNumber
        {
            get { return GetProperty<string>("DRAWINGNO."); }
            set { SetProperty("DRAWINGNO.", value); }

        }

        public string Revision
        {
            get { return GetProperty<string>("REV."); }
            set { SetProperty("REV.", value); }
        }

        public string DrawnBy
        {
            get { return GetProperty<string>("DRAWNBY"); }
            set { SetProperty("DRAWNBY", value); }
        }

        public string CheckedBy
        {
            get { return GetProperty<string>("CHECKEDBY"); }
            set { SetProperty("CHECKEDBY", value); }
        }

        public string Date
        {
            get { return GetProperty<string>("DATE"); }
            set { SetProperty("DATE", value); }
        }
    }
}
