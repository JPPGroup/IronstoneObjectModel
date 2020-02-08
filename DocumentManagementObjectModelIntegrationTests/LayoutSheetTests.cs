using System;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using NUnit.Framework;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel.IntegrationTests
{
    [TestFixture]
    class LayoutSheetTests : IronstoneTestFixture
    {
        public LayoutSheetTests() : base(Assembly.GetExecutingAssembly(), typeof(LayoutSheetTests), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CivilTemplate.dwg")
        { }

        [TestCase("CIV_A0L", "", "", "")]
        [TestCase("CIV_A1L", "", "Test Client", "Test Project")]
        [TestCase("CIV_A2L", "", "", "")]
        [TestCase("CIV_A3L", "", "", "")]
        /*[TestCase("CIV_A4P", "", "", "")]
        [TestCase("CIV_A0P", "", "", "")]
        [TestCase("CIV_A1P", "", "", "")]
        [TestCase("CIV_A2P", "", "", "")]
        [TestCase("CIV_A3P", "", "", "")]*/
        public void TestReadingTitleBlock(string layoutName, string expectedTitle, string expectedClient, string expectedProject)
        {
            TitleResponse response = RunTest<TitleResponse>(nameof(TestReadingTitleBlockResident), layoutName);
            Assert.AreEqual(expectedTitle, response.Title);
            Assert.AreEqual(expectedClient, response.Client);

        }

        public TitleResponse TestReadingTitleBlockResident(string layoutName)
        {
            using (Transaction trans =
                Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                Layout target = Application.DocumentManager.MdiActiveDocument.Database.GetLayout(layoutName);

                LayoutSheet layoutSheet = new LayoutSheet(null, target);
                TitleResponse response = new TitleResponse()
                {
                    Title = layoutSheet.TitleBlock.Title,
                    Client = layoutSheet.TitleBlock.Client,
                    Project = layoutSheet.TitleBlock.Project,
                    DrawingNumber = layoutSheet.TitleBlock.DrawingNumber,
                    ProjectNumber = layoutSheet.TitleBlock.ProjectNumber,
                    Revision = layoutSheet.TitleBlock.Revision,
                };

                return response;
            }
        }

        [Serializable]
        public struct TitleResponse
        {
            public string Title { get; set; }
            public string Client { get; set; }
            public string Project { get; set; }
            public string DrawingNumber { get; set; }
            public string ProjectNumber { get; set; }
            public string Revision { get; set; }
        }
    }
}
