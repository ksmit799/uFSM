using System;

namespace uFSM
{
    public class FSMError : Exception
    {
        public FSMError(string message) : base(message)
        {}
    }
}