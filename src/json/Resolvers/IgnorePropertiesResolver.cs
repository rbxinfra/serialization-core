namespace Roblox.Serialization.Json;

using System;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Contract resolver to ignore properties.
/// </summary>
public class IgnorePropertiesResolver : DefaultContractResolver
{
    private readonly ISet<string> _PropertiesToIgnore;

    /// <summary>
    /// Constructs a new instance of <see cref="IgnorePropertiesResolver"/>
    /// </summary>
    /// <param name="propertiesToIgnore">The names of the properties to ignore.</param>
    /// <exception cref="ArgumentNullException"><paramref name="propertiesToIgnore"/> cannot be null.</exception>
    public IgnorePropertiesResolver(ISet<string> propertiesToIgnore)
    {
        _PropertiesToIgnore = propertiesToIgnore ?? throw new ArgumentNullException(nameof(propertiesToIgnore));
    }

    /// <inheritdoc cref="DefaultContractResolver.CreateProperty(MemberInfo, MemberSerialization)"/>
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);
        if (_PropertiesToIgnore.Contains(prop.PropertyName)) prop.ShouldSerialize = x => false;

        return prop;
    }
}
