using System;

namespace ECommerce.Contracts.Exceptions
{
    public class BusinessEventSchemaValidationException : Exception
    {
        public BusinessEventSchemaValidationException(string message)
            : base(message)
        {
        }

        public BusinessEventSchemaValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

