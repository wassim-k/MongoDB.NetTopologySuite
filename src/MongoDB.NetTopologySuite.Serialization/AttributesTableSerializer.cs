using System;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NetTopologySuite.Features;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class AttributesTableSerializer<TAttributesTable> : SerializerBase<TAttributesTable>
        where TAttributesTable : IAttributesTable, new()
    {
        public static readonly string IdPropertyName = "id";

        private static readonly IBsonSerializer<object> _objectSerializer = BsonSerializer.LookupSerializer<object>();

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TAttributesTable attributes)
        {
            var writer = context.Writer;

            if (attributes == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartDocument();

            foreach (string name in attributes.GetNames())
            {
                if (name == IdPropertyName)
                {
                    continue;
                }

                writer.WriteName(name);
                BsonSerializer.Serialize(writer, attributes.GetType(name), attributes[name]);
            }

            writer.WriteEndDocument();
        }

        public override TAttributesTable Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            var bsonType = reader.GetCurrentBsonType();

            switch (bsonType)
            {
                case BsonType.Document:
                    var dynamicContext = context.With(builder => builder.DynamicDocumentSerializer = this);

                    reader.ReadStartDocument();
                    var attributes = new TAttributesTable();

                    while (reader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var name = reader.ReadName();
                        var value = _objectSerializer.Deserialize(dynamicContext);
                        attributes.Add(name, value);
                    }

                    reader.ReadEndDocument();

                    return attributes;

                default:
                    var message = string.Format(CultureInfo.InvariantCulture, "Cannot deserialize a '{0}' from BsonType '{1}'.", BsonUtils.GetFriendlyTypeName(typeof(TAttributesTable)), bsonType);
                    throw new FormatException(message);
            }
        }
    }
}
