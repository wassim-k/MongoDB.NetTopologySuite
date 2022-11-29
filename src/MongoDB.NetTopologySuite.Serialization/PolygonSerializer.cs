using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class PolygonSerializer : GeometrySerializerBase<Polygon, Coordinate[][]>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;

        public PolygonSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, Polygon value)
        {
            writer.WriteStartArray();

            if (!value.IsEmpty)
            {
                _coordinateHelper.WriteCoordinates(writer, value.ExteriorRing.CoordinateSequence);

                for (int i = 0; i < value.NumInteriorRings; i++)
                {
                    _coordinateHelper.WriteCoordinates(writer, value.GetInteriorRingN(i).CoordinateSequence);
                }
            }

            writer.WriteEndArray();
        }

        internal override Coordinate[][] ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadListOfCoordinates(reader).ToArray();
        }

        internal override Polygon Create(Coordinate[][] coordinates)
        {
            return _geometryFactory.CreatePolygon(
                _geometryFactory.CreateLinearRing(coordinates[0].ToArray()),
                coordinates.Skip(1).Select(coords => _geometryFactory.CreateLinearRing(coords.ToArray())).ToArray());
        }
    }
}
