using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NetTopologySuite.Geometries;

namespace MongoDB.NetTopologySuite.Serialization.Helpers
{
    internal class CoordinateHelper
    {
        private readonly GeometryFactory _factory;

        public CoordinateHelper(GeometryFactory geometryFactory)
        {
            _factory = geometryFactory;
        }

        public IEnumerable<Coordinate[][]> ReadListOfListOfCoordinates(IBsonReader reader)
        {
            reader.ReadStartArray();

            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                yield return ReadListOfCoordinates(reader).ToArray();
            }

            reader.ReadEndArray();
        }

        public IEnumerable<Coordinate[]> ReadListOfCoordinates(IBsonReader reader)
        {
            reader.ReadStartArray();

            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                yield return ReadCoordinates(reader).ToArray();
            }

            reader.ReadEndArray();
        }

        public IEnumerable<Coordinate> ReadCoordinates(IBsonReader reader)
        {
            reader.ReadStartArray();

            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                yield return ReadCoordinate(reader);
            }

            reader.ReadEndArray();
        }

        public Coordinate ReadCoordinate(IBsonReader reader)
        {
            var xyzm = new List<double>(4);

            reader.ReadStartArray();

            while (reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                switch (reader.CurrentBsonType)
                {
                    case BsonType.Int32:
                        xyzm.Add(Convert.ToDouble(reader.ReadInt32(), CultureInfo.InvariantCulture));
                        break;

                    case BsonType.Double:
                        xyzm.Add(reader.ReadDouble());
                        break;

                    case BsonType.Null:
                        xyzm.Add(Coordinate.NullOrdinate);
                        break;
                }
            }

            reader.ReadEndArray();

            var coordinate = xyzm.Count switch
            {
                2 => new Coordinate(xyzm[0], xyzm[1]),
                3 => new CoordinateZ(xyzm[0], xyzm[1], xyzm[2]),
                4 => new CoordinateZM(xyzm[0], xyzm[1], xyzm[2], xyzm[3]),
                _ => new Coordinate()
            };

            _factory.PrecisionModel.MakePrecise(coordinate);

            return coordinate;
        }

        public void WriteCoordinate(IBsonWriter writer, CoordinateSequence sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                WriteCoordinateSequenceAt(writer, sequence, 0);
            }
        }

        public void WriteCoordinates(IBsonWriter writer, CoordinateSequence sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
                return;
            }

            writer.WriteStartArray();

            for (int i = 0; i < sequence.Count; i++)
            {
                WriteCoordinateSequenceAt(writer, sequence, i);
            }

            writer.WriteEndArray();
        }

        private void WriteCoordinateSequenceAt(IBsonWriter writer, CoordinateSequence sequence, int i)
        {
            writer.WriteStartArray();

            writer.WriteDouble(_factory.PrecisionModel.MakePrecise(sequence.GetX(i)));
            writer.WriteDouble(_factory.PrecisionModel.MakePrecise(sequence.GetY(i)));

            if (sequence.HasZ)
            {
                double z = sequence.GetZ(i);
                if (!double.IsNaN(z))
                {
                    writer.WriteDouble(_factory.PrecisionModel.MakePrecise(sequence.GetZ(i)));
                }
            }

            if (sequence.HasM)
            {
                double m = sequence.GetM(i);
                if (!double.IsNaN(m))
                {
                    writer.WriteDouble(_factory.PrecisionModel.MakePrecise(sequence.GetM(i)));
                }
            }

            writer.WriteEndArray();
        }
    }
}
