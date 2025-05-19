namespace Roblox.Serialization.Json;

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Json converter for defined property trackable.
/// </summary>
public class DefinedPropertyTrackableImplConverter : JsonConverter
{
    private static ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> JsonFieldNameToPropertyMappings { get; } = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();

    /// <inheritdoc cref="JsonConverter.CanConvert(Type)"/>
    public override bool CanConvert(Type objectType)
    {
        if (objectType.IsClass) return objectType.GetInterfaces().Any(t => t == typeof(IDefinedPropertyTrackable));

        return false;
    }

    /// <inheritdoc cref="JsonConverter.ReadJson(JsonReader, Type, object?, JsonSerializer)"/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var trackableProperty = ((IDefinedPropertyTrackable)existingValue) ?? ((IDefinedPropertyTrackable)Activator.CreateInstance(objectType));
        trackableProperty.DefinedPropertyNames.Clear();

        foreach (var propertyMapping in GetJsonFieldToPropertyMappings(objectType))
        {
            if (JObject.Load(reader).TryGetValue(propertyMapping.Key, out JToken jtoken))
            {
                if (jtoken.Type != JTokenType.Null)
                    propertyMapping.Value.SetValue(trackableProperty, jtoken.ToObject(propertyMapping.Value.PropertyType, serializer));

                trackableProperty.DefinedPropertyNames.Add(propertyMapping.Value.Name);
            }
        }

        return trackableProperty;
    }

    /// <inheritdoc cref="JsonConverter.CanWrite"/>
    public override bool CanWrite => false;

    /// <inheritdoc cref="JsonConverter.WriteJson(JsonWriter, object?, JsonSerializer)"/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

    private IReadOnlyDictionary<string, PropertyInfo> GetJsonFieldToPropertyMappings(Type objectType)
    {
        if (JsonFieldNameToPropertyMappings.TryGetValue(objectType, out var propertyMapping))
            return propertyMapping;

        var newPropertyMapping = new Dictionary<string, PropertyInfo>();
        foreach (var info in objectType.GetProperties())
        {
            var attribute = info.GetCustomAttribute<DataMemberAttribute>();

            if (attribute != null && info.CanWrite && info.Name != "DefinedPropertyNames")
            {
                if (newPropertyMapping.ContainsKey(attribute.Name))
                    throw new InvalidDataContractException($"DataMemberAttribute with Name property value equal to {attribute.Name} specified more than once.");
                
                newPropertyMapping.Add(attribute.Name, info);
            }
        }

        JsonFieldNameToPropertyMappings.TryAdd(objectType, newPropertyMapping);

        return new ReadOnlyDictionary<string, PropertyInfo>(newPropertyMapping);
    }
}
