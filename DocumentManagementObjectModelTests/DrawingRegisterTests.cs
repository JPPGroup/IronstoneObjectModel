using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel.UnitTests
{
    [TestFixture]
    class DrawingRegisterTests
    {
        [Test]
        public void LoadTestCivilRegister()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Example Register.xlsx";
            DrawingRegister register = new DrawingRegister(path);

            IEnumerable<DrawingInformation> drawings = register.Drawings;

            Assert.AreEqual(6, drawings.Count());
            
            //Test C1
            DrawingInformation c1 = drawings.FirstOrDefault(di => di.DrawingNumber == "C1");
            Assert.IsNotNull(c1);
            Assert.AreEqual("Drawing Title 1", c1.DrawingTitle);
            Assert.AreEqual(DrawingType.Civil, c1.Type);
            Assert.AreEqual("pdf", c1.IssueType);
            Assert.AreEqual("T1", c1.Revisions[0]);
            Assert.AreEqual("T2", c1.Revisions[1]);
            Assert.AreEqual("T3", c1.Revisions[2]);

            //Test C2
            DrawingInformation c2 = drawings.FirstOrDefault(di => di.DrawingNumber == "C2");
            Assert.IsNotNull(c2);
            Assert.AreEqual("Drawing Title 2", c2.DrawingTitle);
            Assert.AreEqual(DrawingType.Civil, c2.Type);
            Assert.AreEqual("pdf", c2.IssueType);
            Assert.AreEqual("T1", c2.Revisions[0]);
            Assert.AreEqual(null, c2.Revisions[1]);
            Assert.AreEqual("T2", c2.Revisions[2]);

            //Test C3
            DrawingInformation c3 = drawings.FirstOrDefault(di => di.DrawingNumber == "C3");
            Assert.IsNotNull(c3);
            Assert.AreEqual("Drawing Title 3", c3.DrawingTitle);
            Assert.AreEqual(DrawingType.Civil, c3.Type);
            Assert.AreEqual("pdf", c3.IssueType);
            Assert.AreEqual(null, c3.Revisions[0]);
            Assert.AreEqual(null, c3.Revisions[1]);
            Assert.AreEqual("T1", c3.Revisions[2]);

            List<DateTime> civilDates = new List<DateTime>()
            {
                new DateTime(2010, 2, 1),
                new DateTime(2010, 2, 2),
                new DateTime(2010, 2, 3)
            };

            Assert.AreEqual(civilDates, register.CivilDates);
        }

        [Test]
        public void LoadTestStructuralRegister()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Example Register.xlsx";
            DrawingRegister register = new DrawingRegister(path);

            IEnumerable<DrawingInformation> drawings = register.Drawings;

            Assert.AreEqual(6, drawings.Count());

            //Test S1
            DrawingInformation s1 = drawings.FirstOrDefault(di => di.DrawingNumber == "S1");
            Assert.IsNotNull(s1);
            Assert.AreEqual("Structural Drawing Title 1", s1.DrawingTitle);
            Assert.AreEqual(DrawingType.Structural, s1.Type);
            Assert.AreEqual("pdf", s1.IssueType);
            Assert.AreEqual("T1", s1.Revisions[0]);
            Assert.AreEqual("T2", s1.Revisions[1]);
            Assert.AreEqual("T3", s1.Revisions[2]);

            //Test S2
            DrawingInformation s2 = drawings.FirstOrDefault(di => di.DrawingNumber == "S2");
            Assert.IsNotNull(s2);
            Assert.AreEqual("Structural Drawing Title 2", s2.DrawingTitle);
            Assert.AreEqual(DrawingType.Structural, s2.Type);
            Assert.AreEqual("pdf", s2.IssueType);
            Assert.AreEqual("T1", s2.Revisions[0]);
            Assert.AreEqual(null, s2.Revisions[1]);
            Assert.AreEqual("T2", s2.Revisions[2]);

            //Test S3
            DrawingInformation s3 = drawings.FirstOrDefault(di => di.DrawingNumber == "S3");
            Assert.IsNotNull(s3);
            Assert.AreEqual("Structural Drawing Title 3", s3.DrawingTitle);
            Assert.AreEqual(DrawingType.Structural, s3.Type);
            Assert.AreEqual("pdf", s3.IssueType);
            Assert.AreEqual(null, s3.Revisions[0]);
            Assert.AreEqual(null, s3.Revisions[1]);
            Assert.AreEqual("T1", s3.Revisions[2]);

            List<DateTime> structuralDates = new List<DateTime>()
            {
                new DateTime(2020, 2, 1),
                new DateTime(2020, 2, 2),
                new DateTime(2020, 2, 3)
            };

            Assert.AreEqual(structuralDates, register.StructuralDates);
        }

        [Test]
        public void ModifyTestRegister()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Example Register.xlsx";
            DrawingRegister register = new DrawingRegister(path);


            string modifiedPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\temp.xlsx";

            if(File.Exists(modifiedPath))
                File.Delete(modifiedPath);
            

            IEnumerable<DrawingInformation> drawings = register.Drawings;

            //Test C1
            DrawingInformation c1 = drawings.FirstOrDefault(di => di.DrawingNumber == "C1");
            c1.DrawingTitle = "Amended Drawing Title";
            register.WriteAs(modifiedPath);

            DrawingRegister modifiedRegister = new DrawingRegister(modifiedPath);

            IEnumerable<DrawingInformation> modifiedDrawings = modifiedRegister.Drawings;

            //Test C1
            DrawingInformation modifiedc1 = modifiedDrawings.FirstOrDefault(di => di.DrawingNumber == "C1");
            Assert.IsNotNull(modifiedc1);
            Assert.AreEqual("Amended Drawing Title", modifiedc1.DrawingTitle);
            Assert.AreEqual(DrawingType.Civil, modifiedc1.Type);
            Assert.AreEqual("pdf", modifiedc1.IssueType);
        }
    }
}
