using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class MultiPointSerializer : GeometrySerializerBase<MultiPoint, Coordinate[]>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;
        private readonly PointSerializer _pointSerializer;

        public MultiPointSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
            _pointSerializer = new PointSerializer(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, MultiPoint value)
        {
            writer.WriteStartArray();

            for (int i = 0; i < value.NumGeometries; i++)
            {
                _pointSerializer.WriteCoordinates(writer, (Point)value.GetGeometryN(i));
            }

            writer.WriteEndArray();
        }

        internal override Coordinate[] ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadCoordinates(reader).ToArray();
        }

        internal override MultiPoint Create(Coordinate[] coordinates)
        {
            return _geometryFactory.CreateMultiPoint(coordinates
                .Select(coord => _pointSerializer.Create(coord))
                .ToArray());
        }
    }
}
