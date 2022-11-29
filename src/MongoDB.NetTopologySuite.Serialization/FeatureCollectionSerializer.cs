using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.NetTopologySuite.Serialization.Exceptions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class FeatureCollectionSerializer : ClassSerializerBase<FeatureCollection>
    {
        private static readonly Lazy<IBsonSerializer<Envelope>> _envelopeSerializer = new(() => BsonSerializer.LookupSerializer<Envelope>());
        private static readonly Lazy<IBsonSerializer<Feature>> _featureSerializer = new(() => BsonSerializer.LookupSerializer<Feature>());

        private readonly SerializerHelper _helper;

        public FeatureCollectionSerializer()
        {
            _helper = new SerializerHelper(
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("features", Flags.Features),
                new SerializerHelper.Member("bbox", Flags.BoundingBox, true));
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, FeatureCollection features)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();

            writer.WriteName("type");
            writer.WriteString(nameof(FeatureCollection));

            writer.WriteName("features");
            writer.WriteStartArray();

            foreach (var feature in features)
            {
                _featureSerializer.Value.Serialize(context, feature);
            }

            writer.WriteEndArray();

            if (features.BoundingBox != null)
            {
                writer.WriteName("bbox");
                _envelopeSerializer.Value.Serialize(context, features.BoundingBox);
            }

            writer.WriteEndDocument();
        }

        public override FeatureCollection Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            var features = new FeatureCollection();

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Type:
                        var type = reader.ReadString();
                        if (type != nameof(FeatureCollection))
                        {
                            throw new InvalidGeometryTypeException(nameof(FeatureCollection), type);
                        }

                        break;
                    case Flags.BoundingBox: features.BoundingBox = _envelopeSerializer.Value.Deserialize(context); break;
                    case Flags.Features:

                        reader.ReadStartArray();

                        while (reader.ReadBsonType() != BsonType.EndOfDocument)
                        {
                            features.Add(_featureSerializer.Value.Deserialize(context));
                        }

                        reader.ReadEndArray();
                        break;
                    default: reader.SkipValue(); break;
                }
            });

            return features;
        }

        private class Flags
        {
            public const long Type = 1;
            public const long Features = 2;
            public const long BoundingBox = 4;
        }
    }
}
