using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest.Exceptions
{
    class CompressionException : Exception
    {
        public CompressionException()
        {}
        public CompressionException(string message)
            :base(message){}
        public CompressionException(string message, Exception innerException)
            :base(message, innerException){}
    }
}
