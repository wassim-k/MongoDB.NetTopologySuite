using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class MultiPolygonSerializer : GeometrySerializerBase<MultiPolygon, Coordinate[][][]>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;
        private readonly PolygonSerializer _polygonSerializer;

        public MultiPolygonSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
            _polygonSerializer = new PolygonSerializer(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, MultiPolygon value)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.NumGeometries; i++)
            {
                _polygonSerializer.WriteCoordinates(writer, (Polygon)value.GetGeometryN(i));
            }

            writer.WriteEndArray();
        }

        internal override Coordinate[][][] ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadListOfListOfCoordinates(reader).ToArray();
        }

        internal override MultiPolygon Create(Coordinate[][][] coordinates)
        {
            return _geometryFactory.CreateMultiPolygon(coordinates
                .Select(coords => _polygonSerializer.Create(coords))
                .ToArray());
        }
    }
}
