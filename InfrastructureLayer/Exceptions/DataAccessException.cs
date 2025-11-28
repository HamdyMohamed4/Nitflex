using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Exceptions
{
    public class DataAccessException : Exception
    {
        public DataAccessException(Exception ex,string customMessage,ILogger logger) 
        { 
            logger.LogError($"main exception {ex.Message} developer custom exception " +
                $"{customMessage}");



        }



        public DataAccessException() : base("Database operation failed") { }

        public DataAccessException(string message) : base(message) { }

        public DataAccessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
