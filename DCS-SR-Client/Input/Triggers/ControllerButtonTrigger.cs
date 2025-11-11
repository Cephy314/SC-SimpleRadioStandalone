using System;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Store the index of the Controller button in relation to its AppLocalDeviceId
/// </summary>
public class ControllerButtonTrigger : InputDeviceTrigger, IEquatable<ControllerButtonTrigger>
{
    /// <summary>
    /// Index of pressed button.
    /// </summary>
    public byte Button { get; set; }

    public bool Equals(ControllerButtonTrigger other)
    {
        if (other is null) return false;

        return base.Equals(other) && Button == other.Button;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;

        if (obj.GetType() != GetType()) return false;
        return Equals((ControllerButtonTrigger)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Button);
    }
}