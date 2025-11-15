using System;
using System.IO;
using System.Text.Json;
using Ciribob.DCS.SimpleRadio.Standalone.Common.Settings;
using NLog;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
///     Manager that handles binding and changing inputs, reading and writing etc.
/// </summary>
public class BindingManager : IBindingManager
{
    private readonly string _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private BindingProfile _currentProfile;
    public EventHandler<BindingProfile> BindingProfileChanged;

    /// <summary>
    ///     Handles all read-write file operations for bindings and individual reading/writing of binding changes.
    /// </summary>
    public BindingManager() { }

    #region IDisposable Members

    public void Dispose()
    {
        // TODO release managed resources here
    }

    #endregion

    /// <summary>
    ///     Write binding file with a given name and bindings object.
    /// </summary>
    /// <param name="bindings">Dictionary with <see cref="InputBinding" /> as key and <see cref="GameInputBinding" /> as value</param>
    /// <returns>True if file written successfully.</returns>
    private bool TryWriteBindingProfile(BindingProfile bindings)
    {
        if (bindings == null)
        {
            _logger.Error("Invalid binding profile object.");
            return false;
        }

        if (string.IsNullOrEmpty(bindings.ProfileName))
        {
            _logger.Error($"Invalid profile name: {bindings.ProfileName}");
            return false;
        }

        var name = bindings.ProfileName.Trim().ToLower();

        var filePath = Path.Combine(_appDirectory, $"bindings-{name}.json");

        try
        {
            var json = JsonSerializer.Serialize(bindings, _jsonSerializerOptions);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, $"Failed to write binding profile: {bindings.ProfileName}");
            return false;
        }
    }

    private bool SaveCurrent()
    {
        if (_currentProfile == null)
        {
            _logger.Error("No binding profile found.");
            return false;
        }

        if (!TryWriteBindingProfile(_currentProfile))
        {
            return false;
        }

        OnBindingProfileChanged(_currentProfile);
        return true;
    }

    /// <summary>
    ///     Read bindings from the filesystem into an object usable by the system.
    /// </summary>
    /// <param name="profileName">Name of the profile bindings are associated with</param>
    /// <param name="bindings">Dictionary with <see cref="InputBinding" /> as key and <see cref="GameInputBinding" /> as value</param>
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

        if (string.IsNullOrEmpty(_appDirectory) || !Directory.Exists(_appDirectory))
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

    public bool SetBinding(InputBinding bind, InputTrigger trigger)
    {
        if (trigger == null)
        {
            _logger.Error($"Got Null InputTrigger: {trigger}");
            return false;
        }

        if (_currentProfile == null)
        {
            _logger.Error("Binding profile object is null.");
            throw new NullReferenceException("Binding profile object is null.");
        }

        if ((int)bind >= 200)
        {
            _currentProfile.Bindings[bind - 100].Modifier = trigger;
        }
        else
        {
            _currentProfile.Bindings[bind].Input = trigger;
        }

        return SaveCurrent();
    }

    public bool ClearBinding(InputBinding binding)
    {
        if (_currentProfile == null)
        {
            _logger.Error("Binding profile object is null.");
            throw new NullReferenceException("Binding profile object is null.");
        }

        return _currentProfile.Bindings.Remove(binding);
    }

    public bool LoadBindingProfile(string profileName)
    {
        TryReadBindingProfile(profileName, out var bindings);
        if (bindings == null)
        {
            _logger.Error($"Failed to load binding profile: {profileName}");
            return false;
        }

        _currentProfile = bindings;
        OnBindingProfileChanged(_currentProfile);
        return true;
    }


    public void OnBindingProfileChanged(BindingProfile bindingProfile)
    {
        BindingProfileChanged?.Invoke(this, bindingProfile);
    }
}