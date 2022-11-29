using System;
using System.Globalization;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class GeometrySerializer : ClassSerializerBase<Geometry>
    {
        protected override Type GetActualType(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var bookmark = bsonReader.GetBookmark();
            bsonReader.ReadStartDocument();
            if (bsonReader.FindElement("type"))
            {
                var discriminator = bsonReader.ReadString();
                bsonReader.ReturnToBookmark(bookmark);

                switch (discriminator)
                {
                    case "GeometryCollection": return typeof(GeometryCollection);
                    case "LineString": return typeof(LineString);
                    case "MultiLineString": return typeof(MultiLineString);
                    case "MultiPoint": return typeof(MultiPoint);
                    case "MultiPolygon": return typeof(MultiPolygon);
                    case "Point": return typeof(Point);
                    case "Polygon": return typeof(Polygon);
                    default:
                        var message = string.Format(CultureInfo.InvariantCulture, "The type field of the GeoJsonGeometry is not valid: '{0}'.", discriminator);
                        throw new FormatException(message);
                }
            }
            else
            {
                throw new FormatException("GeoJsonGeometry object is missing the type field.");
            }
        }
    }
}
