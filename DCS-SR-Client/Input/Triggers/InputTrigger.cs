using System;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Generic Input Trigger that identifies the device, kind and numeric value for the button or key.
/// </summary>
public abstract class InputTrigger : IEquatable<InputTrigger>
{
    public InputTriggerType Type { get; set; }

    public bool Equals(InputTrigger other)
    {
        if (other is null) return false;
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return Equals((InputTrigger)obj);
    }

    public override int GetHashCode()
    {
        return (int)Type;
    }

    public override string ToString()
    {
        return Type switch
        {
            InputTriggerType.GamePadButton or InputTriggerType.ControllerButton => "Button",
            InputTriggerType.ControllerSwitch => "Switch",
            InputTriggerType.Keyboard => "Key",
            InputTriggerType.MouseButton => "Mouse",
            _ => "Unknown"
        };
    }
}