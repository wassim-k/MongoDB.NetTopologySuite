using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.NetTopologySuite.Serialization.Helpers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class LineStringSerializer : GeometrySerializerBase<LineString, Coordinate[]>
    {
        private readonly CoordinateHelper _coordinateHelper;
        private readonly GeometryFactory _geometryFactory;

        public LineStringSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? Wgs84Factory;
            _coordinateHelper = new CoordinateHelper(_geometryFactory);
        }

        internal override void WriteCoordinates(IBsonWriter writer, LineString value)
        {
            _coordinateHelper.WriteCoordinates(writer, value.CoordinateSequence);
        }

        internal override Coordinate[] ReadCoordinates(IBsonReader reader)
        {
            return _coordinateHelper.ReadCoordinates(reader).ToArray();
        }

        internal override LineString Create(Coordinate[] coordinates)
        {
            return _geometryFactory.CreateLineString(coordinates);
        }
    }
}
