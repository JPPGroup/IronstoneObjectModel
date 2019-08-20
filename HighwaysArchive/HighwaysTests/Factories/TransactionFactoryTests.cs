using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Factories
{
    [TestFixture]
    public class TransactionFactoryTests : IronstoneTestFixture
    {
        public TransactionFactoryTests() : base(Assembly.GetExecutingAssembly(), typeof(TransactionFactoryTests)) {}

        [Test]
        public void VerifyTransactionFromTopValid()
        {
            var result = RunTest<bool>(nameof(VerifyTransactionFromTopResident), true);
            Assert.IsTrue(result, "Should create transaction from top.");
        }

        [Test]
        public void VerifyTransactionFromTopInvalid()
        {
            var result = RunTest<bool>(nameof(VerifyTransactionFromTopResident), false);
            Assert.IsFalse(result, "Should not create transaction from top.");
        }

        public bool VerifyTransactionFromTopResident(bool startTransaction)
        {
            try
            {
                Transaction createdTrans;
                if (startTransaction)
                {
                    var db = Application.DocumentManager.MdiActiveDocument.Database;
                    using (var trans = db.TransactionManager.StartTransaction())
                    {
                        createdTrans = TransactionFactory.CreateFromTop();
                    }
                }
                else
                {
                    createdTrans = TransactionFactory.CreateFromTop();
                }

                return createdTrans != null;
            }
            catch
            {
                return false;
            }            
        }

        [Test]
        public void VerifyTransactionFromNew()
        {
            var result = RunTest<bool>(nameof(VerifyTransactionFromNewResident));
            Assert.IsTrue(result, "Should create new transaction.");
        }

        public bool VerifyTransactionFromNewResident()
        {
            try
            {
                using (var createdTrans = TransactionFactory.CreateFromNew())
                {
                    return createdTrans != null;
                }                
            }
            catch
            {
                return false;
            }
        }
    }
}
