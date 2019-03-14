using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Exceptions
{
    public class TransactionException : Exception
    {       
        public TransactionException(string message) : base(message) { }
    }
}
