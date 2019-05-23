using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Extensions
{
    //MOVE: To Core...
    public static class MTextExtension
    {
        public static void AlignTo(this MText dBText, Curve curve)
        {
            var line = curve.StartPoint.GetVectorTo(curve.EndPoint);
            var angle = line.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;

            //Alignment and position points get swapped under different alignment modes http://adndevblog.typepad.com/autocad/2012/12/specifying-text-alignment.html
            if (angle < 180)
            {
                dBText.Rotation = (90 - angle) * Math.PI / 180;
                dBText.Attachment = AttachmentPoint.MiddleRight;          
            }
            else
            {
                dBText.Rotation = (90 - angle + 180) * Math.PI / 180;                          
                dBText.Attachment = AttachmentPoint.MiddleLeft;
            }
        }

    }
}
