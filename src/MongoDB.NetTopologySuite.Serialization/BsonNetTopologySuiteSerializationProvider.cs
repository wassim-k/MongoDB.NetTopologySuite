using System;
using MongoDB.Bson.Serialization;
using NetTopologySuite.Features;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class BsonNetTopologySuiteSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (typeof(IFeature).IsAssignableFrom(type))
            {
                var serializerType = typeof(FeatureSerializer<>).MakeGenericType(type);
                return (IBsonSerializer)Activator.CreateInstance(serializerType);
            }
            else if (typeof(IAttributesTable).IsAssignableFrom(type))
            {
                var serializerType = typeof(AttributesTableSerializer<>).MakeGenericType(type);
                return (IBsonSerializer)Activator.CreateInstance(serializerType);
            }

            return default!;
        }
    }
}
