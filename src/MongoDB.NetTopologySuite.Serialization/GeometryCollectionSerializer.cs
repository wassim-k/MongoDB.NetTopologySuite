using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.NetTopologySuite.Serialization.Exceptions;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class GeometryCollectionSerializer : ClassSerializerBase<GeometryCollection>
    {
        private static readonly Lazy<IBsonSerializer<Geometry>> _geometrySerializer = new(() => BsonSerializer.LookupSerializer<Geometry>());

        private readonly SerializerHelper _helper;
        private readonly GeometryFactory _geometryFactory;

        public GeometryCollectionSerializer(GeometryFactory? geometryFactory = default)
        {
            _geometryFactory = geometryFactory ?? BsonNetTopologySuiteSerializers.Wgs84Factory;
            _helper = new SerializerHelper(
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("geometries", Flags.Geometries));
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, GeometryCollection geometries)
        {
            var writer = context.Writer;

            writer.WriteStartDocument();

            writer.WriteName("type");
            writer.WriteString(nameof(GeometryCollection));

            writer.WriteName("geometries");
            writer.WriteStartArray();

            foreach (var geometry in geometries)
            {
                _geometrySerializer.Value.Serialize(context, geometry);
            }

            writer.WriteEndArray();
            writer.WriteEndDocument();
        }

        public override GeometryCollection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            var geometries = new List<Geometry>();

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Type:
                        var type = reader.ReadString();
                        if (type != nameof(GeometryCollection))
                        {
                            throw new InvalidGeometryTypeException(nameof(GeometryCollection), type);
                        }

                        break;
                    case Flags.Geometries:
                        reader.ReadStartArray();

                        while (reader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            var geometry = _geometrySerializer.Value.Deserialize(context);
                            geometries.Add(geometry);
                        }

                        reader.ReadEndArray();
                        break;
                    default: reader.SkipValue(); break;
                }
            });

            return _geometryFactory.CreateGeometryCollection(geometries.ToArray());
        }

        private class Flags
        {
            public const long Type = 1;
            public const long Geometries = 2;
        }
    }
}
