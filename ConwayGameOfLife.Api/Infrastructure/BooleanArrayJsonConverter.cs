using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConwayGameOfLife.Api.Infrastructure;

/// <summary>
/// JSON converter for 2D boolean arrays
/// </summary>
public class BooleanArrayJsonConverter : JsonConverter<bool[,]>
{
    /// <summary>
    /// Reads a 2D boolean array from JSON
    /// </summary>
    /// <param name="reader">The JSON reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The serializer options</param>
    /// <returns>A 2D boolean array</returns>
    public override bool[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        reader.Read();
        var rows = new List<List<bool>>();
        
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of array for row");
            }

            reader.Read();
            var row = new List<bool>();
            
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
                {
                    throw new JsonException("Expected boolean value");
                }

                row.Add(reader.GetBoolean());
                reader.Read();
            }
            
            rows.Add(row);
            reader.Read();
        }

        if (rows.Count == 0)
        {
            return new bool[0, 0];
        }

        int width = rows[0].Count;
        int height = rows.Count;
        var result = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            if (rows[y].Count != width)
            {
                throw new JsonException("All rows must have the same length");
            }

            for (int x = 0; x < width; x++)
            {
                result[x, y] = rows[y][x];
            }
        }

        return result;
    }

    /// <summary>
    /// Writes a 2D boolean array to JSON
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    /// <param name="value">The 2D boolean array</param>
    /// <param name="options">The serializer options</param>
    public override void Write(Utf8JsonWriter writer, bool[,] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        int width = value.GetLength(0);
        int height = value.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            writer.WriteStartArray();
            
            for (int x = 0; x < width; x++)
            {
                writer.WriteBooleanValue(value[x, y]);
            }
            
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}