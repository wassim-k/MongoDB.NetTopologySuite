using System;
using System.Collections.Generic;
using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public class EnvelopeSerializer : ClassSerializerBase<Envelope>
    {
        private readonly PrecisionModel _precisionModel;

        public EnvelopeSerializer(PrecisionModel? precisionModel = default)
        {
            _precisionModel = precisionModel ?? new PrecisionModel(PrecisionModels.Floating);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, Envelope envelope)
        {
            var writer = context.Writer;

            if (envelope == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            DoubleSerializer.Instance.Serialize(context, _precisionModel.MakePrecise(envelope.MinX));
            DoubleSerializer.Instance.Serialize(context, _precisionModel.MakePrecise(envelope.MinY));
            DoubleSerializer.Instance.Serialize(context, _precisionModel.MakePrecise(envelope.MaxX));
            DoubleSerializer.Instance.Serialize(context, _precisionModel.MakePrecise(envelope.MaxY));
            writer.WriteEndArray();
        }

        protected override Envelope DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var coords = new List<double>(4);
            var reader = context.Reader;

            reader.ReadStartArray();

            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                switch (reader.CurrentBsonType)
                {
                    case BsonType.Int32:
                        coords.Add(_precisionModel.MakePrecise(Convert.ToDouble(reader.ReadInt32(), CultureInfo.InvariantCulture)));
                        break;

                    case BsonType.Double:
                        coords.Add(_precisionModel.MakePrecise(reader.ReadDouble()));
                        break;
                }
            }

            reader.ReadEndArray();

            var minX = coords[0];
            var minY = coords[1];
            var maxX = coords[2];
            var maxY = coords[3];

            return new Envelope(minX, maxX, minY, maxY);
        }
    }
}
