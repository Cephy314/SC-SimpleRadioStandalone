using System;
using GameInputDotNet.Interop.Enums;

namespace Ciribob.DCS.SimpleRadio.Standalone.Client.Input;

/// <summary>
/// Trigger related to mouse button, universal without device ID as only one mouse is expected
/// we don't want to lose binds because user plugged in another mouse.
/// </summary>
public class MouseButtonTrigger : InputTrigger , IEquatable<MouseButtonTrigger>
{
    public GameInputMouseButtons Button;

    public bool Equals(MouseButtonTrigger other)
    {
        if (other is null) return false;
        return base.Equals(other) && Button == other.Button;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (obj.GetType() != GetType()) return false;
        return Equals((MouseButtonTrigger)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), (int)Button);
    }

    public static bool operator ==(MouseButtonTrigger left, MouseButtonTrigger right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MouseButtonTrigger left, MouseButtonTrigger right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{base.ToString()} {Enum.GetName(Button)}";
    }
}