namespace Roblox.Serialization.Json;

using System.Collections.Generic;

/// <summary>
/// Interface for trackable defined properties.
/// </summary>
public interface IDefinedPropertyTrackable
{
    /// <summary>
    /// Gets the defined property names.
    /// </summary>
    HashSet<string> DefinedPropertyNames { get; }
}
