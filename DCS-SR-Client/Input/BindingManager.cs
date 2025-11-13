using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Manager that handles binding and changing inputs, reading and writing etc.
/// </summary>
public class BindingManager : IDisposable
{
    private readonly Logger  _logger = LogManager.GetCurrentClassLogger(); 
    private readonly string _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() {WriteIndented = true};
    BindingManager()
    {
        
    }

    /// <summary>
    /// Write binding file with a given name and bindings object.
    /// </summary>
    /// <param name="profileName">Name of the profile bindings are associated with</param>
    /// <param name="bindings">Dictionary with <see cref="InputBinding"/> as key and <see cref="GameInputBinding" /> as value</param>
    /// <returns>True if file written successfully.</returns>
    private bool TryWriteBindingProfile(string profileName, BindingProfile bindings)
    {
        if (bindings == null || bindings == default(BindingProfile))
        {
            _logger.Error($"Invalid binding profile object.");
            return false;
        }

        if (string.IsNullOrEmpty(profileName))
        {
            _logger.Error($"Invalid profile name: {profileName}");
            return false;
        }

        var name = profileName.Trim().ToLower();
        
        var filePath = Path.Combine(_appDirectory, $"bindings-{name}.json");

        try
        {
            var json = JsonSerializer.Serialize(bindings, _jsonSerializerOptions);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to write binding profile: {profileName}");
            return false;
        }
        
    }

    /// <summary>
    /// Read bindings from the filesystem into an object usable by the system.
    /// </summary>
    /// <param name="profileName">Name of the profile bindings are associated with</param>
    /// <param name="bindings">Dictionary with <see cref="InputBinding"/> as key and <see cref="GameInputBinding" /> as value</param>
    /// <returns>True if file read successfully</returns>
    private bool TryReadBindingProfile(string profileName, out BindingProfile bindings)
    {
        bindings = null;
        if (string.IsNullOrEmpty(profileName))
        {
            _logger.Error("Profile name is null or empty");
            return false;
        }
        var name = profileName.Trim().ToLower();
        
        if (String.IsNullOrEmpty(_appDirectory) || !Directory.Exists(_appDirectory))
        {
            _logger.Error($"Invalid Directory: {_appDirectory}");
        }

        var filePath = Path.Combine(_appDirectory, $"bindings-{name}.json");
        
        if (!File.Exists(filePath))
        {
            _logger.Error($"Binding file not found: {filePath}");
            return false;
        }
        
        try 
        {
            // Read file and convert it to our profile object.
            var json = File.ReadAllText(filePath);
            bindings = JsonSerializer.Deserialize<BindingProfile>(json);
            return true; 
        }
        catch (Exception e)
        {
            _logger.Error(e);
            return false;
        }
    }

    public void SetBinding(GameInputBinding binding)
    {
        if (_currentProfile == null)
        {
            _logger.Error($"Binding profile object is null.");
            throw new NullReferenceException($"Binding profile object is null.");
        }

        _currentProfile.Bindings[binding.Binding] = binding;
    }

    private bool ClearBinding(InputBinding binding)
    {
        if (_currentProfile == null)
        {
            _logger.Error($"Binding profile object is null.");
            throw new NullReferenceException($"Binding profile object is null.");
        }
        
        return _currentProfile.Bindings.Remove(binding);
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
