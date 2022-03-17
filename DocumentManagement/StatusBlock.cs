using EnumsNET;
using Jpp.Ironstone.Core.Autocad;
using System.ComponentModel;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class StatusBlock : BlockRefDrawingObject
    {
        public StatusBlock(BlockRefDrawingObject reference) : base()
        {
            this._document = reference.Document;
            this._database = reference.Database;
            this.BaseObject = reference.BaseObject;

            GetProperties();
        }

        public StatusOptions Status
        {
            get { 
                return Enums.Parse<StatusOptions>(GetProperty<string>("STATUS"));
            }
            set { SetProperty("STATUS", Enums.AsString(value)); }
        }

        public enum StatusOptions
        {
            [Description("FOR PLANNING")]
            Planning,
            [Description("FOR INFORMATION")]
            Information,
            [Description("FOR COMMENT")]
            Comment,
            [Description("FOR TECHNICAL APPROVAL")]
            TechnicalApproval,
            [Description("PRELIMINARY")]
            Preliminary,
            [Description("TENDER")]
            Tender,
            [Description("CONSTRUCTION")]
            Construction,
            [Description("AS BUILT")]
            AsBuilt,
            [Description("WITHDRAWN")]
            Withdrawn
        }
    }
}
