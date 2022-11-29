using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.NetTopologySuite.Serialization.Exceptions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class FeatureSerializer<TFeature> : SerializerBase<TFeature>
        where TFeature : IFeature, new()
    {
        private static readonly Lazy<IBsonSerializer<Envelope>> _envelopeSerializer = new(() => BsonSerializer.LookupSerializer<Envelope>());
        private static readonly Lazy<IBsonSerializer<Geometry>> _geometrySerializer = new(() => BsonSerializer.LookupSerializer<Geometry>());
        private static readonly Lazy<IBsonSerializer<AttributesTable>> _attributesTableSerializer = new(() => BsonSerializer.LookupSerializer<AttributesTable>());

        private readonly SerializerHelper _helper;

        public FeatureSerializer()
        {
            _helper = new SerializerHelper(
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("id", Flags.Id, isOptional: true),
                new SerializerHelper.Member("bbox", Flags.BoundingBox, true),
                new SerializerHelper.Member("geometry", Flags.Geometry),
                new SerializerHelper.Member("properties", Flags.Properties, isOptional: true));
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TFeature feature)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();

            writer.WriteName("type");
            writer.WriteString(nameof(Feature));

            var id = feature.Attributes?.GetOptionalValue("id");
            if (id != null)
            {
                writer.WriteName("id");
                BsonValueSerializer.Instance.Serialize(context, BsonValue.Create(id));
            }

            if (feature.BoundingBox != null)
            {
                writer.WriteName("bbox");
                _envelopeSerializer.Value.Serialize(context, feature.BoundingBox);
            }

            if (feature.Geometry != null)
            {
                writer.WriteName("geometry");
                _geometrySerializer.Value.Serialize(context, feature.Geometry);
            }

            if (feature.Attributes != null)
            {
                writer.WriteName("properties");
                _attributesTableSerializer.Value.Serialize(context, feature.Attributes);
            }

            writer.WriteEndDocument();
        }

        public override TFeature Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            var feature = new TFeature();
            object? featureId = null;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Type:
                        var type = reader.ReadString();
                        if (type != nameof(Feature))
                        {
                            throw new InvalidGeometryTypeException(nameof(Feature), type);
                        }

                        break;
                    case Flags.BoundingBox: feature.BoundingBox = _envelopeSerializer.Value.Deserialize(context); break;
                    case Flags.Geometry: feature.Geometry = _geometrySerializer.Value.Deserialize(context); break;
                    case Flags.Id: featureId = BsonTypeMapper.MapToDotNetValue(BsonValueSerializer.Instance.Deserialize(context)); break;
                    case Flags.Properties: feature.Attributes = _attributesTableSerializer.Value.Deserialize(context); break;
                    default: reader.SkipValue(); break;
                }
            });

            if (featureId != null)
            {
                if (feature.Attributes is null)
                {
                    feature.Attributes = new AttributesTable
                    {
                        { "id", featureId },
                    };
                }
                else if (feature.Attributes.Exists("id"))
                {
                    feature.Attributes["id"] = featureId;
                }
                else
                {
                    feature.Attributes.Add("id", featureId);
                }
            }

            return feature;
        }

        private class Flags
        {
            public const long Type = 1;
            public const long BoundingBox = 2;
            public const long Geometry = 4;
            public const long Id = 16;
            public const long Properties = 32;
        }
    }
}
