using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.NetTopologySuite.Serialization.Exceptions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MongoDB.NetTopologySuite.Serialization.Tests
{
    public class SerializationTests
    {
        private JsonSerializerSettings _jsonSerializerSettings;

        public SerializationTests()
        {
            BsonNetTopologySuiteSerializers.Register();
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters =
                {
                    new FeatureCollectionConverter(),
                    new FeatureConverter(),
                    new AttributesTableConverter(),
                    new GeometryConverter(),
                    new GeometryArrayConverter(),
                    new CoordinateConverter(),
                    new EnvelopeConverter(),
                }
            };
        }

        [Fact]
        public void PointTest()
        {
            var point = new Point(1, 2);

            TestRoundTrip(point);
        }

        [Fact]
        public void PointWithZTest()
        {
            var point = new Point(new CoordinateZ(1, 2, 3));

            var serialized = Serialize(point);
            var deserialized = Deserialize<Point>(serialized);

            Assert.Equal(point, deserialized);
        }

        [Fact]
        public void PointWithZMTest()
        {
            var point = new Point(new CoordinateZM(1, 2, 3, 4));

            var serialized = Serialize(point);
            var deserialized = Deserialize<Point>(serialized);

            Assert.Equal(point, deserialized);
        }

        [Fact]
        public void MultiPointTest()
        {
            var multiPoint = new MultiPoint(new[] { new Point(10, 10), new Point(11, 11), new Point(12, 12), });

            TestRoundTrip(multiPoint);
        }

        [Fact]
        public void LineStringTest()
        {
            var lineString = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) });

            TestRoundTrip(lineString);
        }

        [Fact]
        public void MultiLineStringTest()
        {
            var multiLineString = new MultiLineString(
                new[]
                {
                        new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) }),
                        new LineString(new[] { new Coordinate(10, 11), new Coordinate(20, 21) })
                });

            TestRoundTrip(multiLineString);
        }

        [Fact]
        public void MultiPolygonTest()
        {
            var multiPolygon =
                new MultiPolygon(
                    new[]
                    {
                        new Polygon(
                            new LinearRing(new[] { new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10), new Coordinate(10, 10) }),
                            new[]
                            {
                                new LinearRing(new[] { new Coordinate(11, 11), new Coordinate(19, 11), new Coordinate(19, 19), new Coordinate(11, 11) })
                            }),
                        new Polygon(
                            new LinearRing(
                                new[]
                                {
                                    new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10), new Coordinate(10, 10)
                                }))
                    });

            TestRoundTrip(multiPolygon);
        }

        [Fact]
        public void PolygonTest()
        {
            var polygon = new Polygon(
                new LinearRing(new[]
                {
                    new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10), new Coordinate(10, 10)
                }),
                new[]
                {
                    new LinearRing(new[]
                    {
                        new Coordinate(11, 11), new Coordinate(19, 11), new Coordinate(19, 19), new Coordinate(11, 11)
                    }),
                    new LinearRing(new[]
                    {
                        new Coordinate(12, 12), new Coordinate(20, 12), new Coordinate(20, 20), new Coordinate(12, 12)
                    })
                });

            TestRoundTrip(polygon);
        }

        [Fact]
        public void FeatureTest()
        {
            var attributes = new AttributesTable()
            {
                { "id", 1 },
                { "point", new Point(111, 222) },
                { "text", "value1" },
                { "dynamo", new { X = 1, Y = 2 } },
                { "array", new double[] { 1, 2 } },
            };

            var feature = new Feature(new Point(23, 56), attributes)
            {
                BoundingBox = new Envelope(1, 2, 3, 4)
            };

            TestRoundTrip(feature);
        }

        [Fact]
        public void GeometryCollectionTest()
        {
            var geometryCollection = new GeometryCollection(new Geometry[]
            {
                new Point(23, 56),
                new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) }),
                new Polygon(
                    new LinearRing(new[] { new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10), new Coordinate(10, 10) }),
                    new[]
                    {
                        new LinearRing(new[] { new Coordinate(11, 11), new Coordinate(19, 11), new Coordinate(19, 19), new Coordinate(11, 11) })
                    })
            });

            TestRoundTrip(geometryCollection);
        }

        [Fact]
        public void FeatureCollectionTest()
        {
            var feature = new Feature(new Point(23, 56), null)
            {
                BoundingBox = new Envelope(1, 2, 3, 4)
            };

            var features = new FeatureCollection { feature };

            TestRoundTrip(features);
        }

        [Fact]
        public void GeometryTypeValidationTest()
        {
            var document = BsonDocument.Parse(@"{ ""type"": ""Point"", ""coordinates"": [1, 2] }");

            Assert.Throws<InvalidGeometryTypeException>(() => Deserialize<Polygon>(document));
        }

        private void TestRoundTrip<T>(T value)
        {
            var serialized = Serialize(value);
            var deserialized = Deserialize<T>(serialized);
            var roundtrip = Serialize(deserialized);

            Assert.Equal(serialized, roundtrip);

            var bson = roundtrip.ToJson();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value, _jsonSerializerSettings);

            Assert.Equal(JToken.Parse(bson), JToken.Parse(json));
        }

        private static T Deserialize<T>(BsonDocument document)
        {
            var deserializer = BsonSerializer.LookupSerializer<T>();
            using var reader = new BsonDocumentReader(document);
            var context = BsonDeserializationContext.CreateRoot(reader);
            return deserializer.Deserialize(context);
        }

        private static BsonDocument Serialize<T>(T value)
        {
            var serializer = BsonSerializer.LookupSerializer<T>();
            var document = new BsonDocument();
            using var writer = new BsonDocumentWriter(document);
            var context = BsonSerializationContext.CreateRoot(writer);
            serializer.Serialize(context, value);
            return document;
        }
    }
}
