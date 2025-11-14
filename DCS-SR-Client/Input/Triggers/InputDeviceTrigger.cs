using System;
using GameInputDotNet.Interop;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public class InputDeviceTrigger : InputTrigger, IEquatable<InputDeviceTrigger>
{
    /// <summary>
    /// The Container ID from GameInput.Net to avoid many device ID.
    /// </summary>
    public AppLocalDeviceId Id { get; set; }

    public bool Equals(InputDeviceTrigger other)
    {
        if (other is null) return false;

        return base.Equals(other) && Id.Equals(other.Id);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;

        if (obj.GetType() != GetType()) return false;
        return Equals((InputDeviceTrigger)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id);
    }

    // TODO: Add ToString override that can add device name into mix.
}