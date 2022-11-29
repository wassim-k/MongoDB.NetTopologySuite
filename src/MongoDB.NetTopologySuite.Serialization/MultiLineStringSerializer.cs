using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class MultiLineStringSerializer : GeometrySerializerBase<MultiLineString, Coordinate[][]>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;
        private readonly LineStringSerializer _lineStringSerializer;

        public MultiLineStringSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
            _lineStringSerializer = new LineStringSerializer(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, MultiLineString value)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.NumGeometries; i++)
            {
                _lineStringSerializer.WriteCoordinates(writer, (LineString)value.GetGeometryN(i));
            }

            writer.WriteEndArray();
        }

        internal override Coordinate[][] ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadListOfCoordinates(reader).ToArray();
        }

        internal override MultiLineString Create(Coordinate[][] coordinates)
        {
            return _geometryFactory.CreateMultiLineString(coordinates
                .Select(coords => _lineStringSerializer.Create(coords))
                .ToArray());
        }
    }
}
