using System;
using GameInputDotNet.Interop.Enums;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

public class ControllerSwitchTrigger : InputDeviceTrigger, IEquatable<ControllerSwitchTrigger>
{
    /// <summary>
    /// Index of switch on controller
    /// </summary>
    public byte Index { get; set; }
    
    /// <summary>
    /// Switch position
    /// </summary>
    public GameInputSwitchPosition Position;

    public bool Equals(ControllerSwitchTrigger other)
    {
        if (other is null) return false;

        return base.Equals(other) && Position == other.Position && Index == other.Index;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
  
        if (obj.GetType() != GetType()) return false;
        return Equals((ControllerSwitchTrigger)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), (int)Position, Index);
    }
}