using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Infrastructure.Exceptions
{
    public class DataAccessException : Exception
    {
        public DataAccessException(Exception ex, string message,ILogger logger)
        {
            logger.LogError($"Main Exception Is {ex} Developer CustomException is +" +
               $"{message}");
        }
    }
}
