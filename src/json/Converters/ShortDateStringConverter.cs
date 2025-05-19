namespace Roblox.Serialization.Json;

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Json converter for converting short date strings.
/// </summary>
public class ShortDateStringConverter : DateTimeConverterBase
{
    private const string _DateFormat = "yyyy-MM-dd";
    private static readonly TimeSpan _1Day = TimeSpan.FromDays(1);

    /// <inheritdoc cref="JsonConverter.WriteJson(JsonWriter, object?, JsonSerializer)"/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is DateTime time)
        {
            writer.WriteValue(time.ToString(_DateFormat));

            return;
        }

        if (value is DateTimeOffset offset)
        {
            writer.WriteValue(offset.DateTime.ToString(_DateFormat));

            return;
        }

        throw new JsonSerializationException(string.Format("Unexpected type when converting date. Expected DateTime or DateTimeOffset, got {0}.", value.GetType()));
    }

    /// <inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object?, JsonSerializer)"/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
                throw new JsonSerializationException("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got null.");

            return null;
        }

        if (reader.Value is DateTime time)
        {
            if (IsDateTime(objectType)) return RoundDownDateTime(time);
            if (IsDateTimeOffset(objectType)) return RoundDownDateTimeOffset(time);
        }

        if (reader.Value is DateTimeOffset dateTimeOffset)
        {
            if (IsDateTimeOffset(objectType)) return RoundDownDateTimeOffset(dateTimeOffset);
            if (IsDateTime(objectType)) return RoundDownDateTime(dateTimeOffset.DateTime);
        }

        if (reader.Value is not string data || (!IsDateTime(objectType) && !IsDateTimeOffset(objectType)))
            throw new JsonSerializationException(string.Format("Unexpected value when converting date. Expected string, got {0}.", reader.ValueType));
        
        var dateTime = Convert.ToDateTime(data);
        if (IsDateTimeOffset(objectType)) return RoundDownDateTimeOffset(dateTime);

        return RoundDownDateTime(dateTime);
    }

    private DateTime RoundDownDateTime(DateTime dateTime) 
        => new(RoundDownTicksToNearestDay(dateTime.Ticks), DateTimeKind.Local);
    private DateTimeOffset RoundDownDateTimeOffset(DateTimeOffset dateTimeOffset)
        => new(new DateTime(RoundDownTicksToNearestDay(dateTimeOffset.Ticks)), TimeSpan.Zero);

    private long RoundDownTicksToNearestDay(long ticks) => ticks - ticks % _1Day.Ticks;

    private bool IsDateTime(Type type) => type == typeof(DateTime) || type == typeof(DateTime?);
    private bool IsDateTimeOffset(Type type) => type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);
}
