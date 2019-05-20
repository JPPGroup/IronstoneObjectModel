using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Exceptions
{
    //MOVE: To Core
    public class TransactionException : Exception
    {       
        public TransactionException(string message) : base(message) { }
    }
}
