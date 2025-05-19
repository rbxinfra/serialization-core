namespace Roblox.Serialization.Json;

using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using TimeZoneConverter;

/// <summary>
/// Implementation of <see cref="IsoDateTimeConverter"/> that is aware of <see cref="DateTimeKind"/>
/// </summary>
public class KindAwareDateTimeConverter : IsoDateTimeConverter
{
    private const string _CstZoneId = "Central Standard Time";
    private const string _NotSupportedKindExceptionMessage = "DateTimeKind Unspecified is not supported.";

    /// <summary>
    /// Gets the target read <see cref="DateTimeKind"/>
    /// </summary>
    public DateTimeKind TargetReadDateTimeKind { get; }

    /// <summary>
    /// Gets the target write <see cref="DateTimeKind"/>
    /// </summary>
    public DateTimeKind TargetWriteDateTimeKind { get; }

    /// <summary>
    /// Construct a new instance of <see cref="KindAwareDateTimeConverter"/>
    /// </summary>
    public KindAwareDateTimeConverter()
        : this(DateTimeKind.Utc, DateTimeKind.Utc)
    {
    }

    /// <summary>
    /// Construct a new instance of <see cref="KindAwareDateTimeConverter"/>
    /// </summary>
    /// <param name="targetReadDateTimeKind">The target read <see cref="DateTimeKind"/></param>
    public KindAwareDateTimeConverter(DateTimeKind targetReadDateTimeKind)
        : this(targetReadDateTimeKind, DateTimeKind.Utc)
    {
    }

    /// <summary>
    /// Construct a new instance of <see cref="KindAwareDateTimeConverter"/>
    /// </summary>
    /// <param name="targetReadDateTimeKind">The target read <see cref="DateTimeKind"/></param>
    /// <param name="targetWriteDateTimeKind">The target write <see cref="DateTimeKind"/></param>
    /// <exception cref="ArgumentException">
    /// - <paramref name="targetReadDateTimeKind"/> is unspecified.
    /// - <paramref name="targetWriteDateTimeKind"/> is unspecified.
    /// </exception>
    public KindAwareDateTimeConverter(DateTimeKind targetReadDateTimeKind, DateTimeKind targetWriteDateTimeKind)
    {
        if (targetReadDateTimeKind == DateTimeKind.Unspecified)
            throw new ArgumentException(_NotSupportedKindExceptionMessage, nameof(targetReadDateTimeKind));

        TargetReadDateTimeKind = targetReadDateTimeKind;

        if (targetWriteDateTimeKind == DateTimeKind.Unspecified)
            throw new ArgumentException(_NotSupportedKindExceptionMessage, nameof(targetWriteDateTimeKind));

        TargetWriteDateTimeKind = targetWriteDateTimeKind;
    }

    /// <inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object?, JsonSerializer)"/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var json = base.ReadJson(reader, objectType, existingValue, serializer);
        if (json != null && json is DateTime time)
            return TargetReadDateTimeKind == DateTimeKind.Utc
                ? TranslateToUtc(time)
                : TranslateToLocal(time);

        return json;
    }

    /// <inheritdoc cref="JsonConverter.WriteJson(JsonWriter, object?, JsonSerializer)"/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null || value is not DateTime time)
        {
            base.WriteJson(writer, value, serializer);

            return;
        }

        time = TargetWriteDateTimeKind == DateTimeKind.Utc 
            ? TranslateToUtc(time) 
            : TranslateToLocal(time);

        base.WriteJson(writer, time, serializer);
    }

    private static DateTime TranslateToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc) return dateTime;
        if (dateTime.Kind == DateTimeKind.Unspecified) return TimeZoneInfo.ConvertTimeToUtc(dateTime, TZConvert.GetTimeZoneInfo(_CstZoneId));
        
        return dateTime.ToUniversalTime();
    }

    private static DateTime TranslateToLocal(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local) return dateTime;
        if (dateTime.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

        return dateTime.ToLocalTime();
    }
}
