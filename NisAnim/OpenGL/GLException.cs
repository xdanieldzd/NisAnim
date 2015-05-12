using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NisAnim.OpenGL
{
    class GLException : Exception
    {
        public GLException() : base() { }
        public GLException(string errorMessage) : base(errorMessage) { }
        public GLException(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
        public GLException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
