using System;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class TitleBlock : BlockRefDrawingObject
    {
        public TitleBlock(BlockRefDrawingObject reference) : base()
        {
            this.BaseObject = reference.BaseObject;
        }

        public string Client
        {
            get
            {
                string client1 = GetProperty<string>("CLIENT1");
                string client2 = GetProperty<string>("CLIENT2");

                return String.IsNullOrEmpty(client2) ? client1 : $"{client1}\n{client2}";
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
        }

        public string ProjectNumber
        {
            get { return GetProperty<string>("PROJECTNO"); }
        }

        public string DrawingNumber
        {
            get { return GetProperty<string>("DRAWINGNO."); }
        }

        public string Revision
        {
            get { return GetProperty<string>("REV."); }
        }
    }
}
