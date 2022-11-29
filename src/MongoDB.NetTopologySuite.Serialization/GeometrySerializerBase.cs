using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.NetTopologySuite.Serialization.Exceptions;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization
{
    public abstract class GeometrySerializerBase<TGeometry, TCoordinates> : ClassSerializerBase<TGeometry>
        where TGeometry : Geometry
    {
        private readonly SerializerHelper _helper;
        private readonly string _type;

        public GeometrySerializerBase()
        {
            _type = typeof(TGeometry).Name;
            _helper = new SerializerHelper(
                new SerializerHelper.Member("type", Flags.Type),
                new SerializerHelper.Member("crs", Flags.CoordinateReferenceSystem, true),
                new SerializerHelper.Member("bbox", Flags.BoundingBox, true),
                new SerializerHelper.Member("coordinates", Flags.Coordinates));
        }

        public static GeometryFactory Wgs84Factory => BsonNetTopologySuiteSerializers.Wgs84Factory;

        internal abstract void WriteCoordinates(IBsonWriter writer, TGeometry value);

        internal abstract TCoordinates ReadCoordinates(IBsonReader reader);

        internal abstract TGeometry Create(TCoordinates coordinates);

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TGeometry geometry)
        {
            var writer = context.Writer;
            writer.WriteStartDocument();
            writer.WriteString("type", geometry.GeometryType);
            writer.WriteName("coordinates");
            WriteCoordinates(writer, geometry);
            writer.WriteEndDocument();
        }

        protected override TGeometry DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            TCoordinates coordinates = default!;

            _helper.DeserializeMembers(context, (elementName, flag) =>
            {
                switch (flag)
                {
                    case Flags.Type: ValidateType(reader.ReadString()); break;
                    case Flags.Coordinates: coordinates = ReadCoordinates(reader); break;
                    case Flags.CoordinateReferenceSystem: reader.SkipValue(); break;
                    case Flags.BoundingBox: reader.SkipValue(); break;
                    default: reader.SkipValue(); break;
                }
            });

            return Create(coordinates);
        }

        private void ValidateType(string type)
        {
            if (_type != type)
            {
                throw new InvalidGeometryTypeException(_type, type);
            }
        }

        private class Flags
        {
            public const long Type = 1;
            public const long CoordinateReferenceSystem = 2;
            public const long BoundingBox = 4;
            public const long Coordinates = 16;
        }
    }
}
