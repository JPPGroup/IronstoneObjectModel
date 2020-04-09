using System.Collections.Generic;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    // Must be class to be reference type
    public class DrawingInformation
    {
        public string DrawingNumber { get; set; }
        public string DrawingTitle { get; set; }
        public DrawingType Type { get; set; } 
        public string IssueType { get; set; }
        public string CurrentIssue { get; set; }

        public List<string> Revisions { get; } = new List<string>();
    }

    public enum DrawingType
    {
        Civil,
        Structural
    }
}
