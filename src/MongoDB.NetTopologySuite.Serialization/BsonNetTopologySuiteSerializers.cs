using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public static class BsonNetTopologySuiteSerializers
    {
        public static GeometryFactory Wgs84Factory => NtsGeometryServices.Instance.CreateGeometryFactory(4326);

        public static void Register(GeometryFactory? geometryFactory = default)
        {
            BsonSerializer.RegisterSerializationProvider(new BsonNetTopologySuiteSerializationProvider());

            TryRegisterSerializer(new FeatureCollectionSerializer());
            TryRegisterSerializer(new PointSerializer(geometryFactory));
            TryRegisterSerializer(new MultiPointSerializer(geometryFactory));
            TryRegisterSerializer(new PolygonSerializer(geometryFactory));
            TryRegisterSerializer(new MultiPolygonSerializer(geometryFactory));
            TryRegisterSerializer(new LineStringSerializer(geometryFactory));
            TryRegisterSerializer(new MultiLineStringSerializer(geometryFactory));
            TryRegisterSerializer(new GeometrySerializer());
            TryRegisterSerializer(new GeometryCollectionSerializer(geometryFactory));
            TryRegisterSerializer(new EnvelopeSerializer(geometryFactory?.PrecisionModel));
        }

        private static void TryRegisterSerializer<T>(IBsonSerializer<T> serializer)
        {
            try
            {
                BsonSerializer.RegisterSerializer(serializer);
            }
            catch (BsonSerializationException)
            {
                // Serializer is already registered.
            }
        }
    }
}
