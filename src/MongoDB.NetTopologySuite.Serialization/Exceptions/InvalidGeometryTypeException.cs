using System;
using System.Globalization;

namespace MongoDB.NetTopologySuite.Serialization.Exceptions
{
    public class InvalidGeometryTypeException : FormatException
    {
        public InvalidGeometryTypeException(string expectedType, string actualType)
            : base(string.Format(CultureInfo.InvariantCulture, $"Invalid GeoJson type: '{0}'. Expected: '{1}'.", actualType, expectedType))
        {
        }
    }
}
