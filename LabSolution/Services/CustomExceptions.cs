using System;
using System.Runtime.Serialization;

namespace LabSolution.Services
{
    [Serializable]
    internal class CustomExceptions : Exception
    {
        public CustomExceptions()
        {
        }

        public CustomExceptions(string message) : base(message)
        {
        }

        public CustomExceptions(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CustomExceptions(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}