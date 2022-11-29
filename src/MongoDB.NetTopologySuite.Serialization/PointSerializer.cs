using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class PointSerializer : GeometrySerializerBase<Point, Coordinate>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;

        public PointSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, Point value)
        {
            _coordinateHelper.WriteCoordinate(writer, value.CoordinateSequence);
        }

        internal override Coordinate ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadCoordinate(reader);
        }

        internal override Point Create(Coordinate coordinates)
        {
            return _geometryFactory.CreatePoint(coordinates);
        }
    }
}
