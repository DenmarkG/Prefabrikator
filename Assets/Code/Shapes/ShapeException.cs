using System;

namespace Prefabrikator.Shapes
{
    public class ShapeException : Exception
    {
        public ShapeException()
            : base() { }

        public ShapeException(string message)
            : base(message) { }

        public ShapeException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}